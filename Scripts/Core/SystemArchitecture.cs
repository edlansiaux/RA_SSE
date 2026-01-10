using UnityEngine;
using System;
using System.Collections.Generic;

namespace RASSE.Core
{
    /// <summary>
    /// SystemArchitecture - Architecture modulaire conforme au BDD SysML du rapport
    /// Organise les composants selon la structure:
    /// 1. Lunettes RA (bloc principal)
    ///    - 11. Batterie / 111. Chargeur
    ///    - 12. Microphone
    ///    - 13. Caméra RGB
    ///    - 14. Caméra Thermique
    ///    - 15. Module de Géolocalisation
    ///    - 16. Module Connexion sans fil
    ///    - 17. Module retour vers l'ambulance
    ///    - 18. Capteurs
    ///    - 19. Module_Analyse_Image
    ///         - 191. Système de Localisation des victimes
    ///         - 192. Système d'Analyse des constantes
    /// 2. Module_Classification_START
    /// 3. Interface_RA
    /// 4. Système_Coordination_SMA
    ///    - 41. Base_Donnees_Patient
    ///    - 42. Informations_Hôpitaux
    /// </summary>
    public class SystemArchitecture : MonoBehaviour
    {
        public static SystemArchitecture Instance { get; private set; }

        [Header("=== BLOC 1: LUNETTES RA (Système Principal) ===")]
        [SerializeField] private LunettesRA lunettesRA;

        [Header("=== BLOC 2: MODULE CLASSIFICATION START ===")]
        [SerializeField] private StartTriageSystem moduleClassificationSTART;

        [Header("=== BLOC 3: INTERFACE RA ===")]
        [SerializeField] private ARInterfaceController interfaceRA;

        [Header("=== BLOC 4: SYSTÈME COORDINATION SMA ===")]
        [SerializeField] private SMACoordinator systemeCoordinationSMA;

        // État du système
        public SystemState CurrentState { get; private set; } = SystemState.Initializing;
        public bool IsOperational => CurrentState == SystemState.Operational;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeArchitecture();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeArchitecture()
        {
            Debug.Log("[SystemArchitecture] Initialisation de l'architecture RA SSE conforme au BDD SysML");

            // Vérifier les blocs principaux
            ValidateBlockConnections();

            CurrentState = SystemState.Operational;
        }

        private void ValidateBlockConnections()
        {
            List<string> missingBlocks = new List<string>();

            if (lunettesRA == null) missingBlocks.Add("Bloc 1: Lunettes RA");
            if (moduleClassificationSTART == null) missingBlocks.Add("Bloc 2: Module Classification START");
            if (interfaceRA == null) missingBlocks.Add("Bloc 3: Interface RA");
            if (systemeCoordinationSMA == null) missingBlocks.Add("Bloc 4: Système Coordination SMA");

            if (missingBlocks.Count > 0)
            {
                Debug.LogWarning($"[SystemArchitecture] Blocs manquants: {string.Join(", ", missingBlocks)}");
            }
            else
            {
                Debug.Log("[SystemArchitecture] Tous les blocs principaux sont connectés");
            }
        }

        /// <summary>
        /// Obtient le rapport d'état de l'architecture
        /// </summary>
        public ArchitectureStatusReport GetStatusReport()
        {
            return new ArchitectureStatusReport
            {
                Timestamp = DateTime.Now,
                SystemState = CurrentState,
                LunettesRAStatus = lunettesRA?.GetStatus() ?? new BlockStatus { IsOperational = false },
                ClassificationSTARTStatus = moduleClassificationSTART != null,
                InterfaceRAStatus = interfaceRA != null,
                SMACoordinatorStatus = systemeCoordinationSMA?.GetStatus() ?? new SMAStatus()
            };
        }
    }

    /// <summary>
    /// État global du système
    /// </summary>
    public enum SystemState
    {
        Initializing,
        Operational,
        Degraded,
        Offline,
        Error
    }

    // ═══════════════════════════════════════════════════════════════
    // BLOC 1: LUNETTES RA - Système principal embarqué
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Bloc 1: Lunettes RA - Système principal intégrant tous les composants matériels et logiciels
    /// </summary>
    [Serializable]
    public class LunettesRA : MonoBehaviour
    {
        [Header("=== BLOC 11: BATTERIE ===")]
        public BatterieModule batterie;

        [Header("=== BLOC 12: MICROPHONE ===")]
        public MicrophoneModule microphone;

        [Header("=== BLOC 13: CAMÉRA RGB ===")]
        public CameraRGBModule cameraRGB;

