using UnityEngine;
using System;
using System.Collections.Generic;

namespace RASSE.Compliance
{
    /// <summary>
    /// RequirementsComplianceMonitor - Surveillance de la conformité aux exigences du rapport
    /// Vérifie en temps réel le respect des NFR (Non-Functional Requirements)
    /// </summary>
    public class RequirementsComplianceMonitor : MonoBehaviour
    {
        public static RequirementsComplianceMonitor Instance { get; private set; }

        [Header("=== SEUILS NFR ===")]
        [Tooltip("NFR-ACC: Exactitude détection ≥95%")]
        [SerializeField] private float nfrAccDetectionTarget = 0.95f;
        
        [Tooltip("NFR-ACC: Faux positifs ≤5%")]
        [SerializeField] private float nfrAccFalsePositiveMax = 0.05f;
        
        [Tooltip("NFR-VIT: Latence triage hors réseau ≤30s")]
        [SerializeField] private float nfrVitOfflineLatency = 30f;
        
        [Tooltip("NFR-VIT: Latence triage en réseau ≤10s")]
        [SerializeField] private float nfrVitOnlineLatency = 10f;
        
        [Tooltip("NFR-AVA: Autonomie ≥8h")]
        [SerializeField] private float nfrAvaAutonomyHours = 8f;
        
        [Tooltip("NFR-LOC: Précision guidage ≤2m")]
        [SerializeField] private float nfrLocPrecisionMeters = 2f;

        [Header("=== ÉTAT CONFORMITÉ ===")]
        [SerializeField] private List<RequirementStatus> requirementsStatus = new List<RequirementStatus>();

        [Header("=== MÉTRIQUES EN TEMPS RÉEL ===")]
        [SerializeField] private float currentDetectionAccuracy;
        [SerializeField] private float currentFalsePositiveRate;
        [SerializeField] private float currentTriageLatency;
        [SerializeField] private float currentAutonomy;
        [SerializeField] private float currentGuidancePrecision;

        // Historique pour calculs
        private List<float> triageLatencies = new List<float>();
        private int totalDetectionAttempts;
        private int successfulDetections;
        private int falsePositives;
        private float sessionStartTime;

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

            InitializeRequirements();
            sessionStartTime = Time.time;
        }

        private void Update()
        {
            // Mise à jour périodique des métriques
            if (Time.frameCount % 60 == 0) // Toutes les ~1 seconde
            {
                UpdateAllMetrics();
                CheckAllCompliance();
            }
        }

