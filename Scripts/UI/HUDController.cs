using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace RASSE.UI
{
    /// <summary>
    /// HUDController - Gestion de l'interface utilisateur en jeu
    /// Affiche les informations essentielles pendant le gameplay
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        public static HUDController Instance { get; private set; }

        [Header("=== PANNEAU PRINCIPAL ===")]
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private CanvasGroup hudCanvasGroup;

        [Header("=== TEMPS ET OBJECTIFS ===")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI objectiveText;
        [SerializeField] private Image timerFillImage;
        [SerializeField] private Color timerNormalColor = Color.white;
        [SerializeField] private Color timerWarningColor = Color.yellow;
        [SerializeField] private Color timerCriticalColor = Color.red;

        [Header("=== COMPTEURS VICTIMES ===")]
        [SerializeField] private TextMeshProUGUI totalVictimsText;
        [SerializeField] private TextMeshProUGUI detectedVictimsText;
        [SerializeField] private TextMeshProUGUI triagedVictimsText;
        [SerializeField] private TextMeshProUGUI evacuatedVictimsText;

        [Header("=== COMPTEURS PAR CATÉGORIE ===")]
        [SerializeField] private TextMeshProUGUI redCountText;
        [SerializeField] private TextMeshProUGUI yellowCountText;
        [SerializeField] private TextMeshProUGUI greenCountText;
        [SerializeField] private TextMeshProUGUI blackCountText;

        [Header("=== MINI-CARTE ===")]
        [SerializeField] private RawImage minimapImage;
        [SerializeField] private Camera minimapCamera;
        [SerializeField] private RectTransform playerMarker;
        [SerializeField] private GameObject victimMarkerPrefab;
        [SerializeField] private Transform minimapMarkersContainer;

        [Header("=== BOUSSOLE ===")]
        [SerializeField] private RectTransform compassNeedle;
        [SerializeField] private TextMeshProUGUI compassDirectionText;
        [SerializeField] private RectTransform compassBar;

        [Header("=== INDICATEUR CIBLE ===")]
        [SerializeField] private GameObject targetIndicator;
        [SerializeField] private TextMeshProUGUI targetDistanceText;
        [SerializeField] private Image targetDirectionArrow;

        [Header("=== CROSSHAIR ===")]
        [SerializeField] private Image crosshair;
        [SerializeField] private Color crosshairNormal = Color.white;
        [SerializeField] private Color crosshairInteract = Color.green;
        [SerializeField] private Color crosshairVictim = Color.yellow;

        [Header("=== NOTIFICATIONS ===")]
        [SerializeField] private Transform notificationContainer;
        [SerializeField] private GameObject notificationPrefab;
        [SerializeField] private int maxNotifications = 5;
        [SerializeField] private float notificationDuration = 4f;

        [Header("=== COMMANDES VOCALES ===")]
        [SerializeField] private GameObject voiceCommandIndicator;
        [SerializeField] private TextMeshProUGUI lastCommandText;
        [SerializeField] private Image voiceActivityIndicator;

        [Header("=== SANTÉ/STAMINA (optionnel) ===")]
        [SerializeField] private Slider staminaBar;
        [SerializeField] private Image staminaFill;

        // Références internes
        private Transform playerTransform;
        private Core.GameManager gameManager;
        private float totalTime;
        private System.Collections.Generic.Queue<GameObject> activeNotifications = new System.Collections.Generic.Queue<GameObject>();

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
            // Trouver les références
            var player = GameObject.FindGameObjectWithTag("Rescuer");
            if (player != null)
                playerTransform = player.transform;

            gameManager = Core.GameManager.Instance;

            // Configuration initiale
            if (hudCanvasGroup != null)
                hudCanvasGroup.alpha = 1f;

            if (voiceCommandIndicator != null)
                voiceCommandIndicator.SetActive(false);

            if (targetIndicator != null)
                targetIndicator.SetActive(false);

            // Masquer tous les panneaux si nécessaire
            if (hudPanel != null)
                hudPanel.SetActive(true);
        }

        private void Update()
        {
            UpdateTimer();
            UpdateCompass();
            UpdateCrosshair();
            UpdateTargetIndicator();
            UpdateMinimap();

            if (gameManager != null)
            {
                UpdateVictimCounters();
            }
        }

        #region Timer

        /// <summary>
        /// Initialise le timer avec le temps total
        /// </summary>
        public void InitializeTimer(float totalSeconds)
        {
            totalTime = totalSeconds;
        }

        private void UpdateTimer()
        {
            if (gameManager == null) return;

            float elapsed = gameManager.ElapsedTime;
            float remaining = totalTime > 0 ? Mathf.Max(0, totalTime - elapsed) : elapsed;

            // Formater le temps
            int minutes = (int)(remaining / 60);
            int seconds = (int)(remaining % 60);

            if (timerText != null)
            {
                timerText.text = totalTime > 0 
                    ? $"{minutes:00}:{seconds:00}" 
                    : $"+{minutes:00}:{seconds:00}";
            }

            // Mettre à jour la couleur selon le temps restant
            if (totalTime > 0)
            {
                float ratio = remaining / totalTime;
                
                if (timerFillImage != null)
                    timerFillImage.fillAmount = ratio;

                Color timerColor;
                if (ratio > 0.5f)
                    timerColor = timerNormalColor;
                else if (ratio > 0.25f)
                    timerColor = timerWarningColor;
                else
                    timerColor = timerCriticalColor;

                if (timerText != null)
                    timerText.color = timerColor;

                if (timerFillImage != null)
                    timerFillImage.color = timerColor;
            }
        }

        #endregion

        #region Compteurs

        private void UpdateVictimCounters()
        {
            if (totalVictimsText != null)
                totalVictimsText.text = gameManager.TotalVictims.ToString();

            if (detectedVictimsText != null)
                detectedVictimsText.text = gameManager.VictimsDetected.ToString();

            if (triagedVictimsText != null)
                triagedVictimsText.text = gameManager.VictimsTriaged.ToString();

            if (evacuatedVictimsText != null)
                evacuatedVictimsText.text = gameManager.VictimsEvacuated.ToString();

            // Compteurs par catégorie
            if (redCountText != null)
                redCountText.text = gameManager.RedCount.ToString();

            if (yellowCountText != null)
                yellowCountText.text = gameManager.YellowCount.ToString();

            if (greenCountText != null)
                greenCountText.text = gameManager.GreenCount.ToString();

            if (blackCountText != null)
                blackCountText.text = gameManager.BlackCount.ToString();
        }

        /// <summary>
        /// Met à jour le texte d'objectif
        /// </summary>
        public void SetObjective(string text)
        {
            if (objectiveText != null)
                objectiveText.text = text;
        }

        #endregion

        #region Boussole

        private void UpdateCompass()
        {
            if (playerTransform == null) return;

            float heading = playerTransform.eulerAngles.y;

            // Aiguille de boussole
            if (compassNeedle != null)
            {
                compassNeedle.localRotation = Quaternion.Euler(0, 0, heading);
            }

            // Barre de boussole (défilement horizontal)
            if (compassBar != null)
            {
                float normalizedHeading = heading / 360f;
                compassBar.anchoredPosition = new Vector2(-normalizedHeading * compassBar.sizeDelta.x, 0);
            }

            // Direction textuelle
            if (compassDirectionText != null)
            {
                compassDirectionText.text = GetCardinalDirection(heading);
            }
        }

        private string GetCardinalDirection(float heading)
        {
            if (heading >= 337.5f || heading < 22.5f) return "N";
            if (heading >= 22.5f && heading < 67.5f) return "NE";
            if (heading >= 67.5f && heading < 112.5f) return "E";
            if (heading >= 112.5f && heading < 157.5f) return "SE";
            if (heading >= 157.5f && heading < 202.5f) return "S";
            if (heading >= 202.5f && heading < 247.5f) return "SO";
            if (heading >= 247.5f && heading < 292.5f) return "O";
            if (heading >= 292.5f && heading < 337.5f) return "NO";
            return "N";
        }

        #endregion

        #region Crosshair

        private void UpdateCrosshair()
        {
            if (crosshair == null || playerTransform == null) return;

            // Raycast depuis le centre de l'écran
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 10f))
            {
                if (hit.collider.CompareTag("Victim"))
                {
                    crosshair.color = crosshairVictim;
                }
                else if (hit.collider.GetComponent<IInteractable>() != null)
                {
                    crosshair.color = crosshairInteract;
                }
                else
                {
                    crosshair.color = crosshairNormal;
                }
            }
            else
            {
                crosshair.color = crosshairNormal;
            }
        }

        #endregion

        #region Indicateur de Cible

        private Vector3 currentTarget;
        private bool hasTarget;

        /// <summary>
        /// Définit la cible à suivre
        /// </summary>
        public void SetTarget(Vector3 position)
        {
            currentTarget = position;
            hasTarget = true;
            
            if (targetIndicator != null)
                targetIndicator.SetActive(true);
        }

        /// <summary>
        /// Efface la cible
        /// </summary>
        public void ClearTarget()
        {
            hasTarget = false;
            
            if (targetIndicator != null)
                targetIndicator.SetActive(false);
        }

        private void UpdateTargetIndicator()
        {
            if (!hasTarget || targetIndicator == null || playerTransform == null) return;

            float distance = Vector3.Distance(playerTransform.position, currentTarget);
            
            if (targetDistanceText != null)
                targetDistanceText.text = $"{distance:F1}m";

            // Direction vers la cible
            Vector3 directionToTarget = (currentTarget - playerTransform.position).normalized;
            Vector3 forward = playerTransform.forward;
            forward.y = 0;
            directionToTarget.y = 0;

            float angle = Vector3.SignedAngle(forward, directionToTarget, Vector3.up);

            if (targetDirectionArrow != null)
            {
                targetDirectionArrow.transform.localRotation = Quaternion.Euler(0, 0, -angle);
            }

            // Positionner l'indicateur sur l'écran
            Vector3 screenPos = Camera.main.WorldToScreenPoint(currentTarget);
            
            if (screenPos.z > 0)
            {
                // La cible est devant nous
                screenPos.x = Mathf.Clamp(screenPos.x, 50, Screen.width - 50);
                screenPos.y = Mathf.Clamp(screenPos.y, 50, Screen.height - 50);
            }
            else
            {
                // La cible est derrière nous
                screenPos.x = Screen.width / 2 + (screenPos.x < Screen.width / 2 ? 1 : -1) * (Screen.width / 2 - 50);
                screenPos.y = 50;
            }

            targetIndicator.transform.position = screenPos;
        }

        #endregion

        #region Mini-carte

        private void UpdateMinimap()
        {
            if (minimapCamera == null || playerTransform == null) return;

            // Suivre le joueur
            Vector3 newPos = playerTransform.position;
            newPos.y = minimapCamera.transform.position.y;
            minimapCamera.transform.position = newPos;

            // Rotation selon le joueur
            minimapCamera.transform.rotation = Quaternion.Euler(90, playerTransform.eulerAngles.y, 0);
        }

        #endregion

        #region Notifications

        /// <summary>
        /// Affiche une notification
        /// </summary>
        public void ShowNotification(string message, NotificationType type = NotificationType.Info)
        {
            if (notificationContainer == null || notificationPrefab == null) return;

            // Supprimer les anciennes notifications si nécessaire
            while (activeNotifications.Count >= maxNotifications)
            {
                var old = activeNotifications.Dequeue();
                if (old != null) Destroy(old);
            }

            // Créer la notification
            var notification = Instantiate(notificationPrefab, notificationContainer);
            activeNotifications.Enqueue(notification);

            // Configurer le contenu
            var text = notification.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = message;

            // Couleur selon le type
            var image = notification.GetComponent<Image>();
            if (image != null)
            {
                switch (type)
                {
                    case NotificationType.Success:
                        image.color = new Color(0.2f, 0.8f, 0.2f, 0.9f);
                        break;
                    case NotificationType.Warning:
                        image.color = new Color(1f, 0.8f, 0.2f, 0.9f);
                        break;
                    case NotificationType.Error:
                        image.color = new Color(0.9f, 0.2f, 0.2f, 0.9f);
                        break;
                    case NotificationType.Critical:
                        image.color = new Color(0.6f, 0f, 0f, 0.9f);
                        break;
                    default:
                        image.color = new Color(0.2f, 0.4f, 0.8f, 0.9f);
                        break;
                }
            }

            // Auto-destruction
            StartCoroutine(DestroyNotificationAfterDelay(notification, notificationDuration));
        }

        private IEnumerator DestroyNotificationAfterDelay(GameObject notification, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (notification != null)
            {
                // Animation de fade out
                var canvasGroup = notification.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    float elapsed = 0f;
                    float fadeDuration = 0.5f;

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

        #region Commandes Vocales

        /// <summary>
        /// Affiche une commande vocale reconnue
        /// </summary>
        public void ShowVoiceCommand(string command)
        {
            if (voiceCommandIndicator != null)
            {
                voiceCommandIndicator.SetActive(true);
                StartCoroutine(HideVoiceIndicator(2f));
            }

            if (lastCommandText != null)
                lastCommandText.text = command;
        }

        /// <summary>
        /// Active/désactive l'indicateur d'activité vocale
        /// </summary>
        public void SetVoiceActivity(bool active)
        {
            if (voiceActivityIndicator != null)
            {
                voiceActivityIndicator.color = active 
                    ? new Color(0.2f, 0.8f, 0.2f) 
                    : new Color(0.5f, 0.5f, 0.5f);
            }
        }

        private IEnumerator HideVoiceIndicator(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (voiceCommandIndicator != null)
                voiceCommandIndicator.SetActive(false);
        }

        #endregion

        #region Stamina

        /// <summary>
        /// Met à jour la barre de stamina
        /// </summary>
        public void UpdateStamina(float current, float max)
        {
            if (staminaBar != null)
            {
                staminaBar.value = current / max;
            }

            if (staminaFill != null)
            {
                float ratio = current / max;
                staminaFill.color = Color.Lerp(Color.red, Color.green, ratio);
            }
        }

        #endregion

        #region Contrôle du HUD

        /// <summary>
        /// Affiche/masque le HUD
        /// </summary>
        public void SetHUDVisible(bool visible)
        {
            if (hudCanvasGroup != null)
            {
                StartCoroutine(FadeHUD(visible ? 1f : 0f, 0.3f));
            }
            else if (hudPanel != null)
            {
                hudPanel.SetActive(visible);
            }
        }

        private IEnumerator FadeHUD(float targetAlpha, float duration)
        {
            float startAlpha = hudCanvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                hudCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            hudCanvasGroup.alpha = targetAlpha;
        }

        #endregion
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error,
        Critical
    }

    /// <summary>
    /// Interface pour les objets interactifs
    /// </summary>
    public interface IInteractable
    {
        void Interact();
        string GetInteractionPrompt();
    }
}
