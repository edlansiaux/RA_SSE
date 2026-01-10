using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

namespace RASSE.Core
{
    /// <summary>
    /// VoiceCommandSimulator - Simule les commandes vocales des lunettes AR
    /// En mode développement, les commandes sont déclenchées par clavier
    /// </summary>
    public class VoiceCommandSimulator : MonoBehaviour
    {
        [Header("=== CONFIGURATION ===")]
        [SerializeField] private bool useKeyboardSimulation = true;
        [SerializeField] private bool showDebugUI = true;

        [Header("=== RÉFÉRENCES ===")]
        [SerializeField] private ARInterfaceController arInterface;
        [SerializeField] private StartTriageSystem triageSystem;

        [Header("=== COMMANDES DISPONIBLES ===")]
        [SerializeField] private List<VoiceCommand> commands = new List<VoiceCommand>();

        [Header("=== ÉVÉNEMENTS ===")]
        public UnityEvent<string> OnCommandRecognized;
        public UnityEvent<string> OnCommandFailed;

        // État
        private bool isListening = false;
        private float lastCommandTime;
        private const float COMMAND_COOLDOWN = 1f;

        private void Awake()
        {
            InitializeDefaultCommands();
        }

        private void Update()
        {
            if (useKeyboardSimulation)
            {
                ProcessKeyboardInput();
            }
        }

        /// <summary>
        /// Initialise les commandes vocales par défaut
        /// </summary>
        private void InitializeDefaultCommands()
        {
            commands.Clear();

            // Commandes de triage
            commands.Add(new VoiceCommand
            {
                commandId = "validate",
                phrases = new[] { "valider", "confirmer", "ok", "d'accord" },
                keyCode = KeyCode.Return,
                action = () => ExecuteValidate(),
                description = "Valide la classification suggérée"
            });

            commands.Add(new VoiceCommand
            {
                commandId = "red",
                phrases = new[] { "rouge", "immédiat", "urgence absolue" },
                keyCode = KeyCode.Alpha1,
                action = () => ExecuteTriageOverride(StartCategory.Red),
                description = "Classifie en ROUGE (immédiat)"
            });

            commands.Add(new VoiceCommand
            {
                commandId = "yellow",
                phrases = new[] { "jaune", "différé", "urgence relative" },
                keyCode = KeyCode.Alpha2,
                action = () => ExecuteTriageOverride(StartCategory.Yellow),
                description = "Classifie en JAUNE (différé)"
            });

            commands.Add(new VoiceCommand
            {
                commandId = "green",
                phrases = new[] { "vert", "mineur", "impliqué" },
                keyCode = KeyCode.Alpha3,
                action = () => ExecuteTriageOverride(StartCategory.Green),
                description = "Classifie en VERT (mineur)"
            });

            commands.Add(new VoiceCommand
            {
                commandId = "black",
                phrases = new[] { "noir", "décédé", "décès" },
                keyCode = KeyCode.Alpha4,
                action = () => ExecuteTriageOverride(StartCategory.Black),
                description = "Classifie en NOIR (décédé)"
            });

            // Commandes d'action
            commands.Add(new VoiceCommand
            {
                commandId = "hospital",
                phrases = new[] { "confirmer hôpital", "envoyer hôpital", "évacuer" },
                keyCode = KeyCode.H,
                action = () => ExecuteHospitalConfirm(),
                description = "Confirme l'envoi vers l'hôpital"
            });

            commands.Add(new VoiceCommand
            {
                commandId = "home",
                phrases = new[] { "retour domicile", "maison", "libérer" },
                keyCode = KeyCode.L,
                action = () => ExecuteReleaseHome(),
                description = "Libère le patient (retour domicile)"
            });

            commands.Add(new VoiceCommand
            {
                commandId = "ambulance",
                phrases = new[] { "ambulance", "chercher ambulance", "véhicule" },
                keyCode = KeyCode.A,
                action = () => ExecuteAmbulanceRequest(),
                description = "Demande une ambulance"
            });

            // Commandes de navigation
            commands.Add(new VoiceCommand
            {
                commandId = "next",
                phrases = new[] { "suivant", "prochaine victime", "continuer" },
                keyCode = KeyCode.N,
                action = () => ExecuteNextVictim(),
                description = "Passe à la victime suivante"
            });

            commands.Add(new VoiceCommand
            {
                commandId = "scan",
                phrases = new[] { "scanner", "analyser", "détecter" },
                keyCode = KeyCode.S,
                action = () => ExecuteScan(),
                description = "Active le scan de détection"
            });

            // Commandes système
            commands.Add(new VoiceCommand
            {
                commandId = "help",
                phrases = new[] { "aide", "help", "assistance" },
                keyCode = KeyCode.F1,
                action = () => ExecuteHelp(),
                description = "Affiche l'aide"
            });

            commands.Add(new VoiceCommand
            {
                commandId = "status",
                phrases = new[] { "statut", "résumé", "situation" },
                keyCode = KeyCode.Tab,
                action = () => ExecuteStatus(),
                description = "Affiche le statut général"
            });

            commands.Add(new VoiceCommand
            {
                commandId = "cancel",
                phrases = new[] { "annuler", "retour", "fermer" },
                keyCode = KeyCode.Escape,
                action = () => ExecuteCancel(),
                description = "Annule l'action en cours"
            });
        }