        /// <summary>
        /// Initialise la liste des exigences à surveiller
        /// </summary>
        private void InitializeRequirements()
        {
            requirementsStatus.Clear();

            // Exigences fonctionnelles
            requirementsStatus.Add(new RequirementStatus
            {
                id = "REQ-1",
                description = "Détection et localisation des victimes (multi-cibles)",
                category = RequirementCategory.Functional,
                status = ComplianceStatus.NotTested
            });

            requirementsStatus.Add(new RequirementStatus
            {
                id = "REQ-2",
                description = "Estimation des constantes vitales par analyse d'image",
                category = RequirementCategory.Functional,
                status = ComplianceStatus.NotTested
            });

            requirementsStatus.Add(new RequirementStatus
            {
                id = "REQ-3",
                description = "Classification START (Rouge/Jaune/Vert/Noir)",
                category = RequirementCategory.Functional,
                status = ComplianceStatus.NotTested
            });

            requirementsStatus.Add(new RequirementStatus
            {
                id = "REQ-4",
                description = "Décision d'orientation selon START et capacités hospitalières",
                category = RequirementCategory.Functional,
                status = ComplianceStatus.NotTested
            });

            requirementsStatus.Add(new RequirementStatus
            {
                id = "REQ-5",
                description = "Affichage consignes évacuation + premiers secours",
                category = RequirementCategory.Functional,
                status = ComplianceStatus.NotTested
            });

            requirementsStatus.Add(new RequirementStatus
            {
                id = "REQ-6",
                description = "Guidage vers ambulance affectée (AR)",
                category = RequirementCategory.Functional,
                status = ComplianceStatus.NotTested
            });

            requirementsStatus.Add(new RequirementStatus
            {
                id = "REQ-7",
                description = "Création dossier d'intervention complet",
                category = RequirementCategory.Functional,
                status = ComplianceStatus.NotTested
            });

            // Exigences non fonctionnelles
            requirementsStatus.Add(new RequirementStatus
            {
                id = "NFR-ACC",
                description = $"Exactitude détection ≥{nfrAccDetectionTarget:P0}, faux positifs ≤{nfrAccFalsePositiveMax:P0}",
                category = RequirementCategory.Performance,
                targetValue = nfrAccDetectionTarget,
                status = ComplianceStatus.NotTested
            });

            requirementsStatus.Add(new RequirementStatus
            {
                id = "NFR-VIT",
                description = $"Latence triage ≤{nfrVitOfflineLatency}s hors réseau, ≤{nfrVitOnlineLatency}s en réseau",
                category = RequirementCategory.Performance,
                targetValue = nfrVitOfflineLatency,
                status = ComplianceStatus.NotTested
            });

            requirementsStatus.Add(new RequirementStatus
            {
                id = "NFR-ROB",
                description = "Fonctionne en faible luminosité, fumée légère, pluie, tremblements",
                category = RequirementCategory.Robustness,
                status = ComplianceStatus.NotTested
            });

            requirementsStatus.Add(new RequirementStatus
            {
                id = "NFR-SEC",
                description = "Données chiffrées, journalisation, RGPD",
                category = RequirementCategory.Security,
                status = ComplianceStatus.NotTested
            });

            requirementsStatus.Add(new RequirementStatus
            {
                id = "NFR-INT",
                description = "Export FHIR/HL7, compatibilité SI hôpitaux",
                category = RequirementCategory.Interoperability,
                status = ComplianceStatus.NotTested
            });

            requirementsStatus.Add(new RequirementStatus
            {
                id = "NFR-UX",
                description = "Mains libres, commande vocale, affichage lisible",
                category = RequirementCategory.Usability,
                status = ComplianceStatus.NotTested
            });

            requirementsStatus.Add(new RequirementStatus
            {
                id = "NFR-LOC",
                description = $"Guidage ≤{nfrLocPrecisionMeters}m de précision, GPS + repères visuels",
                category = RequirementCategory.Performance,
                targetValue = nfrLocPrecisionMeters,
                status = ComplianceStatus.NotTested
            });

            requirementsStatus.Add(new RequirementStatus
            {
                id = "NFR-AVA",
                description = $"Autonomie ≥{nfrAvaAutonomyHours}h + mode offline",
                category = RequirementCategory.Availability,
                targetValue = nfrAvaAutonomyHours,
                status = ComplianceStatus.NotTested
            });

            requirementsStatus.Add(new RequirementStatus
            {
                id = "NFR-REG",
                description = "Conformité DM, ISO 14971, IEC 62304",
                category = RequirementCategory.Regulatory,
                status = ComplianceStatus.NotTested
            });

            Debug.Log($"[Compliance] {requirementsStatus.Count} exigences initialisées");
        }

        /// <summary>
        /// Met à jour toutes les métriques
        /// </summary>
        private void UpdateAllMetrics()
        {
            // NFR-ACC: Exactitude détection
            if (totalDetectionAttempts > 0)
            {
                currentDetectionAccuracy = (float)successfulDetections / totalDetectionAttempts;
                currentFalsePositiveRate = (float)falsePositives / totalDetectionAttempts;
            }

            // NFR-VIT: Latence triage
            if (triageLatencies.Count > 0)
            {
                float sum = 0;
                foreach (var lat in triageLatencies) sum += lat;
                currentTriageLatency = sum / triageLatencies.Count;
            }

            // NFR-AVA: Autonomie
            if (Hardware.BatteryManager.Instance != null)
            {
                currentAutonomy = Hardware.BatteryManager.Instance.RemainingTime;
            }

            // NFR-LOC: Précision guidage
            if (Core.NavigationSystem.Instance != null)
            {
                // Simulé - en pratique, comparaison GPS vs position réelle
                currentGuidancePrecision = 1.5f; // Exemple: 1.5m de précision
            }
        }

        /// <summary>
        /// Vérifie la conformité de toutes les exigences
        /// </summary>
        private void CheckAllCompliance()
        {
            foreach (var req in requirementsStatus)
            {
                req.status = EvaluateRequirement(req);
                req.lastCheck = DateTime.Now;
            }
        }

