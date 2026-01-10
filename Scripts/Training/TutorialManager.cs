using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace RASSE.Training
{
    /// <summary>
    /// TutorialManager - Gestion du tutoriel interactif pour apprendre le protocole START
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [Header("=== CONFIGURATION ===")]
        [SerializeField] private List<TutorialStep> tutorialSteps = new List<TutorialStep>();
        [SerializeField] private bool autoAdvance = false;
        [SerializeField] private float autoAdvanceDelay = 5f;
        [SerializeField] private bool skipEnabled = true;
        [SerializeField] private KeyCode skipKey = KeyCode.Escape;
        [SerializeField] private KeyCode nextKey = KeyCode.Space;

        [Header("=== UI REFERENCES ===")]
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private TextMeshProUGUI stepIndicatorText;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private Image highlightOverlay;
        [SerializeField] private RectTransform highlightFrame;
        [SerializeField] private GameObject arrowPointer;

        [Header("=== AUDIO ===")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip stepChangeSound;
        [SerializeField] private AudioClip completionSound;
        [SerializeField] private AudioClip errorSound;

        [Header("=== ÉVÉNEMENTS ===")]
        public UnityEvent OnTutorialStarted;
        public UnityEvent OnTutorialCompleted;
        public UnityEvent<int> OnStepChanged;

        // État interne
        private int currentStepIndex = -1;
        private bool isActive = false;
        private bool waitingForAction = false;
        private Coroutine autoAdvanceCoroutine;
        private Dictionary<string, bool> completedActions = new Dictionary<string, bool>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            InitializeDefaultSteps();
            SetupUI();
        }

        private void Update()
        {
            if (!isActive) return;

            // Raccourcis clavier
            if (skipEnabled && Input.GetKeyDown(skipKey))
            {
                SkipTutorial();
            }

            if (Input.GetKeyDown(nextKey) && !waitingForAction)
            {
                NextStep();
            }
        }

        /// <summary>
        /// Initialise les étapes par défaut du tutoriel START
        /// </summary>
        private void InitializeDefaultSteps()
        {
            if (tutorialSteps.Count > 0) return;

            // Introduction
            tutorialSteps.Add(new TutorialStep
            {
                stepId = "intro",
                title = "Bienvenue dans le Simulateur RA-SSE",
                content = "Ce tutoriel vous guidera dans l'utilisation du système de Réalité Augmentée " +
                         "pour les Situations Sanitaires Exceptionnelles.\n\n" +
                         "Vous apprendrez à utiliser le protocole START pour trier rapidement les victimes.",
                requiresAction = false,
                highlightTarget = null
            });

            // Protocole START - Introduction
            tutorialSteps.Add(new TutorialStep
            {
                stepId = "start_intro",
                title = "Le Protocole START",
                content = "START signifie 'Simple Triage And Rapid Treatment'.\n\n" +
                         "Ce protocole permet de classer rapidement les victimes en 4 catégories:\n" +
                         "• ROUGE - Urgence immédiate\n" +
                         "• JAUNE - Urgence différée\n" +
                         "• VERT - Blessé léger\n" +
                         "• NOIR - Décédé",
                requiresAction = false
            });

            // Étape 1: Respiration
            tutorialSteps.Add(new TutorialStep
            {
                stepId = "start_breathing",
                title = "Étape 1: Vérifier la Respiration",
                content = "La première question est: La victime respire-t-elle?\n\n" +
                         "• Si NON → Effectuer une manœuvre d'ouverture des voies aériennes\n" +
                         "  - Si toujours pas de respiration → NOIR (décédé)\n" +
                         "  - Si respiration reprend → ROUGE (critique)\n\n" +
                         "• Si OUI → Passer à l'étape suivante",
                requiresAction = false
            });

            // Étape 2: Fréquence respiratoire
            tutorialSteps.Add(new TutorialStep
            {
                stepId = "start_resp_rate",
                title = "Étape 2: Fréquence Respiratoire",
                content = "Comptez le nombre de respirations par minute.\n\n" +
                         "• Si FR > 30/min ou FR < 10/min → ROUGE\n" +
                         "• Si FR normale (10-30/min) → Passer à l'étape suivante\n\n" +
                         "Une respiration anormale indique une détresse respiratoire.",
                requiresAction = false
            });

            // Étape 3: Circulation
            tutorialSteps.Add(new TutorialStep
            {
                stepId = "start_circulation",
                title = "Étape 3: Vérifier la Circulation",
                content = "Évaluez la circulation sanguine:\n\n" +
                         "• Temps de Remplissage Capillaire (TRC) > 2 secondes → ROUGE\n" +
                         "• Ou: Absence de pouls radial → ROUGE\n\n" +
                         "Le TRC se mesure en appuyant sur l'ongle et observant " +
                         "le retour de la coloration.",
                requiresAction = false
            });

            // Étape 4: État neurologique
            tutorialSteps.Add(new TutorialStep
            {
                stepId = "start_neuro",
                title = "Étape 4: État Neurologique",
                content = "Évaluez si la victime peut suivre des ordres simples:\n\n" +
                         "• Si ne peut PAS suivre des ordres simples → ROUGE\n" +
                         "• Si peut suivre des ordres → Passer à l'étape suivante\n\n" +
                         "Exemple d'ordre simple: 'Serrez ma main' ou 'Ouvrez les yeux'",
                requiresAction = false
            });

            // Étape 5: Mobilité
            tutorialSteps.Add(new TutorialStep
            {
                stepId = "start_mobility",
                title = "Étape 5: Capacité à Marcher",
                content = "Dernière étape du triage:\n\n" +
                         "• Si la victime PEUT marcher → VERT (blessé léger)\n" +
                         "• Si la victime ne peut PAS marcher → JAUNE (urgence différée)\n\n" +
                         "Les victimes vertes peuvent être dirigées vers une zone d'attente.",
                requiresAction = false
            });

            // Commandes vocales
            tutorialSteps.Add(new TutorialStep
            {
                stepId = "voice_commands",
                title = "Commandes Vocales",
                content = "Les lunettes AR reconnaissent des commandes vocales:\n\n" +
                         "• 'Valider' - Confirme le triage suggéré\n" +
                         "• 'Rouge/Jaune/Vert/Noir' - Force une catégorie\n" +
                         "• 'Suivant' - Passe à la victime suivante\n" +
                         "• 'Scanner' - Recherche les victimes\n" +
                         "• 'Aide' - Affiche l'aide\n\n" +
                         "En simulation, utilisez les touches clavier affichées.",
                requiresAction = false
            });

            // Navigation
            tutorialSteps.Add(new TutorialStep
            {
                stepId = "navigation",
                title = "Navigation AR",
                content = "L'interface AR vous guide vers:\n\n" +
                         "• Les victimes détectées\n" +
                         "• L'ambulance assignée\n" +
                         "• L'hôpital sélectionné\n\n" +
                         "Suivez les indicateurs de direction et la ligne de guidage " +
                         "affichés dans votre champ de vision.",
                requiresAction = false
            });

            // Pratique
            tutorialSteps.Add(new TutorialStep
            {
                stepId = "practice_intro",
                title = "Mise en Pratique",
                content = "Vous allez maintenant pratiquer sur une victime simulée.\n\n" +
                         "1. Approchez-vous de la victime (marqueur orange)\n" +
                         "2. Appuyez sur 'E' pour interagir\n" +
                         "3. Analysez les signes vitaux affichés\n" +
                         "4. Validez ou corrigez le triage suggéré",
                requiresAction = true,
                requiredAction = "interact_victim"
            });

            // Conclusion
            tutorialSteps.Add(new TutorialStep
            {
                stepId = "conclusion",
                title = "Tutoriel Terminé!",
                content = "Félicitations! Vous maîtrisez maintenant les bases du protocole START.\n\n" +
                         "Conseils pour le terrain:\n" +
                         "• Restez calme et méthodique\n" +
                         "• Le triage doit être rapide (< 60 secondes/victime)\n" +
                         "• En cas de doute, sur-triez (catégorie plus grave)\n\n" +
                         "Bonne chance!",
                requiresAction = false
            });
        }

        private void SetupUI()
        {
            if (nextButton != null)
                nextButton.onClick.AddListener(NextStep);
            
            if (previousButton != null)
                previousButton.onClick.AddListener(PreviousStep);
            
            if (skipButton != null)
                skipButton.onClick.AddListener(SkipTutorial);

            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);
        }

        /// <summary>
        /// Démarre le tutoriel
        /// </summary>
        public void StartTutorial()
        {
            if (tutorialSteps.Count == 0)
            {
                Debug.LogWarning("Aucune étape de tutoriel définie!");
                return;
            }

            isActive = true;
            currentStepIndex = -1;
            completedActions.Clear();

            if (tutorialPanel != null)
                tutorialPanel.SetActive(true);

            OnTutorialStarted?.Invoke();
            NextStep();
        }

        /// <summary>
        /// Passe à l'étape suivante
        /// </summary>
        public void NextStep()
        {
            if (waitingForAction) return;

            currentStepIndex++;

            if (currentStepIndex >= tutorialSteps.Count)
            {
                CompleteTutorial();
                return;
            }

            DisplayStep(tutorialSteps[currentStepIndex]);
            OnStepChanged?.Invoke(currentStepIndex);

            if (audioSource != null && stepChangeSound != null)
                audioSource.PlayOneShot(stepChangeSound);
        }

        /// <summary>
        /// Retourne à l'étape précédente
        /// </summary>
        public void PreviousStep()
        {
            if (currentStepIndex <= 0) return;

            currentStepIndex--;
            waitingForAction = false;
            
            if (autoAdvanceCoroutine != null)
                StopCoroutine(autoAdvanceCoroutine);

            DisplayStep(tutorialSteps[currentStepIndex]);
            OnStepChanged?.Invoke(currentStepIndex);
        }

        /// <summary>
        /// Affiche une étape du tutoriel
        /// </summary>
        private void DisplayStep(TutorialStep step)
        {
            if (titleText != null)
                titleText.text = step.title;

            if (contentText != null)
                contentText.text = step.content;

            if (stepIndicatorText != null)
                stepIndicatorText.text = $"Étape {currentStepIndex + 1} / {tutorialSteps.Count}";

            // Gestion des boutons
            if (previousButton != null)
                previousButton.interactable = currentStepIndex > 0;

            if (nextButton != null)
            {
                nextButton.interactable = !step.requiresAction;
                var buttonText = nextButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = currentStepIndex >= tutorialSteps.Count - 1 ? "Terminer" : "Suivant";
                }
            }

            // Highlight
            if (step.highlightTarget != null)
            {
                ShowHighlight(step.highlightTarget, step.highlightOffset);
            }
            else
            {
                HideHighlight();
            }

            // Arrow pointer
            if (arrowPointer != null)
            {
                arrowPointer.SetActive(step.showArrow && step.arrowTarget != null);
                if (step.arrowTarget != null)
                {
                    arrowPointer.transform.position = step.arrowTarget.position + step.arrowOffset;
                    arrowPointer.transform.LookAt(step.arrowTarget);
                }
            }

            // Attente d'action
            waitingForAction = step.requiresAction;

            // Auto-advance
            if (autoAdvance && !step.requiresAction)
            {
                if (autoAdvanceCoroutine != null)
                    StopCoroutine(autoAdvanceCoroutine);
                autoAdvanceCoroutine = StartCoroutine(AutoAdvanceCoroutine());
            }

            // Audio narration
            if (step.narrationClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(step.narrationClip);
            }
        }

        private IEnumerator AutoAdvanceCoroutine()
        {
            yield return new WaitForSeconds(autoAdvanceDelay);
            NextStep();
        }

        /// <summary>
        /// Signale qu'une action requise a été effectuée
        /// </summary>
        public void CompleteAction(string actionId)
        {
            if (!waitingForAction) return;

            var currentStep = tutorialSteps[currentStepIndex];
            if (currentStep.requiredAction == actionId)
            {
                completedActions[actionId] = true;
                waitingForAction = false;

                if (nextButton != null)
                    nextButton.interactable = true;

                // Feedback positif
                if (audioSource != null && completionSound != null)
                    audioSource.PlayOneShot(completionSound);

                // Auto-advance après action complétée
                if (autoAdvance)
                {
                    StartCoroutine(DelayedNextStep(1f));
                }
            }
        }

        private IEnumerator DelayedNextStep(float delay)
        {
            yield return new WaitForSeconds(delay);
            NextStep();
        }

        /// <summary>
        /// Affiche le highlight sur un élément
        /// </summary>
        private void ShowHighlight(RectTransform target, Vector2 offset)
        {
            if (highlightOverlay != null)
                highlightOverlay.gameObject.SetActive(true);

            if (highlightFrame != null && target != null)
            {
                highlightFrame.gameObject.SetActive(true);
                highlightFrame.position = target.position + (Vector3)offset;
                highlightFrame.sizeDelta = target.sizeDelta + new Vector2(20, 20);
            }
        }

        private void HideHighlight()
        {
            if (highlightOverlay != null)
                highlightOverlay.gameObject.SetActive(false);
            
            if (highlightFrame != null)
                highlightFrame.gameObject.SetActive(false);
        }

        /// <summary>
        /// Saute le tutoriel
        /// </summary>
        public void SkipTutorial()
        {
            CompleteTutorial();
        }

        /// <summary>
        /// Termine le tutoriel
        /// </summary>
        private void CompleteTutorial()
        {
            isActive = false;
            waitingForAction = false;

            if (autoAdvanceCoroutine != null)
                StopCoroutine(autoAdvanceCoroutine);

            HideHighlight();

            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);

            if (arrowPointer != null)
                arrowPointer.SetActive(false);

            if (audioSource != null && completionSound != null)
                audioSource.PlayOneShot(completionSound);

            // Sauvegarder que le tutoriel a été complété
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();

            OnTutorialCompleted?.Invoke();
        }

        /// <summary>
        /// Vérifie si le tutoriel a déjà été complété
        /// </summary>
        public bool HasCompletedTutorial()
        {
            return PlayerPrefs.GetInt("TutorialCompleted", 0) == 1;
        }

        /// <summary>
        /// Réinitialise la progression du tutoriel
        /// </summary>
        public void ResetTutorialProgress()
        {
            PlayerPrefs.DeleteKey("TutorialCompleted");
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Aller à une étape spécifique
        /// </summary>
        public void GoToStep(int stepIndex)
        {
            if (stepIndex < 0 || stepIndex >= tutorialSteps.Count) return;

            currentStepIndex = stepIndex - 1;
            NextStep();
        }

        /// <summary>
        /// Aller à une étape par son ID
        /// </summary>
        public void GoToStep(string stepId)
        {
            int index = tutorialSteps.FindIndex(s => s.stepId == stepId);
            if (index >= 0)
            {
                GoToStep(index);
            }
        }

        public bool IsActive => isActive;
        public int CurrentStepIndex => currentStepIndex;
        public int TotalSteps => tutorialSteps.Count;
    }

    /// <summary>
    /// Définition d'une étape du tutoriel
    /// </summary>
    [System.Serializable]
    public class TutorialStep
    {
        public string stepId;
        public string title;
        [TextArea(3, 10)]
        public string content;
        
        [Header("Actions")]
        public bool requiresAction = false;
        public string requiredAction;
        
        [Header("Highlight")]
        public RectTransform highlightTarget;
        public Vector2 highlightOffset;
        
        [Header("Pointeur")]
        public bool showArrow = false;
        public Transform arrowTarget;
        public Vector3 arrowOffset;
        
        [Header("Audio")]
        public AudioClip narrationClip;
    }

    /// <summary>
    /// Composant pour déclencher des actions de tutoriel
    /// </summary>
    public class TutorialTrigger : MonoBehaviour
    {
        [SerializeField] private string actionId;
        [SerializeField] private TriggerType triggerType = TriggerType.OnInteract;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        private bool triggered = false;

        public enum TriggerType
        {
            OnTriggerEnter,
            OnInteract,
            OnClick,
            Automatic
        }

        private void Start()
        {
            if (triggerType == TriggerType.Automatic)
            {
                TriggerAction();
            }
        }

        private void Update()
        {
            if (triggered) return;

            if (triggerType == TriggerType.OnInteract && Input.GetKeyDown(interactKey))
            {
                // Vérifier si le joueur est assez proche
                var player = GameObject.FindGameObjectWithTag("Rescuer");
                if (player != null && Vector3.Distance(player.transform.position, transform.position) < 3f)
                {
                    TriggerAction();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (triggered) return;

            if (triggerType == TriggerType.OnTriggerEnter && other.CompareTag("Rescuer"))
            {
                TriggerAction();
            }
        }

        private void OnMouseDown()
        {
            if (triggered) return;

            if (triggerType == TriggerType.OnClick)
            {
                TriggerAction();
            }
        }

        private void TriggerAction()
        {
            triggered = true;
            
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.CompleteAction(actionId);
            }
        }

        public void ResetTrigger()
        {
            triggered = false;
        }
    }
}
