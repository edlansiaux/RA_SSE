using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

namespace RASSE.UI
{
    /// <summary>
    /// PauseMenuController - Gestion du menu pause en jeu
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        public static PauseMenuController Instance { get; private set; }

        [Header("=== PANNEAUX ===")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject confirmQuitPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("=== BOUTONS ===")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;

        [Header("=== CONFIRMATION ===")]
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;
        [SerializeField] private TextMeshProUGUI confirmMessageText;

        [Header("=== AUDIO ===")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip pauseSound;
        [SerializeField] private AudioClip resumeSound;
        [SerializeField] private AudioClip buttonSound;

        [Header("=== PARAMÈTRES ===")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
        [SerializeField] private bool pauseTimeOnPause = true;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        public bool IsPaused { get; private set; }

        private System.Action pendingConfirmAction;

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
            SetupButtons();
            
            // Masquer les panneaux au démarrage
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);
            
            if (confirmQuitPanel != null)
                confirmQuitPanel.SetActive(false);

            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(pauseKey))
            {
                if (confirmQuitPanel != null && confirmQuitPanel.activeSelf)
                {
                    HideConfirmation();
                }
                else if (settingsPanel != null && settingsPanel.activeSelf)
                {
                    settingsPanel.SetActive(false);
                    pauseMenuPanel.SetActive(true);
                }
                else
                {
                    TogglePause();
                }
            }
        }