        /// <summary>
        /// Traite les entrées clavier pour la simulation
        /// </summary>
        private void ProcessKeyboardInput()
        {
            if (Time.time - lastCommandTime < COMMAND_COOLDOWN) return;

            foreach (var command in commands)
            {
                if (Input.GetKeyDown(command.keyCode))
                {
                    ExecuteCommand(command);
                    break;
                }
            }
        }

        /// <summary>
        /// Simule la reconnaissance d'une phrase vocale
        /// </summary>
        public void ProcessVoiceInput(string spokenPhrase)
        {
            if (string.IsNullOrEmpty(spokenPhrase)) return;

            spokenPhrase = spokenPhrase.ToLower().Trim();

            foreach (var command in commands)
            {
                if (command.phrases.Any(p => spokenPhrase.Contains(p.ToLower())))
                {
                    ExecuteCommand(command);
                    return;
                }
            }

            OnCommandFailed?.Invoke(spokenPhrase);
            Debug.LogWarning($"[VoiceCommand] Commande non reconnue: {spokenPhrase}");
        }

        /// <summary>
        /// Exécute une commande
        /// </summary>
        private void ExecuteCommand(VoiceCommand command)
        {
            lastCommandTime = Time.time;

            Debug.Log($"[VoiceCommand] Exécution: {command.commandId}");
            
            command.action?.Invoke();
            OnCommandRecognized?.Invoke(command.commandId);

            // Feedback visuel/audio
            arInterface?.ShowAlert($"Commande: {command.phrases[0]}", Color.cyan, 1f);
        }

        // === IMPLÉMENTATION DES COMMANDES ===

        private void ExecuteValidate()
        {
            arInterface?.ConfirmSuggestedTriage();
        }

        private void ExecuteTriageOverride(StartCategory category)
        {
            int categoryIndex = (int)category - 1;
            arInterface?.OverrideTriage(categoryIndex);
        }

        private void ExecuteHospitalConfirm()
        {
            // Confirmer l'envoi vers l'hôpital recommandé
            Debug.Log("[VoiceCommand] Confirmation envoi hôpital");
            // TODO: Implémenter la logique
        }

        private void ExecuteReleaseHome()
        {
            Debug.Log("[VoiceCommand] Libération du patient");
            arInterface?.HideVictimInfo();
        }

        private void ExecuteAmbulanceRequest()
        {
            Debug.Log("[VoiceCommand] Demande d'ambulance");
            // TODO: Implémenter la logique d'attribution d'ambulance
        }

        private void ExecuteNextVictim()
        {
            Debug.Log("[VoiceCommand] Passage à la victime suivante");
            arInterface?.HideVictimInfo();
        }

        private void ExecuteScan()
        {
            Debug.Log("[VoiceCommand] Activation du scan");
            // TODO: Déclencher un scan de la zone
        }

        private void ExecuteHelp()
        {
            string helpText = "Commandes disponibles:\n";
            foreach (var cmd in commands)
            {
                helpText += $"• {cmd.phrases[0]} ({cmd.keyCode})\n";
            }
            arInterface?.ShowAlert(helpText, Color.white, 10f);
        }

        private void ExecuteStatus()
        {
            if (GameManager.Instance != null)
            {
                var stats = GameManager.Instance.statistics;
                string status = $"Situation actuelle:\n" +
                               $"Victimes détectées: {stats.victimsDetected}\n" +
                               $"Rouge: {stats.redCount} | Jaune: {stats.yellowCount}\n" +
                               $"Vert: {stats.greenCount} | Noir: {stats.blackCount}\n" +
                               $"Évacuées: {stats.victimsEvacuated}";
                arInterface?.ShowAlert(status, Color.cyan, 8f);
            }
        }

        private void ExecuteCancel()
        {
            arInterface?.HideVictimInfo();
            arInterface?.HideNavigation();
        }

        /// <summary>
        /// Active/désactive l'écoute vocale
        /// </summary>
        public void SetListening(bool listening)
        {
            isListening = listening;
            Debug.Log($"[VoiceCommand] Écoute: {(listening ? "activée" : "désactivée")}");
        }

        private void OnGUI()
        {
            if (!showDebugUI) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Label("<b>Commandes Vocales (Simulation)</b>", new GUIStyle(GUI.skin.label) { richText = true });
            
            foreach (var cmd in commands)
            {
                GUILayout.Label($"[{cmd.keyCode}] {cmd.phrases[0]} - {cmd.description}");
            }

            GUILayout.EndArea();
        }
    }

    /// <summary>
    /// Structure d'une commande vocale
    /// </summary>
    [System.Serializable]
    public class VoiceCommand
    {
        public string commandId;
        public string[] phrases;
        public KeyCode keyCode;
        public System.Action action;
        public string description;
    }
}
