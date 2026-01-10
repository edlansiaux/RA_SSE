using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace RASSE.Core
{
    /// <summary>
    /// AmbulanceManager - Gère la flotte d'ambulances et les affectations
    /// </summary>
    public class AmbulanceManager : MonoBehaviour
    {
        [Header("=== CONFIGURATION ===")]
        [SerializeField] private int numberOfAmbulances = 10;
        [SerializeField] private Transform ambulanceSpawnPoint;
        [SerializeField] private GameObject ambulancePrefab;

        [Header("=== AMBULANCES ===")]
        [SerializeField] private List<AmbulanceController> ambulances = new List<AmbulanceController>();

        [Header("=== ZONE D'INTERVENTION ===")]
        [SerializeField] private Transform[] ambulanceWaitingPoints;

        public void Initialize()
        {
            Debug.Log("[AmbulanceManager] Initialisation de la flotte d'ambulances");
            
            if (ambulances.Count == 0)
            {
                CreateAmbulanceFleet();
            }

            PositionAmbulances();
        }

        /// <summary>
        /// Crée la flotte d'ambulances
        /// </summary>
        private void CreateAmbulanceFleet()
        {
            for (int i = 0; i < numberOfAmbulances; i++)
            {
                AmbulanceController ambulance;

                if (ambulancePrefab != null)
                {
                    GameObject obj = Instantiate(ambulancePrefab, transform);
                    obj.name = $"Ambulance_{i + 1:D2}";
                    ambulance = obj.GetComponent<AmbulanceController>();
                    if (ambulance == null)
                        ambulance = obj.AddComponent<AmbulanceController>();
                }
                else
                {
                    GameObject obj = new GameObject($"Ambulance_{i + 1:D2}");
                    obj.transform.SetParent(transform);
                    ambulance = obj.AddComponent<AmbulanceController>();
                }

                ambulance.Initialize($"AMB-{i + 1:D3}", (AmbulanceType)(i % 3));
                ambulances.Add(ambulance);
            }

            Debug.Log($"[AmbulanceManager] {ambulances.Count} ambulances créées");
        }

        /// <summary>
        /// Positionne les ambulances aux points d'attente
        /// </summary>
        private void PositionAmbulances()
        {
            if (ambulanceWaitingPoints == null || ambulanceWaitingPoints.Length == 0) return;

            for (int i = 0; i < ambulances.Count; i++)
            {
                int pointIndex = i % ambulanceWaitingPoints.Length;
                Vector3 offset = new Vector3(i * 3f, 0, 0);
                ambulances[i].transform.position = ambulanceWaitingPoints[pointIndex].position + offset;
            }
        }

        /// <summary>
        /// Assigne une ambulance à un patient
        /// </summary>
        public AmbulanceController AssignAmbulance(VictimController victim, HospitalData hospital)
        {
            if (victim == null || hospital == null)
            {
                Debug.LogWarning("[AmbulanceManager] Victime ou hôpital null");
                return null;
            }

            // Trouver la meilleure ambulance disponible
            AmbulanceController bestAmbulance = GetBestAvailableAmbulance(victim, hospital);

            if (bestAmbulance == null)
            {
                Debug.LogWarning("[AmbulanceManager] Aucune ambulance disponible!");
                return null;
            }

            // Assigner la mission
            bestAmbulance.AssignMission(victim, hospital);

            Debug.Log($"[AmbulanceManager] {bestAmbulance.AmbulanceId} assignée à {victim.PatientId}");
            return bestAmbulance;
        }

        /// <summary>
        /// Trouve la meilleure ambulance disponible pour un cas
        /// </summary>
        private AmbulanceController GetBestAvailableAmbulance(VictimController victim, HospitalData hospital)
        {
            var availableAmbulances = ambulances
                .Where(a => a.Status == AmbulanceStatus.Available)
                .ToList();

            if (availableAmbulances.Count == 0) return null;

            // Prioriser selon le type de cas
            StartCategory category = victim.TriageCategory;

            // Pour les cas critiques, privilégier les SMUR
            if (category == StartCategory.Red)
            {
                var smurAmbulance = availableAmbulances
                    .FirstOrDefault(a => a.Type == AmbulanceType.SMUR);
                if (smurAmbulance != null) return smurAmbulance;
            }

            // Trouver l'ambulance la plus proche
            return availableAmbulances
                .OrderBy(a => Vector3.Distance(a.transform.position, victim.transform.position))
                .First();
        }

        /// <summary>
        /// Obtient toutes les ambulances disponibles
        /// </summary>
        public List<AmbulanceController> GetAvailableAmbulances()
        {
            return ambulances.Where(a => a.Status == AmbulanceStatus.Available).ToList();
        }

        /// <summary>
        /// Obtient le nombre d'ambulances par statut
        /// </summary>
        public Dictionary<AmbulanceStatus, int> GetStatusCount()
        {
            return ambulances
                .GroupBy(a => a.Status)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Obtient un résumé de la flotte
        /// </summary>
        public string GetFleetSummary()
        {
            var statusCount = GetStatusCount();
            int available = statusCount.GetValueOrDefault(AmbulanceStatus.Available, 0);
            int enRoute = statusCount.GetValueOrDefault(AmbulanceStatus.EnRouteToScene, 0) +
                         statusCount.GetValueOrDefault(AmbulanceStatus.EnRouteToHospital, 0);
            int onScene = statusCount.GetValueOrDefault(AmbulanceStatus.OnScene, 0);

            return $"Ambulances: {available} disponibles | {enRoute} en route | {onScene} sur site";
        }
    }

    /// <summary>
    /// Contrôleur d'une ambulance individuelle
    /// </summary>
    public class AmbulanceController : MonoBehaviour
    {
        [Header("=== IDENTIFICATION ===")]
        [SerializeField] private string ambulanceId;
        public string AmbulanceId => ambulanceId;

        [SerializeField] private AmbulanceType type;
        public AmbulanceType Type => type;

        [Header("=== ÉTAT ===")]
        [SerializeField] private AmbulanceStatus status = AmbulanceStatus.Available;
        public AmbulanceStatus Status => status;

        [Header("=== MISSION ACTUELLE ===")]
        public VictimController CurrentPatient { get; private set; }
        public HospitalData TargetHospital { get; private set; }

        [Header("=== ÉQUIPAGE ===")]
        public int crewSize = 2;
        public bool hasMedic = false;
        public bool hasDoctor = false;

        [Header("=== MOUVEMENT ===")]
        [SerializeField] private float moveSpeed = 15f;
        [SerializeField] private float rotationSpeed = 5f;

        private Vector3 targetPosition;
        private bool isMoving;

        public void Initialize(string id, AmbulanceType ambulanceType)
        {
            ambulanceId = id;
            type = ambulanceType;
            status = AmbulanceStatus.Available;

            // Configurer selon le type
            switch (type)
            {
                case AmbulanceType.Basic:
                    crewSize = 2;
                    hasMedic = false;
                    hasDoctor = false;
                    break;
                case AmbulanceType.Advanced:
                    crewSize = 3;
                    hasMedic = true;
                    hasDoctor = false;
                    break;
                case AmbulanceType.SMUR:
                    crewSize = 4;
                    hasMedic = true;
                    hasDoctor = true;
                    break;
            }
        }

        private void Update()
        {
            if (isMoving)
            {
                MoveTowardsTarget();
            }
        }

        /// <summary>
        /// Assigne une mission à l'ambulance
        /// </summary>
        public void AssignMission(VictimController victim, HospitalData hospital)
        {
            CurrentPatient = victim;
            TargetHospital = hospital;
            status = AmbulanceStatus.EnRouteToScene;

            // Démarrer le mouvement vers la victime
            SetTarget(victim.transform.position);

            Debug.Log($"[{ambulanceId}] Mission assignée - Patient: {victim.PatientId}");
        }

        /// <summary>
        /// Arrive sur les lieux
        /// </summary>
        public void ArriveOnScene()
        {
            status = AmbulanceStatus.OnScene;
            isMoving = false;

            Debug.Log($"[{ambulanceId}] Arrivée sur les lieux");
            
            // Notifier l'interface AR
            GameManager.Instance?.arInterface?.ShowAlert(
                $"Ambulance {ambulanceId} arrivée",
                Color.green, 3f);
        }

        /// <summary>
        /// Charge le patient et part vers l'hôpital
        /// </summary>
        public void LoadPatientAndDepart()
        {
            if (CurrentPatient == null) return;

            status = AmbulanceStatus.EnRouteToHospital;
            
            // Déplacer vers l'hôpital
            SetTarget(TargetHospital.position);

            Debug.Log($"[{ambulanceId}] Départ vers {TargetHospital.hospitalName}");
        }

        /// <summary>
        /// Arrive à l'hôpital
        /// </summary>
        public void ArriveAtHospital()
        {
            status = AmbulanceStatus.AtHospital;
            isMoving = false;

            // Marquer la victime comme évacuée
            if (CurrentPatient != null)
            {
                GameManager.Instance?.RegisterVictimEvacuation(CurrentPatient);
            }

            Debug.Log($"[{ambulanceId}] Arrivée à l'hôpital");

            // Après un délai, redevenir disponible
            Invoke(nameof(BecomeAvailable), 30f);
        }

        /// <summary>
        /// Redevient disponible
        /// </summary>
        public void BecomeAvailable()
        {
            status = AmbulanceStatus.Available;
            CurrentPatient = null;
            TargetHospital = null;

            Debug.Log($"[{ambulanceId}] Disponible");
        }

        private void SetTarget(Vector3 position)
        {
            targetPosition = position;
            isMoving = true;
        }

        private void MoveTowardsTarget()
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0;

            if (direction.magnitude < 2f)
            {
                // Arrivée à destination
                switch (status)
                {
                    case AmbulanceStatus.EnRouteToScene:
                        ArriveOnScene();
                        break;
                    case AmbulanceStatus.EnRouteToHospital:
                        ArriveAtHospital();
                        break;
                }
                return;
            }

            // Mouvement
            transform.position += direction.normalized * moveSpeed * Time.deltaTime;

            // Rotation
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Obtient le temps estimé d'arrivée
        /// </summary>
        public float GetETA()
        {
            if (!isMoving) return 0f;
            float distance = Vector3.Distance(transform.position, targetPosition);
            return distance / moveSpeed;
        }
    }

    public enum AmbulanceType
    {
        Basic,      // Ambulance de base (VSAV)
        Advanced,   // Ambulance médicalisée
        SMUR        // Service Mobile d'Urgence et de Réanimation
    }

    public enum AmbulanceStatus
    {
        Available,          // Disponible
        EnRouteToScene,     // En route vers les lieux
        OnScene,            // Sur les lieux
        EnRouteToHospital,  // En route vers l'hôpital
        AtHospital,         // À l'hôpital
        OutOfService        // Hors service
    }
}
