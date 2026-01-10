using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace RASSE.UI
{
    /// <summary>
    /// UIManager - Gestionnaire principal de l'interface utilisateur
    /// Gère les menus, HUD, panneaux d'information et transitions
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("=== PANNEAUX PRINCIPAUX ===")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gameHUDPanel;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject endGamePanel;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private GameObject tutorialPanel;

        [Header("=== HUD - STATISTIQUES ===")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI victimCountText;
        [SerializeField] private TextMeshProUGUI triagedCountText;
        [SerializeField] private TextMeshProUGUI evacuatedCountText;
        [SerializeField] private Image redCountIndicator;
        [SerializeField] private Image yellowCountIndicator;
        [SerializeField] private Image greenCountIndicator;
        [SerializeField] private Image blackCountIndicator;
        [SerializeField] private TextMeshProUGUI redCountText;
        [SerializeField] private TextMeshProUGUI yellowCountText;
        [SerializeField] private TextMeshProUGUI greenCountText;
        [SerializeField] private TextMeshProUGUI blackCountText;

        [Header("=== HUD - BOUSSOLE ET MINIMAP ===")]
        [SerializeField] private RectTransform compassNeedle;
        [SerializeField] private RawImage minimapImage;
        [SerializeField] private Camera minimapCamera;

        [Header("=== PANNEAU VICTIME ===")]
        [SerializeField] private GameObject victimInfoPanel;
        [SerializeField] private TextMeshProUGUI victimNameText;
        [SerializeField] private TextMeshProUGUI victimAgeText;
        [SerializeField] private TextMeshProUGUI victimIdText;
        [SerializeField] private Image victimCategoryImage;
        [SerializeField] private TextMeshProUGUI respiratoryRateText;
        [SerializeField] private TextMeshProUGUI heartRateText;
        [SerializeField] private TextMeshProUGUI spO2Text;
        [SerializeField] private TextMeshProUGUI bloodPressureText;
        [SerializeField] private TextMeshProUGUI capillaryRefillText;
        [SerializeField] private TextMeshProUGUI consciousnessText;
        [SerializeField] private TextMeshProUGUI injuryText;

        [Header("=== PANNEAU TRIAGE ===")]
        [SerializeField] private GameObject triagePanel;
        [SerializeField] private TextMeshProUGUI suggestedCategoryText;
        [SerializeField] private Image suggestedCategoryIcon;
        [SerializeField] private Button confirmTriageButton;
        [SerializeField] private Button redOverrideButton;
        [SerializeField] private Button yellowOverrideButton;
        [SerializeField] private Button greenOverrideButton;
        [SerializeField] private Button blackOverrideButton;

        [Header("=== PANNEAU HÔPITAL ===")]
        [SerializeField] private GameObject hospitalPanel;
        [SerializeField] private Transform hospitalListContent;
        [SerializeField] private GameObject hospitalItemPrefab;
        [SerializeField] private TextMeshProUGUI selectedHospitalText;
        [SerializeField] private Button confirmHospitalButton;

        [Header("=== NOTIFICATIONS ===")]
        [SerializeField] private GameObject notificationPrefab;
        [SerializeField] private Transform notificationContainer;
        [SerializeField] private float notificationDuration = 3f;

        [Header("=== FIN DE PARTIE ===")]
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI accuracyText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI evacuationsText;
        [SerializeField] private TextMeshProUGUI gradeText;
        [SerializeField] private GameObject starContainer;

        [Header("=== PARAMÈTRES ===")]
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private Toggle voiceCommandToggle;
        [SerializeField] private TMP_Dropdown difficultyDropdown;
        [SerializeField] private TMP_Dropdown qualityDropdown;

        [Header("=== CHARGEMENT ===")]
        [SerializeField] private Slider loadingBar;
        [SerializeField] private TextMeshProUGUI loadingText;

        [Header("=== COULEURS ===")]
        [SerializeField] private Color redColor = new Color(0.9f, 0.2f, 0.2f);
        [SerializeField] private Color yellowColor = new Color(0.9f, 0.9f, 0.2f);
        [SerializeField] private Color greenColor = new Color(0.2f, 0.9f, 0.2f);
        [SerializeField] private Color blackColor = new Color(0.1f, 0.1f, 0.1f);

        // État
        private bool isPaused = false;
        private Core.VictimController currentVictim;
        private string selectedHospitalId;
        private Queue<GameObject> activeNotifications = new Queue<GameObject>();

        #region Lifecycle
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
            InitializeUI();
            SetupButtonListeners();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }

            UpdateCompass();
            UpdateHUDStats();
        }
        #endregion

        #region Initialization
        private void InitializeUI()
        {
            // Cacher tous les panneaux sauf le menu principal
            HideAllPanels();
            
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);

            // Initialiser les paramètres
            if (volumeSlider != null)
                volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
            
            if (sensitivitySlider != null)
                sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", 2f);
        }

        private void SetupButtonListeners()
        {
            // Boutons de triage
            if (confirmTriageButton != null)
                confirmTriageButton.onClick.AddListener(ConfirmTriage);
            
            if (redOverrideButton != null)
                redOverrideButton.onClick.AddListener(() => OverrideTriage(Core.StartCategory.Red));
            
            if (yellowOverrideButton != null)
                yellowOverrideButton.onClick.AddListener(() => OverrideTriage(Core.StartCategory.Yellow));
            
            if (greenOverrideButton != null)
                greenOverrideButton.onClick.AddListener(() => OverrideTriage(Core.StartCategory.Green));
            
            if (blackOverrideButton != null)
                blackOverrideButton.onClick.AddListener(() => OverrideTriage(Core.StartCategory.Black));

            // Bouton hôpital
            if (confirmHospitalButton != null)
                confirmHospitalButton.onClick.AddListener(ConfirmHospitalSelection);
        }
        #endregion

        #region Panel Management
        public void HideAllPanels()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (gameHUDPanel != null) gameHUDPanel.SetActive(false);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (endGamePanel != null) endGamePanel.SetActive(false);
            if (loadingPanel != null) loadingPanel.SetActive(false);
            if (tutorialPanel != null) tutorialPanel.SetActive(false);
            if (victimInfoPanel != null) victimInfoPanel.SetActive(false);
            if (triagePanel != null) triagePanel.SetActive(false);
            if (hospitalPanel != null) hospitalPanel.SetActive(false);
        }

        public void ShowMainMenu()
        {
            HideAllPanels();
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);
            
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ShowGameHUD()
        {
            HideAllPanels();
            if (gameHUDPanel != null)
                gameHUDPanel.SetActive(true);
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void TogglePause()
        {
            isPaused = !isPaused;
            
            if (isPaused)
            {
                Time.timeScale = 0f;
                if (pauseMenuPanel != null)
                    pauseMenuPanel.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Time.timeScale = 1f;
                if (pauseMenuPanel != null)
                    pauseMenuPanel.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void ShowSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }

        public void HideSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        public void ShowTutorial()
        {
            if (tutorialPanel != null)
                tutorialPanel.SetActive(true);
        }
        #endregion

        #region HUD Updates
        private void UpdateHUDStats()
        {
            if (Core.GameManager.Instance == null) return;

            var stats = Core.GameManager.Instance.GetStatistics();

            // Timer
            if (timerText != null)
            {
                float elapsed = stats.elapsedTime;
                int minutes = Mathf.FloorToInt(elapsed / 60f);
                int seconds = Mathf.FloorToInt(elapsed % 60f);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }

            // Compteurs
            if (victimCountText != null)
                victimCountText.text = $"Victimes: {stats.totalVictims}";
            
            if (triagedCountText != null)
                triagedCountText.text = $"Triées: {stats.triagedCount}";
            
            if (evacuatedCountText != null)
                evacuatedCountText.text = $"Évacuées: {stats.evacuatedCount}";

            // Catégories START
            if (redCountText != null) redCountText.text = stats.redCount.ToString();
            if (yellowCountText != null) yellowCountText.text = stats.yellowCount.ToString();
            if (greenCountText != null) greenCountText.text = stats.greenCount.ToString();
            if (blackCountText != null) blackCountText.text = stats.blackCount.ToString();
        }

        private void UpdateCompass()
        {
            if (compassNeedle == null || Camera.main == null) return;

            float angle = Camera.main.transform.eulerAngles.y;
            compassNeedle.localRotation = Quaternion.Euler(0, 0, angle);
        }
        #endregion

        #region Victim Info Panel
        public void ShowVictimInfo(Core.VictimController victim)
        {
            if (victim == null || victimInfoPanel == null) return;

            currentVictim = victim;
            victimInfoPanel.SetActive(true);

            // Informations de base
            if (victimNameText != null)
                victimNameText.text = victim.VictimName;
            
            if (victimAgeText != null)
                victimAgeText.text = $"{victim.Age} ans";
            
            if (victimIdText != null)
                victimIdText.text = $"ID: {victim.PatientId}";

            // Couleur catégorie
            if (victimCategoryImage != null)
                victimCategoryImage.color = GetCategoryColor(victim.CurrentCategory);

            // Signes vitaux
            var vitals = victim.GetVitalSigns();
            
            if (respiratoryRateText != null)
            {
                respiratoryRateText.text = $"FR: {vitals.respiratoryRate}/min";
                respiratoryRateText.color = GetVitalColor(vitals.respiratoryRate, 12, 20, 10, 30);
            }

            if (heartRateText != null)
            {
                heartRateText.text = $"FC: {vitals.heartRate} bpm";
                heartRateText.color = GetVitalColor(vitals.heartRate, 60, 100, 40, 120);
            }

            if (spO2Text != null)
            {
                spO2Text.text = $"SpO2: {vitals.spO2}%";
                spO2Text.color = vitals.spO2 >= 95 ? greenColor : (vitals.spO2 >= 90 ? yellowColor : redColor);
            }

            if (bloodPressureText != null)
            {
                bloodPressureText.text = $"TA: {vitals.systolicBP}/{vitals.diastolicBP}";
            }

            if (capillaryRefillText != null)
            {
                capillaryRefillText.text = $"RC: {vitals.capillaryRefillTime:F1}s";
                capillaryRefillText.color = vitals.capillaryRefillTime <= 2f ? greenColor : redColor;
            }

            if (consciousnessText != null)
            {
                consciousnessText.text = vitals.canFollowCommands ? "Conscient" : "Inconscient";
                consciousnessText.color = vitals.canFollowCommands ? greenColor : redColor;
            }

            if (injuryText != null)
            {
                injuryText.text = victim.GetInjuryDescription();
            }
        }

        public void HideVictimInfo()
        {
            if (victimInfoPanel != null)
                victimInfoPanel.SetActive(false);
            currentVictim = null;
        }

        private Color GetVitalColor(float value, float normalMin, float normalMax, float criticalMin, float criticalMax)
        {
            if (value >= normalMin && value <= normalMax)
                return greenColor;
            else if (value < criticalMin || value > criticalMax)
                return redColor;
            else
                return yellowColor;
        }
        #endregion

        #region Triage Panel
        public void ShowTriagePanel(Core.StartCategory suggestedCategory)
        {
            if (triagePanel == null) return;

            triagePanel.SetActive(true);

            if (suggestedCategoryText != null)
            {
                suggestedCategoryText.text = GetCategoryName(suggestedCategory);
                suggestedCategoryText.color = GetCategoryColor(suggestedCategory);
            }

            if (suggestedCategoryIcon != null)
                suggestedCategoryIcon.color = GetCategoryColor(suggestedCategory);
        }

        public void HideTriagePanel()
        {
            if (triagePanel != null)
                triagePanel.SetActive(false);
        }

        private void ConfirmTriage()
        {
            if (currentVictim == null) return;

            var category = Core.StartTriageSystem.Instance?.CalculateTriageCategory(currentVictim.GetVitalSigns());
            if (category.HasValue)
            {
                currentVictim.SetTriageCategory(category.Value, false);
                Core.GameManager.Instance?.RegisterTriage(currentVictim, category.Value);
                ShowNotification($"Victime classée: {GetCategoryName(category.Value)}", GetCategoryColor(category.Value));
            }

            HideTriagePanel();
            HideVictimInfo();
        }

        private void OverrideTriage(Core.StartCategory category)
        {
            if (currentVictim == null) return;

            currentVictim.SetTriageCategory(category, true);
            Core.GameManager.Instance?.RegisterTriage(currentVictim, category);
            ShowNotification($"Triage manuel: {GetCategoryName(category)}", GetCategoryColor(category));

            HideTriagePanel();
            HideVictimInfo();
        }
        #endregion

        #region Hospital Panel
        public void ShowHospitalSelection(List<Hospital.HospitalData> hospitals)
        {
            if (hospitalPanel == null || hospitalListContent == null) return;

            hospitalPanel.SetActive(true);

            // Nettoyer la liste existante
            foreach (Transform child in hospitalListContent)
            {
                Destroy(child.gameObject);
            }

            // Créer les items d'hôpital
            foreach (var hospital in hospitals)
            {
                if (hospitalItemPrefab != null)
                {
                    GameObject item = Instantiate(hospitalItemPrefab, hospitalListContent);
                    SetupHospitalItem(item, hospital);
                }
            }
        }

        private void SetupHospitalItem(GameObject item, Hospital.HospitalData hospital)
        {
            // Trouver les composants UI
            var nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var distanceText = item.transform.Find("DistanceText")?.GetComponent<TextMeshProUGUI>();
            var capacityText = item.transform.Find("CapacityText")?.GetComponent<TextMeshProUGUI>();
            var selectButton = item.GetComponent<Button>();

            if (nameText != null)
                nameText.text = hospital.hospitalName;
            
            if (distanceText != null)
                distanceText.text = $"{hospital.distanceKm:F1} km";
            
            if (capacityText != null)
                capacityText.text = $"{hospital.availableBeds}/{hospital.totalBeds} lits";

            if (selectButton != null)
            {
                string hospitalId = hospital.hospitalId;
                selectButton.onClick.AddListener(() => SelectHospital(hospitalId, hospital.hospitalName));
            }
        }

        private void SelectHospital(string hospitalId, string hospitalName)
        {
            selectedHospitalId = hospitalId;
            if (selectedHospitalText != null)
                selectedHospitalText.text = $"Sélectionné: {hospitalName}";
        }

        private void ConfirmHospitalSelection()
        {
            if (string.IsNullOrEmpty(selectedHospitalId) || currentVictim == null) return;

            // Assigner l'hôpital et demander une ambulance
            Hospital.HospitalCoordinationSystem.Instance?.ReserveBed(selectedHospitalId);
            Hospital.AmbulanceManager.Instance?.RequestAmbulance(currentVictim, selectedHospitalId);

            ShowNotification("Ambulance en route", Color.cyan);
            
            if (hospitalPanel != null)
                hospitalPanel.SetActive(false);
        }

        public void HideHospitalPanel()
        {
            if (hospitalPanel != null)
                hospitalPanel.SetActive(false);
        }
        #endregion

        #region End Game
        public void ShowEndGame(float score, float accuracy, float time, int evacuations, string grade)
        {
            HideAllPanels();
            if (endGamePanel != null)
                endGamePanel.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (finalScoreText != null)
                finalScoreText.text = $"Score: {score:F0}";
            
            if (accuracyText != null)
                accuracyText.text = $"Précision: {accuracy:F1}%";
            
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            if (timeText != null)
                timeText.text = $"Temps: {minutes:00}:{seconds:00}";
            
            if (evacuationsText != null)
                evacuationsText.text = $"Évacuations: {evacuations}";
            
            if (gradeText != null)
            {
                gradeText.text = grade;
                gradeText.color = GetGradeColor(grade);
            }

            // Afficher les étoiles selon le grade
            UpdateStars(grade);
        }

        private void UpdateStars(string grade)
        {
            if (starContainer == null) return;

            int starCount = grade switch
            {
                "S" => 5,
                "A" => 4,
                "B" => 3,
                "C" => 2,
                "D" => 1,
                _ => 0
            };

            for (int i = 0; i < starContainer.childCount; i++)
            {
                var star = starContainer.GetChild(i);
                star.gameObject.SetActive(i < starCount);
            }
        }

        private Color GetGradeColor(string grade)
        {
            return grade switch
            {
                "S" => Color.yellow,
                "A" => greenColor,
                "B" => Color.cyan,
                "C" => yellowColor,
                "D" => Color.gray,
                _ => redColor
            };
        }
        #endregion

        #region Notifications
        public void ShowNotification(string message, Color color)
        {
            if (notificationPrefab == null || notificationContainer == null) return;

            GameObject notification = Instantiate(notificationPrefab, notificationContainer);
            
            var text = notification.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = message;
                text.color = color;
            }

            var image = notification.GetComponent<Image>();
            if (image != null)
            {
                Color bgColor = color;
                bgColor.a = 0.8f;
                image.color = bgColor;
            }

            activeNotifications.Enqueue(notification);
            StartCoroutine(RemoveNotificationAfterDelay(notification));
        }

        private IEnumerator RemoveNotificationAfterDelay(GameObject notification)
        {
            yield return new WaitForSeconds(notificationDuration);
            
            if (notification != null)
            {
                // Animation de fade out
                var canvasGroup = notification.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    float elapsed = 0f;
                    float fadeDuration = 0.3f;
                    while (elapsed < fadeDuration)
                    {
                        elapsed += Time.deltaTime;
                        canvasGroup.alpha = 1f - (elapsed / fadeDuration);
                        yield return null;
                    }
                }

                Destroy(notification);
            }
        }
        #endregion

        #region Loading
        public void ShowLoading(string message = "Chargement...")
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(true);
            
            if (loadingText != null)
                loadingText.text = message;
            
            if (loadingBar != null)
                loadingBar.value = 0f;
        }

        public void UpdateLoading(float progress, string message = null)
        {
            if (loadingBar != null)
                loadingBar.value = progress;
            
            if (message != null && loadingText != null)
                loadingText.text = message;
        }

        public void HideLoading()
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(false);
        }

        public IEnumerator LoadSceneAsync(string sceneName)
        {
            ShowLoading($"Chargement de {sceneName}...");

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                UpdateLoading(operation.progress);
                yield return null;
            }

            UpdateLoading(1f, "Prêt!");
            yield return new WaitForSeconds(0.5f);

            operation.allowSceneActivation = true;
            HideLoading();
        }
        #endregion

        #region Settings
        public void SetVolume(float volume)
        {
            AudioListener.volume = volume;
            PlayerPrefs.SetFloat("Volume", volume);
        }

        public void SetSensitivity(float sensitivity)
        {
            PlayerPrefs.SetFloat("Sensitivity", sensitivity);
            
            // Appliquer au RescuerController si présent
            var rescuer = FindObjectOfType<Core.RescuerController>();
            if (rescuer != null)
            {
                rescuer.SetMouseSensitivity(sensitivity);
            }
        }

        public void SetVoiceCommands(bool enabled)
        {
            PlayerPrefs.SetInt("VoiceCommands", enabled ? 1 : 0);
        }

        public void SetDifficulty(int difficulty)
        {
            PlayerPrefs.SetInt("Difficulty", difficulty);
        }

        public void SetQuality(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
            PlayerPrefs.SetInt("Quality", qualityIndex);
        }
        #endregion

        #region Menu Actions
        public void StartGame()
        {
            int difficulty = PlayerPrefs.GetInt("Difficulty", 1);
            Core.GameManager.Instance?.StartScenario((Core.DifficultyLevel)difficulty);
            ShowGameHUD();
        }

        public void ResumeGame()
        {
            TogglePause();
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void QuitToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
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

        #region Utility
        public Color GetCategoryColor(Core.StartCategory category)
        {
            return category switch
            {
                Core.StartCategory.Red => redColor,
                Core.StartCategory.Yellow => yellowColor,
                Core.StartCategory.Green => greenColor,
                Core.StartCategory.Black => blackColor,
                _ => Color.white
            };
        }

        public string GetCategoryName(Core.StartCategory category)
        {
            return category switch
            {
                Core.StartCategory.Red => "ROUGE - Urgence Absolue",
                Core.StartCategory.Yellow => "JAUNE - Urgence Relative",
                Core.StartCategory.Green => "VERT - Urgence Différée",
                Core.StartCategory.Black => "NOIR - Décédé",
                _ => "Non classé"
            };
        }
        #endregion
    }
}