        [Header("=== BLOC 14: CAMÉRA THERMIQUE ===")]
        public CameraThermique cameraThermic;

        [Header("=== BLOC 15: MODULE GÉOLOCALISATION ===")]
        public GeolocationModule geolocalisation;

        [Header("=== BLOC 16: MODULE CONNEXION SANS FIL ===")]
        public WirelessConnectionModule connexionSansFil;

        [Header("=== BLOC 17: MODULE RETOUR AMBULANCE ===")]
        public AmbulanceGuidanceModule retourAmbulance;

        [Header("=== BLOC 18: CAPTEURS ===")]
        public SensorModule capteurs;

        [Header("=== BLOC 19: MODULE ANALYSE IMAGE ===")]
        public ModuleAnalyseImage analyseImage;

        public BlockStatus GetStatus()
        {
            return new BlockStatus
            {
                BlockId = "1",
                BlockName = "Lunettes RA",
                IsOperational = true,
                BatteryLevel = batterie?.GetBatteryLevel() ?? 100f,
                IsConnected = connexionSansFil?.IsConnected ?? false,
                SubBlockStatuses = new Dictionary<string, bool>
                {
                    { "11-Batterie", batterie != null },
                    { "12-Microphone", microphone != null },
                    { "13-CameraRGB", cameraRGB != null },
                    { "14-CameraThermic", cameraThermic != null },
                    { "15-Geolocalisation", geolocalisation != null },
                    { "16-ConnexionSansFil", connexionSansFil != null },
                    { "17-RetourAmbulance", retourAmbulance != null },
                    { "18-Capteurs", capteurs != null },
                    { "19-AnalyseImage", analyseImage != null }
                }
            };
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // SOUS-BLOCS DES LUNETTES RA
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Bloc 11: Batterie - Alimentation du système
    /// </summary>
    [Serializable]
    public class BatterieModule : MonoBehaviour
    {
        [Header("Bloc 111: Chargeur")]
        public ChargeurModule chargeur;

        [Header("État Batterie")]
        [Range(0, 100)]
        public float batteryLevel = 100f;
        public float maxAutonomyHours = 8f; // NFR-AVA: Autonomie ≥ 8h
        public bool isCharging = false;

        private float consumptionRatePerSecond = 0.003f; // ~8h d'autonomie

        private void Update()
        {
            if (!isCharging && batteryLevel > 0)
            {
                batteryLevel -= consumptionRatePerSecond * Time.deltaTime;
                batteryLevel = Mathf.Max(0, batteryLevel);
            }
        }

        public float GetBatteryLevel() => batteryLevel;
        public float GetRemainingHours() => (batteryLevel / 100f) * maxAutonomyHours;
        public bool IsCritical() => batteryLevel < 20f;
    }

    /// <summary>
    /// Bloc 111: Chargeur
    /// </summary>
    [Serializable]
    public class ChargeurModule : MonoBehaviour
    {
        public bool isConnected = false;
        public float chargingRate = 0.1f; // % par seconde
    }

    /// <summary>
    /// Bloc 12: Microphone - Capture vocale pour commandes mains libres (NFR-UX)
    /// </summary>
    [Serializable]
    public class MicrophoneModule : MonoBehaviour
    {
        public bool isActive = true;
        public float sensitivity = 1f;
        public bool noiseReduction = true;

        public event Action<string> OnVoiceCommandDetected;

        public void ProcessVoiceInput(string command)
        {
            OnVoiceCommandDetected?.Invoke(command);
        }
    }

    /// <summary>
    /// Bloc 13: Caméra RGB - Capture visuelle pour détection et reconnaissance
    /// </summary>
    [Serializable]
    public class CameraRGBModule : MonoBehaviour
    {
        public Camera mainCamera;
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;
        public float fieldOfView = 70f;
        public bool isActive = true;

        public RenderTexture CaptureFrame()
        {
            if (mainCamera == null) return null;
            RenderTexture rt = new RenderTexture(resolutionWidth, resolutionHeight, 24);
            mainCamera.targetTexture = rt;
            mainCamera.Render();
            mainCamera.targetTexture = null;
            return rt;
        }
    }

    /// <summary>
    /// Bloc 14: Caméra Thermique - Données thermiques pour détection température/saignement
    /// </summary>
    [Serializable]
    public class CameraThermic : MonoBehaviour
    {
        public bool isActive = true;
        public float minTemperature = 20f;
        public float maxTemperature = 45f;
        public float sensitivity = 0.1f; // Résolution thermique en °C

        /// <summary>
        /// Simule la lecture de température corporelle
        /// </summary>
        public float ReadBodyTemperature(Vector3 targetPosition)
        {
            // En simulation, retourne une température réaliste
            return UnityEngine.Random.Range(35f, 39f);
        }

        /// <summary>
        /// Détecte les zones de saignement (zones plus chaudes)
        /// </summary>
        public bool DetectBleeding(Vector3 targetPosition)
        {
            // Simulation de détection
            return UnityEngine.Random.value < 0.3f;
        }
    }

    /// <summary>
    /// Bloc 15: Module de Géolocalisation - GPS et positionnement (NFR-LOC)
    /// </summary>
    [Serializable]
    public class GeolocationModule : MonoBehaviour
    {
        public bool isActive = true;
        public float accuracy = 2f; // NFR-LOC: ≤ 2m de précision
        public Vector3 currentPosition;
        public float currentLatitude;
        public float currentLongitude;

        private void Update()
        {
            // En simulation, utilise la position Unity
            currentPosition = transform.position;
        }

        public Vector3 GetPosition() => currentPosition;
        public float GetAccuracy() => accuracy;
    }

    /// <summary>
    /// Bloc 16: Module Connexion sans fil - Communication 4G/5G/WiFi
    /// </summary>
    [Serializable]
    public class WirelessConnectionModule : MonoBehaviour
    {
        public bool isConnected = true;
        public ConnectionType connectionType = ConnectionType.Network4G;
        public float signalStrength = 0.8f; // 0-1
        public float latency = 50f; // ms

        public bool IsConnected => isConnected && signalStrength > 0.2f;

        public enum ConnectionType
        {
            WiFi,
            Network4G,
            Network5G,
            Offline
        }

        /// <summary>
        /// Bascule en mode hors ligne (NFR-AVA)
        /// </summary>
        public void SwitchToOfflineMode()
        {
            connectionType = ConnectionType.Offline;
            isConnected = false;
            Debug.Log("[WirelessModule] Passage en mode offline");
        }
    }

    /// <summary>
    /// Bloc 17: Module retour vers l'ambulance - Guidage AR vers véhicule (REQ-6)
    /// </summary>
    [Serializable]
    public class AmbulanceGuidanceModule : MonoBehaviour
    {
        public NavigationSystem navigationSystem;
        public Transform currentAmbulanceTarget;
        public float distanceToAmbulance;
        public string guidanceInstructions;

        public void StartGuidance(Transform ambulancePosition, string ambulanceId)
        {
            currentAmbulanceTarget = ambulancePosition;
            navigationSystem?.StartNavigation(ambulancePosition, $"Ambulance {ambulanceId}");
            
            // Valide REQ-6
            RequirementsManager.Instance?.ValidateRequirement("REQ-6", $"Guidage vers ambulance {ambulanceId}");
        }

        private void Update()
        {
            if (currentAmbulanceTarget != null)
            {
                distanceToAmbulance = Vector3.Distance(transform.position, currentAmbulanceTarget.position);
            }
        }
    }

    /// <summary>
    /// Bloc 18: Capteurs - Ensemble des capteurs embarqués
    /// </summary>
    [Serializable]
    public class SensorModule : MonoBehaviour
    {
        public CameraRGBModule cameraRGB;
        public CameraThermic cameraThermic;
        public MicrophoneModule microphone;
        public bool allSensorsOperational = true;

        public SensorData GetCurrentSensorData()
        {
            return new SensorData
            {
                Timestamp = DateTime.Now,
                RGBActive = cameraRGB?.isActive ?? false,
                ThermalActive = cameraThermic?.isActive ?? false,
                MicrophoneActive = microphone?.isActive ?? false
            };
        }
    }

    /// <summary>
    /// Bloc 19: Module Analyse Image - Traitement des flux vidéo
    /// </summary>
    [Serializable]
    public class ModuleAnalyseImage : MonoBehaviour
    {
        [Header("Bloc 191: Système de Localisation des victimes")]
        public VictimLocalizationSystem localisationVictimes;

        [Header("Bloc 192: Système d'Analyse des constantes")]
        public VitalSignsAnalyzer analyseConstantes;

        /// <summary>
        /// Analyse complète d'une frame pour détecter les victimes et leurs constantes
        /// </summary>
        public ImageAnalysisResult AnalyzeFrame(RenderTexture frame)
        {
            var result = new ImageAnalysisResult
            {
                Timestamp = DateTime.Now,
                DetectedVictims = localisationVictimes?.DetectVictims() ?? new List<DetectedVictim>()
            };

            // Pour chaque victime détectée, analyser les constantes
            foreach (var victim in result.DetectedVictims)
            {
                victim.EstimatedVitals = analyseConstantes?.AnalyzeVitals(victim);
            }

            // Valide REQ-1 si des victimes sont détectées
            if (result.DetectedVictims.Count > 0)
            {
                RequirementsManager.Instance?.ValidateRequirement("REQ-1", $"{result.DetectedVictims.Count} victimes détectées");
            }

            return result;
        }
    }

    /// <summary>
    /// Bloc 191: Système de Localisation des victimes (REQ-1)
    /// </summary>
    [Serializable]
    public class VictimLocalizationSystem : MonoBehaviour
    {
        public float detectionRange = 20f;
        public float detectionAccuracy = 0.95f; // NFR-ACC: ≥ 95%
        public float falsePositiveRate = 0.03f; // NFR-ACC: ≤ 5%

        public List<DetectedVictim> DetectVictims()
        {
            var detected = new List<DetectedVictim>();

            // Recherche des victimes dans le rayon de détection
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange);

            foreach (var col in colliders)
            {
                if (col.CompareTag("Victim"))
                {
                    var victimController = col.GetComponent<VictimController>();
                    if (victimController != null)
                    {
                        detected.Add(new DetectedVictim
                        {
                            Position = col.transform.position,
                            Distance = Vector3.Distance(transform.position, col.transform.position),
                            Confidence = detectionAccuracy,
                            VictimId = victimController.PatientId
                        });
                    }
                }
            }

            return detected;
        }
    }

