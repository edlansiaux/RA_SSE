using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RASSE.Core
{
    /// <summary>
    /// RequirementsManager - Gestion et traçabilité des exigences du système RA SSE
    /// Basé sur le document de spécification "Réalité Augmentée en Situations Sanitaires Exceptionnelles"
    /// </summary>
    public class RequirementsManager : MonoBehaviour
    {
        public static RequirementsManager Instance { get; private set; }

        [Header("=== TRAÇABILITÉ DES EXIGENCES ===")]
        [SerializeField] private bool enableRequirementTracking = true;
        [SerializeField] private bool logRequirementValidation = true;

        // Dictionnaire des exigences et leur état de satisfaction
        private Dictionary<string, RequirementStatus> requirementStatuses = new Dictionary<string, RequirementStatus>();

        // Liste complète des exigences selon le rapport
        public static readonly List<Requirement> SystemRequirements = new List<Requirement>
        {
            // ========== EXIGENCE SYSTÈME ==========
            new Requirement
            {
                Id = "REQ-0",
                Type = RequirementType.System,
                Description = "Le système RA SSE doit supporter un triage START bout-en-bout en situation sanitaire exceptionnelle.",
                Emitter = "Direction médicale / régulation nationale / opérateur terrain",
                Priority = RequirementPriority.Critical,
                ImplementedIn = new[] { "GameManager", "StartTriageSystem", "ARInterfaceController" }
            },

            // ========== EXIGENCES FONCTIONNELLES ==========
            new Requirement
            {
                Id = "REQ-1",
                Type = RequirementType.Functional,
                Description = "La LRA doit détecter et localiser les victimes dans le champ de vision (détection multi-cibles).",
                Emitter = "Personnel de secours / opérateur terrain",
                Priority = RequirementPriority.Critical,
                ImplementedIn = new[] { "VictimDetectionModule", "RescuerController", "ARInterfaceController" }
            },

            new Requirement
            {
                Id = "REQ-2",
                Type = RequirementType.Functional,
                Description = "La LRA doit estimer les constantes vitales par analyse d'image (FR, FC, couleur cutanée, mobilité...).",
                Emitter = "Personnel de secours, médecin régulateur",
                Priority = RequirementPriority.Critical,
                ImplementedIn = new[] { "VitalSignsAnalyzer", "VictimController" }
            },

            new Requirement
            {
                Id = "REQ-3",
                Type = RequirementType.Functional,
                Description = "Le système doit classifier l'état de la victime selon START (Rouge / Jaune / Vert / Noir).",
                Emitter = "Médecin régulateur / doctrine START",
                Priority = RequirementPriority.Critical,
                ImplementedIn = new[] { "StartTriageSystem", "StartClassificationModule" }
            },

            new Requirement
            {
                Id = "REQ-4",
                Type = RequirementType.Functional,
                Description = "Le système doit décider de l'orientation (destination optimale) selon START et capacités hospitalières.",
                Emitter = "SAMU / régulation / SMA",
                Priority = RequirementPriority.High,
                ImplementedIn = new[] { "HospitalCoordinationSystem", "SMACoordinator" }
            },

            new Requirement
            {
                Id = "REQ-5",
                Type = RequirementType.Functional,
                Description = "La LRA doit afficher des consignes : évacuation + premiers secours adaptés.",
                Emitter = "Personnel de secours / protocole médical",
                Priority = RequirementPriority.High,
                ImplementedIn = new[] { "ARInterfaceController", "GuidanceModule" }
            },

            new Requirement
            {
                Id = "REQ-6",
                Type = RequirementType.Functional,
                Description = "La LRA doit guider le soignant jusqu'à l'ambulance affectée (assignation + itinéraire AR).",
                Emitter = "Régulation / SMA",
                Priority = RequirementPriority.High,
                ImplementedIn = new[] { "NavigationSystem", "AmbulanceManager", "AmbulanceGuidanceModule" }
            },

            new Requirement
            {
                Id = "REQ-7",
                Type = RequirementType.Functional,
                Description = "Le système doit créer un dossier d'intervention complet (mesures, décisions, traces).",
                Emitter = "Hôpital / régulation / référentiel médical",
                Priority = RequirementPriority.High,
                ImplementedIn = new[] { "PatientRecordSystem", "PatientDatabaseModule" }
            },

            // ========== EXIGENCES NON FONCTIONNELLES ==========
            new Requirement
            {
                Id = "NFR-ACC",
                Type = RequirementType.NonFunctional,
                Description = "Exactitude : détection victimes ≥ 95 %, faux positifs ≤ 5 %.",
                Emitter = "Spécifications techniques / experts IA",
                Priority = RequirementPriority.Critical,
                ImplementedIn = new[] { "VictimDetectionModule" },
                ValidationCriteria = "DetectionAccuracy >= 0.95 && FalsePositiveRate <= 0.05"
            },

            new Requirement
            {
                Id = "NFR-VIT",
                Type = RequirementType.NonFunctional,
                Description = "Latence triage ≤ 30 s hors réseau, ≤ 10 s en réseau.",
                Emitter = "Opérateur terrain / soignants",
                Priority = RequirementPriority.Critical,
                ImplementedIn = new[] { "StartTriageSystem" },
                ValidationCriteria = "TriageLatencyOffline <= 30 && TriageLatencyOnline <= 10"
            },

            new Requirement
            {
                Id = "NFR-ROB",
                Type = RequirementType.NonFunctional,
                Description = "Fonctionne en faible luminosité, fumée légère, pluie, tremblements.",
                Emitter = "Opérateur terrain / contraintes opérationnelles",
                Priority = RequirementPriority.High,
                ImplementedIn = new[] { "EnvironmentalConditionsHandler", "SensorModule" }
            },

            new Requirement
            {
                Id = "NFR-SEC",
                Type = RequirementType.NonFunctional,
                Description = "Données chiffrées, journalisation, RGPD.",
                Emitter = "Régulation médicale / conformités légales",
                Priority = RequirementPriority.Critical,
                ImplementedIn = new[] { "SecurityModule", "PatientRecordSystem" }
            },

            new Requirement
            {
                Id = "NFR-INT",
                Type = RequirementType.NonFunctional,
                Description = "Export FHIR/HL7, compatibilité SI hôpitaux.",
                Emitter = "SI hospitalier / autorités médicales",
                Priority = RequirementPriority.Medium,
                ImplementedIn = new[] { "DataExportModule", "HospitalCoordinationSystem" }
            },

            new Requirement
            {
                Id = "NFR-UX",
                Type = RequirementType.NonFunctional,
                Description = "Mains libres, commande vocale, affichage lisible.",
                Emitter = "Soignants / ergonomie",
                Priority = RequirementPriority.High,
                ImplementedIn = new[] { "VoiceCommandSimulator", "ARInterfaceController" }
            },

            new Requirement
            {
                Id = "NFR-LOC",
                Type = RequirementType.NonFunctional,
                Description = "Guidage ≤ 2 m de précision, GPS + repères visuels.",
                Emitter = "Opérateur terrain / régulation",
                Priority = RequirementPriority.High,
                ImplementedIn = new[] { "NavigationSystem", "GeolocationModule" },
                ValidationCriteria = "GuidancePrecision <= 2.0"
            },

            new Requirement
            {
                Id = "NFR-AVA",
                Type = RequirementType.NonFunctional,
                Description = "Autonomie ≥ 8h + mode offline.",
                Emitter = "Soignants / équipe technique",
                Priority = RequirementPriority.High,
                ImplementedIn = new[] { "BatteryModule", "OfflineModeManager" },
                ValidationCriteria = "BatteryAutonomy >= 8.0"
            },

            new Requirement
            {
                Id = "NFR-REG",
                Type = RequirementType.NonFunctional,
                Description = "Conformité DM, ISO 14971, IEC 62304.",
                Emitter = "Réglementation médicale / marquage CE",
                Priority = RequirementPriority.Critical,
                ImplementedIn = new[] { "ComplianceManager" }
            }
        };

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeRequirementTracking();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeRequirementTracking()
        {
            foreach (var req in SystemRequirements)
            {
                requirementStatuses[req.Id] = new RequirementStatus
                {
                    RequirementId = req.Id,
                    IsSatisfied = false,
                    LastValidationTime = DateTime.MinValue,
                    ValidationCount = 0
                };
            }

            Debug.Log($"[RequirementsManager] {SystemRequirements.Count} exigences initialisées pour traçabilité");
        }

        /// <summary>
        /// Valide une exigence comme satisfaite
        /// </summary>
        public void ValidateRequirement(string requirementId, string validationContext = "")
        {
            if (!enableRequirementTracking) return;

            if (requirementStatuses.TryGetValue(requirementId, out var status))
            {
                status.IsSatisfied = true;
                status.LastValidationTime = DateTime.Now;
                status.ValidationCount++;
                status.LastValidationContext = validationContext;

                if (logRequirementValidation)
                {
                    var req = SystemRequirements.Find(r => r.Id == requirementId);
                    Debug.Log($"[REQ-VALIDATION] {requirementId} satisfait: {req?.Description.Substring(0, Math.Min(50, req.Description.Length))}...");
                }
            }
        }

        /// <summary>
        /// Invalide une exigence
        /// </summary>
        public void InvalidateRequirement(string requirementId, string reason = "")
        {
            if (requirementStatuses.TryGetValue(requirementId, out var status))
            {
                status.IsSatisfied = false;
                status.FailureReason = reason;
                
                Debug.LogWarning($"[REQ-INVALIDATION] {requirementId} non satisfait: {reason}");
            }
        }

        /// <summary>
        /// Obtient le rapport de conformité des exigences
        /// </summary>
        public RequirementsComplianceReport GetComplianceReport()
        {
            var report = new RequirementsComplianceReport
            {
                GeneratedAt = DateTime.Now,
                TotalRequirements = SystemRequirements.Count,
                SatisfiedRequirements = requirementStatuses.Count(kvp => kvp.Value.IsSatisfied),
                RequirementDetails = new List<RequirementComplianceDetail>()
            };

            foreach (var req in SystemRequirements)
            {
                var status = requirementStatuses.GetValueOrDefault(req.Id);
                report.RequirementDetails.Add(new RequirementComplianceDetail
                {
                    Requirement = req,
                    Status = status,
                    CompliancePercentage = status?.IsSatisfied == true ? 100f : 0f
                });
            }

            report.OverallCompliancePercentage = (float)report.SatisfiedRequirements / report.TotalRequirements * 100f;

            return report;
        }

        /// <summary>
        /// Génère un rapport textuel de conformité
        /// </summary>
        public string GenerateTextReport()
        {
            var report = GetComplianceReport();
            var text = "═══════════════════════════════════════════════════════════════\n";
            text += "        RAPPORT DE CONFORMITÉ DES EXIGENCES - RA SSE\n";
            text += "═══════════════════════════════════════════════════════════════\n\n";
            text += $"Date de génération: {report.GeneratedAt:dd/MM/yyyy HH:mm:ss}\n";
            text += $"Conformité globale: {report.OverallCompliancePercentage:F1}%\n";
            text += $"Exigences satisfaites: {report.SatisfiedRequirements}/{report.TotalRequirements}\n\n";

            text += "─── EXIGENCE SYSTÈME ───\n";
            foreach (var detail in report.RequirementDetails.Where(d => d.Requirement.Type == RequirementType.System))
            {
                text += FormatRequirementLine(detail);
            }

            text += "\n─── EXIGENCES FONCTIONNELLES ───\n";
            foreach (var detail in report.RequirementDetails.Where(d => d.Requirement.Type == RequirementType.Functional))
            {
                text += FormatRequirementLine(detail);
            }

            text += "\n─── EXIGENCES NON FONCTIONNELLES ───\n";
            foreach (var detail in report.RequirementDetails.Where(d => d.Requirement.Type == RequirementType.NonFunctional))
            {
                text += FormatRequirementLine(detail);
            }

            text += "\n═══════════════════════════════════════════════════════════════\n";
            return text;
        }

        private string FormatRequirementLine(RequirementComplianceDetail detail)
        {
            string status = detail.Status?.IsSatisfied == true ? "✓" : "✗";
            string shortDesc = detail.Requirement.Description.Length > 60 
                ? detail.Requirement.Description.Substring(0, 57) + "..." 
                : detail.Requirement.Description;
            return $"[{status}] {detail.Requirement.Id}: {shortDesc}\n";
        }

        /// <summary>
        /// Obtient les exigences par type
        /// </summary>
        public List<Requirement> GetRequirementsByType(RequirementType type)
        {
            return SystemRequirements.Where(r => r.Type == type).ToList();
        }

        /// <summary>
        /// Obtient les exigences par priorité
        /// </summary>
        public List<Requirement> GetRequirementsByPriority(RequirementPriority priority)
        {
            return SystemRequirements.Where(r => r.Priority == priority).ToList();
        }

        /// <summary>
        /// Vérifie si une exigence est satisfaite
        /// </summary>
        public bool IsRequirementSatisfied(string requirementId)
        {
            return requirementStatuses.TryGetValue(requirementId, out var status) && status.IsSatisfied;
        }

        /// <summary>
        /// Trace un événement lié à une exigence (pour audit)
        /// </summary>
        public void TraceRequirementEvent(string requirementId, string eventDescription)
        {
            if (!enableRequirementTracking) return;

            var traceEntry = new RequirementTraceEntry
            {
                Timestamp = DateTime.Now,
                RequirementId = requirementId,
                Event = eventDescription,
                SessionId = GameManager.Instance?.SessionId ?? "N/A"
            };

            // En production, ceci serait sauvegardé dans un fichier de log
            Debug.Log($"[REQ-TRACE] {traceEntry.Timestamp:HH:mm:ss} | {requirementId} | {eventDescription}");
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // STRUCTURES DE DONNÉES POUR LES EXIGENCES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Définition d'une exigence système
    /// </summary>
    [Serializable]
    public class Requirement
    {
        public string Id;
        public RequirementType Type;
        public string Description;
        public string Emitter;
        public RequirementPriority Priority;
        public string[] ImplementedIn;
        public string ValidationCriteria;
        public string[] DependsOn;
    }

    /// <summary>
    /// Type d'exigence
    /// </summary>
    public enum RequirementType
    {
        System,         // REQ-0
        Functional,     // REQ-1 à REQ-7
        NonFunctional   // NFR-*
    }

    /// <summary>
    /// Priorité d'exigence
    /// </summary>
    public enum RequirementPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    /// État de satisfaction d'une exigence
    /// </summary>
    [Serializable]
    public class RequirementStatus
    {
        public string RequirementId;
        public bool IsSatisfied;
        public DateTime LastValidationTime;
        public int ValidationCount;
        public string LastValidationContext;
        public string FailureReason;
    }

    /// <summary>
    /// Rapport de conformité des exigences
    /// </summary>
    [Serializable]
    public class RequirementsComplianceReport
    {
        public DateTime GeneratedAt;
        public int TotalRequirements;
        public int SatisfiedRequirements;
        public float OverallCompliancePercentage;
        public List<RequirementComplianceDetail> RequirementDetails;
    }

    /// <summary>
    /// Détail de conformité d'une exigence
    /// </summary>
    [Serializable]
    public class RequirementComplianceDetail
    {
        public Requirement Requirement;
        public RequirementStatus Status;
        public float CompliancePercentage;
    }

    /// <summary>
    /// Entrée de traçabilité pour audit
    /// </summary>
    [Serializable]
    public class RequirementTraceEntry
    {
        public DateTime Timestamp;
        public string RequirementId;
        public string Event;
        public string SessionId;
    }
}
