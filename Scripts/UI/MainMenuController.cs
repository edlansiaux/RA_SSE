using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections;
using TMPro;

namespace RASSE.UI
{
    /// <summary>
    /// MainMenuController - Gestion du menu principal du jeu
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("=== PANNEAUX ===")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject playMenuPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject creditsPanel;
        [SerializeField] private GameObject loadingPanel;

        [Header("=== BOUTONS MENU PRINCIPAL ===")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button tutorialButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;

        [Header("=== BOUTONS MENU JOUER ===")]
        [SerializeField] private Button quickPlayButton;
        [SerializeField] private Button scenarioSelectButton;
        [SerializeField] private Button freePlayButton;
        [SerializeField] private Button playBackButton;

        [Header("=== SÉLECTION DIFFICULTÉ ===")]
        [SerializeField] private TMP_Dropdown difficultyDropdown;
        [SerializeField] private TextMeshProUGUI difficultyDescriptionText;

        [Header("=== PARAMÈTRES ===")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Slider mouseSensitivitySlider;
        [SerializeField] private Toggle invertYToggle;
        [SerializeField] private Button settingsBackButton;
        [SerializeField] private Button settingsApplyButton;
        [SerializeField] private Button settingsResetButton;

        [Header("=== CHARGEMENT ===")]
        [SerializeField] private Slider loadingProgressBar;
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private TextMeshProUGUI loadingTipText;

        [Header("=== AUDIO ===")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip buttonHoverSound;
        [SerializeField] private AudioClip menuMusic;

        [Header("=== SCÈNES ===")]
        [SerializeField] private string tutorialSceneName = "TutorialScene";
        [SerializeField] private string gameSceneName = "MainGameScene";

        [Header("=== ÉVÉNEMENTS ===")]
        public UnityEvent OnMenuOpened;
        public UnityEvent OnGameStarted;

        private Resolution[] resolutions;
        private Core.DifficultyLevel selectedDifficulty = Core.DifficultyLevel.Normal;

        private string[] loadingTips = new string[]
        {
            "Conseil: Le protocole START permet de trier jusqu'à 60 victimes par heure.",
            "Conseil: Une victime qui peut marcher est automatiquement classée VERT.",
            "Conseil: Le temps de remplissage capillaire se mesure en appuyant sur l'ongle.",
            "Conseil: En cas de doute, sur-triez la victime (catégorie plus grave).",
            "Conseil: Les commandes vocales accélèrent considérablement le triage.",
            "Conseil: Surveillez la détérioration des victimes non évacuées.",
            "Conseil: Les ambulances SMUR sont réservées aux cas les plus critiques.",
            "Conseil: Vérifiez toujours la respiration en premier."
        };

        private void Start()
        {
            InitializeMenu();
            SetupButtons();
            LoadSettings();
            ShowPanel(mainMenuPanel);

            if (menuMusic != null && audioSource != null)
            {
                audioSource.clip = menuMusic;
                audioSource.loop = true;
                audioSource.Play();
            }

            OnMenuOpened?.Invoke();
        }

        private void InitializeMenu()
        {
            // Initialiser les résolutions
            resolutions = Screen.resolutions;
            if (resolutionDropdown != null)
            {
                resolutionDropdown.ClearOptions();
                var options = new System.Collections.Generic.List<string>();
                int currentResolutionIndex = 0;

                for (int i = 0; i < resolutions.Length; i++)
                {
                    string option = $"{resolutions[i].width} x {resolutions[i].height} @ {resolutions[i].refreshRateRatio}Hz";
                    options.Add(option);

                    if (resolutions[i].width == Screen.currentResolution.width &&
                        resolutions[i].height == Screen.currentResolution.height)
                    {
                        currentResolutionIndex = i;
                    }
                }

                resolutionDropdown.AddOptions(options);
                resolutionDropdown.value = currentResolutionIndex;
                resolutionDropdown.RefreshShownValue();
            }

            // Initialiser les niveaux de qualité
            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
                qualityDropdown.value = QualitySettings.GetQualityLevel();
                qualityDropdown.RefreshShownValue();
            }

            // Initialiser la difficulté
            if (difficultyDropdown != null)
            {
                difficultyDropdown.ClearOptions();
                difficultyDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "Tutoriel", "Facile", "Normal", "Difficile", "Expert"
                });
                difficultyDropdown.value = 2; // Normal par défaut
                difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
                UpdateDifficultyDescription();
            }
        }