    /// <summary>
    /// Bloc 192: Système d'Analyse des constantes (REQ-2)
    /// </summary>
    [Serializable]
    public class VitalSignsAnalyzer : MonoBehaviour
    {
        /// <summary>
        /// Analyse les signes vitaux par traitement d'image
        /// </summary>
        public VitalSigns AnalyzeVitals(DetectedVictim victim)
        {
            // En simulation, récupère les vraies données du VictimController
            var victimObj = GameObject.Find(victim.VictimId);
            var victimController = victimObj?.GetComponent<VictimController>();

            if (victimController != null)
            {
                // Valide REQ-2
                RequirementsManager.Instance?.ValidateRequirement("REQ-2", $"Constantes analysées pour {victim.VictimId}");
                return victimController.VitalSigns;
            }

            // Fallback: signes vitaux simulés
            return VitalSigns.Normal();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // BLOC 4: SYSTÈME COORDINATION SMA
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Bloc 4: Système de Coordination SMA - Gestion communication terrain-hôpital
    /// </summary>
    [Serializable]
    public class SMACoordinator : MonoBehaviour
    {
        [Header("Bloc 41: Base Données Patient")]
        public PatientDatabaseModule baseDonneesPatient;

        [Header("Bloc 42: Informations Hôpitaux")]
        public HospitalInformationModule informationsHopitaux;

        [Header("Références")]
        public HospitalCoordinationSystem hospitalSystem;
        public AmbulanceManager ambulanceManager;

        /// <summary>
        /// Coordonne l'orientation d'un patient (REQ-4)
        /// </summary>
        public OrientationDecision DecideOrientation(VictimController victim, StartCategory category)
        {
            var decision = new OrientationDecision
            {
                PatientId = victim.PatientId,
                Category = category,
                Timestamp = DateTime.Now
            };

            switch (category)
            {
                case StartCategory.Red:
                case StartCategory.Yellow:
                    // Évacuation vers hôpital
                    decision.ActionType = OrientationAction.EvacuateToHospital;
                    decision.RecommendedHospital = informationsHopitaux?.GetBestHospital(category);
                    decision.AssignedAmbulance = ambulanceManager?.GetBestAvailableAmbulance(victim);
                    break;

                case StartCategory.Green:
                    // PRV ou retour domicile
                    decision.ActionType = OrientationAction.SendToPRV;
                    break;

                case StartCategory.Black:
                    // Signalement décès
                    decision.ActionType = OrientationAction.DeclareDeceased;
                    break;
            }

            // Enregistrer dans la base de données patient
            baseDonneesPatient?.RecordDecision(decision);

            // Valide REQ-4
            RequirementsManager.Instance?.ValidateRequirement("REQ-4", $"Orientation décidée pour {victim.PatientId}: {decision.ActionType}");

            return decision;
        }

        public SMAStatus GetStatus()
        {
            return new SMAStatus
            {
                IsConnected = true,
                PatientDatabaseActive = baseDonneesPatient != null,
                HospitalInfoActive = informationsHopitaux != null,
                TotalPatientsRecorded = baseDonneesPatient?.GetPatientCount() ?? 0,
                AvailableHospitals = informationsHopitaux?.GetAvailableHospitalCount() ?? 0
            };
        }
    }

    /// <summary>
    /// Bloc 41: Base de Données Patient (REQ-7)
    /// </summary>
    [Serializable]
    public class PatientDatabaseModule : MonoBehaviour
    {
        private Dictionary<string, PatientDossier> dossiers = new Dictionary<string, PatientDossier>();

        /// <summary>
        /// Crée ou met à jour un dossier patient
        /// </summary>
        public void CreateOrUpdateDossier(VictimController victim)
        {
            if (!dossiers.ContainsKey(victim.PatientId))
            {
                dossiers[victim.PatientId] = new PatientDossier
                {
                    PatientId = victim.PatientId,
                    CreatedAt = DateTime.Now,
                    Entries = new List<DossierEntry>()
                };

                // Valide REQ-7
                RequirementsManager.Instance?.ValidateRequirement("REQ-7", $"Dossier créé pour {victim.PatientId}");
            }

            // Ajouter les mesures actuelles
            dossiers[victim.PatientId].Entries.Add(new DossierEntry
            {
                Timestamp = DateTime.Now,
                Type = EntryType.VitalSigns,
                Data = victim.VitalSigns
            });
        }

        public void RecordDecision(OrientationDecision decision)
        {
            if (dossiers.TryGetValue(decision.PatientId, out var dossier))
            {
                dossier.Entries.Add(new DossierEntry
                {
                    Timestamp = DateTime.Now,
                    Type = EntryType.Decision,
                    Data = decision
                });
            }
        }

        public int GetPatientCount() => dossiers.Count;
    }

    /// <summary>
    /// Bloc 42: Informations Hôpitaux
    /// </summary>
    [Serializable]
    public class HospitalInformationModule : MonoBehaviour
    {
        public HospitalCoordinationSystem hospitalSystem;

        public HospitalData GetBestHospital(StartCategory category)
        {
            return hospitalSystem?.GetBestHospital(category, transform.position);
        }

        public int GetAvailableHospitalCount()
        {
            return hospitalSystem?.GetAvailableHospitals()?.Count ?? 0;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // STRUCTURES DE DONNÉES
    // ═══════════════════════════════════════════════════════════════

    [Serializable]
    public class BlockStatus
    {
        public string BlockId;
        public string BlockName;
        public bool IsOperational;
        public float BatteryLevel;
        public bool IsConnected;
        public Dictionary<string, bool> SubBlockStatuses;
    }

    [Serializable]
    public class SMAStatus
    {
        public bool IsConnected;
        public bool PatientDatabaseActive;
        public bool HospitalInfoActive;
        public int TotalPatientsRecorded;
        public int AvailableHospitals;
    }

    [Serializable]
    public class SensorData
    {
        public DateTime Timestamp;
        public bool RGBActive;
        public bool ThermalActive;
        public bool MicrophoneActive;
    }

    [Serializable]
    public class DetectedVictim
    {
        public string VictimId;
        public Vector3 Position;
        public float Distance;
        public float Confidence;
        public VitalSigns EstimatedVitals;
    }

    [Serializable]
    public class ImageAnalysisResult
    {
        public DateTime Timestamp;
        public List<DetectedVictim> DetectedVictims;
    }

    [Serializable]
    public class OrientationDecision
    {
        public string PatientId;
        public StartCategory Category;
        public OrientationAction ActionType;
        public HospitalData RecommendedHospital;
        public AmbulanceController AssignedAmbulance;
        public DateTime Timestamp;
    }

    public enum OrientationAction
    {
        EvacuateToHospital,
        SendToPRV,
        SendHome,
        DeclareDeceased
    }

    [Serializable]
    public class PatientDossier
    {
        public string PatientId;
        public DateTime CreatedAt;
        public List<DossierEntry> Entries;
    }

    [Serializable]
    public class DossierEntry
    {
        public DateTime Timestamp;
        public EntryType Type;
        public object Data;
    }

    [Serializable]
    public class ArchitectureStatusReport
    {
        public DateTime Timestamp;
        public SystemState SystemState;
        public BlockStatus LunettesRAStatus;
        public bool ClassificationSTARTStatus;
        public bool InterfaceRAStatus;
        public SMAStatus SMACoordinatorStatus;
    }
}
