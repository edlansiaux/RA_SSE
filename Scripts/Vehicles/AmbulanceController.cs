using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace RASSE.Vehicles
{
    /// <summary>
    /// États possibles d'une ambulance
    /// </summary>
    public enum AmbulanceState
    {
        Available,          // Disponible à la base
        Dispatched,         // En route vers le site
        OnScene,            // Sur le site d'intervention
        Loading,            // Chargement d'un patient
        Transporting,       // Transport vers l'hôpital
        AtHospital,         // À l'hôpital (déchargement)
        Returning,          // Retour vers le site ou base
        OutOfService        // Hors service
    }

    /// <summary>
    /// Types d'ambulances françaises
    /// </summary>
    public enum AmbulanceType
    {
        VSAV,       // Véhicule de Secours et d'Assistance aux Victimes
        ASSU,       // Ambulance de Secours et de Soins d'Urgence
        UMH,        // Unité Mobile Hospitalière (SMUR)
        VLM,        // Véhicule Léger Médicalisé
        AR          // Ambulance de Réanimation
    }

    /// <summary>
    /// Contrôleur d'ambulance pour la gestion des évacuations.
    /// Gère le cycle complet: dispatch, chargement, transport, déchargement.
    /// Conforme aux protocoles français de médecine d'urgence.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class AmbulanceController : MonoBehaviour
    {
        [Header("Identification")]
        [SerializeField] private string ambulanceId;
        [SerializeField] private string callSign = "VSAV 01";
        [SerializeField] private AmbulanceType ambulanceType = AmbulanceType.VSAV;
        
        [Header("État")]
        [SerializeField] private AmbulanceState currentState = AmbulanceState.Available;
        [SerializeField] private bool isAvailable = true;
        
        [Header("Capacité")]
        [SerializeField] private int maxPatientCapacity = 2;
        [SerializeField] private int currentPatientCount = 0;
        [SerializeField] private bool canTransportRedCategory = true;
        [SerializeField] private bool hasMedicalEquipment = true;
        
        [Header("Équipage")]
        [SerializeField] private int crewSize = 3;
        [SerializeField] private bool hasMedic = false;
        [SerializeField] private bool hasDoctor = false;
        
        [Header("Navigation")]
        [SerializeField] private float normalSpeed = 15f;
        [SerializeField] private float emergencySpeed = 25f;
        [SerializeField] private float loadingTime = 60f;
        [SerializeField] private float unloadingTime = 30f;
        
        [Header("Points de référence")]
        [SerializeField] private Transform loadingPoint;
        [SerializeField] private Transform[] patientPositions;
        
        [Header("Visuel")]
        [SerializeField] private GameObject sirenLights;
        [SerializeField] private AudioSource sirenAudio;
        [SerializeField] private Light[] emergencyLights;
        
        [Header("Événements")]
        public UnityEvent<AmbulanceState> OnStateChanged;
        public UnityEvent<GameObject> OnPatientLoaded;
        public UnityEvent<GameObject> OnPatientUnloaded;
        public UnityEvent OnArrivalAtDestination;
        public UnityEvent OnMissionComplete;
        
        // Composants
        private NavMeshAgent navAgent;
        
        // État interne
        private List<GameObject> patients = new List<GameObject>();
        private Transform currentDestination;
        private Transform assignedHospital;
        private Transform homeBase;
        private Mission currentMission;
        private float missionStartTime;
        
        // Statistiques
        private int totalMissions = 0;
        private int totalPatientsTransported = 0;
        private float totalDistanceTraveled = 0f;
        
        // Propriétés publiques
        public string AmbulanceId => ambulanceId;
        public string CallSign => callSign;
        public AmbulanceType Type => ambulanceType;
        public AmbulanceState State => currentState;
        public bool IsAvailable => isAvailable && currentState == AmbulanceState.Available;
        public int AvailableCapacity => maxPatientCapacity - currentPatientCount;
        public bool CanAcceptPatient => currentPatientCount < maxPatientCapacity;
        public IReadOnlyList<GameObject> Patients => patients.AsReadOnly();
        
        private void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
            
            if (string.IsNullOrEmpty(ambulanceId))
            {
                ambulanceId = $"AMB_{ambulanceType}_{Guid.NewGuid().ToString().Substring(0, 8)}";
            }
            
            ConfigureNavAgent();
        }
        
        private void Start()
        {
            SetState(AmbulanceState.Available);
            
            if (sirenLights != null)
            {
                sirenLights.SetActive(false);
            }
        }
        
        private void Update()
        {
            UpdateState();
            UpdateEmergencyLights();
        }
        
        private void ConfigureNavAgent()
        {
            navAgent.speed = normalSpeed;
            navAgent.angularSpeed = 120f;
            navAgent.acceleration = 8f;
            navAgent.stoppingDistance = 2f;
            navAgent.autoBraking = true;
        }
        
        private void UpdateState()
        {
            switch (currentState)
            {
                case AmbulanceState.Dispatched:
                case AmbulanceState.Transporting:
                case AmbulanceState.Returning:
                    CheckArrival();
                    TrackDistance();
                    break;
            }
        }
        
        private void CheckArrival()
        {
            if (navAgent.remainingDistance <= navAgent.stoppingDistance && !navAgent.pathPending)
            {
                OnArrived();
            }
        }
        
        private void OnArrived()
        {
            OnArrivalAtDestination?.Invoke();
            
            switch (currentState)
            {
                case AmbulanceState.Dispatched:
                    SetState(AmbulanceState.OnScene);
                    break;
                    
                case AmbulanceState.Transporting:
                    SetState(AmbulanceState.AtHospital);
                    StartCoroutine(UnloadPatientsRoutine());
                    break;
                    
                case AmbulanceState.Returning:
                    if (currentMission != null && currentMission.ReturnToScene)
                    {
                        SetState(AmbulanceState.OnScene);
                    }
                    else
                    {
                        SetState(AmbulanceState.Available);
                        CompleteMission();
                    }
                    break;
            }
        }
        
        private void TrackDistance()
        {
            if (navAgent.velocity.magnitude > 0.1f)
            {
                totalDistanceTraveled += navAgent.velocity.magnitude * Time.deltaTime;
            }
        }
        
        /// <summary>
        /// Dispatche l'ambulance vers une destination
        /// </summary>
        public bool Dispatch(Transform destination, Mission mission = null)
        {
            if (!IsAvailable && currentState != AmbulanceState.OnScene)
            {
                Debug.LogWarning($"[Ambulance] {callSign} n'est pas disponible pour dispatch");
                return false;
            }
            
            currentDestination = destination;
            currentMission = mission;
            missionStartTime = Time.time;
            
            navAgent.speed = emergencySpeed;
            navAgent.SetDestination(destination.position);
            
            SetState(AmbulanceState.Dispatched);
            ActivateSiren(true);
            
            Debug.Log($"[Ambulance] {callSign} dispatché vers {destination.name}");
            return true;
        }
        
        /// <summary>
        /// Charge un patient dans l'ambulance
        /// </summary>
        public bool LoadPatient(GameObject patient)
        {
            if (!CanAcceptPatient)
            {
                Debug.LogWarning($"[Ambulance] {callSign} est pleine");
                return false;
            }
            
            if (currentState != AmbulanceState.OnScene && currentState != AmbulanceState.Loading)
            {
                Debug.LogWarning($"[Ambulance] {callSign} n'est pas en position de chargement");
                return false;
            }
            
            SetState(AmbulanceState.Loading);
            StartCoroutine(LoadPatientRoutine(patient));
            return true;
        }
        
        private IEnumerator LoadPatientRoutine(GameObject patient)
        {
            Debug.Log($"[Ambulance] {callSign} - Chargement de {patient.name}...");
            
            yield return new WaitForSeconds(loadingTime);
            
            patients.Add(patient);
            currentPatientCount = patients.Count;
            
            // Positionner le patient dans l'ambulance
            if (patientPositions != null && currentPatientCount <= patientPositions.Length)
            {
                patient.transform.SetParent(patientPositions[currentPatientCount - 1]);
                patient.transform.localPosition = Vector3.zero;
                patient.SetActive(false); // Cacher le patient pendant le transport
            }
            
            totalPatientsTransported++;
            OnPatientLoaded?.Invoke(patient);
            
            Debug.Log($"[Ambulance] {callSign} - {patient.name} chargé ({currentPatientCount}/{maxPatientCapacity})");
            
            // Retourner à l'état OnScene pour permettre d'autres chargements
            SetState(AmbulanceState.OnScene);
        }
        
        /// <summary>
        /// Démarre le transport vers l'hôpital
        /// </summary>
        public bool StartTransport(Transform hospital)
        {
            if (currentPatientCount == 0)
            {
                Debug.LogWarning($"[Ambulance] {callSign} - Aucun patient à transporter");
                return false;
            }
            
            assignedHospital = hospital;
            navAgent.speed = emergencySpeed;
            navAgent.SetDestination(hospital.position);
            
            SetState(AmbulanceState.Transporting);
            ActivateSiren(true);
            
            Debug.Log($"[Ambulance] {callSign} - Transport vers {hospital.name} ({currentPatientCount} patients)");
            return true;
        }
        
        private IEnumerator UnloadPatientsRoutine()
        {
            Debug.Log($"[Ambulance] {callSign} - Déchargement des patients...");
            
            List<GameObject> patientsToUnload = new List<GameObject>(patients);
            
            foreach (var patient in patientsToUnload)
            {
                yield return new WaitForSeconds(unloadingTime);
                
                patients.Remove(patient);
                currentPatientCount = patients.Count;
                
                if (patient != null)
                {
                    patient.transform.SetParent(null);
                    patient.SetActive(true);
                    // Positionner près de l'hôpital
                    patient.transform.position = assignedHospital.position + Vector3.right * 2f;
                }
                
                OnPatientUnloaded?.Invoke(patient);
                Debug.Log($"[Ambulance] {callSign} - Patient déchargé");
            }
            
            // Retour
            if (currentMission != null && currentMission.ReturnToScene && currentMission.SceneLocation != null)
            {
                navAgent.SetDestination(currentMission.SceneLocation.position);
                SetState(AmbulanceState.Returning);
            }
            else if (homeBase != null)
            {
                navAgent.SetDestination(homeBase.position);
                SetState(AmbulanceState.Returning);
            }
            else
            {
                SetState(AmbulanceState.Available);
                CompleteMission();
            }
            
            ActivateSiren(false);
        }
        
        /// <summary>
        /// Définit la base de l'ambulance
        /// </summary>
        public void SetHomeBase(Transform baseLocation)
        {
            homeBase = baseLocation;
        }
        
        /// <summary>
        /// Retourne l'ambulance à sa base
        /// </summary>
        public void ReturnToBase()
        {
            if (homeBase != null)
            {
                navAgent.speed = normalSpeed;
                navAgent.SetDestination(homeBase.position);
                SetState(AmbulanceState.Returning);
                ActivateSiren(false);
            }
        }
        
        /// <summary>
        /// Met l'ambulance hors service
        /// </summary>
        public void SetOutOfService(bool outOfService)
        {
            if (outOfService)
            {
                SetState(AmbulanceState.OutOfService);
                isAvailable = false;
            }
            else
            {
                SetState(AmbulanceState.Available);
                isAvailable = true;
            }
        }
        
        private void SetState(AmbulanceState newState)
        {
            if (currentState != newState)
            {
                AmbulanceState previousState = currentState;
                currentState = newState;
                isAvailable = (newState == AmbulanceState.Available);
                
                OnStateChanged?.Invoke(newState);
                Debug.Log($"[Ambulance] {callSign} - État: {previousState} -> {newState}");
            }
        }
        
        private void CompleteMission()
        {
            if (currentMission != null)
            {
                totalMissions++;
                float missionDuration = Time.time - missionStartTime;
                Debug.Log($"[Ambulance] {callSign} - Mission terminée (durée: {missionDuration:F1}s)");
                currentMission = null;
            }
            
            OnMissionComplete?.Invoke();
        }
        
        private void ActivateSiren(bool active)
        {
            if (sirenLights != null)
            {
                sirenLights.SetActive(active);
            }
            
            if (sirenAudio != null)
            {
                if (active && !sirenAudio.isPlaying)
                {
                    sirenAudio.Play();
                }
                else if (!active && sirenAudio.isPlaying)
                {
                    sirenAudio.Stop();
                }
            }
        }
        
        private void UpdateEmergencyLights()
        {
            if (emergencyLights == null || emergencyLights.Length == 0) return;
            if (currentState == AmbulanceState.Available || currentState == AmbulanceState.OutOfService) return;
            
            // Effet de gyrophare
            float time = Time.time * 5f;
            for (int i = 0; i < emergencyLights.Length; i++)
            {
                if (emergencyLights[i] != null)
                {
                    float phase = (time + i * Mathf.PI) % (2 * Mathf.PI);
                    emergencyLights[i].intensity = Mathf.Abs(Mathf.Sin(phase)) * 2f;
                }
            }
        }
        
        /// <summary>
        /// Obtient les statistiques de l'ambulance
        /// </summary>
        public AmbulanceStats GetStats()
        {
            return new AmbulanceStats
            {
                AmbulanceId = ambulanceId,
                CallSign = callSign,
                Type = ambulanceType,
                State = currentState,
                IsAvailable = IsAvailable,
                CurrentPatients = currentPatientCount,
                MaxCapacity = maxPatientCapacity,
                TotalMissions = totalMissions,
                TotalPatientsTransported = totalPatientsTransported,
                TotalDistanceKm = totalDistanceTraveled / 1000f,
                HasMedic = hasMedic,
                HasDoctor = hasDoctor,
                CrewSize = crewSize,
                Position = transform.position
            };
        }
        
        private void OnDrawGizmosSelected()
        {
            // Destination actuelle
            if (currentDestination != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, currentDestination.position);
                Gizmos.DrawWireSphere(currentDestination.position, 1f);
            }
            
            // Point de chargement
            if (loadingPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(loadingPoint.position, new Vector3(2, 0.1f, 3));
            }
        }
    }
    
    /// <summary>
    /// Mission d'ambulance
    /// </summary>
    [Serializable]
    public class Mission
    {
        public string MissionId;
        public Transform SceneLocation;
        public Transform Hospital;
        public List<GameObject> TargetPatients;
        public bool ReturnToScene;
        public DateTime StartTime;
        public MissionPriority Priority;
    }
    
    public enum MissionPriority
    {
        Low,
        Normal,
        High,
        Critical
    }
    
    /// <summary>
    /// Statistiques d'ambulance
    /// </summary>
    [Serializable]
    public class AmbulanceStats
    {
        public string AmbulanceId;
        public string CallSign;
        public AmbulanceType Type;
        public AmbulanceState State;
        public bool IsAvailable;
        public int CurrentPatients;
        public int MaxCapacity;
        public int TotalMissions;
        public int TotalPatientsTransported;
        public float TotalDistanceKm;
        public bool HasMedic;
        public bool HasDoctor;
        public int CrewSize;
        public Vector3 Position;
    }
}
