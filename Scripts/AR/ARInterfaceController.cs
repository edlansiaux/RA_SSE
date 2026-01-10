using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace RASSE.Core
{
    /// <summary>
    /// ARInterfaceController - Simule l'affichage AR des lunettes
    /// Affiche les informations médicales en overlay sur la vue du joueur
    /// </summary>
    public class ARInterfaceController : MonoBehaviour
    {
        [Header("=== RÉFÉRENCES UI ===")]
        [SerializeField] private Canvas arCanvas;
        [SerializeField] private Camera arCamera;
        
        [Header("=== PANNEAUX PRINCIPAUX ===")]
        [SerializeField] private GameObject victimInfoPanel;
        [SerializeField] private GameObject triagePanel;
        [SerializeField] private GameObject navigationPanel;
        [SerializeField] private GameObject statsPanel;
        [SerializeField] private GameObject alertPanel;

        [Header("=== INFOS VICTIME ===")]
        [SerializeField] private TextMeshProUGUI patientIdText;
        [SerializeField] private TextMeshProUGUI patientNameText;
        [SerializeField] private TextMeshProUGUI vitalSignsText;
        [SerializeField] private TextMeshProUGUI injuryText;
        [SerializeField] private Image categoryColorIndicator;
        [SerializeField] private TextMeshProUGUI categoryText;
        [SerializeField] private TextMeshProUGUI actionText;

        [Header("=== TRIAGE ===")]
        [SerializeField] private TextMeshProUGUI triageSuggestionText;
        [SerializeField] private Button confirmTriageButton;
        [SerializeField] private Button[] overrideButtons; // Rouge, Jaune, Vert, Noir

        [Header("=== NAVIGATION ===")]
        [SerializeField] private TextMeshProUGUI distanceText;
        [SerializeField] private TextMeshProUGUI directionText;
        [SerializeField] private RectTransform compassArrow;
        [SerializeField] private TextMeshProUGUI destinationText;

        [Header("=== STATISTIQUES ===")]
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI victimsCountText;
        [SerializeField] private TextMeshProUGUI redCountText;
        [SerializeField] private TextMeshProUGUI yellowCountText;
        [SerializeField] private TextMeshProUGUI greenCountText;
        [SerializeField] private TextMeshProUGUI blackCountText;

        [Header("=== ALERTES ===")]
        [SerializeField] private TextMeshProUGUI alertText;
        [SerializeField] private Image alertBackground;

        [Header("=== MARQUEURS 3D ===")]
        [SerializeField] private GameObject victimMarkerPrefab;
        [SerializeField] private GameObject ambulanceMarkerPrefab;
        [SerializeField] private GameObject hospitalMarkerPrefab;

        [Header("=== AUDIO ===")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip alertSound;
        [SerializeField] private AudioClip confirmSound;
        [SerializeField] private AudioClip voiceConfirmSound;

        // État courant
        private VictimController currentVictim;
        private StartCategory suggestedCategory;
        private List<ARMarker> activeMarkers = new List<ARMarker>();
        private Coroutine alertCoroutine;

        public void Initialize()
        {
            Debug.Log("[ARInterface] Initialisation de l'interface AR");

            // Cacher tous les panneaux au départ
            HideAllPanels();
            
            // Configurer les boutons de triage
            SetupTriageButtons();

            // Afficher le panneau de stats
            if (statsPanel != null)
                statsPanel.SetActive(true);
        }

        private void Update()
        {
            UpdateStatsDisplay();
            UpdateMarkers();
        }

        /// <summary>
        /// Affiche les informations d'une victime
        /// </summary>
        public void ShowVictimInfo(VictimController victim)
        {
            if (victim == null) return;

            currentVictim = victim;

            if (victimInfoPanel != null)
                victimInfoPanel.SetActive(true);

            // Remplir les informations
            if (patientIdText != null)
                patientIdText.text = $"ID: {victim.PatientId}";

            if (patientNameText != null)
                patientNameText.text = $"{victim.firstName} {victim.lastName}, {victim.age} ans";

            UpdateVitalSignsDisplay(victim.VitalSigns);
            UpdateInjuryDisplay(victim);

            // Effectuer le triage automatique
            PerformAutomaticTriage(victim);

            // Jouer un son
            PlaySound(alertSound);
        }

        /// <summary>
        /// Met à jour l'affichage des signes vitaux
        /// </summary>
        private void UpdateVitalSignsDisplay(VitalSigns vitals)
        {
            if (vitalSignsText == null) return;

            string vitalText = "";
            
            // Fréquence respiratoire
            vitalText += $"<color={(GetVitalColor(vitals.respiratoryRate, 12, 20, 10, 30))}>";
            vitalText += $"FR: {vitals.respiratoryRate}/min</color>\n";

            // Fréquence cardiaque
            vitalText += $"<color={(GetVitalColor(vitals.heartRate, 60, 100, 40, 120))}>";
            vitalText += $"FC: {vitals.heartRate} bpm</color>\n";

            // Saturation O2
            vitalText += $"<color={(GetVitalColor(vitals.oxygenSaturation, 95, 100, 85, 94))}>";
            vitalText += $"SpO2: {vitals.oxygenSaturation}%</color>\n";

            // Remplissage capillaire
            string rcColor = vitals.capillaryRefillTime <= 2f ? "#00FF00" : "#FF0000";
            vitalText += $"<color={rcColor}>RC: {vitals.capillaryRefillTime:F1}s</color>\n";

            // État neurologique
            vitalText += $"GCS: {vitals.glasgowComaScale}/15 | ";
            vitalText += vitals.canFollowCommands ? "<color=#00FF00>Répond</color>" : "<color=#FF0000>Non-répondant</color>";
            vitalText += "\n";
            vitalText += vitals.canWalk ? "<color=#00FF00>Peut marcher</color>" : "<color=#FFFF00>Ne peut pas marcher</color>";

            vitalSignsText.text = vitalText;
        }

        private string GetVitalColor(float value, float normalLow, float normalHigh, float dangerLow, float dangerHigh)
        {
            if (value >= normalLow && value <= normalHigh)
                return "#00FF00"; // Vert
            if (value < dangerLow || value > dangerHigh)
                return "#FF0000"; // Rouge
            return "#FFFF00"; // Jaune
        }

        private void UpdateInjuryDisplay(VictimController victim)
        {
            if (injuryText == null) return;

            string severityColor = victim.injurySeverity switch
            {
                InjurySeverity.Minor => "#00FF00",
                InjurySeverity.Moderate => "#FFFF00",
                InjurySeverity.Serious => "#FFA500",
                InjurySeverity.Critical => "#FF0000",
                InjurySeverity.Fatal => "#000000",
                _ => "#FFFFFF"
            };

            injuryText.text = $"<color={severityColor}>{victim.primaryInjury}</color>\n" +
                             $"Localisation: {victim.injuryLocation}\n" +
                             $"Sévérité: {victim.injurySeverity}";
        }

        /// <summary>
        /// Effectue le triage automatique et affiche la suggestion
        /// </summary>
        private void PerformAutomaticTriage(VictimController victim)
        {
            if (victim.IsTriaged)
            {
                ShowTriageResult(victim.TriageCategory);
                return;
            }

            // Calculer la catégorie suggérée
            suggestedCategory = GameManager.Instance.triageSystem.CalculateStartCategory(victim.VitalSigns);

            // Afficher le panneau de triage
            if (triagePanel != null)
            {
                triagePanel.SetActive(true);

                if (triageSuggestionText != null)
                {
                    string colorHex = ColorUtility.ToHtmlStringRGB(StartTriageSystem.GetCategoryColor(suggestedCategory));
                    triageSuggestionText.text = $"Classification suggérée:\n" +
                                                $"<color=#{colorHex}><size=150%>{suggestedCategory}</size></color>\n\n" +
                                                $"{StartTriageSystem.GetCategoryDescription(suggestedCategory)}\n\n" +
                                                $"<i>Dites \"Valider\" ou sélectionnez une catégorie</i>";
                }
            }

            ShowAlert($"Victime détectée - Triage: {suggestedCategory}", 
                     StartTriageSystem.GetCategoryColor(suggestedCategory));
        }

        /// <summary>
        /// Affiche le résultat du triage
        /// </summary>
        public void ShowTriageResult(StartCategory category)
        {
            if (categoryColorIndicator != null)
                categoryColorIndicator.color = StartTriageSystem.GetCategoryColor(category);

            if (categoryText != null)
            {
                categoryText.text = category.ToString();
                categoryText.color = StartTriageSystem.GetCategoryColor(category);
            }

            if (actionText != null)
                actionText.text = StartTriageSystem.GetRecommendedAction(category);

            // Cacher le panneau de triage
            if (triagePanel != null)
                triagePanel.SetActive(false);
        }

        /// <summary>
        /// Confirme le triage avec la catégorie suggérée
        /// </summary>
        public void ConfirmSuggestedTriage()
        {
            if (currentVictim != null)
            {
                GameManager.Instance.triageSystem.ValidateTriage(currentVictim, suggestedCategory);
                ShowTriageResult(suggestedCategory);
                PlaySound(confirmSound);
                
                // Proposer l'action suivante
                ShowNextAction();
            }
        }

        /// <summary>
        /// Override le triage avec une catégorie différente
        /// </summary>
        public void OverrideTriage(int categoryIndex)
        {
            StartCategory category = (StartCategory)(categoryIndex + 1); // 1=Red, 2=Yellow, etc.
            
            if (currentVictim != null)
            {
                GameManager.Instance.triageSystem.ValidateTriage(currentVictim, category, true);
                ShowTriageResult(category);
                PlaySound(confirmSound);
                
                ShowNextAction();
            }
        }

        /// <summary>
        /// Affiche l'action suivante recommandée
        /// </summary>
        private void ShowNextAction()
        {
            if (currentVictim == null) return;

            StartCategory category = currentVictim.TriageCategory;
            
            switch (category)
            {
                case StartCategory.Red:
                case StartCategory.Yellow:
                    // Proposer un hôpital
                    ShowHospitalSelection();
                    break;
                case StartCategory.Green:
                    ShowAlert("Patient dirigé vers le PRV", Color.green);
                    HideVictimInfo();
                    break;
                case StartCategory.Black:
                    ShowAlert("Patient décédé - Signalement effectué", Color.gray);
                    HideVictimInfo();
                    break;
            }
        }

        /// <summary>
        /// Affiche la sélection d'hôpital
        /// </summary>
        private void ShowHospitalSelection()
        {
            if (currentVictim == null) return;

            HospitalData bestHospital = GameManager.Instance.hospitalSystem.GetBestHospital(
                currentVictim.TriageCategory,
                transform.position
            );

            if (bestHospital != null)
            {
                ShowAlert($"Hôpital recommandé: {bestHospital.hospitalName}\n" +
                         $"Distance: {bestHospital.distanceKm:F1} km\n" +
                         $"Dites \"Confirmer hôpital\" pour valider",
                         new Color(0.2f, 0.6f, 1f));
            }
        }

        /// <summary>
        /// Active la navigation vers une ambulance
        /// </summary>
        public void StartNavigation(Transform destination, string destinationName)
        {
            if (navigationPanel != null)
            {
                navigationPanel.SetActive(true);
                
                if (destinationText != null)
                    destinationText.text = destinationName;
            }

            StartCoroutine(UpdateNavigationCoroutine(destination));
        }

        private IEnumerator UpdateNavigationCoroutine(Transform destination)
        {
            while (destination != null && navigationPanel.activeSelf)
            {
                float distance = Vector3.Distance(transform.position, destination.position);
                
                if (distanceText != null)
                    distanceText.text = $"{distance:F1} m";

                // Calculer la direction
                Vector3 direction = destination.position - transform.position;
                direction.y = 0;
                float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);

                if (compassArrow != null)
                    compassArrow.rotation = Quaternion.Euler(0, 0, -angle);

                if (directionText != null)
                {
                    if (Mathf.Abs(angle) < 20f)
                        directionText.text = "↑ TOUT DROIT";
                    else if (angle > 0)
                        directionText.text = $"→ DROITE ({Mathf.Abs(angle):F0}°)";
                    else
                        directionText.text = $"← GAUCHE ({Mathf.Abs(angle):F0}°)";
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

        /// <summary>
        /// Affiche une alerte
        /// </summary>
        public void ShowAlert(string message, Color color, float duration = 5f)
        {
            if (alertCoroutine != null)
                StopCoroutine(alertCoroutine);

            alertCoroutine = StartCoroutine(ShowAlertCoroutine(message, color, duration));
        }

        private IEnumerator ShowAlertCoroutine(string message, Color color, float duration)
        {
            if (alertPanel != null)
            {
                alertPanel.SetActive(true);
                
                if (alertText != null)
                    alertText.text = message;
                
                if (alertBackground != null)
                    alertBackground.color = new Color(color.r, color.g, color.b, 0.8f);
            }

            yield return new WaitForSeconds(duration);

            if (alertPanel != null)
                alertPanel.SetActive(false);
        }

        /// <summary>
        /// Met à jour l'affichage des statistiques
        /// </summary>
        private void UpdateStatsDisplay()
        {
            if (GameManager.Instance == null) return;

            var stats = GameManager.Instance.statistics;
            float elapsedTime = GameManager.Instance.ElapsedTime;

            if (timeText != null)
                timeText.text = $"Temps: {FormatTime(elapsedTime)}";

            if (victimsCountText != null)
            {
                int detected = stats.victimsDetected;
                int total = GameManager.Instance.AllVictims.Count;
                victimsCountText.text = $"Victimes: {detected}/{total}";
            }

            if (redCountText != null)
                redCountText.text = stats.redCount.ToString();
            if (yellowCountText != null)
                yellowCountText.text = stats.yellowCount.ToString();
            if (greenCountText != null)
                greenCountText.text = stats.greenCount.ToString();
            if (blackCountText != null)
                blackCountText.text = stats.blackCount.ToString();
        }

        private string FormatTime(float seconds)
        {
            int minutes = (int)(seconds / 60);
            int secs = (int)(seconds % 60);
            return $"{minutes:D2}:{secs:D2}";
        }

        /// <summary>
        /// Met à jour les marqueurs 3D
        /// </summary>
        private void UpdateMarkers()
        {
            // Mettre à jour la position des marqueurs pour qu'ils fassent face à la caméra
            foreach (var marker in activeMarkers)
            {
                if (marker != null && marker.markerObject != null && arCamera != null)
                {
                    marker.markerObject.transform.LookAt(arCamera.transform);
                }
            }
        }

        /// <summary>
        /// Ajoute un marqueur de victime
        /// </summary>
        public void AddVictimMarker(VictimController victim)
        {
            if (victimMarkerPrefab == null) return;

            GameObject marker = Instantiate(victimMarkerPrefab, victim.transform.position + Vector3.up * 2.5f, Quaternion.identity);
            activeMarkers.Add(new ARMarker { markerObject = marker, target = victim.transform });
        }

        /// <summary>
        /// Cache les informations de victime
        /// </summary>
        public void HideVictimInfo()
        {
            if (victimInfoPanel != null)
                victimInfoPanel.SetActive(false);
            if (triagePanel != null)
                triagePanel.SetActive(false);
            
            currentVictim = null;
        }

        /// <summary>
        /// Cache le panneau de navigation
        /// </summary>
        public void HideNavigation()
        {
            if (navigationPanel != null)
                navigationPanel.SetActive(false);
        }

        private void HideAllPanels()
        {
            if (victimInfoPanel != null) victimInfoPanel.SetActive(false);
            if (triagePanel != null) triagePanel.SetActive(false);
            if (navigationPanel != null) navigationPanel.SetActive(false);
            if (alertPanel != null) alertPanel.SetActive(false);
        }

        private void SetupTriageButtons()
        {
            if (confirmTriageButton != null)
                confirmTriageButton.onClick.AddListener(ConfirmSuggestedTriage);

            if (overrideButtons != null)
            {
                for (int i = 0; i < overrideButtons.Length; i++)
                {
                    int index = i;
                    if (overrideButtons[i] != null)
                        overrideButtons[i].onClick.AddListener(() => OverrideTriage(index));
                }
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }

    /// <summary>
    /// Structure pour les marqueurs AR
    /// </summary>
    [System.Serializable]
    public class ARMarker
    {
        public GameObject markerObject;
        public Transform target;
    }
}
