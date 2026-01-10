using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace RASSE.Core
{
    /// <summary>
    /// GameManager - Gestionnaire principal du simulateur RA SSE
    /// Coordonne tous les systèmes : triage, hôpitaux, ambulances, interface AR
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("=== CONFIGURATION SCÉNARIO ===")]
        [Tooltip("Type de scénario d'urgence")]
        public ScenarioType scenarioType = ScenarioType.BuildingCollapse;
        
        [Tooltip("Nombre de victimes à générer")]
        [Range(1, 50)]
        public int numberOfVictims = 10;
        
        [Tooltip("Difficulté du scénario")]
        public DifficultyLevel difficulty = DifficultyLevel.Normal;

        [Header("=== RÉFÉRENCES SYSTÈMES ===")]
        public StartTriageSystem triageSystem;
        public HospitalCoordinationSystem hospitalSystem;
        public AmbulanceManager ambulanceManager;
        public ARInterfaceController arInterface;
        public VictimSpawner victimSpawner;
        public RescuerController rescuer;

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
        public List<VictimController> AllVictims { get; private set; } = new List<VictimController>();
        public List<VictimController> TriagedVictims { get; private set; } = new List<VictimController>();
        public List<VictimController> EvacuatedVictims { get; private set; } = new List<VictimController>();

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

            InitializeStatistics();
        }

        private void Start()
        {
            StartScenario();
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
        /// Démarre le scénario d'urgence
        /// </summary>
        public void StartScenario()
        {
            Debug.Log($"[GameManager] Démarrage scénario: {scenarioType}");
            
            CurrentState = GameState.Running;
            ElapsedTime = 0f;

            // Générer les victimes
            if (victimSpawner != null)
            {
                AllVictims = victimSpawner.SpawnVictims(numberOfVictims, difficulty);
            }

            // Initialiser les systèmes
            hospitalSystem?.Initialize();
            ambulanceManager?.Initialize();
            arInterface?.Initialize();

            OnGameStart?.Invoke();

            Debug.Log($"[GameManager] {AllVictims.Count} victimes générées");
        }

        /// <summary>
        /// Enregistre la détection d'une victime
        /// </summary>
        public void RegisterVictimDetection(VictimController victim)
        {
            if (victim != null && !victim.IsDetected)
            {
                victim.IsDetected = true;
                statistics.victimsDetected++;
                OnVictimDetected?.Invoke(victim);
                
                // Afficher les infos AR
                arInterface?.ShowVictimInfo(victim);
                
                Debug.Log($"[GameManager] Victime détectée: {victim.PatientId}");
            }
        }

        /// <summary>
        /// Enregistre le triage d'une victime
        /// </summary>
        public void RegisterVictimTriage(VictimController victim, StartCategory category)
        {
            if (victim != null && !TriagedVictims.Contains(victim))
            {
                victim.TriageCategory = category;
                victim.IsTriaged = true;
                TriagedVictims.Add(victim);
                
                // Mettre à jour les stats par catégorie
                UpdateTriageStats(category);
                
                OnVictimTriaged?.Invoke(victim, category);
                
                Debug.Log($"[GameManager] Victime {victim.PatientId} triée: {category}");
            }
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
        /// Demande d'attribution d'une ambulance pour une victime
        /// </summary>
        public AmbulanceController RequestAmbulance(VictimController victim, HospitalData hospital)
        {
            if (ambulanceManager != null)
            {
                return ambulanceManager.AssignAmbulance(victim, hospital);
            }
            return null;
        }

        /// <summary>
        /// Termine le scénario
        /// </summary>
        public void EndScenario()
        {
            CurrentState = GameState.Ended;
            
            // Calculer le score final
            CalculateFinalScore();
            
            OnGameEnd?.Invoke();
            
            Debug.Log("[GameManager] Scénario terminé");
            Debug.Log($"Score final: {statistics.finalScore}");
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
                EndScenario();
            }
        }

        private void CalculateFinalScore()
        {
            // Score basé sur:
            // - Précision du triage (comparaison avec classification réelle)
            // - Temps de réponse
            // - Victimes évacuées
            
            float accuracyScore = CalculateTriageAccuracy() * 50f;
            float timeScore = Mathf.Max(0, 100f - ElapsedTime / 60f) * 0.3f;
            float evacuationScore = (float)EvacuatedVictims.Count / AllVictims.Count * 20f;
            
            statistics.finalScore = Mathf.RoundToInt(accuracyScore + timeScore + evacuationScore);
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
            
            return (float)correctTriages / TriagedVictims.Count;
        }

        /// <summary>
        /// Met en pause le jeu
        /// </summary>
        public void PauseGame()
        {
            if (CurrentState == GameState.Running)
            {
                CurrentState = GameState.Paused;
                Time.timeScale = 0f;
            }
        }

        /// <summary>
        /// Reprend le jeu
        /// </summary>
        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                CurrentState = GameState.Running;
                Time.timeScale = 1f;
            }
        }
    }

    // Enums
    public enum GameState
    {
        Initializing,
        Running,
        Paused,
        Ended
    }

    public enum ScenarioType
    {
        BuildingCollapse,       // Effondrement de bâtiment
        TrafficAccident,        // Accident de la route
        ChemicalIncident,       // Incident chimique
        MassEvent,              // Événement de masse
        NaturalDisaster,        // Catastrophe naturelle
        TerroristAttack         // Attaque terroriste
    }

    public enum DifficultyLevel
    {
        Easy,       // Plus de temps, moins de victimes critiques
        Normal,     // Équilibré
        Hard,       // Moins de temps, plus de cas complexes
        Expert      // Réaliste, conditions dégradées
    }

    // Structure pour les statistiques
    [System.Serializable]
    public class GameStatistics
    {
        public int victimsDetected;
        public int victimsEvacuated;
        public int redCount;
        public int yellowCount;
        public int greenCount;
        public int blackCount;
        public float totalTime;
        public float totalTriageTime;
        public float averageTriageTime;
        public int finalScore;
        public int correctTriages;
        public int incorrectTriages;
    }
}