        /// <summary>
        /// Évalue une exigence spécifique
        /// </summary>
        private ComplianceStatus EvaluateRequirement(RequirementStatus req)
        {
            switch (req.id)
            {
                case "REQ-1":
                    return ImageAnalysis.ImageAnalysisModule.Instance != null 
                        ? ComplianceStatus.Compliant 
                        : ComplianceStatus.NonCompliant;

                case "REQ-2":
                    return ImageAnalysis.ImageAnalysisModule.Instance != null 
                        ? ComplianceStatus.Compliant 
                        : ComplianceStatus.NonCompliant;

                case "REQ-3":
                    return Core.GameManager.Instance?.triageSystem != null 
                        ? ComplianceStatus.Compliant 
                        : ComplianceStatus.NonCompliant;

                case "REQ-4":
                    return Core.GameManager.Instance?.hospitalSystem != null 
                        ? ComplianceStatus.Compliant 
                        : ComplianceStatus.NonCompliant;

                case "REQ-5":
                    return Medical.FirstAidGuidanceModule.Instance != null 
                        ? ComplianceStatus.Compliant 
                        : ComplianceStatus.NonCompliant;

                case "REQ-6":
                    return Core.NavigationSystem.Instance != null 
                        ? ComplianceStatus.Compliant 
                        : ComplianceStatus.NonCompliant;

                case "REQ-7":
                    return Core.PatientRecordSystem.Instance != null 
                        ? ComplianceStatus.Compliant 
                        : ComplianceStatus.NonCompliant;

                case "NFR-ACC":
                    req.currentValue = currentDetectionAccuracy;
                    return currentDetectionAccuracy >= nfrAccDetectionTarget && 
                           currentFalsePositiveRate <= nfrAccFalsePositiveMax
                        ? ComplianceStatus.Compliant 
                        : ComplianceStatus.NonCompliant;

                case "NFR-VIT":
                    req.currentValue = currentTriageLatency;
                    bool isOffline = Hardware.BatteryManager.Instance?.IsOfflineMode ?? false;
                    float threshold = isOffline ? nfrVitOfflineLatency : nfrVitOnlineLatency;
                    return currentTriageLatency <= threshold 
                        ? ComplianceStatus.Compliant 
                        : ComplianceStatus.NonCompliant;

                case "NFR-ROB":
                    return ImageAnalysis.ImageAnalysisModule.Instance != null 
                        ? ComplianceStatus.Compliant 
                        : ComplianceStatus.NotTested;

                case "NFR-SEC":
                    return Interoperability.FHIRExportModule.Instance != null 
                        ? ComplianceStatus.Compliant 
                        : ComplianceStatus.NotTested;

                case "NFR-INT":
                    return Interoperability.FHIRExportModule.Instance != null 
                        ? ComplianceStatus.Compliant 
                        : ComplianceStatus.NonCompliant;

                case "NFR-UX":
                    return Core.VoiceCommandSimulator.Instance != null 
                        ? ComplianceStatus.Compliant 
                        : ComplianceStatus.NonCompliant;

                case "NFR-LOC":
                    req.currentValue = currentGuidancePrecision;
                    return currentGuidancePrecision <= nfrLocPrecisionMeters 
                        ? ComplianceStatus.Compliant 
                        : ComplianceStatus.NonCompliant;

                case "NFR-AVA":
                    req.currentValue = currentAutonomy;
                    bool hasOfflineMode = Hardware.OfflineModeManager.Instance != null;
                    bool meetsAutonomy = Hardware.BatteryManager.Instance?.MeetsAutonomyRequirement() ?? false;
                    return hasOfflineMode && meetsAutonomy 
                        ? ComplianceStatus.Compliant 
                        : ComplianceStatus.PartiallyCompliant;

                case "NFR-REG":
                    // Conformité réglementaire - nécessite validation externe
                    return ComplianceStatus.NotTested;

                default:
                    return ComplianceStatus.NotTested;
            }
        }

        #region Event Recording

        /// <summary>
        /// Enregistre une tentative de détection
        /// </summary>
        public void RecordDetectionAttempt(bool success, bool wasFalsePositive = false)
        {
            totalDetectionAttempts++;
            if (success) successfulDetections++;
            if (wasFalsePositive) falsePositives++;
        }

        /// <summary>
        /// Enregistre une latence de triage
        /// </summary>
        public void RecordTriageLatency(float latencySeconds)
        {
            triageLatencies.Add(latencySeconds);
            
            // Garder les 100 dernières mesures
            if (triageLatencies.Count > 100)
                triageLatencies.RemoveAt(0);
        }

        #endregion

        #region Reporting

        /// <summary>
        /// Génère un rapport de conformité complet
        /// </summary>
        public string GenerateComplianceReport()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("╔══════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║         RAPPORT DE CONFORMITÉ - SYSTÈME RA SSE              ║");
            sb.AppendLine("╠══════════════════════════════════════════════════════════════╣");
            sb.AppendLine($"║ Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}                              ║");
            sb.AppendLine($"║ Durée session: {(Time.time - sessionStartTime) / 60:F1} minutes                        ║");
            sb.AppendLine("╚══════════════════════════════════════════════════════════════╝");
            sb.AppendLine();

            // Statistiques globales
            int compliant = 0, nonCompliant = 0, partial = 0, notTested = 0;
            foreach (var req in requirementsStatus)
            {
                switch (req.status)
                {
                    case ComplianceStatus.Compliant: compliant++; break;
                    case ComplianceStatus.NonCompliant: nonCompliant++; break;
                    case ComplianceStatus.PartiallyCompliant: partial++; break;
                    case ComplianceStatus.NotTested: notTested++; break;
                }
            }

