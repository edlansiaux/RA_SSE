using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RASSE.Hardware
{
    /// <summary>
    /// BatteryManager - Gestion de l'autonomie des lunettes RA (Bloc 11)
    /// Conformité NFR-AVA: Autonomie ≥8h + mode offline
    /// </summary>
    public class BatteryManager : MonoBehaviour
    {
        public static BatteryManager Instance { get; private set; }

        [Header("=== CONFIGURATION BATTERIE ===")]
        [Tooltip("Capacité totale en mAh")]
        [SerializeField] private float batteryCapacity = 5000f; // mAh typique pour lunettes RA

        [Tooltip("Autonomie cible en heures (NFR-AVA: ≥8h)")]
        [SerializeField] private float targetAutonomy = 8f;

        [Tooltip("Niveau actuel de batterie (0-100%)")]
        [Range(0f, 100f)]
        [SerializeField] private float currentBatteryLevel = 100f;

        [Header("=== CONSOMMATION ===")]
        [Tooltip("Consommation de base (mA)")]
        [SerializeField] private float baseConsumption = 400f;

        [SerializeField] private float cameraRGBConsumption = 150f;
        [SerializeField] private float cameraThermalConsumption = 200f;
        [SerializeField] private float displayConsumption = 250f;
        [SerializeField] private float processingConsumption = 300f;
        [SerializeField] private float networkConsumption = 100f;
        [SerializeField] private float gpsConsumption = 50f;

        [Header("=== SEUILS D'ALERTE ===")]
        [SerializeField] private float warningThreshold = 30f;  // %
        [SerializeField] private float criticalThreshold = 15f; // %
        [SerializeField] private float shutdownThreshold = 5f;  // %

        [Header("=== MODE ÉCONOMIE D'ÉNERGIE ===")]
        [SerializeField] private bool powerSavingMode = false;
        [SerializeField] private float powerSavingReduction = 0.6f; // 40% de réduction

        [Header("=== ÉTAT CONNEXION ===")]
        [SerializeField] private ConnectionState connectionState = ConnectionState.Connected;
        [SerializeField] private float signalStrength = 100f;

        [Header("=== ÉVÉNEMENTS ===")]
        public UnityEvent<float> OnBatteryLevelChanged;
        public UnityEvent OnBatteryWarning;
        public UnityEvent OnBatteryCritical;
        public UnityEvent OnBatteryShutdown;
        public UnityEvent OnPowerSavingEnabled;
        public UnityEvent OnPowerSavingDisabled;
        public UnityEvent OnConnectionLost;
        public UnityEvent OnConnectionRestored;
        public UnityEvent OnOfflineModeActivated;

        // État des composants
        private Dictionary<string, bool> activeComponents = new Dictionary<string, bool>();
        private float lastUpdateTime;
        private bool isCharging = false;
        private float chargeRate = 2000f; // mA (charge rapide)

        // Propriétés publiques
        public float BatteryLevel => currentBatteryLevel;
        public float RemainingTime => CalculateRemainingTime();
        public bool IsPowerSavingMode => powerSavingMode;
        public bool IsOfflineMode => connectionState == ConnectionState.Offline;
        public ConnectionState CurrentConnectionState => connectionState;
        public float SignalStrength => signalStrength;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeComponents();
        }

        private void Start()
        {
            lastUpdateTime = Time.time;
            StartCoroutine(BatteryUpdateLoop());
            StartCoroutine(ConnectionMonitorLoop());
        }

        /// <summary>
        /// Initialise l'état des composants
        /// </summary>
        private void InitializeComponents()
        {
            activeComponents["CameraRGB"] = true;
            activeComponents["CameraThermal"] = true;
            activeComponents["Display"] = true;
            activeComponents["Processing"] = true;
            activeComponents["Network"] = true;
            activeComponents["GPS"] = true;
        }

        /// <summary>
        /// Boucle de mise à jour de la batterie
        /// </summary>
        private IEnumerator BatteryUpdateLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f); // Mise à jour chaque seconde (temps réel simulé)

                float deltaTime = Time.time - lastUpdateTime;
                lastUpdateTime = Time.time;

                if (isCharging)
                {
                    // Mode charge
                    float chargeAmount = (chargeRate / batteryCapacity) * (deltaTime / 3600f) * 100f;
                    currentBatteryLevel = Mathf.Min(100f, currentBatteryLevel + chargeAmount);
                }
                else
                {
                    // Consommation normale (accélérée pour la simulation)
                    float consumption = CalculateTotalConsumption();
                    float simulationSpeed = 60f; // 1 minute réelle = 1 heure simulée pour les tests
                    float drainAmount = (consumption / batteryCapacity) * (deltaTime / 3600f) * 100f * simulationSpeed;
                    
                    currentBatteryLevel = Mathf.Max(0f, currentBatteryLevel - drainAmount);
                }

                // Vérifier les seuils
                CheckBatteryThresholds();

                OnBatteryLevelChanged?.Invoke(currentBatteryLevel);
            }
        }

        /// <summary>
        /// Boucle de surveillance de la connexion
        /// </summary>
        private IEnumerator ConnectionMonitorLoop()
        {
            ConnectionState previousState = connectionState;

            while (true)
            {
                yield return new WaitForSeconds(2f);

                // Simuler les variations de signal
                SimulateNetworkConditions();

                // Détecter les changements d'état
                if (connectionState != previousState)
                {
                    if (connectionState == ConnectionState.Offline && previousState != ConnectionState.Offline)
                    {
                        OnConnectionLost?.Invoke();
                        OnOfflineModeActivated?.Invoke();
                        Debug.Log("[Battery] Mode OFFLINE activé - NFR-AVA");
                    }
                    else if (connectionState != ConnectionState.Offline && previousState == ConnectionState.Offline)
                    {
                        OnConnectionRestored?.Invoke();
                        Debug.Log("[Battery] Connexion rétablie");
                    }

                    previousState = connectionState;
                }
            }
        }

        /// <summary>
        /// Calcule la consommation totale actuelle
        /// </summary>
        private float CalculateTotalConsumption()
        {
            float total = baseConsumption;

            if (activeComponents.GetValueOrDefault("CameraRGB", true))
                total += cameraRGBConsumption;
            
            if (activeComponents.GetValueOrDefault("CameraThermal", true))
                total += cameraThermalConsumption;
            
            if (activeComponents.GetValueOrDefault("Display", true))
                total += displayConsumption;
            
            if (activeComponents.GetValueOrDefault("Processing", true))
                total += processingConsumption;
            
            if (activeComponents.GetValueOrDefault("Network", true) && connectionState != ConnectionState.Offline)
                total += networkConsumption;
            
            if (activeComponents.GetValueOrDefault("GPS", true))
                total += gpsConsumption;

            // Appliquer le mode économie d'énergie
            if (powerSavingMode)
                total *= powerSavingReduction;

            return total;
        }

        /// <summary>
        /// Calcule le temps restant estimé
        /// </summary>
        private float CalculateRemainingTime()
        {
            float consumption = CalculateTotalConsumption();
            if (consumption <= 0) return float.MaxValue;

            float remainingCapacity = (currentBatteryLevel / 100f) * batteryCapacity;
            return remainingCapacity / consumption; // En heures
        }

        /// <summary>
        /// Vérifie les seuils de batterie
        /// </summary>
        private void CheckBatteryThresholds()
        {
            if (currentBatteryLevel <= shutdownThreshold)
            {
                OnBatteryShutdown?.Invoke();
                Debug.LogError("[Battery] ARRÊT IMMINENT - Batterie critique!");
            }
            else if (currentBatteryLevel <= criticalThreshold)
            {
                OnBatteryCritical?.Invoke();
                
                // Activer automatiquement le mode économie d'énergie
                if (!powerSavingMode)
                    EnablePowerSavingMode(true);
                
                Debug.LogWarning("[Battery] Niveau CRITIQUE - Mode économie activé");
            }
            else if (currentBatteryLevel <= warningThreshold)
            {
                OnBatteryWarning?.Invoke();
                Debug.Log("[Battery] Niveau BAS - Attention");
            }
        }

        /// <summary>
        /// Simule les conditions réseau
        /// </summary>
        private void SimulateNetworkConditions()
        {
            // Simuler une perte de signal aléatoire occasionnelle
            if (UnityEngine.Random.value < 0.02f) // 2% de chance par tick
            {
                signalStrength = UnityEngine.Random.Range(0f, 30f);
            }
            else
            {
                signalStrength = Mathf.Clamp(signalStrength + UnityEngine.Random.Range(-5f, 5f), 0f, 100f);
            }

            // Déterminer l'état de connexion selon la force du signal
            if (signalStrength < 10f)
                connectionState = ConnectionState.Offline;
            else if (signalStrength < 30f)
                connectionState = ConnectionState.Degraded;
            else if (signalStrength < 70f)
                connectionState = ConnectionState.Limited;
            else
                connectionState = ConnectionState.Connected;
        }

        #region Public Methods

        /// <summary>
        /// Active/désactive le mode économie d'énergie
        /// </summary>
        public void EnablePowerSavingMode(bool enable)
        {
            if (powerSavingMode == enable) return;

            powerSavingMode = enable;

            if (enable)
            {
                // Désactiver les composants non essentiels
                SetComponentActive("CameraThermal", false);
                SetComponentActive("Network", connectionState != ConnectionState.Connected);
                OnPowerSavingEnabled?.Invoke();
                Debug.Log("[Battery] Mode économie d'énergie ACTIVÉ");
            }
            else
            {
                // Réactiver tous les composants
                SetComponentActive("CameraThermal", true);
                SetComponentActive("Network", true);
                OnPowerSavingDisabled?.Invoke();
                Debug.Log("[Battery] Mode économie d'énergie DÉSACTIVÉ");
            }
        }

        /// <summary>
        /// Active/désactive un composant
        /// </summary>
        public void SetComponentActive(string componentName, bool active)
        {
            if (activeComponents.ContainsKey(componentName))
            {
                activeComponents[componentName] = active;
                Debug.Log($"[Battery] Composant {componentName}: {(active ? "ON" : "OFF")}");
            }
        }

        /// <summary>
        /// Simule le branchement du chargeur
        /// </summary>
        public void SetCharging(bool charging)
        {
            isCharging = charging;
            Debug.Log($"[Battery] Charge: {(charging ? "EN COURS" : "ARRÊTÉE")}");
        }

        /// <summary>
        /// Force le passage en mode offline
        /// </summary>
        public void ForceOfflineMode()
        {
            connectionState = ConnectionState.Offline;
            signalStrength = 0f;
            OnConnectionLost?.Invoke();
            OnOfflineModeActivated?.Invoke();
            Debug.Log("[Battery] Mode OFFLINE forcé");
        }

        /// <summary>
        /// Restaure la connexion
        /// </summary>
        public void RestoreConnection()
        {
            connectionState = ConnectionState.Connected;
            signalStrength = 100f;
            OnConnectionRestored?.Invoke();
            Debug.Log("[Battery] Connexion restaurée");
        }

        /// <summary>
        /// Définit le niveau de batterie (pour les tests)
        /// </summary>
        public void SetBatteryLevel(float level)
        {
            currentBatteryLevel = Mathf.Clamp(level, 0f, 100f);
            OnBatteryLevelChanged?.Invoke(currentBatteryLevel);
        }

        /// <summary>
        /// Obtient un rapport d'état complet
        /// </summary>
        public string GetStatusReport()
        {
            float remainingHours = RemainingTime;
            string remainingStr = remainingHours > 24 ? $">{24}h" : $"{remainingHours:F1}h";

            return $"=== ÉTAT BATTERIE (NFR-AVA) ===\n" +
                   $"Niveau: {currentBatteryLevel:F1}%\n" +
                   $"Autonomie restante: {remainingStr}\n" +
                   $"Consommation actuelle: {CalculateTotalConsumption():F0}mA\n" +
                   $"Mode économie: {(powerSavingMode ? "OUI" : "NON")}\n" +
                   $"En charge: {(isCharging ? "OUI" : "NON")}\n" +
                   $"\n=== CONNEXION ===\n" +
                   $"État: {connectionState}\n" +
                   $"Signal: {signalStrength:F0}%\n" +
                   $"Mode offline: {(IsOfflineMode ? "ACTIF" : "Inactif")}\n" +
                   $"\n=== COMPOSANTS ACTIFS ===\n" +
                   string.Join("\n", GetActiveComponentsList());
        }

        /// <summary>
        /// Liste des composants et leur état
        /// </summary>
        public List<string> GetActiveComponentsList()
        {
            var list = new List<string>();
            foreach (var kvp in activeComponents)
            {
                list.Add($"• {kvp.Key}: {(kvp.Value ? "ON" : "OFF")}");
            }
            return list;
        }

        /// <summary>
        /// Vérifie si l'autonomie cible est respectée
        /// </summary>
        public bool MeetsAutonomyRequirement()
        {
            // NFR-AVA: ≥8h
            float expectedAutonomy = batteryCapacity / CalculateTotalConsumption();
            bool meets = expectedAutonomy >= targetAutonomy;
            
            if (!meets)
            {
                Debug.LogWarning($"[Battery] NFR-AVA non respecté: {expectedAutonomy:F1}h < {targetAutonomy}h requis");
            }
            
            return meets;
        }

        #endregion
    }

    /// <summary>
    /// État de la connexion réseau
    /// </summary>
    public enum ConnectionState
    {
        Connected,  // Connexion normale (4G/5G/WiFi)
        Limited,    // Connexion limitée
        Degraded,   // Connexion dégradée
        Offline     // Mode hors ligne (NFR-AVA)
    }

    /// <summary>
    /// OfflineModeManager - Gestion du fonctionnement dégradé hors connexion
    /// </summary>
    public class OfflineModeManager : MonoBehaviour
    {
        public static OfflineModeManager Instance { get; private set; }

        [Header("=== CACHE LOCAL ===")]
        [SerializeField] private int maxCachedPatients = 50;
        [SerializeField] private int maxCachedHospitals = 10;
        [SerializeField] private float cacheExpirationHours = 24f;

        [Header("=== DONNÉES OFFLINE ===")]
        [SerializeField] private List<CachedPatientData> cachedPatients = new List<CachedPatientData>();
        [SerializeField] private List<CachedHospitalData> cachedHospitals = new List<CachedHospitalData>();
        [SerializeField] private List<PendingSyncData> pendingSyncQueue = new List<PendingSyncData>();

        [Header("=== ÉTAT ===")]
        [SerializeField] private bool isOfflineMode = false;
        [SerializeField] private DateTime lastSyncTime;
        [SerializeField] private int pendingSyncCount = 0;

        // Événements
        public UnityEvent OnOfflineModeEntered;
        public UnityEvent OnOfflineModeExited;
        public UnityEvent<int> OnSyncCompleted;

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
            // S'abonner aux événements de connexion
            if (BatteryManager.Instance != null)
            {
                BatteryManager.Instance.OnOfflineModeActivated.AddListener(EnterOfflineMode);
                BatteryManager.Instance.OnConnectionRestored.AddListener(ExitOfflineMode);
            }

            // Charger le cache au démarrage
            LoadLocalCache();
        }

        /// <summary>
        /// Active le mode offline
        /// </summary>
        public void EnterOfflineMode()
        {
            if (isOfflineMode) return;

            isOfflineMode = true;
            Debug.Log("[OfflineMode] Mode OFFLINE activé - Fonctionnement local uniquement");

            // Sauvegarder l'état actuel
            SaveLocalCache();

            OnOfflineModeEntered?.Invoke();
        }

        /// <summary>
        /// Désactive le mode offline et synchronise
        /// </summary>
        public void ExitOfflineMode()
        {
            if (!isOfflineMode) return;

            isOfflineMode = false;
            Debug.Log("[OfflineMode] Connexion rétablie - Synchronisation en cours...");

            // Synchroniser les données en attente
            StartCoroutine(SyncPendingData());

            OnOfflineModeExited?.Invoke();
        }

        /// <summary>
        /// Ajoute des données patient au cache local
        /// </summary>
        public void CachePatientData(Core.VictimController victim, Core.StartCategory triageCategory)
        {
            if (victim == null) return;

            var cachedData = new CachedPatientData
            {
                patientId = victim.PatientId,
                firstName = victim.firstName,
                lastName = victim.lastName,
                age = victim.age,
                triageCategory = triageCategory,
                vitalSigns = victim.VitalSigns,
                cacheTime = DateTime.Now,
                synced = !isOfflineMode
            };

            // Vérifier si existe déjà
            int existingIndex = cachedPatients.FindIndex(p => p.patientId == victim.PatientId);
            if (existingIndex >= 0)
            {
                cachedPatients[existingIndex] = cachedData;
            }
            else
            {
                if (cachedPatients.Count >= maxCachedPatients)
                {
                    // Supprimer le plus ancien
                    cachedPatients.RemoveAt(0);
                }
                cachedPatients.Add(cachedData);
            }

            // Si offline, ajouter à la queue de sync
            if (isOfflineMode)
            {
                AddToPendingSync(PendingSyncType.PatientData, cachedData);
            }

            Debug.Log($"[OfflineMode] Patient {victim.PatientId} mis en cache");
        }

        /// <summary>
        /// Met en cache les données hospitalières
        /// </summary>
        public void CacheHospitalData(Core.HospitalData hospital)
        {
            if (hospital == null) return;

            var cachedData = new CachedHospitalData
            {
                hospitalId = hospital.hospitalId,
                hospitalName = hospital.hospitalName,
                availableBeds = hospital.availableBeds,
                totalBeds = hospital.totalBeds,
                distanceKm = hospital.distanceKm,
                specialties = hospital.specialties,
                cacheTime = DateTime.Now
            };

            int existingIndex = cachedHospitals.FindIndex(h => h.hospitalId == hospital.hospitalId);
            if (existingIndex >= 0)
            {
                cachedHospitals[existingIndex] = cachedData;
            }
            else
            {
                if (cachedHospitals.Count >= maxCachedHospitals)
                    cachedHospitals.RemoveAt(0);
                cachedHospitals.Add(cachedData);
            }

            Debug.Log($"[OfflineMode] Hôpital {hospital.hospitalName} mis en cache");
        }

        /// <summary>
        /// Récupère les données patient du cache
        /// </summary>
        public CachedPatientData GetCachedPatient(string patientId)
        {
            return cachedPatients.Find(p => p.patientId == patientId && !IsExpired(p.cacheTime));
        }

        /// <summary>
        /// Récupère les hôpitaux du cache
        /// </summary>
        public List<CachedHospitalData> GetCachedHospitals()
        {
            return cachedHospitals.FindAll(h => !IsExpired(h.cacheTime));
        }

        /// <summary>
        /// Ajoute une action à la queue de synchronisation
        /// </summary>
        private void AddToPendingSync(PendingSyncType type, object data)
        {
            pendingSyncQueue.Add(new PendingSyncData
            {
                type = type,
                data = data,
                timestamp = DateTime.Now
            });
            pendingSyncCount = pendingSyncQueue.Count;
        }

        /// <summary>
        /// Synchronise les données en attente
        /// </summary>
        private IEnumerator SyncPendingData()
        {
            int synced = 0;

            foreach (var pending in pendingSyncQueue.ToArray())
            {
                // Simuler l'envoi au serveur
                yield return new WaitForSeconds(0.2f);

                // Marquer comme synchronisé
                if (pending.type == PendingSyncType.PatientData)
                {
                    var patientData = pending.data as CachedPatientData;
                    if (patientData != null)
                    {
                        patientData.synced = true;
                        Debug.Log($"[OfflineMode] Patient {patientData.patientId} synchronisé");
                    }
                }

                synced++;
            }

            pendingSyncQueue.Clear();
            pendingSyncCount = 0;
            lastSyncTime = DateTime.Now;

            OnSyncCompleted?.Invoke(synced);
            Debug.Log($"[OfflineMode] Synchronisation terminée: {synced} éléments");
        }

        /// <summary>
        /// Vérifie si les données sont expirées
        /// </summary>
        private bool IsExpired(DateTime cacheTime)
        {
            return (DateTime.Now - cacheTime).TotalHours > cacheExpirationHours;
        }

        /// <summary>
        /// Sauvegarde le cache local
        /// </summary>
        private void SaveLocalCache()
        {
            // Sauvegarder en JSON dans PlayerPrefs (simplifié)
            string patientsJson = JsonUtility.ToJson(new CachedPatientList { patients = cachedPatients });
            PlayerPrefs.SetString("CachedPatients", patientsJson);
            PlayerPrefs.Save();
            Debug.Log("[OfflineMode] Cache sauvegardé");
        }

        /// <summary>
        /// Charge le cache local
        /// </summary>
        private void LoadLocalCache()
        {
            string patientsJson = PlayerPrefs.GetString("CachedPatients", "");
            if (!string.IsNullOrEmpty(patientsJson))
            {
                try
                {
                    var list = JsonUtility.FromJson<CachedPatientList>(patientsJson);
                    cachedPatients = list.patients ?? new List<CachedPatientData>();
                    Debug.Log($"[OfflineMode] Cache chargé: {cachedPatients.Count} patients");
                }
                catch
                {
                    cachedPatients = new List<CachedPatientData>();
                }
            }
        }

        /// <summary>
        /// Obtient le statut du mode offline
        /// </summary>
        public string GetOfflineStatus()
        {
            return $"=== MODE OFFLINE (NFR-AVA) ===\n" +
                   $"État: {(isOfflineMode ? "ACTIF" : "Inactif")}\n" +
                   $"Patients en cache: {cachedPatients.Count}/{maxCachedPatients}\n" +
                   $"Hôpitaux en cache: {cachedHospitals.Count}/{maxCachedHospitals}\n" +
                   $"En attente de sync: {pendingSyncCount}\n" +
                   $"Dernière sync: {(lastSyncTime != default ? lastSyncTime.ToString("HH:mm:ss") : "Jamais")}";
        }
    }

    #region Data Structures

    [Serializable]
    public class CachedPatientData
    {
        public string patientId;
        public string firstName;
        public string lastName;
        public int age;
        public Core.StartCategory triageCategory;
        public Core.VitalSigns vitalSigns;
        public DateTime cacheTime;
        public bool synced;
    }

    [Serializable]
    public class CachedHospitalData
    {
        public string hospitalId;
        public string hospitalName;
        public int availableBeds;
        public int totalBeds;
        public float distanceKm;
        public Core.HospitalSpecialty specialties;
        public DateTime cacheTime;
    }

    [Serializable]
    public class PendingSyncData
    {
        public PendingSyncType type;
        public object data;
        public DateTime timestamp;
    }

    public enum PendingSyncType
    {
        PatientData,
        TriageDecision,
        HospitalAssignment,
        AmbulanceAssignment
    }

    [Serializable]
    public class CachedPatientList
    {
        public List<CachedPatientData> patients;
    }

    #endregion
}
