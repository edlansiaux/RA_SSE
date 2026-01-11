using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace RASSE.Core
{
    /// <summary>
    /// Gestionnaire de chargement de scènes avec écran de chargement.
    /// Gère les transitions entre les scènes du simulateur RA-SSE.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private float minimumLoadTime = 1f;
        [SerializeField] private bool useAsyncLoading = true;
        
        [Header("UI References")]
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private UnityEngine.UI.Slider progressBar;
        [SerializeField] private TMPro.TextMeshProUGUI loadingText;
        [SerializeField] private TMPro.TextMeshProUGUI tipText;
        
        [Header("Événements")]
        public UnityEvent OnLoadStarted;
        public UnityEvent<float> OnLoadProgress;
        public UnityEvent OnLoadCompleted;
        
        // État
        private bool isLoading = false;
        private AsyncOperation currentLoadOperation;
        
        // Conseils affichés pendant le chargement
        private readonly string[] loadingTips = new string[]
        {
            "Conseil: Les victimes ROUGES nécessitent une évacuation immédiate.",
            "Conseil: Utilisez la commande vocale 'RASSE' pour activer les contrôles mains-libres.",
            "Conseil: Vérifiez toujours la respiration avant de classifier une victime.",
            "Conseil: Les victimes qui marchent sont généralement classées VERTES.",
            "Conseil: La fréquence respiratoire > 30 indique une urgence absolue.",
            "Conseil: L'absence de pouls radial suggère un état de choc.",
            "Conseil: Réévaluez régulièrement les victimes - leur état peut changer.",
            "Conseil: Coordonnez avec le PMA pour optimiser les évacuations.",
            "Conseil: Les zones de danger sont marquées en orange - restez vigilant.",
            "Conseil: Utilisez la caméra thermique pour détecter les victimes cachées."
        };

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(false);
            }
        }

        #region Public Methods

        /// <summary>
        /// Charge une scène par son nom
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (!isLoading)
            {
                StartCoroutine(LoadSceneAsync(sceneName));
            }
        }

        /// <summary>
        /// Charge une scène par son index de build
        /// </summary>
        public void LoadScene(int sceneIndex)
        {
            if (!isLoading)
            {
                StartCoroutine(LoadSceneAsync(sceneIndex));
            }
        }

        /// <summary>
        /// Recharge la scène actuelle
        /// </summary>
        public void ReloadCurrentScene()
        {
            LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Charge le menu principal
        /// </summary>
        public void LoadMainMenu()
        {
            LoadScene(GameConstants.SCENE_MAIN_MENU);
        }

        /// <summary>
        /// Charge la scène de formation
        /// </summary>
        public void LoadTrainingScene()
        {
            LoadScene(GameConstants.SCENE_TRAINING);
        }

        /// <summary>
        /// Charge un scénario par son identifiant
        /// </summary>
        public void LoadScenario(string scenarioId)
        {
            string sceneName = scenarioId switch
            {
                "SCN_INDUST_001" => GameConstants.SCENE_INDUSTRIAL,
                "SCN_TRAIN_001" => GameConstants.SCENE_TRAIN,
                "SCN_COLLAPSE_001" => GameConstants.SCENE_BUILDING,
                "SCN_TUTORIAL_001" => GameConstants.SCENE_TRAINING,
                _ => scenarioId
            };
            
            LoadScene(sceneName);
        }

        /// <summary>
        /// Quitte l'application
        /// </summary>
        public void QuitApplication()
        {
            Debug.Log("[SceneLoader] Fermeture de l'application...");
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        #endregion

        #region Async Loading

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            isLoading = true;
            OnLoadStarted?.Invoke();
            
            // Afficher l'écran de chargement
            ShowLoadingScreen();
            UpdateTip();
            
            float startTime = Time.time;
            
            if (useAsyncLoading)
            {
                // Chargement asynchrone
                currentLoadOperation = SceneManager.LoadSceneAsync(sceneName);
                currentLoadOperation.allowSceneActivation = false;
                
                while (!currentLoadOperation.isDone)
                {
                    // Le progrès va de 0 à 0.9 pendant le chargement
                    float progress = Mathf.Clamp01(currentLoadOperation.progress / 0.9f);
                    UpdateProgress(progress);
                    
                    // Permettre l'activation quand prêt et temps minimum écoulé
                    if (currentLoadOperation.progress >= 0.9f && 
                        Time.time - startTime >= minimumLoadTime)
                    {
                        currentLoadOperation.allowSceneActivation = true;
                    }
                    
                    yield return null;
                }
            }
            else
            {
                // Chargement synchrone (pour les petites scènes)
                yield return new WaitForSeconds(minimumLoadTime);
                SceneManager.LoadScene(sceneName);
            }
            
            // Masquer l'écran de chargement
            HideLoadingScreen();
            
            isLoading = false;
            OnLoadCompleted?.Invoke();
            
            Debug.Log($"[SceneLoader] Scène '{sceneName}' chargée en {Time.time - startTime:F2}s");
        }

        private IEnumerator LoadSceneAsync(int sceneIndex)
        {
            string sceneName = SceneManager.GetSceneByBuildIndex(sceneIndex).name;
            yield return LoadSceneAsync(sceneName);
        }

        #endregion

        #region UI Updates

        private void ShowLoadingScreen()
        {
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(true);
            }
        }

        private void HideLoadingScreen()
        {
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(false);
            }
        }

        private void UpdateProgress(float progress)
        {
            if (progressBar != null)
            {
                progressBar.value = progress;
            }
            
            if (loadingText != null)
            {
                loadingText.text = $"Chargement... {(progress * 100):F0}%";
            }
            
            OnLoadProgress?.Invoke(progress);
        }

        private void UpdateTip()
        {
            if (tipText != null && loadingTips.Length > 0)
            {
                tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
            }
        }

        #endregion

        #region Scene Info

        /// <summary>
        /// Obtient le nom de la scène actuelle
        /// </summary>
        public string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// Vérifie si une scène est la scène actuelle
        /// </summary>
        public bool IsCurrentScene(string sceneName)
        {
            return SceneManager.GetActiveScene().name == sceneName;
        }

        /// <summary>
        /// Obtient l'index de build de la scène actuelle
        /// </summary>
        public int GetCurrentSceneIndex()
        {
            return SceneManager.GetActiveScene().buildIndex;
        }

        #endregion
    }
}