        private void SetupButtons()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(Resume);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OpenSettings);

            if (restartButton != null)
                restartButton.onClick.AddListener(() => ShowConfirmation("Recommencer la mission?", RestartLevel));

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(() => ShowConfirmation("Retourner au menu principal?\nLa progression sera perdue.", GoToMainMenu));

            if (quitButton != null)
                quitButton.onClick.AddListener(() => ShowConfirmation("Quitter le jeu?", QuitGame));

            if (confirmYesButton != null)
                confirmYesButton.onClick.AddListener(ConfirmAction);

            if (confirmNoButton != null)
                confirmNoButton.onClick.AddListener(HideConfirmation);
        }

        /// <summary>
        /// Bascule l'état de pause
        /// </summary>
        public void TogglePause()
        {
            if (IsPaused)
                Resume();
            else
                Pause();
        }

        /// <summary>
        /// Met le jeu en pause
        /// </summary>
        public void Pause()
        {
            IsPaused = true;

            if (pauseTimeOnPause)
                Time.timeScale = 0f;

            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(true);

            // Afficher le curseur
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (audioSource != null && pauseSound != null)
                audioSource.PlayOneShot(pauseSound);
        }

        /// <summary>
        /// Reprend le jeu
        /// </summary>
        public void Resume()
        {
            IsPaused = false;

            Time.timeScale = 1f;

            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);

            if (confirmQuitPanel != null)
                confirmQuitPanel.SetActive(false);

            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            // Masquer le curseur
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (audioSource != null && resumeSound != null)
                audioSource.PlayOneShot(resumeSound);
        }

        private void OpenSettings()
        {
            PlayButtonSound();
            
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);
            
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }

        private void ShowConfirmation(string message, System.Action onConfirm)
        {
            PlayButtonSound();
            pendingConfirmAction = onConfirm;

            if (confirmMessageText != null)
                confirmMessageText.text = message;

            if (confirmQuitPanel != null)
                confirmQuitPanel.SetActive(true);
        }

        private void HideConfirmation()
        {
            PlayButtonSound();
            pendingConfirmAction = null;

            if (confirmQuitPanel != null)
                confirmQuitPanel.SetActive(false);
        }

        private void ConfirmAction()
        {
            PlayButtonSound();
            
            if (confirmQuitPanel != null)
                confirmQuitPanel.SetActive(false);

            pendingConfirmAction?.Invoke();
            pendingConfirmAction = null;
        }

        private void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        private void PlayButtonSound()
        {
            if (audioSource != null && buttonSound != null)
                audioSource.PlayOneShot(buttonSound);
        }
    }

    /// <summary>
    /// ResultsScreenController - Écran de fin de mission affichant les statistiques
    /// </summary>
    public class ResultsScreenController : MonoBehaviour
    {
        public static ResultsScreenController Instance { get; private set; }

        [Header("=== PANNEAU ===")]
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private CanvasGroup resultsCanvasGroup;

        [Header("=== TITRE ===")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private Image backgroundImage;

        [Header("=== STATISTIQUES PRINCIPALES ===")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI gradeText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private Slider scoreBar;

        [Header("=== STATISTIQUES VICTIMES ===")]
        [SerializeField] private TextMeshProUGUI totalVictimsText;
        [SerializeField] private TextMeshProUGUI detectedText;
        [SerializeField] private TextMeshProUGUI triagedText;
        [SerializeField] private TextMeshProUGUI evacuatedText;
        [SerializeField] private TextMeshProUGUI deceasedText;

        [Header("=== STATISTIQUES PAR CATÉGORIE ===")]
        [SerializeField] private TextMeshProUGUI redStatsText;
        [SerializeField] private TextMeshProUGUI yellowStatsText;
        [SerializeField] private TextMeshProUGUI greenStatsText;
        [SerializeField] private TextMeshProUGUI blackStatsText;

        [Header("=== PRÉCISION ===")]
        [SerializeField] private TextMeshProUGUI accuracyText;
        [SerializeField] private Slider accuracyBar;
        [SerializeField] private TextMeshProUGUI correctTriagesText;
        [SerializeField] private TextMeshProUGUI incorrectTriagesText;

        [Header("=== MÉDAILLES/RÉCOMPENSES ===")]
        [SerializeField] private Transform medalsContainer;
        [SerializeField] private GameObject medalPrefab;

        [Header("=== BOUTONS ===")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button nextMissionButton;
        [SerializeField] private Button mainMenuButton;

        [Header("=== AUDIO ===")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip victorySound;
        [SerializeField] private AudioClip defeatSound;
        [SerializeField] private AudioClip scoreCountSound;

        [Header("=== ANIMATIONS ===")]
        [SerializeField] private float statRevealDelay = 0.1f;
        [SerializeField] private float scoreCountDuration = 2f;

        [Header("=== COULEURS ===")]
        [SerializeField] private Color victoryColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color defeatColor = new Color(0.8f, 0.2f, 0.2f);

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
            SetupButtons();

            if (resultsPanel != null)
                resultsPanel.SetActive(false);
        }

        private void SetupButtons()
        {
            if (retryButton != null)
                retryButton.onClick.AddListener(RetryMission);

            if (nextMissionButton != null)
                nextMissionButton.onClick.AddListener(NextMission);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        /// <summary>
        /// Affiche l'écran de résultats
        /// </summary>
        public void ShowResults(MissionResults results)
        {
            if (resultsPanel == null) return;

            resultsPanel.SetActive(true);

            // Arrêter le temps
            Time.timeScale = 0f;

            // Afficher le curseur
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Lancer l'animation
            StartCoroutine(AnimateResults(results));
        }

        private IEnumerator AnimateResults(MissionResults results)
        {
            // Titre et fond
            bool isVictory = results.isVictory;
            
            if (titleText != null)
                titleText.text = isVictory ? "MISSION ACCOMPLIE" : "MISSION ÉCHOUÉE";

            if (subtitleText != null)
                subtitleText.text = results.scenarioName;

            if (backgroundImage != null)
                backgroundImage.color = isVictory ? victoryColor : defeatColor;

            // Son
            if (audioSource != null)
            {
                AudioClip sound = isVictory ? victorySound : defeatSound;
                if (sound != null)
                    audioSource.PlayOneShot(sound);
            }

            yield return new WaitForSecondsRealtime(0.5f);

            // Temps
            if (timeText != null)
            {
                int minutes = (int)(results.elapsedTime / 60);
                int seconds = (int)(results.elapsedTime % 60);
                timeText.text = $"{minutes:00}:{seconds:00}";
            }

            yield return new WaitForSecondsRealtime(statRevealDelay);

            // Statistiques victimes
            if (totalVictimsText != null)
                totalVictimsText.text = results.totalVictims.ToString();

            yield return new WaitForSecondsRealtime(statRevealDelay);

            if (detectedText != null)
                detectedText.text = results.victimsDetected.ToString();

            yield return new WaitForSecondsRealtime(statRevealDelay);

            if (triagedText != null)
                triagedText.text = results.victimsTriaged.ToString();

            yield return new WaitForSecondsRealtime(statRevealDelay);

            if (evacuatedText != null)
                evacuatedText.text = results.victimsEvacuated.ToString();

            yield return new WaitForSecondsRealtime(statRevealDelay);

            if (deceasedText != null)
                deceasedText.text = results.victimsDeceased.ToString();

            yield return new WaitForSecondsRealtime(statRevealDelay);

            // Statistiques par catégorie
            if (redStatsText != null)
                redStatsText.text = $"{results.redTriaged} / {results.redEvacuated} évac.";

            if (yellowStatsText != null)
                yellowStatsText.text = $"{results.yellowTriaged} / {results.yellowEvacuated} évac.";

            if (greenStatsText != null)
                greenStatsText.text = $"{results.greenTriaged} / {results.greenEvacuated} évac.";

            if (blackStatsText != null)
                blackStatsText.text = results.blackTriaged.ToString();

            yield return new WaitForSecondsRealtime(statRevealDelay);

            // Précision
            float accuracy = results.victimsTriaged > 0 
                ? (float)results.correctTriages / results.victimsTriaged * 100f 
                : 0f;

            if (accuracyText != null)
                accuracyText.text = $"{accuracy:F1}%";

            if (accuracyBar != null)
                accuracyBar.value = accuracy / 100f;

            if (correctTriagesText != null)
                correctTriagesText.text = results.correctTriages.ToString();

            if (incorrectTriagesText != null)
                incorrectTriagesText.text = results.incorrectTriages.ToString();

            yield return new WaitForSecondsRealtime(0.5f);

            // Animation du score
            yield return StartCoroutine(AnimateScore(results.finalScore));

            // Grade
            if (gradeText != null)
            {
                gradeText.text = GetGrade(results.finalScore);
                gradeText.color = GetGradeColor(results.finalScore);
            }

            // Médailles
            ShowMedals(results);
        }

        private IEnumerator AnimateScore(int targetScore)
        {
            int currentScore = 0;
            float elapsed = 0f;

            while (elapsed < scoreCountDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / scoreCountDuration;
                currentScore = (int)Mathf.Lerp(0, targetScore, t);

                if (scoreText != null)
                    scoreText.text = currentScore.ToString("N0");

                if (scoreBar != null)
                    scoreBar.value = t;

                if (audioSource != null && scoreCountSound != null && Time.frameCount % 5 == 0)
                    audioSource.PlayOneShot(scoreCountSound, 0.2f);

                yield return null;
            }

            if (scoreText != null)
                scoreText.text = targetScore.ToString("N0");

            if (scoreBar != null)
                scoreBar.value = 1f;
        }

        private string GetGrade(int score)
        {
            if (score >= 9500) return "S+";
            if (score >= 9000) return "S";
            if (score >= 8000) return "A";
            if (score >= 7000) return "B";
            if (score >= 6000) return "C";
            if (score >= 5000) return "D";
            return "F";
        }

        private Color GetGradeColor(int score)
        {
            if (score >= 9000) return new Color(1f, 0.84f, 0f);  // Or
            if (score >= 8000) return new Color(0.75f, 0.75f, 0.75f);  // Argent
            if (score >= 7000) return new Color(0.8f, 0.5f, 0.2f);  // Bronze
            if (score >= 6000) return Color.green;
            if (score >= 5000) return Color.yellow;
            return Color.red;
        }

        private void ShowMedals(MissionResults results)
        {
            if (medalsContainer == null || medalPrefab == null) return;

            // Nettoyer les anciennes médailles
            foreach (Transform child in medalsContainer)
                Destroy(child.gameObject);

            // Médailles possibles
            if (results.perfectTriage)
                CreateMedal("Triage Parfait", "100% de précision");

            if (results.noDeaths)
                CreateMedal("Zéro Perte", "Aucun décès");

            if (results.speedBonus)
                CreateMedal("Rapide", "Terminé sous le temps limite");

            if (results.allEvacuated)
                CreateMedal("Évacuation Totale", "Toutes les victimes évacuées");

            if (results.firstTrySuccess)
                CreateMedal("Premier Essai", "Réussi du premier coup");
        }

        private void CreateMedal(string title, string description)
        {
            if (medalPrefab == null || medalsContainer == null) return;

            var medal = Instantiate(medalPrefab, medalsContainer);
            
            var texts = medal.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                texts[0].text = title;
                texts[1].text = description;
            }
        }

        private void RetryMission()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void NextMission()
        {
            Time.timeScale = 1f;
            // TODO: Charger la mission suivante
            Debug.Log("Mission suivante - À implémenter");
        }

        private void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }

    /// <summary>
    /// Structure contenant les résultats d'une mission
    /// </summary>
    [System.Serializable]
    public class MissionResults
    {
        public bool isVictory;
        public string scenarioName;
        public float elapsedTime;
        public int finalScore;

        // Statistiques victimes
        public int totalVictims;
        public int victimsDetected;
        public int victimsTriaged;
        public int victimsEvacuated;
        public int victimsDeceased;

        // Par catégorie
        public int redTriaged;
        public int redEvacuated;
        public int yellowTriaged;
        public int yellowEvacuated;
        public int greenTriaged;
        public int greenEvacuated;
        public int blackTriaged;

        // Précision
        public int correctTriages;
        public int incorrectTriages;

        // Médailles
        public bool perfectTriage;
        public bool noDeaths;
        public bool speedBonus;
        public bool allEvacuated;
        public bool firstTrySuccess;
    }
}