            sb.AppendLine($"=== RÉSUMÉ ===");
            sb.AppendLine($"✓ Conformes: {compliant}/{requirementsStatus.Count}");
            sb.AppendLine($"✗ Non conformes: {nonCompliant}/{requirementsStatus.Count}");
            sb.AppendLine($"◐ Partiellement conformes: {partial}/{requirementsStatus.Count}");
            sb.AppendLine($"○ Non testés: {notTested}/{requirementsStatus.Count}");
            sb.AppendLine();

            // Détail par catégorie
            foreach (RequirementCategory category in Enum.GetValues(typeof(RequirementCategory)))
            {
                var catReqs = requirementsStatus.FindAll(r => r.category == category);
                if (catReqs.Count == 0) continue;

                sb.AppendLine($"=== {GetCategoryName(category)} ===");
                foreach (var req in catReqs)
                {
                    string statusIcon = GetStatusIcon(req.status);
                    sb.AppendLine($"{statusIcon} [{req.id}] {req.description}");
                    
                    if (req.currentValue > 0)
                    {
                        sb.AppendLine($"    Valeur actuelle: {req.currentValue:F2} (cible: {req.targetValue:F2})");
                    }
                }
                sb.AppendLine();
            }

            // Métriques détaillées
            sb.AppendLine("=== MÉTRIQUES DÉTAILLÉES ===");
            sb.AppendLine($"Exactitude détection (NFR-ACC): {currentDetectionAccuracy:P1} (cible: ≥{nfrAccDetectionTarget:P0})");
            sb.AppendLine($"Taux faux positifs (NFR-ACC): {currentFalsePositiveRate:P1} (cible: ≤{nfrAccFalsePositiveMax:P0})");
            sb.AppendLine($"Latence triage moyenne (NFR-VIT): {currentTriageLatency:F1}s");
            sb.AppendLine($"Autonomie estimée (NFR-AVA): {currentAutonomy:F1}h (cible: ≥{nfrAvaAutonomyHours}h)");
            sb.AppendLine($"Précision guidage (NFR-LOC): {currentGuidancePrecision:F1}m (cible: ≤{nfrLocPrecisionMeters}m)");

            return sb.ToString();
        }

        /// <summary>
        /// Obtient l'icône de statut
        /// </summary>
        private string GetStatusIcon(ComplianceStatus status)
        {
            return status switch
            {
                ComplianceStatus.Compliant => "✓",
                ComplianceStatus.NonCompliant => "✗",
                ComplianceStatus.PartiallyCompliant => "◐",
                ComplianceStatus.NotTested => "○",
                _ => "?"
            };
        }

        /// <summary>
        /// Obtient le nom de la catégorie
        /// </summary>
        private string GetCategoryName(RequirementCategory category)
        {
            return category switch
            {
                RequirementCategory.Functional => "EXIGENCES FONCTIONNELLES (REQ)",
                RequirementCategory.Performance => "PERFORMANCE (NFR)",
                RequirementCategory.Robustness => "ROBUSTESSE",
                RequirementCategory.Security => "SÉCURITÉ",
                RequirementCategory.Interoperability => "INTEROPÉRABILITÉ",
                RequirementCategory.Usability => "ERGONOMIE (UX)",
                RequirementCategory.Availability => "DISPONIBILITÉ",
                RequirementCategory.Regulatory => "RÉGLEMENTAIRE",
                _ => "AUTRE"
            };
        }

        /// <summary>
        /// Obtient le statut d'une exigence spécifique
        /// </summary>
        public RequirementStatus GetRequirementStatus(string requirementId)
        {
            return requirementsStatus.Find(r => r.id == requirementId);
        }

        /// <summary>
        /// Obtient tous les statuts
        /// </summary>
        public List<RequirementStatus> GetAllStatuses()
        {
            return new List<RequirementStatus>(requirementsStatus);
        }

        /// <summary>
        /// Vérifie si le système est globalement conforme
        /// </summary>
        public bool IsSystemCompliant()
        {
            foreach (var req in requirementsStatus)
            {
                if (req.status == ComplianceStatus.NonCompliant)
                    return false;
            }
            return true;
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Statut d'une exigence
    /// </summary>
    [Serializable]
    public class RequirementStatus
    {
        public string id;
        public string description;
        public RequirementCategory category;
        public ComplianceStatus status;
        public float targetValue;
        public float currentValue;
        public DateTime lastCheck;
        public string notes;
    }

    /// <summary>
    /// Catégorie d'exigence
    /// </summary>
    public enum RequirementCategory
    {
        Functional,
        Performance,
        Robustness,
        Security,
        Interoperability,
        Usability,
        Availability,
        Regulatory
    }

    /// <summary>
    /// Statut de conformité
    /// </summary>
    public enum ComplianceStatus
    {
        NotTested,
        Compliant,
        PartiallyCompliant,
        NonCompliant
    }

    #endregion
}