        private void SetupButtons()
        {
            // Menu principal
            if (playButton != null) playButton.onClick.AddListener(() => { PlaySound(buttonClickSound); ShowPanel(playMenuPanel); });
            if (tutorialButton != null) tutorialButton.onClick.AddListener(() => { PlaySound(buttonClickSound); StartTutorial(); });
            if (settingsButton != null) settingsButton.onClick.AddListener(() => { PlaySound(buttonClickSound); ShowPanel(settingsPanel); });
            if (creditsButton != null) creditsButton.onClick.AddListener(() => { PlaySound(buttonClickSound); ShowPanel(creditsPanel); });
            if (quitButton != null) quitButton.onClick.AddListener(() => { PlaySound(buttonClickSound); QuitGame(); });

            // Menu Jouer
            if (quickPlayButton != null) quickPlayButton.onClick.AddListener(() => { PlaySound(buttonClickSound); StartQuickPlay(); });
            if (scenarioSelectButton != null) scenarioSelectButton.onClick.AddListener(() => { PlaySound(buttonClickSound); OpenScenarioSelect(); });
            if (freePlayButton != null) freePlayButton.onClick.AddListener(() => { PlaySound(buttonClickSound); StartFreePlay(); });
            if (playBackButton != null) playBackButton.onClick.AddListener(() => { PlaySound(buttonClickSound); ShowPanel(mainMenuPanel); });

            // Paramètres
            if (settingsBackButton != null) settingsBackButton.onClick.AddListener(() => { PlaySound(buttonClickSound); ShowPanel(mainMenuPanel); });
            if (settingsApplyButton != null) settingsApplyButton.onClick.AddListener(() => { PlaySound(buttonClickSound); ApplySettings(); });
            if (settingsResetButton != null) settingsResetButton.onClick.AddListener(() => { PlaySound(buttonClickSound); ResetSettings(); });

            // Sliders
            if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

            // Ajouter sons de survol aux boutons
            AddHoverSounds(playButton, tutorialButton, settingsButton, creditsButton, quitButton);
            AddHoverSounds(quickPlayButton, scenarioSelectButton, freePlayButton, playBackButton);
        }

        private void AddHoverSounds(params Button[] buttons)
        {
            foreach (var button in buttons)
            {
                if (button != null)
                {
                    var trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                    var entry = new UnityEngine.EventSystems.EventTrigger.Entry();
                    entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
                    entry.callback.AddListener((data) => { PlaySound(buttonHoverSound); });
                    trigger.triggers.Add(entry);
                }
            }
        }

        private void ShowPanel(GameObject panel)
        {
            // Cacher tous les panneaux
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (playMenuPanel != null) playMenuPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
            if (loadingPanel != null) loadingPanel.SetActive(false);

            // Afficher le panneau demandé
            if (panel != null) panel.SetActive(true);
        }

        private void OnDifficultyChanged(int index)
        {
            selectedDifficulty = (Core.DifficultyLevel)index;
            UpdateDifficultyDescription();
            PlaySound(buttonClickSound);
        }

        private void UpdateDifficultyDescription()
        {
            if (difficultyDescriptionText == null) return;

            switch (selectedDifficulty)
            {
                case Core.DifficultyLevel.Tutorial:
                    difficultyDescriptionText.text = "Mode guidé avec conseils et indices. Parfait pour apprendre le protocole START.";
                    break;
                case Core.DifficultyLevel.Easy:
                    difficultyDescriptionText.text = "5 victimes, signes vitaux clairs, temps illimité. Idéal pour débuter.";
                    break;
                case Core.DifficultyLevel.Normal:
                    difficultyDescriptionText.text = "10 victimes, cas variés, limite de temps 15 minutes.";
                    break;
                case Core.DifficultyLevel.Hard:
                    difficultyDescriptionText.text = "15 victimes, cas complexes, détérioration rapide, 12 minutes.";
                    break;
                case Core.DifficultyLevel.Expert:
                    difficultyDescriptionText.text = "20+ victimes, conditions difficiles, victimes qui se détériorent, 10 minutes.";
                    break;
            }
        }

        #region Actions de Menu

        public void StartTutorial()
        {
            PlayerPrefs.SetInt("GameMode", 0); // Tutorial mode
            StartCoroutine(LoadSceneAsync(tutorialSceneName));
        }

        public void StartQuickPlay()
        {
            PlayerPrefs.SetInt("GameMode", 1); // Quick play
            PlayerPrefs.SetInt("Difficulty", (int)selectedDifficulty);
            StartCoroutine(LoadSceneAsync(gameSceneName));
        }

        public void OpenScenarioSelect()
        {
            // TODO: Ouvrir le menu de sélection de scénario
            Debug.Log("Sélection de scénario - À implémenter");
        }

