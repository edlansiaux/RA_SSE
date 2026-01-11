using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace RASSE.Zones
{
    /// <summary>
    /// Statut opérationnel du PMA
    /// </summary>
    public enum PMAStatus
    {
        Deploying,      // En cours de déploiement
        Operational,    // Opérationnel
        Saturated,      // Saturé
        Relocating,     // En cours de relocalisation
        Closed          // Fermé
    }

    /// <summary>
    /// Contrôleur du Poste Médical Avancé (PMA).
    /// Gère les zones de triage internes, le personnel et les ressources.
    /// Conforme aux protocoles français de médecine de catastrophe.
    /// </summary>
    public class PMAController : MonoBehaviour
    {
        [Header("Identification")]
        [SerializeField] private string pmaId;
        [SerializeField] private string pmaName = "Poste Médical Avancé";
        [SerializeField] private PMAStatus status = PMAStatus.Deploying;
        
        [Header("Capacité Globale")]
        [SerializeField] private int totalCapacity = 50;
        [SerializeField] private int redZoneCapacity = 15;
        [SerializeField] private int yellowZoneCapacity = 20;
        [SerializeField] private int greenZoneCapacity = 15;
        [SerializeField] private int currentPatients = 0;
        
        [Header("Personnel")]
        [SerializeField] private int medecins = 0;
        [SerializeField] private int infirmiers = 0;
        [SerializeField] private int secouristes = 0;
        [SerializeField] private int brancardiers = 0;
        
        [Header("Équipements")]
        [SerializeField] private List<string> availableEquipment = new List<string>();
        [SerializeField] private int oxygenBottlesCount = 0;
        [SerializeField] private int defibrillatorCount = 0;
        [SerializeField] private int stretcherCount = 0;
        [SerializeField] private int ivKitsCount = 0;
        
        [Header("Zones Internes")]
        [SerializeField] private TriageZoneController zoneRouge;
        [SerializeField] private TriageZoneController zoneJaune;
        [SerializeField] private TriageZoneController zoneVerte;
        [SerializeField] private Transform entryPoint;
        [SerializeField] private Transform exitPoint;
        [SerializeField] private Transform ambulanceLoadingZone;
        
        [Header("Configuration")]
        [SerializeField] private float deploymentTimeSeconds = 300f;
        [SerializeField] private bool isOperational = false;
        [SerializeField] private bool autoAssignPatients = true;
        
        [Header("Événements")]
        public UnityEvent OnPMAOperational;
        public UnityEvent OnPMASaturated;
        public UnityEvent<GameObject> OnPatientAdmitted;
        public UnityEvent<GameObject> OnPatientEvacuated;
        public UnityEvent<PMAStatus> OnStatusChanged;
        
        // État interne
        private float deploymentProgress = 0f;
        private List<GameObject> allPatients = new List<GameObject>();
        private Dictionary<string, DateTime> patientAdmissionTimes = new Dictionary<string, DateTime>();
        private Queue<GameObject> evacuationQueue = new Queue<GameObject>();
        
        // Statistiques
        private int totalAdmissions = 0;
        private int totalEvacuations = 0;
        private int totalDeaths = 0;
        
        // Propriétés publiques
        public string PMAId => pmaId;
        public string PMAName => pmaName;
        public PMAStatus Status => status;
        public bool IsOperational => isOperational;
        public int TotalCapacity => totalCapacity;
        public int CurrentPatients => currentPatients;
        public int AvailableSpots => Mathf.Max(0, totalCapacity - currentPatients);
        public float OccupancyPercentage => totalCapacity > 0 ? (float)currentPatients / totalCapacity * 100f : 0f;
        public bool IsSaturated => currentPatients >= totalCapacity * 0.9f;
        
        private void Awake()
        {
            if (string.IsNullOrEmpty(pmaId))
            {
                pmaId = $"PMA_{Guid.NewGuid().ToString().Substring(0, 8)}";
            }
        }
        
        private void Start()
        {
            InitializeZones();
            
            if (status == PMAStatus.Deploying)
            {
                StartCoroutine(DeployPMA());
            }
            else if (status == PMAStatus.Operational)
            {
                SetOperational();
            }
        }
        
        private void Update()
        {
            if (!isOperational) return;
            
            // Vérifier la saturation
            if (IsSaturated && status != PMAStatus.Saturated)
            {
                SetStatus(PMAStatus.Saturated);
                OnPMASaturated?.Invoke();
            }
            else if (!IsSaturated && status == PMAStatus.Saturated)
            {
                SetStatus(PMAStatus.Operational);
            }
            
            // Traiter la file d'évacuation
            ProcessEvacuationQueue();
        }
        
        private void InitializeZones()
        {
            // Chercher les zones enfants si non assignées
            if (zoneRouge == null)
            {
                Transform redTransform = transform.Find("Zone_Rouge") ?? transform.Find("Zone_Red");
                if (redTransform != null)
                {
                    zoneRouge = redTransform.GetComponent<TriageZoneController>();
                }
            }
            
            if (zoneJaune == null)
            {
                Transform yellowTransform = transform.Find("Zone_Jaune") ?? transform.Find("Zone_Yellow");
                if (yellowTransform != null)
                {
                    zoneJaune = yellowTransform.GetComponent<TriageZoneController>();
                }
            }
            
            if (zoneVerte == null)
            {
                Transform greenTransform = transform.Find("Zone_Verte") ?? transform.Find("Zone_Green");
                if (greenTransform != null)
                {
                    zoneVerte = greenTransform.GetComponent<TriageZoneController>();
                }
            }
            
            // Configurer les capacités
            if (zoneRouge != null) zoneRouge.SetCapacity(redZoneCapacity);
            if (zoneJaune != null) zoneJaune.SetCapacity(yellowZoneCapacity);
            if (zoneVerte != null) zoneVerte.SetCapacity(greenZoneCapacity);
        }
        
        private System.Collections.IEnumerator DeployPMA()
        {
            Debug.Log($"[PMA] Début du déploiement de {pmaName}...");
            
            float elapsed = 0f;
            while (elapsed < deploymentTimeSeconds)
            {
                elapsed += Time.deltaTime;
                deploymentProgress = elapsed / deploymentTimeSeconds;
                yield return null;
            }
            
            SetOperational();
        }
        
        private void SetOperational()
        {
            isOperational = true;
            deploymentProgress = 1f;
            SetStatus(PMAStatus.Operational);
            OnPMAOperational?.Invoke();
            Debug.Log($"[PMA] {pmaName} est maintenant opérationnel!");
        }
        
        private void SetStatus(PMAStatus newStatus)
        {
            if (status != newStatus)
            {
                status = newStatus;
                OnStatusChanged?.Invoke(status);
                Debug.Log($"[PMA] {pmaName} - Nouveau statut: {status}");
            }
        }
        
        /// <summary>
        /// Admet un patient au PMA
        /// </summary>
        public bool AdmitPatient(GameObject patient, TriageCategory category)
        {
            if (!isOperational)
            {
                Debug.LogWarning($"[PMA] {pmaName} n'est pas encore opérationnel");
                return false;
            }
            
            if (patient == null) return false;
            
            // Vérifier la capacité
            if (currentPatients >= totalCapacity)
            {
                Debug.LogWarning($"[PMA] {pmaName} est saturé - Impossible d'admettre {patient.name}");
                return false;
            }
            
            // Assigner à la zone appropriée
            TriageZoneController targetZone = GetZoneForCategory(category);
            if (targetZone == null || targetZone.IsFull)
            {
                Debug.LogWarning($"[PMA] Zone {category} pleine dans {pmaName}");
                return false;
            }
            
            // Déplacer le patient vers la zone
            if (targetZone.AddVictim(patient))
            {
                allPatients.Add(patient);
                currentPatients = allPatients.Count;
                
                string patientId = patient.GetInstanceID().ToString();
                patientAdmissionTimes[patientId] = DateTime.Now;
                
                totalAdmissions++;
                
                OnPatientAdmitted?.Invoke(patient);
                Debug.Log($"[PMA] Patient {patient.name} admis en zone {category} ({currentPatients}/{totalCapacity})");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Évacue un patient du PMA vers un hôpital
        /// </summary>
        public bool EvacuatePatient(GameObject patient)
        {
            if (patient == null) return false;
            
            if (!allPatients.Contains(patient))
            {
                Debug.LogWarning($"[PMA] Patient {patient.name} non trouvé dans {pmaName}");
                return false;
            }
            
            // Retirer des zones
            zoneRouge?.RemoveVictim(patient);
            zoneJaune?.RemoveVictim(patient);
            zoneVerte?.RemoveVictim(patient);
            
            allPatients.Remove(patient);
            currentPatients = allPatients.Count;
            
            string patientId = patient.GetInstanceID().ToString();
            patientAdmissionTimes.Remove(patientId);
            
            totalEvacuations++;
            
            OnPatientEvacuated?.Invoke(patient);
            Debug.Log($"[PMA] Patient {patient.name} évacué de {pmaName}");
            return true;
        }
        
        /// <summary>
        /// Ajoute un patient à la file d'évacuation
        /// </summary>
        public void QueueForEvacuation(GameObject patient)
        {
            if (patient != null && !evacuationQueue.Contains(patient))
            {
                evacuationQueue.Enqueue(patient);
                Debug.Log($"[PMA] Patient {patient.name} ajouté à la file d'évacuation");
            }
        }
        
        private void ProcessEvacuationQueue()
        {
            // Traiter la file d'évacuation (appelé par Update)
            // La logique réelle d'évacuation serait gérée par AmbulanceManager
        }
        
        /// <summary>
        /// Obtient la zone appropriée pour une catégorie de triage
        /// </summary>
        private TriageZoneController GetZoneForCategory(TriageCategory category)
        {
            switch (category)
            {
                case TriageCategory.Red:
                    return zoneRouge;
                case TriageCategory.Yellow:
                    return zoneJaune;
                case TriageCategory.Green:
                    return zoneVerte;
                default:
                    return null;
            }
        }
        
        /// <summary>
        /// Reclasse un patient vers une nouvelle catégorie
        /// </summary>
        public bool ReclassifyPatient(GameObject patient, TriageCategory newCategory)
        {
            if (patient == null || !allPatients.Contains(patient)) return false;
            
            // Retirer de la zone actuelle
            zoneRouge?.RemoveVictim(patient);
            zoneJaune?.RemoveVictim(patient);
            zoneVerte?.RemoveVictim(patient);
            
            // Ajouter à la nouvelle zone
            TriageZoneController newZone = GetZoneForCategory(newCategory);
            if (newZone != null)
            {
                return newZone.AddVictim(patient);
            }
            
            return false;
        }
        
        /// <summary>
        /// Enregistre un décès dans le PMA
        /// </summary>
        public void RegisterDeath(GameObject patient)
        {
            if (patient != null && allPatients.Contains(patient))
            {
                EvacuatePatient(patient);
                totalDeaths++;
                Debug.Log($"[PMA] Décès enregistré: {patient.name}");
            }
        }
        
        /// <summary>
        /// Ajoute du personnel au PMA
        /// </summary>
        public void AddStaff(int doctors = 0, int nurses = 0, int rescuers = 0, int stretchers = 0)
        {
            medecins += doctors;
            infirmiers += nurses;
            secouristes += rescuers;
            brancardiers += stretchers;
            
            // Distribuer le personnel aux zones
            if (zoneRouge != null) zoneRouge.AddMedicalStaff(doctors > 0 ? 1 : 0);
            
            Debug.Log($"[PMA] Personnel ajouté - Médecins: {medecins}, Infirmiers: {infirmiers}, Secouristes: {secouristes}");
        }
        
        /// <summary>
        /// Ajoute de l'équipement au PMA
        /// </summary>
        public void AddEquipment(string equipmentId, int quantity = 1)
        {
            for (int i = 0; i < quantity; i++)
            {
                availableEquipment.Add(equipmentId);
            }
            
            // Mettre à jour les compteurs spécifiques
            if (equipmentId.Contains("O2") || equipmentId.Contains("Oxygen"))
            {
                oxygenBottlesCount += quantity;
            }
            else if (equipmentId.Contains("DSA") || equipmentId.Contains("Defibrillator"))
            {
                defibrillatorCount += quantity;
            }
            else if (equipmentId.Contains("Brancard") || equipmentId.Contains("Stretcher"))
            {
                stretcherCount += quantity;
            }
        }
        
        /// <summary>
        /// Obtient les statistiques complètes du PMA
        /// </summary>
        public PMAStats GetStats()
        {
            var stats = new PMAStats
            {
                PMAId = pmaId,
                PMAName = pmaName,
                Status = status,
                IsOperational = isOperational,
                DeploymentProgress = deploymentProgress,
                
                TotalCapacity = totalCapacity,
                CurrentPatients = currentPatients,
                AvailableSpots = AvailableSpots,
                OccupancyPercentage = OccupancyPercentage,
                
                RedZoneOccupancy = zoneRouge?.CurrentOccupancy ?? 0,
                RedZoneCapacity = redZoneCapacity,
                YellowZoneOccupancy = zoneJaune?.CurrentOccupancy ?? 0,
                YellowZoneCapacity = yellowZoneCapacity,
                GreenZoneOccupancy = zoneVerte?.CurrentOccupancy ?? 0,
                GreenZoneCapacity = greenZoneCapacity,
                
                Medecins = medecins,
                Infirmiers = infirmiers,
                Secouristes = secouristes,
                Brancardiers = brancardiers,
                TotalStaff = medecins + infirmiers + secouristes + brancardiers,
                
                OxygenBottles = oxygenBottlesCount,
                Defibrillators = defibrillatorCount,
                Stretchers = stretcherCount,
                IVKits = ivKitsCount,
                
                TotalAdmissions = totalAdmissions,
                TotalEvacuations = totalEvacuations,
                TotalDeaths = totalDeaths,
                EvacuationQueueLength = evacuationQueue.Count,
                
                Position = transform.position
            };
            
            return stats;
        }
        
        /// <summary>
        /// Obtient le prochain patient à évacuer (priorité: Rouge > Jaune > Vert)
        /// </summary>
        public GameObject GetNextPatientForEvacuation()
        {
            // D'abord vérifier la file d'attente
            if (evacuationQueue.Count > 0)
            {
                return evacuationQueue.Peek();
            }
            
            // Sinon prendre le patient en attente depuis le plus longtemps en zone rouge
            if (zoneRouge != null && zoneRouge.CurrentOccupancy > 0)
            {
                return zoneRouge.GetLongestWaitingVictim();
            }
            
            // Puis zone jaune
            if (zoneJaune != null && zoneJaune.CurrentOccupancy > 0)
            {
                return zoneJaune.GetLongestWaitingVictim();
            }
            
            return null;
        }
        
        /// <summary>
        /// Ferme le PMA
        /// </summary>
        public void ClosePMA()
        {
            if (currentPatients > 0)
            {
                Debug.LogWarning($"[PMA] Impossible de fermer {pmaName} - {currentPatients} patients encore présents");
                return;
            }
            
            SetStatus(PMAStatus.Closed);
            isOperational = false;
            Debug.Log($"[PMA] {pmaName} fermé");
        }
        
        private void OnDrawGizmos()
        {
            // Dessiner le contour du PMA
            Gizmos.color = isOperational ? Color.blue : Color.gray;
            Gizmos.DrawWireCube(transform.position, new Vector3(30, 5, 20));
            
            // Point d'entrée
            if (entryPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(entryPoint.position, 1f);
            }
            
            // Point de sortie
            if (exitPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(exitPoint.position, 1f);
            }
            
            // Zone de chargement ambulances
            if (ambulanceLoadingZone != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(ambulanceLoadingZone.position, new Vector3(10, 1, 5));
            }
        }
    }
    
    /// <summary>
    /// Statistiques du PMA
    /// </summary>
    [Serializable]
    public class PMAStats
    {
        // Identification
        public string PMAId;
        public string PMAName;
        public PMAStatus Status;
        public bool IsOperational;
        public float DeploymentProgress;
        
        // Capacité
        public int TotalCapacity;
        public int CurrentPatients;
        public int AvailableSpots;
        public float OccupancyPercentage;
        
        // Zones
        public int RedZoneOccupancy;
        public int RedZoneCapacity;
        public int YellowZoneOccupancy;
        public int YellowZoneCapacity;
        public int GreenZoneOccupancy;
        public int GreenZoneCapacity;
        
        // Personnel
        public int Medecins;
        public int Infirmiers;
        public int Secouristes;
        public int Brancardiers;
        public int TotalStaff;
        
        // Équipement
        public int OxygenBottles;
        public int Defibrillators;
        public int Stretchers;
        public int IVKits;
        
        // Statistiques
        public int TotalAdmissions;
        public int TotalEvacuations;
        public int TotalDeaths;
        public int EvacuationQueueLength;
        
        // Position
        public Vector3 Position;
    }
}
