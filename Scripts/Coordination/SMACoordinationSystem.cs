using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RASSE.Coordination
{
    /// <summary>
    /// SMACoordinationSystem - Système de Coordination SMA (Bloc 4)
    /// Gère la communication entre les lunettes RA et les systèmes hospitaliers/ambulances
    /// Conforme au diagramme de blocs du rapport
    /// </summary>
    public class SMACoordinationSystem : MonoBehaviour
    {
        public static SMACoordinationSystem Instance { get; private set; }

        [Header("=== RÉFÉRENCES MODULES ===")]
        [SerializeField] private Core.HospitalCoordinationSystem hospitalSystem;
        [SerializeField] private Core.AmbulanceManager ambulanceSystem;
        [SerializeField] private Core.PatientRecordSystem patientRecordSystem;

        [Header("=== CONFIGURATION RÉSEAU ===")]
        [SerializeField] private float syncInterval = 5f;
        [SerializeField] private float connectionTimeout = 10f;
        [SerializeField] private int maxRetryAttempts = 3;

        [Header("=== BASE DE DONNÉES PATIENT (Bloc 41) ===")]
        [SerializeField] private PatientDatabase patientDatabase;

        [Header("=== INFORMATIONS HÔPITAUX (Bloc 42) ===")]
        [SerializeField] private HospitalInformationSystem hospitalInfoSystem;

        [Header("=== ÉTAT ===")]
        [SerializeField] private SMAConnectionState connectionState = SMAConnectionState.Disconnected;
        [SerializeField] private int activeCoordinations;
        [SerializeField] private int pendingTransfers;

        [Header("=== ÉVÉNEMENTS ===")]
        public UnityEvent<CoordinationEvent> OnCoordinationEvent;
        public UnityEvent<string> OnHospitalCapacityUpdate;
        public UnityEvent<string> OnAmbulanceStatusUpdate;
        public UnityEvent OnConnectionEstablished;
        public UnityEvent OnConnectionLost;

        // Files d'attente de messages
        private Queue<CoordinationMessage> outgoingMessages = new Queue<CoordinationMessage>();
        private Queue<CoordinationMessage> incomingMessages = new Queue<CoordinationMessage>();
        private Coroutine syncCoroutine;

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

            InitializeSubsystems();
        }

        private void Start()
        {
            // Démarrer la synchronisation
            syncCoroutine = StartCoroutine(SynchronizationLoop());

            // S'abonner aux événements du système de batterie/connexion
            if (Hardware.BatteryManager.Instance != null)
            {
                Hardware.BatteryManager.Instance.OnConnectionLost.AddListener(HandleConnectionLost);
                Hardware.BatteryManager.Instance.OnConnectionRestored.AddListener(HandleConnectionRestored);
            }
        }

        /// <summary>
        /// Initialise les sous-systèmes
        /// </summary>
        private void InitializeSubsystems()
        {
            // Base de données patient (Bloc 41)
            patientDatabase = new PatientDatabase();

            // Informations hôpitaux (Bloc 42)
            hospitalInfoSystem = new HospitalInformationSystem();

            // Charger les données initiales
            hospitalInfoSystem.LoadHospitalData();

            Debug.Log("[SMA] Système de coordination initialisé");
        }

        /// <summary>
        /// Boucle de synchronisation avec les systèmes externes
        /// </summary>
        private IEnumerator SynchronizationLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(syncInterval);

                if (connectionState == SMAConnectionState.Connected)
                {
                    // Envoyer les messages en attente
                    ProcessOutgoingMessages();

                    // Traiter les messages reçus
                    ProcessIncomingMessages();

                    // Synchroniser les capacités hospitalières
                    SyncHospitalCapacities();

                    // Synchroniser les statuts des ambulances
                    SyncAmbulanceStatuses();
                }
            }
        }

        #region Coordination des Évacuations

        /// <summary>
        /// Coordonne l'évacuation d'une victime (flux principal du diagramme de séquence)
        /// </summary>
        public IEnumerator CoordinateEvacuation(Core.VictimController victim)
        {
            if (victim == null) yield break;

            activeCoordinations++;
            var coordination = new EvacuationCoordination
            {
                coordinationId = Guid.NewGuid().ToString(),
                patientId = victim.PatientId,
                startTime = DateTime.Now,
                status = CoordinationStatus.Initiated
            };

            Debug.Log($"[SMA] Coordination évacuation démarrée pour {victim.PatientId}");

            // Étape 1: Enregistrer le patient dans la base de données
            patientDatabase.RegisterPatient(victim);
            coordination.status = CoordinationStatus.PatientRegistered;
            yield return new WaitForSeconds(0.5f);

            // Étape 2: Déterminer le meilleur hôpital selon START et capacités
            Core.HospitalData selectedHospital = DetermineOptimalHospital(victim);
            if (selectedHospital == null)
            {
                coordination.status = CoordinationStatus.NoHospitalAvailable;
                Debug.LogWarning("[SMA] Aucun hôpital disponible!");
                yield break;
            }
            coordination.assignedHospital = selectedHospital.hospitalId;
            yield return new WaitForSeconds(0.5f);

            // Étape 3: Réserver un lit
            bool bedReserved = hospitalInfoSystem.ReserveBed(selectedHospital.hospitalId, victim);
            if (!bedReserved)
            {
                coordination.status = CoordinationStatus.ReservationFailed;
                Debug.LogWarning("[SMA] Échec de réservation de lit");
                yield break;
            }
            coordination.status = CoordinationStatus.BedReserved;
            yield return new WaitForSeconds(0.5f);

            // Étape 4: Affecter une ambulance
            Core.AmbulanceController ambulance = AssignAmbulance(victim, selectedHospital);
            if (ambulance == null)
            {
                coordination.status = CoordinationStatus.NoAmbulanceAvailable;
                Debug.LogWarning("[SMA] Aucune ambulance disponible!");
                yield break;
            }
            coordination.assignedAmbulance = ambulance.AmbulanceId;
            coordination.status = CoordinationStatus.AmbulanceAssigned;
            yield return new WaitForSeconds(0.5f);

            // Étape 5: Envoyer le préavis à l'hôpital
            SendHospitalNotification(selectedHospital, victim, ambulance);
            coordination.status = CoordinationStatus.HospitalNotified;

            // Étape 6: Mettre à jour le dossier patient
            patientDatabase.UpdatePatientRecord(victim.PatientId, new PatientRecordUpdate
            {
                assignedHospital = selectedHospital.hospitalName,
                assignedAmbulance = ambulance.AmbulanceId,
                evacuationTime = DateTime.Now
            });

            coordination.status = CoordinationStatus.InProgress;
            coordination.endTime = DateTime.Now;

            // Notifier
            OnCoordinationEvent?.Invoke(new CoordinationEvent
            {
                type = CoordinationEventType.EvacuationStarted,
                coordinationId = coordination.coordinationId,
                patientId = victim.PatientId,
                message = $"Évacuation vers {selectedHospital.hospitalName} avec {ambulance.AmbulanceId}"
            });

            activeCoordinations--;
            pendingTransfers++;

            Debug.Log($"[SMA] Évacuation coordonnée: {victim.PatientId} → {selectedHospital.hospitalName}");
        }

        /// <summary>
        /// Détermine l'hôpital optimal selon le protocole START et les capacités
        /// Correspond au diagramme de séquence "Choix de l'action à réaliser"
        /// </summary>
        private Core.HospitalData DetermineOptimalHospital(Core.VictimController victim)
        {
            if (hospitalSystem == null) return null;

            // Obtenir les hôpitaux disponibles
            var hospitals = hospitalInfoSystem.GetAvailableHospitals();
            if (hospitals.Count == 0) return null;

            // Filtrer selon la catégorie START
            Core.StartCategory category = victim.TriageCategory;
            Core.InjuryType injury = victim.primaryInjury;

            // Algorithme de sélection
            Core.HospitalData best = null;
            float bestScore = float.MinValue;

            foreach (var hospital in hospitals)
            {
                float score = CalculateHospitalScore(hospital, category, injury);
                
                if (score > bestScore)
                {
                    bestScore = score;
                    best = hospital;
                }
            }

            return best;
        }

        /// <summary>
        /// Calcule le score d'un hôpital pour un cas donné
        /// </summary>
        private float CalculateHospitalScore(Core.HospitalData hospital, Core.StartCategory category, Core.InjuryType injury)
        {
            float score = 0f;

            // Score de base selon la capacité
            float capacityRatio = (float)hospital.availableBeds / hospital.totalBeds;
            score += capacityRatio * 30f;

            // Score de distance (inversé - plus proche = meilleur)
            score += (50f - hospital.distanceKm) / 50f * 25f;

            // Bonus pour les spécialités requises
            if (category == Core.StartCategory.Red)
            {
                if (hospital.isTraumaCenter) score += 20f;
            }

            // Bonus pour les spécialités spécifiques aux blessures
            if (injury == Core.InjuryType.Burn && (hospital.specialties & Core.HospitalSpecialty.BurnUnit) != 0)
                score += 15f;
            
            if (injury == Core.InjuryType.Concussion && (hospital.specialties & Core.HospitalSpecialty.Neurology) != 0)
                score += 15f;

            // Pénalité pour temps d'attente élevé
            score -= hospital.averageWaitTime * 0.5f;

            return score;
        }

        /// <summary>
        /// Affecte une ambulance appropriée
        /// </summary>
        private Core.AmbulanceController AssignAmbulance(Core.VictimController victim, Core.HospitalData hospital)
        {
            if (ambulanceSystem == null) return null;

            return ambulanceSystem.AssignAmbulance(victim, hospital);
        }

        /// <summary>
        /// Envoie une notification de préavis à l'hôpital
        /// </summary>
        private void SendHospitalNotification(Core.HospitalData hospital, Core.VictimController victim, Core.AmbulanceController ambulance)
        {
            var notification = new HospitalNotification
            {
                hospitalId = hospital.hospitalId,
                patientId = victim.PatientId,
                triageCategory = victim.TriageCategory,
                primaryInjury = victim.primaryInjury,
                estimatedArrival = ambulance.GetETA(),
                vitalSigns = victim.VitalSigns,
                ambulanceId = ambulance.AmbulanceId,
                timestamp = DateTime.Now
            };

            // Ajouter à la file d'envoi
            outgoingMessages.Enqueue(new CoordinationMessage
            {
                type = MessageType.HospitalNotification,
                payload = JsonUtility.ToJson(notification),
                targetId = hospital.hospitalId,
                priority = victim.TriageCategory == Core.StartCategory.Red ? MessagePriority.Critical : MessagePriority.High
            });

            Debug.Log($"[SMA] Préavis envoyé à {hospital.hospitalName}: ETA {ambulance.GetETA():F0} min");
        }

        #endregion

        #region Synchronisation

        /// <summary>
        /// Traite les messages sortants
        /// </summary>
        private void ProcessOutgoingMessages()
        {
            int processedCount = 0;
            while (outgoingMessages.Count > 0 && processedCount < 10)
            {
                var message = outgoingMessages.Dequeue();
                // Simuler l'envoi
                Debug.Log($"[SMA] Message envoyé: {message.type} → {message.targetId}");
                processedCount++;
            }
        }

        /// <summary>
        /// Traite les messages entrants
        /// </summary>
        private void ProcessIncomingMessages()
        {
            while (incomingMessages.Count > 0)
            {
                var message = incomingMessages.Dequeue();
                HandleIncomingMessage(message);
            }
        }

        /// <summary>
        /// Gère un message entrant
        /// </summary>
        private void HandleIncomingMessage(CoordinationMessage message)
        {
            switch (message.type)
            {
                case MessageType.HospitalCapacityUpdate:
                    hospitalInfoSystem.UpdateCapacity(message.targetId, message.payload);
                    OnHospitalCapacityUpdate?.Invoke(message.targetId);
                    break;

                case MessageType.AmbulanceStatusUpdate:
                    OnAmbulanceStatusUpdate?.Invoke(message.targetId);
                    break;

                case MessageType.PatientArrivalConfirmation:
                    HandlePatientArrival(message.payload);
                    break;
            }
        }

        /// <summary>
        /// Synchronise les capacités hospitalières
        /// </summary>
        private void SyncHospitalCapacities()
        {
            hospitalInfoSystem.RefreshCapacities();
        }

        /// <summary>
        /// Synchronise les statuts des ambulances
        /// </summary>
        private void SyncAmbulanceStatuses()
        {
            // Mise à jour des statuts depuis l'AmbulanceManager
            if (ambulanceSystem != null)
            {
                var summary = ambulanceSystem.GetFleetSummary();
                Debug.Log($"[SMA] {summary}");
            }
        }

        /// <summary>
        /// Gère la confirmation d'arrivée d'un patient
        /// </summary>
        private void HandlePatientArrival(string payload)
        {
            pendingTransfers--;
            Debug.Log($"[SMA] Patient arrivé à l'hôpital: {payload}");
        }

        #endregion

        #region Gestion de Connexion

        /// <summary>
        /// Établit la connexion au système SMA
        /// </summary>
        public void Connect()
        {
            StartCoroutine(EstablishConnection());
        }

        private IEnumerator EstablishConnection()
        {
            connectionState = SMAConnectionState.Connecting;
            Debug.Log("[SMA] Connexion en cours...");

            // Simuler la connexion
            yield return new WaitForSeconds(2f);

            connectionState = SMAConnectionState.Connected;
            OnConnectionEstablished?.Invoke();
            Debug.Log("[SMA] Connexion établie");
        }

        private void HandleConnectionLost()
        {
            connectionState = SMAConnectionState.Disconnected;
            OnConnectionLost?.Invoke();
            Debug.LogWarning("[SMA] Connexion perdue - Mode dégradé actif");
        }

        private void HandleConnectionRestored()
        {
            Connect();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Obtient le statut de la coordination
        /// </summary>
        public string GetCoordinationStatus()
        {
            return $"=== SYSTÈME SMA (Bloc 4) ===\n" +
                   $"État connexion: {connectionState}\n" +
                   $"Coordinations actives: {activeCoordinations}\n" +
                   $"Transferts en cours: {pendingTransfers}\n" +
                   $"Messages en attente: {outgoingMessages.Count}\n" +
                   $"\n--- Base Données Patient (Bloc 41) ---\n" +
                   patientDatabase.GetStatus() +
                   $"\n--- Info Hôpitaux (Bloc 42) ---\n" +
                   hospitalInfoSystem.GetStatus();
        }

        /// <summary>
        /// Demande de retour à domicile (cas VERT uniquement)
        /// </summary>
        public bool RequestHomeReturn(Core.VictimController victim)
        {
            if (victim == null || victim.TriageCategory != Core.StartCategory.Green)
            {
                Debug.LogWarning("[SMA] Retour domicile refusé - Catégorie non VERT");
                return false;
            }

            patientDatabase.RegisterHomeReturn(victim);
            
            OnCoordinationEvent?.Invoke(new CoordinationEvent
            {
                type = CoordinationEventType.HomeReturnApproved,
                patientId = victim.PatientId,
                message = "Retour à domicile autorisé"
            });

            return true;
        }

        #endregion
    }

    #region Subsystems

    /// <summary>
    /// Base de données patient locale (Bloc 41)
    /// </summary>
    [Serializable]
    public class PatientDatabase
    {
        private Dictionary<string, PatientEntry> patients = new Dictionary<string, PatientEntry>();
        private int totalRegistered;
        private int totalEvacuated;
        private int totalHomeReturns;

        public void RegisterPatient(Core.VictimController victim)
        {
            var entry = new PatientEntry
            {
                patientId = victim.PatientId,
                firstName = victim.firstName,
                lastName = victim.lastName,
                age = victim.age,
                triageCategory = victim.TriageCategory,
                registrationTime = DateTime.Now,
                status = PatientStatus.Registered
            };

            patients[victim.PatientId] = entry;
            totalRegistered++;
        }

        public void UpdatePatientRecord(string patientId, PatientRecordUpdate update)
        {
            if (patients.TryGetValue(patientId, out PatientEntry entry))
            {
                entry.assignedHospital = update.assignedHospital;
                entry.assignedAmbulance = update.assignedAmbulance;
                entry.evacuationTime = update.evacuationTime;
                entry.status = PatientStatus.BeingEvacuated;
            }
        }

        public void RegisterHomeReturn(Core.VictimController victim)
        {
            if (patients.TryGetValue(victim.PatientId, out PatientEntry entry))
            {
                entry.status = PatientStatus.ReturnedHome;
                totalHomeReturns++;
            }
        }

        public string GetStatus()
        {
            return $"Patients enregistrés: {totalRegistered}\n" +
                   $"Évacués: {totalEvacuated}\n" +
                   $"Retours domicile: {totalHomeReturns}";
        }
    }

    /// <summary>
    /// Système d'information hospitalière (Bloc 42)
    /// </summary>
    [Serializable]
    public class HospitalInformationSystem
    {
        private List<Core.HospitalData> hospitals = new List<Core.HospitalData>();
        private Dictionary<string, int> reservedBeds = new Dictionary<string, int>();

        public void LoadHospitalData()
        {
            // Charger depuis le HospitalCoordinationSystem
            if (Core.HospitalCoordinationSystem.Instance != null)
            {
                hospitals = Core.HospitalCoordinationSystem.Instance.GetAvailableHospitals();
            }
        }

        public List<Core.HospitalData> GetAvailableHospitals()
        {
            return hospitals.FindAll(h => h.availableBeds > reservedBeds.GetValueOrDefault(h.hospitalId, 0));
        }

        public bool ReserveBed(string hospitalId, Core.VictimController victim)
        {
            var hospital = hospitals.Find(h => h.hospitalId == hospitalId);
            if (hospital == null) return false;

            int reserved = reservedBeds.GetValueOrDefault(hospitalId, 0);
            if (hospital.availableBeds <= reserved) return false;

            reservedBeds[hospitalId] = reserved + 1;
            return true;
        }

        public void UpdateCapacity(string hospitalId, string payload)
        {
            // Mise à jour depuis le réseau
        }

        public void RefreshCapacities()
        {
            // Rafraîchir les données
            LoadHospitalData();
        }

        public string GetStatus()
        {
            int totalBeds = 0, availableBeds = 0;
            foreach (var h in hospitals)
            {
                totalBeds += h.totalBeds;
                availableBeds += h.availableBeds - reservedBeds.GetValueOrDefault(h.hospitalId, 0);
            }
            return $"Hôpitaux: {hospitals.Count}\n" +
                   $"Lits disponibles: {availableBeds}/{totalBeds}";
        }
    }

    #endregion

    #region Data Structures

    public enum SMAConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Degraded
    }

    public enum CoordinationStatus
    {
        Initiated,
        PatientRegistered,
        BedReserved,
        AmbulanceAssigned,
        HospitalNotified,
        InProgress,
        Completed,
        NoHospitalAvailable,
        NoAmbulanceAvailable,
        ReservationFailed
    }

    public enum PatientStatus
    {
        Registered,
        BeingEvacuated,
        AtHospital,
        ReturnedHome,
        Deceased
    }

    public enum MessageType
    {
        HospitalNotification,
        HospitalCapacityUpdate,
        AmbulanceStatusUpdate,
        PatientArrivalConfirmation,
        CoordinationRequest
    }

    public enum MessagePriority
    {
        Low,
        Normal,
        High,
        Critical
    }

    public enum CoordinationEventType
    {
        EvacuationStarted,
        EvacuationCompleted,
        HomeReturnApproved,
        AmbulanceAssigned,
        HospitalAssigned
    }

    [Serializable]
    public class EvacuationCoordination
    {
        public string coordinationId;
        public string patientId;
        public string assignedHospital;
        public string assignedAmbulance;
        public DateTime startTime;
        public DateTime endTime;
        public CoordinationStatus status;
    }

    [Serializable]
    public class PatientEntry
    {
        public string patientId;
        public string firstName;
        public string lastName;
        public int age;
        public Core.StartCategory triageCategory;
        public DateTime registrationTime;
        public DateTime evacuationTime;
        public string assignedHospital;
        public string assignedAmbulance;
        public PatientStatus status;
    }

    [Serializable]
    public class PatientRecordUpdate
    {
        public string assignedHospital;
        public string assignedAmbulance;
        public DateTime evacuationTime;
    }

    [Serializable]
    public class HospitalNotification
    {
        public string hospitalId;
        public string patientId;
        public Core.StartCategory triageCategory;
        public Core.InjuryType primaryInjury;
        public float estimatedArrival;
        public Core.VitalSigns vitalSigns;
        public string ambulanceId;
        public DateTime timestamp;
    }

    [Serializable]
    public class CoordinationMessage
    {
        public MessageType type;
        public string payload;
        public string targetId;
        public MessagePriority priority;
        public DateTime timestamp;
    }

    [Serializable]
    public class CoordinationEvent
    {
        public CoordinationEventType type;
        public string coordinationId;
        public string patientId;
        public string message;
    }

    #endregion
}
