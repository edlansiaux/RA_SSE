using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace RASSE.Interoperability
{
    /// <summary>
    /// FHIRExportModule - Export des données patient au format FHIR/HL7 (NFR-INT)
    /// Assure la compatibilité avec les Systèmes d'Information hospitaliers
    /// Standards: FHIR R4, HL7 v2.5
    /// </summary>
    public class FHIRExportModule : MonoBehaviour
    {
        public static FHIRExportModule Instance { get; private set; }

        [Header("=== CONFIGURATION ===")]
        [SerializeField] private string organizationId = "RA-SSE-001";
        [SerializeField] private string systemVersion = "1.0.0";
        [SerializeField] private ExportFormat defaultFormat = ExportFormat.FHIR_JSON;

        [Header("=== ENDPOINTS ===")]
        [SerializeField] private string fhirServerUrl = "https://hospital.example.com/fhir/r4";
        [SerializeField] private string hl7ServerUrl = "mllp://hospital.example.com:2575";

        [Header("=== SÉCURITÉ (NFR-SEC) ===")]
        [SerializeField] private bool encryptionEnabled = true;
        [SerializeField] private bool auditLoggingEnabled = true;

        private List<ExportLog> exportLogs = new List<ExportLog>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #region FHIR Export

        /// <summary>
        /// Exporte un patient au format FHIR R4 (Patient Resource)
        /// </summary>
        public string ExportPatientFHIR(Core.VictimController victim)
        {
            if (victim == null) return null;

            var fhirPatient = new FHIRPatient
            {
                resourceType = "Patient",
                id = victim.PatientId,
                meta = new FHIRMeta
                {
                    versionId = "1",
                    lastUpdated = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    source = $"urn:uuid:{organizationId}"
                },
                identifier = new List<FHIRIdentifier>
                {
                    new FHIRIdentifier
                    {
                        system = "urn:oid:1.2.250.1.213.1.4.8", // INS-C France
                        value = victim.PatientId
                    }
                },
                name = new List<FHIRHumanName>
                {
                    new FHIRHumanName
                    {
                        use = "official",
                        family = victim.lastName,
                        given = new List<string> { victim.firstName }
                    }
                },
                gender = victim.gender == Core.Gender.Male ? "male" : (victim.gender == Core.Gender.Female ? "female" : "unknown"),
                birthDate = CalculateBirthDate(victim.age)
            };

            string json = JsonUtility.ToJson(fhirPatient, true);
            LogExport(victim.PatientId, ExportFormat.FHIR_JSON, "Patient");
            return json;
        }

        /// <summary>
        /// Exporte une observation (constantes vitales) au format FHIR
        /// </summary>
        public string ExportVitalSignsFHIR(Core.VictimController victim)
        {
            if (victim == null) return null;

            var vitals = victim.VitalSigns;
            var observations = new FHIRBundle
            {
                resourceType = "Bundle",
                type = "collection",
                entry = new List<FHIRBundleEntry>()
            };

            // Fréquence respiratoire
            observations.entry.Add(CreateObservationEntry(
                victim.PatientId,
                "9279-1", // LOINC code
                "Respiratory rate",
                vitals.respiratoryRate,
                "/min"
            ));

            // Fréquence cardiaque
            observations.entry.Add(CreateObservationEntry(
                victim.PatientId,
                "8867-4",
                "Heart rate",
                vitals.heartRate,
                "beats/min"
            ));

            // SpO2
            observations.entry.Add(CreateObservationEntry(
                victim.PatientId,
                "2708-6",
                "Oxygen saturation",
                (int)vitals.spO2,
                "%"
            ));

            // Température
            observations.entry.Add(CreateObservationEntry(
                victim.PatientId,
                "8310-5",
                "Body temperature",
                vitals.temperature,
                "Cel"
            ));

            // Glasgow
            observations.entry.Add(CreateObservationEntry(
                victim.PatientId,
                "9269-2",
                "Glasgow coma score",
                vitals.glasgowComaScale,
                "{score}"
            ));

            string json = JsonUtility.ToJson(observations, true);
            LogExport(victim.PatientId, ExportFormat.FHIR_JSON, "Observations");
            return json;
        }

        /// <summary>
        /// Exporte le triage au format FHIR (Condition Resource)
        /// </summary>
        public string ExportTriageFHIR(Core.VictimController victim)
        {
            if (victim == null) return null;

            var triageCondition = new FHIRCondition
            {
                resourceType = "Condition",
                id = $"triage-{victim.PatientId}",
                clinicalStatus = new FHIRCodeableConcept
                {
                    coding = new List<FHIRCoding>
                    {
                        new FHIRCoding
                        {
                            system = "http://terminology.hl7.org/CodeSystem/condition-clinical",
                            code = "active"
                        }
                    }
                },
                category = new List<FHIRCodeableConcept>
                {
                    new FHIRCodeableConcept
                    {
                        coding = new List<FHIRCoding>
                        {
                            new FHIRCoding
                            {
                                system = "http://snomed.info/sct",
                                code = "225390008",
                                display = "Triage"
                            }
                        }
                    }
                },
                severity = new FHIRCodeableConcept
                {
                    coding = new List<FHIRCoding>
                    {
                        new FHIRCoding
                        {
                            system = "urn:oid:2.16.840.1.113883.6.1", // START Triage
                            code = GetSTARTCode(victim.TriageCategory),
                            display = victim.TriageCategory.GetFrenchName()
                        }
                    }
                },
                subject = new FHIRReference
                {
                    reference = $"Patient/{victim.PatientId}"
                },
                recordedDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            string json = JsonUtility.ToJson(triageCondition, true);
            LogExport(victim.PatientId, ExportFormat.FHIR_JSON, "Triage");
            return json;
        }

        /// <summary>
        /// Crée une entrée d'observation FHIR
        /// </summary>
        private FHIRBundleEntry CreateObservationEntry(string patientId, string loincCode, string display, float value, string unit)
        {
            return new FHIRBundleEntry
            {
                resource = new FHIRObservation
                {
                    resourceType = "Observation",
                    id = $"obs-{patientId}-{loincCode}",
                    status = "final",
                    code = new FHIRCodeableConcept
                    {
                        coding = new List<FHIRCoding>
                        {
                            new FHIRCoding
                            {
                                system = "http://loinc.org",
                                code = loincCode,
                                display = display
                            }
                        }
                    },
                    subject = new FHIRReference { reference = $"Patient/{patientId}" },
                    effectiveDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    valueQuantity = new FHIRQuantity
                    {
                        value = value,
                        unit = unit,
                        system = "http://unitsofmeasure.org",
                        code = unit
                    }
                }
            };
        }

        #endregion

        #region HL7 v2.5 Export

        /// <summary>
        /// Exporte un message ADT^A04 (Registration) au format HL7 v2.5
        /// </summary>
        public string ExportPatientHL7(Core.VictimController victim)
        {
            if (victim == null) return null;

            var sb = new StringBuilder();
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string messageId = Guid.NewGuid().ToString("N").Substring(0, 20);

            // MSH - Message Header
            sb.AppendLine($"MSH|^~\\&|RA-SSE|{organizationId}|HIS|HOSPITAL|{timestamp}||ADT^A04|{messageId}|P|2.5|||AL|NE||8859/1");

            // EVN - Event Type
            sb.AppendLine($"EVN|A04|{timestamp}|||{organizationId}");

            // PID - Patient Identification
            string gender = victim.gender == Core.Gender.Male ? "M" : (victim.gender == Core.Gender.Female ? "F" : "U");
            string birthDate = CalculateBirthDate(victim.age).Replace("-", "");
            sb.AppendLine($"PID|1||{victim.PatientId}^^^RA-SSE||{victim.lastName}^{victim.firstName}||{birthDate}|{gender}|||||||||||||||||||");

            // PV1 - Patient Visit
            sb.AppendLine($"PV1|1|E||U|||||||||||||||{victim.PatientId}|||||||||||||||||||||||||||{timestamp}");

            // OBX - Observations (constantes vitales)
            var vitals = victim.VitalSigns;
            int obxSeq = 1;

            sb.AppendLine($"OBX|{obxSeq++}|NM|9279-1^Respiratory rate^LN||{vitals.respiratoryRate}|/min||N|||F|||{timestamp}");
            sb.AppendLine($"OBX|{obxSeq++}|NM|8867-4^Heart rate^LN||{vitals.heartRate}|bpm||N|||F|||{timestamp}");
            sb.AppendLine($"OBX|{obxSeq++}|NM|2708-6^Oxygen saturation^LN||{vitals.spO2:F0}|%||N|||F|||{timestamp}");
            sb.AppendLine($"OBX|{obxSeq++}|NM|8310-5^Body temperature^LN||{vitals.temperature:F1}|Cel||N|||F|||{timestamp}");
            sb.AppendLine($"OBX|{obxSeq++}|NM|9269-2^Glasgow coma score^LN||{vitals.glasgowComaScale}|{{score}}||N|||F|||{timestamp}");

            // DG1 - Diagnosis (Triage START)
            string triageCode = GetSTARTCode(victim.TriageCategory);
            sb.AppendLine($"DG1|1||{triageCode}^{victim.TriageCategory.GetFrenchName()}^START||{timestamp}|W");

            string hl7Message = sb.ToString();
            LogExport(victim.PatientId, ExportFormat.HL7_V25, "ADT^A04");
            return hl7Message;
        }

        /// <summary>
        /// Exporte un message ORU^R01 (Observation Result) au format HL7 v2.5
        /// </summary>
        public string ExportObservationsHL7(Core.VictimController victim)
        {
            if (victim == null) return null;

            var sb = new StringBuilder();
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string messageId = Guid.NewGuid().ToString("N").Substring(0, 20);

            // MSH
            sb.AppendLine($"MSH|^~\\&|RA-SSE|{organizationId}|HIS|HOSPITAL|{timestamp}||ORU^R01|{messageId}|P|2.5|||AL|NE||8859/1");

            // PID
            sb.AppendLine($"PID|1||{victim.PatientId}^^^RA-SSE||{victim.lastName}^{victim.firstName}");

            // OBR - Observation Request
            sb.AppendLine($"OBR|1||{victim.PatientId}-VS|85354-9^Vital signs^LN|||{timestamp}||||||||||||||{timestamp}|||F");

            // OBX - Observations
            var vitals = victim.VitalSigns;
            int obxSeq = 1;

            sb.AppendLine($"OBX|{obxSeq++}|NM|9279-1^Respiratory rate^LN||{vitals.respiratoryRate}|/min||{GetAbnormalFlag(vitals.respiratoryRate, 12, 20)}|||F|||{timestamp}");
            sb.AppendLine($"OBX|{obxSeq++}|NM|8867-4^Heart rate^LN||{vitals.heartRate}|bpm||{GetAbnormalFlag(vitals.heartRate, 60, 100)}|||F|||{timestamp}");
            sb.AppendLine($"OBX|{obxSeq++}|NM|2708-6^Oxygen saturation^LN||{vitals.spO2:F0}|%||{(vitals.spO2 < 95 ? "L" : "N")}|||F|||{timestamp}");
            sb.AppendLine($"OBX|{obxSeq++}|NM|8480-6^Systolic BP^LN||{vitals.systolicBP}|mmHg||{GetAbnormalFlag(vitals.systolicBP, 90, 140)}|||F|||{timestamp}");
            sb.AppendLine($"OBX|{obxSeq++}|NM|8462-4^Diastolic BP^LN||{vitals.diastolicBP}|mmHg||{GetAbnormalFlag(vitals.diastolicBP, 60, 90)}|||F|||{timestamp}");
            sb.AppendLine($"OBX|{obxSeq++}|NM|9269-2^Glasgow coma score^LN||{vitals.glasgowComaScale}|{{score}}||{(vitals.glasgowComaScale < 15 ? "L" : "N")}|||F|||{timestamp}");

            string hl7Message = sb.ToString();
            LogExport(victim.PatientId, ExportFormat.HL7_V25, "ORU^R01");
            return hl7Message;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Exporte le dossier complet d'intervention (REQ-7)
        /// </summary>
        public InterventionExport ExportFullIntervention(List<Core.VictimController> victims, string scenarioId)
        {
            var export = new InterventionExport
            {
                exportId = Guid.NewGuid().ToString(),
                exportTimestamp = DateTime.UtcNow,
                scenarioId = scenarioId,
                systemVersion = systemVersion,
                totalVictims = victims.Count,
                fhirBundle = new List<string>(),
                hl7Messages = new List<string>()
            };

            foreach (var victim in victims)
            {
                if (victim == null) continue;

                // FHIR exports
                export.fhirBundle.Add(ExportPatientFHIR(victim));
                export.fhirBundle.Add(ExportVitalSignsFHIR(victim));
                export.fhirBundle.Add(ExportTriageFHIR(victim));

                // HL7 exports
                export.hl7Messages.Add(ExportPatientHL7(victim));
                export.hl7Messages.Add(ExportObservationsHL7(victim));
            }

            // Statistiques par catégorie
            export.statistics = new TriageStatistics
            {
                redCount = victims.FindAll(v => v.TriageCategory == Core.StartCategory.Red).Count,
                yellowCount = victims.FindAll(v => v.TriageCategory == Core.StartCategory.Yellow).Count,
                greenCount = victims.FindAll(v => v.TriageCategory == Core.StartCategory.Green).Count,
                blackCount = victims.FindAll(v => v.TriageCategory == Core.StartCategory.Black).Count
            };

            Debug.Log($"[FHIRExport] Export complet généré: {export.fhirBundle.Count} ressources FHIR, {export.hl7Messages.Count} messages HL7");
            return export;
        }

        /// <summary>
        /// Sauvegarde l'export dans un fichier
        /// </summary>
        public void SaveExportToFile(string content, string filename, ExportFormat format)
        {
            string extension = format == ExportFormat.FHIR_JSON ? ".json" : ".hl7";
            string fullPath = Path.Combine(Application.persistentDataPath, "Exports", filename + extension);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllText(fullPath, content);

            Debug.Log($"[FHIRExport] Fichier sauvegardé: {fullPath}");
        }

        /// <summary>
        /// Obtient le code START pour FHIR/HL7
        /// </summary>
        private string GetSTARTCode(Core.StartCategory category)
        {
            return category switch
            {
                Core.StartCategory.Red => "START-R",
                Core.StartCategory.Yellow => "START-Y",
                Core.StartCategory.Green => "START-G",
                Core.StartCategory.Black => "START-B",
                _ => "START-U"
            };
        }

        /// <summary>
        /// Calcule la date de naissance approximative
        /// </summary>
        private string CalculateBirthDate(int age)
        {
            return DateTime.Now.AddYears(-age).ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// Détermine le flag d'anomalie HL7
        /// </summary>
        private string GetAbnormalFlag(float value, float low, float high)
        {
            if (value < low) return "L";
            if (value > high) return "H";
            return "N";
        }

        /// <summary>
        /// Log l'export pour audit (NFR-SEC)
        /// </summary>
        private void LogExport(string patientId, ExportFormat format, string resourceType)
        {
            if (!auditLoggingEnabled) return;

            exportLogs.Add(new ExportLog
            {
                timestamp = DateTime.UtcNow,
                patientId = patientId,
                format = format,
                resourceType = resourceType,
                success = true
            });
        }

        /// <summary>
        /// Obtient le journal d'audit
        /// </summary>
        public List<ExportLog> GetAuditLog()
        {
            return new List<ExportLog>(exportLogs);
        }

        #endregion
    }

    #region FHIR Data Structures

    [Serializable] public class FHIRPatient
    {
        public string resourceType;
        public string id;
        public FHIRMeta meta;
        public List<FHIRIdentifier> identifier;
        public List<FHIRHumanName> name;
        public string gender;
        public string birthDate;
    }

    [Serializable] public class FHIRMeta
    {
        public string versionId;
        public string lastUpdated;
        public string source;
    }

    [Serializable] public class FHIRIdentifier
    {
        public string system;
        public string value;
    }

    [Serializable] public class FHIRHumanName
    {
        public string use;
        public string family;
        public List<string> given;
    }

    [Serializable] public class FHIRBundle
    {
        public string resourceType;
        public string type;
        public List<FHIRBundleEntry> entry;
    }

    [Serializable] public class FHIRBundleEntry
    {
        public FHIRObservation resource;
    }

    [Serializable] public class FHIRObservation
    {
        public string resourceType;
        public string id;
        public string status;
        public FHIRCodeableConcept code;
        public FHIRReference subject;
        public string effectiveDateTime;
        public FHIRQuantity valueQuantity;
    }

    [Serializable] public class FHIRCondition
    {
        public string resourceType;
        public string id;
        public FHIRCodeableConcept clinicalStatus;
        public List<FHIRCodeableConcept> category;
        public FHIRCodeableConcept severity;
        public FHIRReference subject;
        public string recordedDate;
    }

    [Serializable] public class FHIRCodeableConcept
    {
        public List<FHIRCoding> coding;
    }

    [Serializable] public class FHIRCoding
    {
        public string system;
        public string code;
        public string display;
    }

    [Serializable] public class FHIRReference
    {
        public string reference;
    }

    [Serializable] public class FHIRQuantity
    {
        public float value;
        public string unit;
        public string system;
        public string code;
    }

    #endregion

    #region Export Data Structures

    public enum ExportFormat
    {
        FHIR_JSON,
        FHIR_XML,
        HL7_V25,
        HL7_V3
    }

    [Serializable]
    public class InterventionExport
    {
        public string exportId;
        public DateTime exportTimestamp;
        public string scenarioId;
        public string systemVersion;
        public int totalVictims;
        public List<string> fhirBundle;
        public List<string> hl7Messages;
        public TriageStatistics statistics;
    }

    [Serializable]
    public class TriageStatistics
    {
        public int redCount;
        public int yellowCount;
        public int greenCount;
        public int blackCount;
    }

    [Serializable]
    public class ExportLog
    {
        public DateTime timestamp;
        public string patientId;
        public ExportFormat format;
        public string resourceType;
        public bool success;
        public string errorMessage;
    }

    #endregion
}
