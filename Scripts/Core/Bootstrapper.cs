using UnityEngine;
using UnityEngine.SceneManagement;

namespace RASSE.Core
{
    /// <summary>
    /// Initialise automatiquement tous les systèmes du simulateur au démarrage.
    /// Doit être placé dans la première scène chargée (MainMenu ou Bootstrapper).
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool initializeOnAwake = true;
        [SerializeField] private bool loadMainMenuAfterInit = true;
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool skipInitInEditor = false;
        
        private static bool isInitialized = false;
        
        public static bool IsInitialized => isInitialized;
        
        private void Awake()
        {
            if (isInitialized)
            {
                Log("Déjà initialisé, destruction du duplicat");
                Destroy(gameObject);
                return;
            }
            
            #if UNITY_EDITOR
            if (skipInitInEditor)
            {
                Log("Skip d'initialisation en mode Editor");
                isInitialized = true;
                return;
            }
            #endif
            
            if (initializeOnAwake)
            {
                Initialize();
            }
        }
        
        /// <summary>
        /// Initialise tous les systèmes du simulateur
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
            {
                Log("Tentative de ré-initialisation ignorée");
                return;
            }
            
            Log("=== DÉMARRAGE INITIALISATION RA-SSE ===");
            
            DontDestroyOnLoad(gameObject);
            
            // 1. Initialiser les singletons essentiels
            InitializeSingletons();
            
            // 2. Charger la configuration système
            LoadSystemConfiguration();
            
            // 3. Initialiser les systèmes
            InitializeSystems();
            
            // 4. Vérifier les dépendances
            VerifyDependencies();
            
            isInitialized = true;
            Log("=== INITIALISATION TERMINÉE ===");
            
            // Charger le menu principal si configuré
            if (loadMainMenuAfterInit && SceneManager.GetActiveScene().name != mainMenuSceneName)
            {
                LoadMainMenu();
            }
        }
        
        private void InitializeSingletons()
        {
            Log("Initialisation des singletons...");
            
            // Forcer la création des singletons essentiels
            var gameManager = GameManager.Instance;
            var eventManager = EventManager.Instance;
            var sceneLoader = SceneLoader.Instance;
            
            Log($"  - GameManager: {(gameManager != null ? "OK" : "ÉCHEC")}");
            Log($"  - EventManager: {(eventManager != null ? "OK" : "ÉCHEC")}");
            Log($"  - SceneLoader: {(sceneLoader != null ? "OK" : "ÉCHEC")}");
        }
        
        private void LoadSystemConfiguration()
        {
            Log("Chargement de la configuration...");
            
            // Charger les settings depuis Resources
            var settings = Resources.Load<RASSE.Data.SystemSettingsSO>("Settings/SystemSettings_Default");
            
            if (settings != null)
            {
                Log($"  - Configuration chargée: {settings.name}");
                ApplySettings(settings);
            }
            else
            {
                Log("  - Configuration par défaut utilisée");
                ApplyDefaultSettings();
            }
        }
        
        private void ApplySettings(RASSE.Data.SystemSettingsSO settings)
        {
            // Appliquer les paramètres de qualité
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            
            // Configurer l'audio
            AudioListener.volume = 1f;
            
            Log("  - Paramètres appliqués");
        }
        
        private void ApplyDefaultSettings()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
        
        private void InitializeSystems()
        {
            Log("Initialisation des systèmes...");
            
            // Configurer les layers de physique
            ConfigurePhysicsLayers();
            
            // Initialiser le système de localisation si disponible
            InitializeLocalization();
            
            Log("  - Systèmes initialisés");
        }
        
        private void ConfigurePhysicsLayers()
        {
            // Configurer les collisions entre layers
            // Layer 8: Victim, Layer 9: Rescuer, Layer 12: DangerZone, etc.
            
            // Les victimes ne collisionnent pas entre elles
            Physics.IgnoreLayerCollision(8, 8, true);
            
            // Les zones de triage sont des triggers uniquement
            Physics.IgnoreLayerCollision(11, 11, true);
        }
        
        private void InitializeLocalization()
        {
            // Définir la langue par défaut (français)
            // Si UnityEngine.Localization est disponible, l'initialiser ici
            
            Log("  - Localisation: Français (fr-FR)");
        }
        
        private void VerifyDependencies()
        {
            Log("Vérification des dépendances...");
            
            bool allOk = true;
            
            // Vérifier les composants critiques
            if (GameManager.Instance == null)
            {
                LogError("GameManager manquant!");
                allOk = false;
            }
            
            // Vérifier les Resources essentielles
            var triageProtocol = Resources.Load("TriageProtocols/TriageProtocol_START");
            if (triageProtocol == null)
            {
                LogWarning("Protocole START non trouvé dans Resources");
            }
            
            if (allOk)
            {
                Log("  - Toutes les dépendances OK");
            }
        }
        
        private void LoadMainMenu()
        {
            Log($"Chargement de {mainMenuSceneName}...");
            
            if (SceneLoader.HasInstance)
            {
                SceneLoader.Instance.LoadScene(mainMenuSceneName);
            }
            else
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }
        
        #region Logging
        
        private void Log(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[Bootstrapper] {message}");
            }
        }
        
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[Bootstrapper] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[Bootstrapper] {message}");
        }
        
        #endregion
        
        #region Runtime API
        
        /// <summary>
        /// Réinitialise le simulateur (pour debug)
        /// </summary>
        [ContextMenu("Réinitialiser")]
        public void Reinitialize()
        {
            isInitialized = false;
            Initialize();
        }
        
        /// <summary>
        /// Affiche l'état du système
        /// </summary>
        [ContextMenu("Afficher État")]
        public void ShowStatus()
        {
            Debug.Log("=== ÉTAT DU SYSTÈME ===");
            Debug.Log($"Initialisé: {isInitialized}");
            Debug.Log($"GameManager: {(GameManager.HasInstance ? "OK" : "Non")}");
            Debug.Log($"EventManager: {(EventManager.HasInstance ? "OK" : "Non")}");
            Debug.Log($"SceneLoader: {(SceneLoader.HasInstance ? "OK" : "Non")}");
            Debug.Log($"Scène active: {SceneManager.GetActiveScene().name}");
            Debug.Log($"FPS cible: {Application.targetFrameRate}");
        }
        
        #endregion
    }
}
