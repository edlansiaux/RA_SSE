using UnityEngine;

namespace RASSE.Core
{
    /// <summary>
    /// RescuerController - Contrôle du joueur/secouriste
    /// Gère les mouvements et les interactions avec les victimes
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class RescuerController : MonoBehaviour
    {
        [Header("=== MOUVEMENT ===")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 10f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float gravity = 20f;

        [Header("=== CAMÉRA ===")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float maxLookAngle = 80f;

        [Header("=== DÉTECTION ===")]
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float detectionAngle = 60f;
        [SerializeField] private LayerMask victimLayer;
        [SerializeField] private float scanCooldown = 2f;

        [Header("=== INTERACTION ===")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private KeyCode scanKey = KeyCode.Q;

        [Header("=== AUDIO ===")]
        [SerializeField] private AudioSource footstepSource;
        [SerializeField] private AudioClip[] footstepSounds;
        [SerializeField] private float footstepInterval = 0.5f;

        [Header("=== RÉFÉRENCES ===")]
        [SerializeField] private ARInterfaceController arInterface;

        // Composants
        private CharacterController characterController;
        private float verticalVelocity;
        private float cameraPitch;
        private float lastFootstepTime;
        private float lastScanTime;

        // État
        public bool IsMoving { get; private set; }
        public VictimController CurrentTarget { get; private set; }
        public VictimController NearestVictim { get; private set; }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            
            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>();

            // Verrouiller le curseur
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Tag pour la détection
            gameObject.tag = "Rescuer";
        }

        private void Update()
        {
            HandleMouseLook();
            HandleMovement();
            HandleInteraction();
            HandleScanning();
            UpdateNearestVictim();
        }

        /// <summary>
        /// Gère la rotation de la caméra avec la souris
        /// </summary>
        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Rotation horizontale du personnage
            transform.Rotate(Vector3.up * mouseX);

            // Rotation verticale de la caméra
            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
            
            if (playerCamera != null)
            {
                playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
            }
        }

        /// <summary>
        /// Gère les déplacements du joueur
        /// </summary>
        private void HandleMovement()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 movement = transform.right * horizontal + transform.forward * vertical;
            
            // Courir avec Shift
            float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            movement *= speed;

            // Appliquer la gravité
            if (characterController.isGrounded)
            {
                verticalVelocity = -0.5f;
            }
            else
            {
                verticalVelocity -= gravity * Time.deltaTime;
            }

            movement.y = verticalVelocity;

            // Déplacer le personnage
            characterController.Move(movement * Time.deltaTime);

            // État de mouvement
            IsMoving = movement.magnitude > 0.1f;

            // Sons de pas
            if (IsMoving && characterController.isGrounded)
            {
                PlayFootsteps();
            }
        }

        /// <summary>
        /// Gère les interactions avec les victimes
        /// </summary>
        private void HandleInteraction()
        {
            if (Input.GetKeyDown(interactKey))
            {
                // Chercher une victime à portée
                VictimController victim = GetVictimInRange();
                
                if (victim != null)
                {
                    InteractWithVictim(victim);
                }
            }
        }

        /// <summary>
        /// Gère le scan de détection
        /// </summary>
        private void HandleScanning()
        {
            if (Input.GetKeyDown(scanKey))
            {
                if (Time.time - lastScanTime >= scanCooldown)
                {
                    PerformScan();
                    lastScanTime = Time.time;
                }
            }
        }

        /// <summary>
        /// Effectue un scan de la zone
        /// </summary>
        private void PerformScan()
        {
            Debug.Log("[Rescuer] Scan en cours...");

            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange, victimLayer);

            int detected = 0;
            foreach (var col in colliders)
            {
                VictimController victim = col.GetComponent<VictimController>();
                if (victim != null && !victim.IsDetected)
                {
                    // Vérifier l'angle de vue
                    Vector3 directionToVictim = (victim.transform.position - transform.position).normalized;
                    float angle = Vector3.Angle(transform.forward, directionToVictim);

                    if (angle <= detectionAngle)
                    {
                        victim.IsDetected = true;
                        GameManager.Instance?.RegisterVictimDetection(victim);
                        detected++;
                    }
                }
            }

            // Feedback
            string message = detected > 0 
                ? $"Scan: {detected} victime(s) détectée(s)"
                : "Scan: Aucune nouvelle victime";
            
            arInterface?.ShowAlert(message, detected > 0 ? Color.green : Color.yellow, 2f);
        }

        /// <summary>
        /// Trouve la victime la plus proche à portée d'interaction
        /// </summary>
        private VictimController GetVictimInRange()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRange, victimLayer);
            
            VictimController nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var col in colliders)
            {
                VictimController victim = col.GetComponent<VictimController>();
                if (victim != null)
                {
                    float distance = Vector3.Distance(transform.position, victim.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = victim;
                    }
                }
            }

            return nearest;
        }

        /// <summary>
        /// Met à jour la victime la plus proche (pour l'affichage)
        /// </summary>
        private void UpdateNearestVictim()
        {
            VictimController victim = GetVictimInRange();

            if (victim != NearestVictim)
            {
                NearestVictim = victim;
                
                // Afficher l'indicateur d'interaction si à portée
                if (victim != null)
                {
                    // Peut afficher un prompt "Appuyez sur E pour interagir"
                }
            }
        }

        /// <summary>
        /// Interagit avec une victime
        /// </summary>
        private void InteractWithVictim(VictimController victim)
        {
            CurrentTarget = victim;

            Debug.Log($"[Rescuer] Interaction avec {victim.PatientId}");

            // Détecter si pas encore fait
            if (!victim.IsDetected)
            {
                GameManager.Instance?.RegisterVictimDetection(victim);
            }

            // Afficher les infos dans l'interface AR
            arInterface?.ShowVictimInfo(victim);
        }

        /// <summary>
        /// Joue les sons de pas
        /// </summary>
        private void PlayFootsteps()
        {
            if (Time.time - lastFootstepTime >= footstepInterval)
            {
                lastFootstepTime = Time.time;
                
                if (footstepSource != null && footstepSounds != null && footstepSounds.Length > 0)
                {
                    AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
                    footstepSource.PlayOneShot(clip);
                }
            }
        }

        /// <summary>
        /// Téléporte le joueur à une position
        /// </summary>
        public void TeleportTo(Vector3 position)
        {
            characterController.enabled = false;
            transform.position = position;
            characterController.enabled = true;
        }

        /// <summary>
        /// Affiche la zone de détection dans l'éditeur
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // Zone de détection
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Zone d'interaction
            Gizmos.color = new Color(0, 0, 1, 0.3f);
            Gizmos.DrawWireSphere(transform.position, interactionRange);

            // Cône de vision
            Gizmos.color = Color.yellow;
            Vector3 leftBound = Quaternion.Euler(0, -detectionAngle, 0) * transform.forward * detectionRange;
            Vector3 rightBound = Quaternion.Euler(0, detectionAngle, 0) * transform.forward * detectionRange;
            Gizmos.DrawLine(transform.position, transform.position + leftBound);
            Gizmos.DrawLine(transform.position, transform.position + rightBound);
        }
    }
}
