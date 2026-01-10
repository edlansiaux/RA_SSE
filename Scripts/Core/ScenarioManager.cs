using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RASSE.Core
{
    /// <summary>
    /// ScenarioManager - Gestion du scénario "Effondrement de bâtiment"
    /// Implémente la séquence exacte décrite dans le rapport:
    /// 
    /// DIAGRAMME DE SÉQUENCE 1 - CLASSIFICATION START:
    /// 1. Secouriste arrive et équipe la LRA
    /// 2. Déplacement et analyse des images pour localiser victimes
    /// 3. Calcul automatique des constantes
    /// 4. Classification START automatique
    /// 5. Création du dossier patient
    /// 6. Attente validation/modification par commande vocale
    /// 
    /// DIAGRAMME DE SÉQUENCE 2 - CHOIX DE L'ACTION:
    /// 7. Proposition d'action (hôpital ou domicile)
    /// 8. Validation de l'action
    /// 9. Si évacuation: proposition meilleur hôpital
    /// 10. Validation hôpital
    /// 11. Affectation ambulance
    /// 12. Guidage vers ambulance
    /// 13. Retour à la recherche de victimes
    /// </summary>
    public class ScenarioManager : MonoBehaviour
    {
        public static ScenarioManager Instance { get; private set; }

        [Header("=== CONFIGURATION SCÉNARIO ===")]
        [SerializeField] private ScenarioType currentScenario = ScenarioType.BuildingCollapse;
        [SerializeField] private string scenarioName = "Effondrement de bâtiment";
        [SerializeField] private string scenarioDescription = "Un bâtiment s'est effondré. Le secouriste utilise la LRA pour trier et évacuer les victimes.";

        [Header("=== RÉFÉRENCES SYSTÈME ===")]
        [SerializeField] private SystemArchitecture systemArchitecture;
        [SerializeField] private LunettesRA lunettesRA;
        [SerializeField] private StartTriageSystem triageSystem;
        [SerializeField] private ARInterfaceController arInterface;
        [SerializeField] private SMACoordinator smaCoordinator;
        [SerializeField] private VoiceCommandSimulator voiceCommands;

        [Header("=== ÉTAT DU SCÉNARIO ===")]
        [SerializeField] private ScenarioPhase currentPhase = ScenarioPhase.NotStarted;
        [SerializeField] private TriageWorkflowState workflowState = TriageWorkflowState.Idle;

        [Header("=== VICTIMES ===")]
        [SerializeField] private List<VictimController> pendingVictims = new List<VictimController>();
        [SerializeField] private List<VictimController> processedVictims = new List<VictimController>();
        [SerializeField] private VictimController currentVictim;

        [Header("=== DÉCISIONS EN ATTENTE ===")]
        [SerializeField] private StartCategory pendingTriageCategory;
        [SerializeField] private HospitalData pendingHospital;
        [SerializeField] private OrientationAction pendingAction;

        [Header("=== ÉVÉNEMENTS ===")]
        public UnityEvent OnScenarioStarted;
        public UnityEvent OnScenarioCompleted;
        public UnityEvent<VictimController> OnVictimLocalized;
        public UnityEvent<VictimController, VitalSigns> OnConstantesAnalyzed;
        public UnityEvent<VictimController, StartCategory> OnSTARTClassified;
        public UnityEvent<VictimController> OnDossierCreated;
        public UnityEvent<VictimController, OrientationAction> OnActionProposed;
        public UnityEvent<VictimController, HospitalData> OnHospitalProposed;
        public UnityEvent<VictimController, AmbulanceController> OnAmbulanceAssigned;
        public UnityEvent<VictimController> OnGuidanceStarted;
        public UnityEvent<VictimController> OnVictimProcessed;

        // Métriques
        private DateTime scenarioStartTime;
        private int totalVictimsToProcess;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // S'abonner aux commandes vocales
            if (voiceCommands != null)
            {
                voiceCommands.OnCommandRecognized.AddListener(HandleVoiceCommand);
            }
        }

        /// <summary>
        /// Démarre le scénario d'effondrement de bâtiment
        /// </summary>
        public void StartScenario()
        {
            Debug.Log($"[ScenarioManager] ═══════════════════════════════════════");
            Debug.Log($"[ScenarioManager] DÉMARRAGE SCÉNARIO: {scenarioName}");
            Debug.Log($"[ScenarioManager] {scenarioDescription}");
            Debug.Log($"[ScenarioManager] ═══════════════════════════════════════");

            scenarioStartTime = DateTime.Now;
            currentPhase = ScenarioPhase.Initialization;

            // Étape 1: Le secouriste équipe la LRA
            StartCoroutine(InitializeLRA());
        }

        /// <summary>
        /// Phase 1: Initialisation de la LRA
        /// </summary>
        private IEnumerator InitializeLRA()
        {
            Debug.Log("[ScenarioManager] Phase 1: Le secouriste équipe la LRA");
            
            // Vérifier que tous les modules sont opérationnels
            if (lunettesRA != null)
            {
                var status = lunettesRA.GetStatus();
                Debug.Log($"[ScenarioManager] État LRA: Batterie {status.BatteryLevel}%, Connecté: {status.IsConnected}");
            }

            arInterface?.ShowAlert("LRA activée - Système opérationnel", Color.green, 3f);

            yield return new WaitForSeconds(2f);

            currentPhase = ScenarioPhase.VictimSearch;
            workflowState = TriageWorkflowState.SearchingVictims;

            // Valider l'exigence système
            RequirementsManager.Instance?.ValidateRequirement("REQ-0", "Scénario START démarré");

            OnScenarioStarted?.Invoke();

            Debug.Log("[ScenarioManager] Phase 2: Recherche des victimes - Déplacez-vous et scannez la zone");
        }

        /// <summary>
        /// Appelé quand une victime est détectée dans le champ de vision
        /// Implémente: Diagramme Séquence 1 - Étapes 2-6
        /// </summary>
        public void OnVictimDetected(VictimController victim)
        {
            if (victim == null || processedVictims.Contains(victim)) return;

            Debug.Log($"[ScenarioManager] ─── VICTIME DÉTECTÉE: {victim.PatientId} ───");

            // Étape 2: Localisation de la victime
            currentVictim = victim;
            workflowState = TriageWorkflowState.VictimLocalized;
            OnVictimLocalized?.Invoke(victim);

            // Valider REQ-1
            RequirementsManager.Instance?.ValidateRequirement("REQ-1", $"Victime {victim.PatientId} localisée");

            // Démarrer la séquence de triage
            StartCoroutine(TriageSequence(victim));
        }

        /// <summary>
        /// Séquence de triage complète (Diagramme Séquence 1)
        /// </summary>
        private IEnumerator TriageSequence(VictimController victim)
        {
            // Étape 3: Calcul automatique des constantes
            Debug.Log($"[ScenarioManager] Étape 3: Analyse des constantes vitales...");
            workflowState = TriageWorkflowState.AnalyzingVitals;

            yield return new WaitForSeconds(1.5f); // Simulation du temps d'analyse

            var vitals = victim.VitalSigns;
            Debug.Log($"[ScenarioManager] Constantes: FR={vitals.respiratoryRate}/min, FC={vitals.heartRate}bpm, RC={vitals.capillaryRefillTime}s");
            
            // Valider REQ-2
            RequirementsManager.Instance?.ValidateRequirement("REQ-2", $"Constantes analysées pour {victim.PatientId}");
            OnConstantesAnalyzed?.Invoke(victim, vitals);

            // Afficher les constantes en AR
            arInterface?.ShowVictimInfo(victim);

            yield return new WaitForSeconds(1f);

            // Étape 4: Classification START automatique
            Debug.Log($"[ScenarioManager] Étape 4: Classification START...");
            workflowState = TriageWorkflowState.ClassifyingSTART;

            pendingTriageCategory = triageSystem.CalculateStartCategory(vitals);
            Debug.Log($"[ScenarioManager] Classification suggérée: {pendingTriageCategory}");

            // Valider REQ-3
            RequirementsManager.Instance?.ValidateRequirement("REQ-3", $"Classification START: {pendingTriageCategory}");
            OnSTARTClassified?.Invoke(victim, pendingTriageCategory);

            // Étape 5: Création du dossier patient
            Debug.Log($"[ScenarioManager] Étape 5: Création dossier patient...");
            smaCoordinator?.baseDonneesPatient?.CreateOrUpdateDossier(victim);
            
            // Valider REQ-7
            RequirementsManager.Instance?.ValidateRequirement("REQ-7", $"Dossier créé pour {victim.PatientId}");
            OnDossierCreated?.Invoke(victim);

            // Étape 6: Attente validation par commande vocale
            Debug.Log($"[ScenarioManager] Étape 6: En attente de validation...");
            Debug.Log($"[ScenarioManager] Dites 'VALIDER' pour confirmer {pendingTriageCategory}, ou 'ROUGE/JAUNE/VERT/NOIR' pour modifier");
            
            workflowState = TriageWorkflowState.WaitingTriageValidation;
            currentPhase = ScenarioPhase.TriageValidation;

            arInterface?.ShowAlert(
                $"Classification suggérée: {pendingTriageCategory}\nDites 'Valider' ou modifiez", 
                StartTriageSystem.GetCategoryColor(pendingTriageCategory), 
                10f
            );
        }

        /// <summary>
        /// Gestion des commandes vocales pour la validation
        /// </summary>
        private void HandleVoiceCommand(string commandId)
        {
            Debug.Log($"[ScenarioManager] Commande vocale reçue: {commandId}");

            switch (workflowState)
            {
                case TriageWorkflowState.WaitingTriageValidation:
                    HandleTriageValidationCommand(commandId);
                    break;

                case TriageWorkflowState.WaitingActionValidation:
                    HandleActionValidationCommand(commandId);
                    break;

                case TriageWorkflowState.WaitingHospitalValidation:
                    HandleHospitalValidationCommand(commandId);
                    break;
            }
        }

        /// <summary>
        /// Validation du triage (Diagramme Séquence 1 - Étape 6)
        /// </summary>
        private void HandleTriageValidationCommand(string command)
        {
            StartCategory finalCategory = pendingTriageCategory;
            bool isOverride = false;

            switch (command.ToLower())
            {
                case "validate":
                    // Validation de la suggestion
                    break;
                case "red":
                    finalCategory = StartCategory.Red;
                    isOverride = true;
                    break;
                case "yellow":
                    finalCategory = StartCategory.Yellow;
                    isOverride = true;
                    break;
                case "green":
                    finalCategory = StartCategory.Green;
                    isOverride = true;
                    break;
                case "black":
                    finalCategory = StartCategory.Black;
                    isOverride = true;
                    break;
                default:
                    return; // Commande non pertinente
            }

            // Valider le triage
            triageSystem.ValidateTriage(currentVictim, finalCategory, isOverride);
            Debug.Log($"[ScenarioManager] Triage validé: {finalCategory}" + (isOverride ? " (modifié)" : ""));

            // Passer à la phase de choix d'action (Diagramme Séquence 2)
            StartCoroutine(ActionDecisionSequence(currentVictim, finalCategory));
        }

        /// <summary>
        /// Séquence de décision d'action (Diagramme Séquence 2)
        /// </summary>
        private IEnumerator ActionDecisionSequence(VictimController victim, StartCategory category)
        {
            currentPhase = ScenarioPhase.ActionDecision;

            // Étape 7: Le LRA propose une action
            Debug.Log($"[ScenarioManager] ─── DIAGRAMME SÉQUENCE 2: CHOIX ACTION ───");
            Debug.Log($"[ScenarioManager] Étape 7: Proposition d'action...");

            yield return new WaitForSeconds(1f);

            // Déterminer l'action selon la catégorie
            switch (category)
            {
                case StartCategory.Red:
                case StartCategory.Yellow:
                    pendingAction = OrientationAction.EvacuateToHospital;
                    arInterface?.ShowAlert(
                        $"Action proposée: ÉVACUATION vers hôpital\nDites 'Valider' pour confirmer",
                        Color.cyan, 10f
                    );
                    break;

                case StartCategory.Green:
                    pendingAction = OrientationAction.SendToPRV;
                    arInterface?.ShowAlert(
                        $"Action proposée: Direction PRV (Point de Regroupement)\nDites 'Valider' ou 'Domicile'",
                        Color.green, 10f
                    );
                    break;

                case StartCategory.Black:
                    pendingAction = OrientationAction.DeclareDeceased;
                    arInterface?.ShowAlert(
                        $"Patient décédé - Signalement effectué\nDites 'Valider' pour confirmer",
                        Color.gray, 10f
                    );
                    break;
            }

            // Valider REQ-5
            RequirementsManager.Instance?.ValidateRequirement("REQ-5", $"Consignes affichées pour {victim.PatientId}");
            OnActionProposed?.Invoke(victim, pendingAction);

            // Étape 8: Attente validation de l'action
            Debug.Log($"[ScenarioManager] Étape 8: En attente de validation de l'action...");
            workflowState = TriageWorkflowState.WaitingActionValidation;
        }

        /// <summary>
        /// Validation de l'action
        /// </summary>
        private void HandleActionValidationCommand(string command)
        {
            if (command.ToLower() == "validate" || command.ToLower() == "hospital")
            {
                Debug.Log($"[ScenarioManager] Action validée: {pendingAction}");

                if (pendingAction == OrientationAction.EvacuateToHospital)
                {
                    // Étapes 9-12: Évacuation vers hôpital
                    StartCoroutine(HospitalEvacuationSequence(currentVictim));
                }
                else
                {
                    // Fin du traitement pour cette victime
                    CompleteVictimProcessing(currentVictim);
                }
            }
            else if (command.ToLower() == "home" && currentVictim.TriageCategory == StartCategory.Green)
            {
                pendingAction = OrientationAction.SendHome;
                Debug.Log($"[ScenarioManager] Action modifiée: Retour domicile");
                CompleteVictimProcessing(currentVictim);
            }
        }

        /// <summary>
        /// Séquence d'évacuation vers hôpital (Diagramme Séquence 2 - Étapes 9-12)
        /// </summary>
        private IEnumerator HospitalEvacuationSequence(VictimController victim)
        {
            currentPhase = ScenarioPhase.HospitalSelection;

            // Étape 9: Le LRA propose le meilleur hôpital
            Debug.Log($"[ScenarioManager] Étape 9: Recherche du meilleur hôpital...");
            
            yield return new WaitForSeconds(1f);

            pendingHospital = smaCoordinator?.informationsHopitaux?.GetBestHospital(victim.TriageCategory);

            if (pendingHospital != null)
            {
                Debug.Log($"[ScenarioManager] Hôpital recommandé: {pendingHospital.hospitalName} ({pendingHospital.distanceKm}km)");
                
                arInterface?.ShowAlert(
                    $"Hôpital recommandé:\n{pendingHospital.hospitalName}\nDistance: {pendingHospital.distanceKm:F1}km\nDites 'Valider' pour confirmer",
                    new Color(0.2f, 0.6f, 1f), 10f
                );

                // Valider REQ-4
                RequirementsManager.Instance?.ValidateRequirement("REQ-4", $"Hôpital {pendingHospital.hospitalName} proposé");
                OnHospitalProposed?.Invoke(victim, pendingHospital);
            }

            // Étape 10: Attente validation du choix d'hôpital
            Debug.Log($"[ScenarioManager] Étape 10: En attente de validation de l'hôpital...");
            workflowState = TriageWorkflowState.WaitingHospitalValidation;
        }

        /// <summary>
        /// Validation du choix d'hôpital
        /// </summary>
        private void HandleHospitalValidationCommand(string command)
        {
            if (command.ToLower() == "validate" || command.ToLower() == "hospital")
            {
                Debug.Log($"[ScenarioManager] Hôpital validé: {pendingHospital?.hospitalName}");
                
                // Étape 11: Affectation d'une ambulance
                StartCoroutine(AmbulanceAssignmentSequence(currentVictim, pendingHospital));
            }
        }

        /// <summary>
        /// Affectation ambulance et guidage (Étapes 11-12)
        /// </summary>
        private IEnumerator AmbulanceAssignmentSequence(VictimController victim, HospitalData hospital)
        {
            currentPhase = ScenarioPhase.AmbulanceAssignment;

            // Étape 11: Le LRA affecte une ambulance disponible
            Debug.Log($"[ScenarioManager] Étape 11: Affectation d'une ambulance...");

            yield return new WaitForSeconds(1f);

            var ambulance = GameManager.Instance?.RequestAmbulance(victim, hospital);

            if (ambulance != null)
            {
                Debug.Log($"[ScenarioManager] Ambulance affectée: {ambulance.AmbulanceId}");
                
                arInterface?.ShowAlert(
                    $"Ambulance {ambulance.AmbulanceId} affectée\nSuivez le guidage AR",
                    Color.green, 5f
                );

                OnAmbulanceAssigned?.Invoke(victim, ambulance);

                yield return new WaitForSeconds(2f);

                // Étape 12: Guidage vers l'ambulance
                Debug.Log($"[ScenarioManager] Étape 12: Démarrage du guidage vers l'ambulance...");
                currentPhase = ScenarioPhase.AmbulanceGuidance;

                lunettesRA?.retourAmbulance?.StartGuidance(ambulance.transform, ambulance.AmbulanceId);
                
                // Valider REQ-6
                RequirementsManager.Instance?.ValidateRequirement("REQ-6", $"Guidage vers ambulance {ambulance.AmbulanceId}");
                OnGuidanceStarted?.Invoke(victim);
            }

            // Étape 13: Fin du traitement, retour à la recherche
            CompleteVictimProcessing(victim);
        }

        /// <summary>
        /// Finalise le traitement d'une victime
        /// </summary>
        private void CompleteVictimProcessing(VictimController victim)
        {
            Debug.Log($"[ScenarioManager] ─── VICTIME {victim.PatientId} TRAITÉE ───");

            processedVictims.Add(victim);
            pendingVictims.Remove(victim);
            currentVictim = null;

            OnVictimProcessed?.Invoke(victim);

            // Vérifier s'il reste des victimes
            if (pendingVictims.Count > 0)
            {
                Debug.Log($"[ScenarioManager] {pendingVictims.Count} victime(s) restante(s)");
                Debug.Log($"[ScenarioManager] Retour à la recherche de victimes...");

                currentPhase = ScenarioPhase.VictimSearch;
                workflowState = TriageWorkflowState.SearchingVictims;

                arInterface?.ShowAlert("Continuez la recherche de victimes", Color.yellow, 3f);
            }
            else
            {
                // Scénario terminé
                EndScenario();
            }
        }

        /// <summary>
        /// Termine le scénario
        /// </summary>
        private void EndScenario()
        {
            Debug.Log($"[ScenarioManager] ═══════════════════════════════════════");
            Debug.Log($"[ScenarioManager] SCÉNARIO TERMINÉ");
            Debug.Log($"[ScenarioManager] Durée: {(DateTime.Now - scenarioStartTime).TotalMinutes:F1} minutes");
            Debug.Log($"[ScenarioManager] Victimes traitées: {processedVictims.Count}");
            Debug.Log($"[ScenarioManager] ═══════════════════════════════════════");

            currentPhase = ScenarioPhase.Completed;
            workflowState = TriageWorkflowState.Idle;

            // Générer le rapport de conformité
            var complianceReport = RequirementsManager.Instance?.GenerateTextReport();
            Debug.Log(complianceReport);

            arInterface?.ShowAlert(
                $"Scénario terminé!\n{processedVictims.Count} victimes traitées",
                Color.green, 10f
            );

            OnScenarioCompleted?.Invoke();
        }

        /// <summary>
        /// Enregistre une nouvelle victime à traiter
        /// </summary>
        public void RegisterVictim(VictimController victim)
        {
            if (!pendingVictims.Contains(victim) && !processedVictims.Contains(victim))
            {
                pendingVictims.Add(victim);
                totalVictimsToProcess++;
            }
        }

        // Getters pour l'état du scénario
        public ScenarioPhase CurrentPhase => currentPhase;
        public TriageWorkflowState WorkflowState => workflowState;
        public int RemainingVictims => pendingVictims.Count;
        public int ProcessedVictimCount => processedVictims.Count;
    }

    /// <summary>
    /// Phases du scénario
    /// </summary>
    public enum ScenarioPhase
    {
        NotStarted,
        Initialization,         // Équipement de la LRA
        VictimSearch,          // Recherche des victimes
        VictimDetected,        // Victime détectée
        VitalsAnalysis,        // Analyse des constantes
        STARTClassification,   // Classification START
        TriageValidation,      // Validation du triage
        ActionDecision,        // Choix de l'action
        HospitalSelection,     // Sélection de l'hôpital
        AmbulanceAssignment,   // Affectation ambulance
        AmbulanceGuidance,     // Guidage vers ambulance
        Completed              // Scénario terminé
    }

    /// <summary>
    /// États du workflow de triage
    /// </summary>
    public enum TriageWorkflowState
    {
        Idle,
        SearchingVictims,
        VictimLocalized,
        AnalyzingVitals,
        ClassifyingSTART,
        WaitingTriageValidation,
        WaitingActionValidation,
        WaitingHospitalValidation,
        GuidingToAmbulance
    }
}