        public void StartFreePlay()
        {
            PlayerPrefs.SetInt("GameMode", 2); // Free play - sans limite
            PlayerPrefs.SetInt("Difficulty", (int)selectedDifficulty);
            StartCoroutine(LoadSceneAsync(gameSceneName));
        }

        public void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        #endregion

        #region Chargement de Scène

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            ShowPanel(loadingPanel);

            // Afficher un conseil aléatoire
            if (loadingTipText != null)
            {
                loadingTipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
            }

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            while (!operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);

                if (loadingProgressBar != null)
                    loadingProgressBar.value = progress;

                if (loadingText != null)
                    loadingText.text = $"Chargement... {(int)(progress * 100)}%";

                // Changer de conseil toutes les 3 secondes
                if (Time.time % 3 < Time.deltaTime && loadingTipText != null)
                {
                    loadingTipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
                }

                if (operation.progress >= 0.9f)
                {
                    if (loadingText != null)
                        loadingText.text = "Appuyez sur une touche pour continuer...";

                    if (Input.anyKeyDown)
                    {
                        operation.allowSceneActivation = true;
                    }
                }

                yield return null;
            }

            OnGameStarted?.Invoke();
        }

        #endregion

        #region Paramètres

        private void LoadSettings()
        {
            if (masterVolumeSlider != null)
                masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);

            if (musicVolumeSlider != null)
                musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

            if (fullscreenToggle != null)
                fullscreenToggle.isOn = Screen.fullScreen;

            if (mouseSensitivitySlider != null)
                mouseSensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 2f);

            if (invertYToggle != null)
                invertYToggle.isOn = PlayerPrefs.GetInt("InvertY", 0) == 1;
        }

        private void ApplySettings()
        {
            // Audio
            float masterVol = masterVolumeSlider != null ? masterVolumeSlider.value : 1f;
            float musicVol = musicVolumeSlider != null ? musicVolumeSlider.value : 0.7f;
            float sfxVol = sfxVolumeSlider != null ? sfxVolumeSlider.value : 1f;

            PlayerPrefs.SetFloat("MasterVolume", masterVol);
            PlayerPrefs.SetFloat("MusicVolume", musicVol);
            PlayerPrefs.SetFloat("SFXVolume", sfxVol);

            AudioListener.volume = masterVol;

            // Graphiques
            if (qualityDropdown != null)
            {
                QualitySettings.SetQualityLevel(qualityDropdown.value);
                PlayerPrefs.SetInt("QualityLevel", qualityDropdown.value);
            }

            if (resolutionDropdown != null && resolutions != null && resolutionDropdown.value < resolutions.Length)
            {
                Resolution res = resolutions[resolutionDropdown.value];
                bool fullscreen = fullscreenToggle != null && fullscreenToggle.isOn;
                Screen.SetResolution(res.width, res.height, fullscreen);
                PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
                PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
            }

            // Contrôles
            if (mouseSensitivitySlider != null)
                PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivitySlider.value);

            if (invertYToggle != null)
                PlayerPrefs.SetInt("InvertY", invertYToggle.isOn ? 1 : 0);

            PlayerPrefs.Save();
            Debug.Log("Paramètres appliqués et sauvegardés");
        }

        private void ResetSettings()
        {
            // Valeurs par défaut
            if (masterVolumeSlider != null) masterVolumeSlider.value = 1f;
            if (musicVolumeSlider != null) musicVolumeSlider.value = 0.7f;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = 1f;
            if (qualityDropdown != null) qualityDropdown.value = QualitySettings.names.Length - 1;
            if (fullscreenToggle != null) fullscreenToggle.isOn = true;
            if (mouseSensitivitySlider != null) mouseSensitivitySlider.value = 2f;
            if (invertYToggle != null) invertYToggle.isOn = false;

            ApplySettings();
        }

        private void OnMasterVolumeChanged(float value)
        {
            AudioListener.volume = value;
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (audioSource != null)
                audioSource.volume = value;
        }

        private void OnSFXVolumeChanged(float value)
        {
            // Sera appliqué aux sons SFX via le AudioManager
        }

        #endregion

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip, PlayerPrefs.GetFloat("SFXVolume", 1f));
            }
        }

        private void Update()
        {
            // Échapper pour revenir au menu principal
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (playMenuPanel != null && playMenuPanel.activeSelf)
                    ShowPanel(mainMenuPanel);
                else if (settingsPanel != null && settingsPanel.activeSelf)
                    ShowPanel(mainMenuPanel);
                else if (creditsPanel != null && creditsPanel.activeSelf)
                    ShowPanel(mainMenuPanel);
            }
        }
    }
}
