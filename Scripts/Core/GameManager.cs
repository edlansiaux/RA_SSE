using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

namespace RASSE.Core
{
    /// <summary>
    /// GameManager - Gestionnaire principal du simulateur RA SSE
    /// Coordonne tous les systèmes conformément à l'architecture BDD SysML:
    /// - Bloc 2: Module Classification START
    /// - Bloc 3: Interface RA  
    /// - Bloc 4: Système Coordination SMA
    /// 
    /// Implémente REQ-0: Support triage START bout-en-bout
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("=== IDENTIFICATION SESSION ===")]
        [SerializeField] private string sessionId;
        public string SessionId => sessionId;

        [Header("=== CONFIGURATION SCÉNARIO ===")]
        [Tooltip("Type de scénario d'urgence")]
        public ScenarioType scenarioType = ScenarioType.BuildingCollapse;
        
        [Tooltip("Nombre de victimes à générer")]
        [Range(1, 50)]
        public int numberOfVictims = 10;
        
        [Tooltip("Difficulté du scénario")]
        public DifficultyLevel difficulty = DifficultyLevel.Normal;

        [Header("=== RÉFÉRENCES ARCHITECTURE (BDD SysML) ===")]
        [Tooltip("Bloc 2: Module Classification START")]
        public StartTriageSystem triageSystem;
        
        [Tooltip("Bloc 4: Système Coordination SMA - Hôpitaux")]
        public HospitalCoordinationSystem hospitalSystem;
        
        [Tooltip("Bloc 4: Système Coordination SMA - Ambulances")]
        public AmbulanceManager ambulanceManager;
        
        [Tooltip("Bloc 3: Interface RA")]
        public ARInterfaceController arInterface;
        
        [Tooltip("Générateur de victimes")]
        public VictimSpawner victimSpawner;
        
        [Tooltip("Contrôleur secouriste")]
        public RescuerController rescuer;

        [Header("=== GESTIONNAIRES ADDITIONNELS ===")]
        public ScenarioManager scenarioManager;
        public RequirementsManager requirementsManager;
        public SystemArchitecture systemArchitecture;

        [Header("=== STATISTIQUES ===")]
        public GameStatistics statistics;

        [Header("=== ÉVÉNEMENTS ===")]
        public UnityEvent OnGameStart;
        public UnityEvent OnGameEnd;
        public UnityEvent<VictimController> OnVictimDetected;
        public UnityEvent<VictimController, StartCategory> OnVictimTriaged;
        public UnityEvent<VictimController> OnVictimEvacuated;

        // État du jeu
        public GameState CurrentState { get; private set; } = GameState.Initializing;
        public float ElapsedTime { get; private set; }
        
        // Listes de victimes
        public List<VictimController> AllVictims { get; private set; } = new List<VictimController>();
        public List<VictimController> TriagedVictims { get; private set; } = new List<VictimController>();
        public List<VictimController> EvacuatedVictims { get; private set; } = new List<VictimController>();

        // Propriétés pour compatibilité avec UI et rapports
        public int TotalVictims => AllVictims.Count;
        public int VictimsDetected => statistics.victimsDetected;
        public int VictimsTriaged => TriagedVictims.Count;
        public int VictimsEvacuated => EvacuatedVictims.Count;
        public int RedCount => statistics.redCount;
        public int YellowCount => statistics.yellowCount;
        public int GreenCount => statistics.greenCount;
        public int BlackCount => statistics.blackCount;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Générer ID de session unique
            sessionId = $"SESSION-{DateTime.Now:yyyyMMdd-HHmmss}-{UnityEngine.Random.Range(1000, 9999)}";

            InitializeStatistics();
        }

        private void Start()
        {
            // Valider REQ-0 au démarrage
            RequirementsManager.Instance?.TraceRequirementEvent("REQ-0", "Système RA SSE initialisé");

            // Si le scénario doit démarrer automatiquement
            // StartScenario();
        }

        private void Update()
        {
            if (CurrentState == GameState.Running)
            {
                ElapsedTime += Time.deltaTime;
                UpdateStatistics();
                CheckEndConditions();
            }
        }

        /// <summary>
        /// Démarre le scénario d'urgence (REQ-0)
        /// </summary>
        public void StartScenario()
        {
            Debug.Log($"[GameManager] ═══════════════════════════════════════");
            Debug.Log($"[GameManager] Démarrage Session: {sessionId}");
            Debug.Log($"[GameManager] Scénario: {scenarioType}");
            Debug.Log($"[GameManager] Difficulté: {difficulty}");
            Debug.Log($"[GameManager] ═══════════════════════════════════════");
            
            CurrentState = GameState.Running;
            ElapsedTime = 0f;

            // Générer les victimes
            if (victimSpawner != null)
            {
                AllVictims = victimSpawner.SpawnVictims(numberOfVictims, difficulty);
                
                // Enregistrer les victimes dans le ScenarioManager
                foreach (var victim in AllVictims)
                {
                    scenarioManager?.RegisterVictim(victim);
                }
            }

            // Initialiser les systèmes (Blocs 2, 3, 4)
            triageSystem?.Initialize();
            hospitalSystem?.Initialize();
            ambulanceManager?.Initialize();
            arInterface?.Initialize();

            // Valider REQ-0
            RequirementsManager.Instance?.ValidateRequirement("REQ-0", $"Scénario {scenarioType} démarré avec {AllVictims.Count} victimes");

            OnGameStart?.Invoke();

            Debug.Log($"[GameManager] {AllVictims.Count} victimes générées - Système opérationnel");
        }

        /// <summary>
        /// Démarre le scénario avec un niveau de difficulté spécifique
        /// </summary>
        public void StartScenario(DifficultyLevel difficultyLevel)
        {
            difficulty = difficultyLevel;
            StartScenario();
        }

        /// <summary>
        /// Enregistre la détection d'une victime (REQ-1)
        /// </summary>
        public void RegisterVictimDetection(VictimController victim)
        {
            if (victim != null && !victim.IsDetected)
            {
                victim.IsDetected = true;
                victim.DetectionTime = Time.time;
                statistics.victimsDetected++;
                
                OnVictimDetected?.Invoke(victim);
                
                // Notifier le ScenarioManager pour déclencher la séquence de triage
                scenarioManager?.OnVictimDetected(victim);
                
                // Afficher les infos AR (Bloc 3)
                arInterface?.ShowVictimInfo(victim);
                
                // Valider REQ-1
                RequirementsManager.Instance?.ValidateRequirement("REQ-1", $"Victime {victim.PatientId} détectée");
                
                Debug.Log($"[GameManager] Victime détectée: {victim.PatientId}");
            }
        }

        /// <summary>
        /// Enregistre le triage d'une victime (REQ-3)
        /// </summary>
        public void RegisterVictimTriage(VictimController victim, StartCategory category)
        {
            if (victim != null && !TriagedVictims.Contains(victim))
            {
                victim.TriageCategory = category;
                victim.IsTriaged = true;
                victim.TriageTime = Time.time;
                TriagedVictims.Add(victim);
                
                // Mettre à jour les stats par catégorie
                UpdateTriageStats(category);
                
                // Calculer le temps de triage
                float triageDuration = victim.TriageTime - victim.DetectionTime;
                statistics.totalTriageTime += triageDuration;
                
                // Vérifier NFR-VIT (latence ≤ 30s)
                if (triageDuration <= 30f)
                {
                    RequirementsManager.Instance?.ValidateRequirement("NFR-VIT", $"Triage en {triageDuration:F1}s");
                }
                
                OnVictimTriaged?.Invoke(victim, category);
                
                Debug.Log($"[GameManager] Victime {victim.PatientId} triée: {category} (durée: {triageDuration:F1}s)");
            }
        }

        /// <summary>
        /// Alias pour RegisterVictimTriage (compatibilité)
        /// </summary>
        public void RegisterTriage(VictimController victim, StartCategory category)
        {
            RegisterVictimTriage(victim, category);
        }

        /// <summary>
        /// Enregistre l'évacuation d'une victime
        /// </summary>
        public void RegisterVictimEvacuation(VictimController victim)
        {
            if (victim != null && !EvacuatedVictims.Contains(victim))
            {
                victim.IsEvacuated = true;
                EvacuatedVictims.Add(victim);
                statistics.victimsEvacuated++;
                
                OnVictimEvacuated?.Invoke(victim);
                
                Debug.Log($"[GameManager] Victime {victim.PatientId} évacuée");
            }
        }

        /// <summary>
        /// Demande d'attribution d'une ambulance pour une victime (REQ-6)
        /// </summary>
        public AmbulanceController RequestAmbulance(VictimController victim, HospitalData hospital)
        {
            if (ambulanceManager != null)
            {
                var ambulance = ambulanceManager.AssignAmbulance(victim, hospital);
                
                if (ambulance != null)
                {
                    RequirementsManager.Instance?.ValidateRequirement("REQ-6", 
                        $"Ambulance {ambulance.AmbulanceId} assignée pour {victim.PatientId}");
                }
                
                return ambulance;
            }
            return null;
        }

        /// <summary>
        /// Met en pause le scénario
        /// </summary>
        public void PauseScenario()
        {
            if (CurrentState == GameState.Running)
            {
                CurrentState = GameState.Paused;
                Time.timeScale = 0f;
                Debug.Log("[GameManager] Scénario en pause");
            }
        }

        /// <summary>
        /// Alias pour PauseScenario (compatibilité)
        /// </summary>
        public void PauseGame()
        {
            PauseScenario();
        }

        /// <summary>
        /// Reprend le scénario
        /// </summary>
        public void ResumeScenario()
        {
            if (CurrentState == GameState.Paused)
            {
                CurrentState = GameState.Running;
                Time.timeScale = 1f;
                Debug.Log("[GameManager] Scénario repris");
            }
        }

        /// <summary>
        /// Alias pour ResumeScenario (compatibilité)
        /// </summary>
        public void ResumeGame()
        {
            ResumeScenario();
        }

        /// <summary>
        /// Termine le scénario
        /// </summary>
        public void EndScenario(bool success = true)
        {
            CurrentState = GameState.Ended;
            Time.timeScale = 1f;
            
            // Calculer le score final
            CalculateFinalScore();
            
            // Générer le rapport de conformité
            string complianceReport = RequirementsManager.Instance?.GenerateTextReport();
            Debug.Log(complianceReport);
            
            OnGameEnd?.Invoke();
            
            Debug.Log($"[GameManager] ═══════════════════════════════════════");
            Debug.Log($"[GameManager] Scénario terminé - {(success ? "SUCCÈS" : "ÉCHEC")}");
            Debug.Log($"[GameManager] Score final: {statistics.finalScore}");
            Debug.Log($"[GameManager] Précision: {CalculateTriageAccuracy() * 100:F1}%");
            Debug.Log($"[GameManager] ═══════════════════════════════════════");
        }

        /// <summary>
        /// Obtient les statistiques actuelles
        /// </summary>
        public GameStatistics GetStatistics()
        {
            // Mettre à jour les statistiques avant de les retourner
            statistics.elapsedTime = ElapsedTime;
            statistics.totalVictims = AllVictims.Count;
            statistics.triagedCount = TriagedVictims.Count;
            statistics.evacuatedCount = EvacuatedVictims.Count;
            
            return statistics;
        }

        private void InitializeStatistics()
        {
            statistics = new GameStatistics();
        }

        private void UpdateStatistics()
        {
            statistics.totalTime = ElapsedTime;
            statistics.averageTriageTime = TriagedVictims.Count > 0 
                ? statistics.totalTriageTime / TriagedVictims.Count 
                : 0f;
        }

        private void UpdateTriageStats(StartCategory category)
        {
            switch (category)
            {
                case StartCategory.Red:
                    statistics.redCount++;
                    break;
                case StartCategory.Yellow:
                    statistics.yellowCount++;
                    break;
                case StartCategory.Green:
                    statistics.greenCount++;
                    break;
                case StartCategory.Black:
                    statistics.blackCount++;
                    break;
            }
        }

        private void CheckEndConditions()
        {
            // Vérifier si toutes les victimes ont été traitées
            if (EvacuatedVictims.Count >= AllVictims.Count && AllVictims.Count > 0)
            {
                EndScenario(true);
            }
        }

        private void CalculateFinalScore()
        {
            // Score basé sur:
            // - Précision du triage (40%)
            // - Temps de réponse (25%)
            // - Victimes évacuées (20%)
            // - Survie (15%)
            
            float accuracyScore = CalculateTriageAccuracy() * 4000f;
            float timeScore = Mathf.Max(0, 2500f - ElapsedTime / 60f * 100f);
            float evacuationScore = AllVictims.Count > 0 
                ? (float)EvacuatedVictims.Count / AllVictims.Count * 2000f 
                : 0f;
            float survivalScore = CalculateSurvivalRate() * 1500f;
            
            statistics.finalScore = Mathf.RoundToInt(accuracyScore + timeScore + evacuationScore + survivalScore);
        }

        private float CalculateTriageAccuracy()
        {
            if (TriagedVictims.Count == 0) return 0f;
            
            int correctTriages = 0;
            foreach (var victim in TriagedVictims)
            {
                // Comparer avec la catégorie calculée automatiquement
                StartCategory calculatedCategory = triageSystem.CalculateStartCategory(victim.VitalSigns);
                if (victim.TriageCategory == calculatedCategory)
                {
                    correctTriages++;
                }
            }

            statistics.correctTriages = correctTriages;
            statistics.incorrectTriages = TriagedVictims.Count - correctTriages;
            
            return (float)correctTriages / TriagedVictims.Count;
        }

        private float CalculateSurvivalRate()
        {
            if (AllVictims.Count == 0) return 1f;
            
            int survivors = 0;
            foreach (var victim in AllVictims)
            {
                if (victim.TriageCategory != StartCategory.Black && victim.IsEvacuated)
                {
                    survivors++;
                }
            }
            
            int nonBlackVictims = AllVictims.Count - statistics.blackCount;
            return nonBlackVictims > 0 ? (float)survivors / nonBlackVictims : 1f;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // ENUMS ET STRUCTURES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// État du jeu
    /// </summary>
    public enum GameState
    {
        Initializing,
        Running,
        Paused,
        Ended
    }

    /// <summary>
    /// Types de scénarios (Use Cases du rapport)
    /// </summary>
    public enum ScenarioType
    {
        BuildingCollapse,       // Effondrement de bâtiment (cas principal)
        TrafficAccident,        // Accident de la route
        ChemicalIncident,       // Incident chimique
        MassEvent,              // Événement de masse
        NaturalDisaster,        // Catastrophe naturelle
        TerroristAttack,        // Attaque terroriste
        Training                // Entraînement
    }

    /// <summary>
    /// Niveaux de difficulté
    /// </summary>
    public enum DifficultyLevel
    {
        Tutorial,   // Tutoriel guidé
        Easy,       // 5 victimes, cas simples
        Normal,     // 10 victimes, cas variés
        Hard,       // 15 victimes, détérioration rapide
        Expert      // 20+ victimes, conditions difficiles
    }

    /// <summary>
    /// Structure pour les statistiques de jeu
    /// </summary>
    [Serializable]
    public class GameStatistics
    {
        [Header("Compteurs généraux")]
        public int totalVictims;
        public int victimsDetected;
        public int triagedCount;
        public int evacuatedCount;
        public int victimsEvacuated;

        [Header("Compteurs par catégorie START")]
        public int redCount;
        public int yellowCount;
        public int greenCount;
        public int blackCount;

        [Header("Temps")]
        public float elapsedTime;
        public float totalTime;
        public float totalTriageTime;
        public float averageTriageTime;

        [Header("Score")]
        public int finalScore;
        public int correctTriages;
        public int incorrectTriages;
    }
}
