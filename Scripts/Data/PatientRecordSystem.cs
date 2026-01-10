using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace RASSE.Core
{
    /// <summary>
    /// PatientRecordSystem - Gestion des dossiers d'intervention
    /// Crée et maintient les dossiers médicaux de chaque victime
    /// </summary>
    public class PatientRecordSystem : MonoBehaviour
    {
        [Header("=== CONFIGURATION ===")]
        [SerializeField] private bool autoSaveRecords = true;
        [SerializeField] private float autoSaveInterval = 60f;
        [SerializeField] private string recordsPath = "PatientRecords";

        [Header("=== DOSSIERS ===")]
        [SerializeField] private List<PatientRecord> patientRecords = new List<PatientRecord>();

        private float lastSaveTime;

        public static PatientRecordSystem Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (autoSaveRecords && Time.time - lastSaveTime > autoSaveInterval)
            {
                SaveAllRecords();
                lastSaveTime = Time.time;
            }
        }

        /// <summary>
        /// Crée un nouveau dossier patient
        /// </summary>
        public PatientRecord CreateRecord(VictimController victim)
        {
            if (victim == null) return null;

            // Vérifier si un dossier existe déjà
            PatientRecord existingRecord = GetRecord(victim.PatientId);
            if (existingRecord != null)
            {
                Debug.Log($"[PatientRecord] Dossier existant pour {victim.PatientId}");
                return existingRecord;
            }

            // Créer un nouveau dossier
            PatientRecord record = new PatientRecord
            {
                patientId = victim.PatientId,
                creationTime = DateTime.Now,
                personalInfo = new PersonalInfo
                {
                    firstName = victim.firstName,
                    lastName = victim.lastName,
                    age = victim.age,
                    gender = victim.gender
                },
                initialVitals = CloneVitalSigns(victim.VitalSigns),
                injuryInfo = new InjuryInfo
                {
                    primaryInjury = victim.primaryInjury,
                    location = victim.injuryLocation,
                    severity = victim.injurySeverity
                }
            };

            // Ajouter l'entrée initiale
            record.AddEntry("Création du dossier", "Système", EntryType.System);
            record.AddEntry($"Détection de la victime", "Secouriste", EntryType.Detection);

            patientRecords.Add(record);
            
            Debug.Log($"[PatientRecord] Dossier créé pour {victim.PatientId}");
            return record;
        }

        /// <summary>
        /// Enregistre le triage d'une victime
        /// </summary>
        public void RecordTriage(VictimController victim, StartCategory category, bool isManual)
        {
            PatientRecord record = GetOrCreateRecord(victim);
            if (record == null) return;

            record.triageCategory = category;
            record.triageTime = DateTime.Now;
            record.wasManualTriage = isManual;

            string description = isManual 
                ? $"Triage MANUEL: {category}" 
                : $"Triage AUTO: {category}";
            
            record.AddEntry(description, "Secouriste", EntryType.Triage);

            // Enregistrer les vitaux au moment du triage
            record.triageVitals = CloneVitalSigns(victim.VitalSigns);
        }

        /// <summary>
        /// Enregistre une action médicale
        /// </summary>
        public void RecordAction(VictimController victim, string action, string performer)
        {
            PatientRecord record = GetOrCreateRecord(victim);
            if (record == null) return;

            record.AddEntry(action, performer, EntryType.Action);
        }

        /// <summary>
        /// Enregistre l'affectation d'un hôpital
        /// </summary>
        public void RecordHospitalAssignment(VictimController victim, HospitalData hospital)
        {
            PatientRecord record = GetOrCreateRecord(victim);
            if (record == null) return;

            record.assignedHospital = hospital.hospitalName;
            record.hospitalAssignmentTime = DateTime.Now;

            record.AddEntry($"Affectation: {hospital.hospitalName}", "Régulation", EntryType.Assignment);
        }

        /// <summary>
        /// Enregistre l'affectation d'une ambulance
        /// </summary>
        public void RecordAmbulanceAssignment(VictimController victim, AmbulanceController ambulance)
        {
            PatientRecord record = GetOrCreateRecord(victim);
            if (record == null) return;

            record.assignedAmbulance = ambulance.AmbulanceId;
            record.AddEntry($"Ambulance: {ambulance.AmbulanceId}", "Régulation", EntryType.Assignment);
        }

        /// <summary>
        /// Enregistre l'évacuation
        /// </summary>
        public void RecordEvacuation(VictimController victim)
        {
            PatientRecord record = GetOrCreateRecord(victim);
            if (record == null) return;

            record.evacuationTime = DateTime.Now;
            record.isEvacuated = true;
            record.evacuationVitals = CloneVitalSigns(victim.VitalSigns);

            record.AddEntry("Évacuation effectuée", "Ambulance", EntryType.Evacuation);
        }

        /// <summary>
        /// Obtient un dossier patient existant
        /// </summary>
        public PatientRecord GetRecord(string patientId)
        {
            return patientRecords.Find(r => r.patientId == patientId);
        }

        /// <summary>
        /// Obtient ou crée un dossier patient
        /// </summary>
        private PatientRecord GetOrCreateRecord(VictimController victim)
        {
            PatientRecord record = GetRecord(victim.PatientId);
            if (record == null)
            {
                record = CreateRecord(victim);
            }
            return record;
        }

        /// <summary>
        /// Sauvegarde tous les dossiers
        /// </summary>
        public void SaveAllRecords()
        {
            string path = Path.Combine(Application.persistentDataPath, recordsPath);
            Directory.CreateDirectory(path);

            foreach (var record in patientRecords)
            {
                string json = JsonUtility.ToJson(record, true);
                string filePath = Path.Combine(path, $"{record.patientId}.json");
                File.WriteAllText(filePath, json);
            }

            Debug.Log($"[PatientRecord] {patientRecords.Count} dossiers sauvegardés");
        }

        /// <summary>
        /// Génère un rapport texte du dossier
        /// </summary>
        public string GenerateReport(PatientRecord record)
        {
            if (record == null) return "";

            string report = "═══════════════════════════════════════\n";
            report += "     DOSSIER D'INTERVENTION - RA SSE    \n";
            report += "═══════════════════════════════════════\n\n";

            report += $"ID Patient: {record.patientId}\n";
            report += $"Date: {record.creationTime:dd/MM/yyyy HH:mm}\n\n";

            report += "─── INFORMATIONS PERSONNELLES ───\n";
            report += $"Nom: {record.personalInfo.lastName} {record.personalInfo.firstName}\n";
            report += $"Âge: {record.personalInfo.age} ans\n";
            report += $"Genre: {record.personalInfo.gender}\n\n";

            report += "─── TRIAGE START ───\n";
            report += $"Catégorie: {record.triageCategory}\n";
            report += $"Heure: {record.triageTime:HH:mm:ss}\n";
            report += $"Mode: {(record.wasManualTriage ? "Manuel" : "Automatique")}\n\n";

            report += "─── CONSTANTES INITIALES ───\n";
            report += FormatVitals(record.initialVitals);

            report += "\n─── BLESSURES ───\n";
            report += $"Type: {record.injuryInfo.primaryInjury}\n";
            report += $"Localisation: {record.injuryInfo.location}\n";
            report += $"Sévérité: {record.injuryInfo.severity}\n\n";

            report += "─── ORIENTATION ───\n";
            report += $"Hôpital: {record.assignedHospital}\n";
            report += $"Ambulance: {record.assignedAmbulance}\n";
            report += $"Évacuation: {(record.isEvacuated ? record.evacuationTime.ToString("HH:mm:ss") : "En attente")}\n\n";

            report += "─── CHRONOLOGIE ───\n";
            foreach (var entry in record.entries)
            {
                report += $"[{entry.timestamp:HH:mm:ss}] {entry.performer}: {entry.description}\n";
            }

            report += "\n═══════════════════════════════════════\n";

            return report;
        }

        private string FormatVitals(VitalSigns vitals)
        {
            if (vitals == null) return "Non disponible\n";

            return $"FR: {vitals.respiratoryRate}/min | FC: {vitals.heartRate} bpm\n" +
                   $"SpO2: {vitals.oxygenSaturation}% | RC: {vitals.capillaryRefillTime}s\n" +
                   $"TA: {vitals.systolicBloodPressure}/{vitals.diastolicBloodPressure} mmHg\n" +
                   $"GCS: {vitals.glasgowComaScale}/15\n";
        }

        private VitalSigns CloneVitalSigns(VitalSigns original)
        {
            if (original == null) return new VitalSigns();

            return new VitalSigns
            {
                isBreathing = original.isBreathing,
                breathingAfterAirwayManeuver = original.breathingAfterAirwayManeuver,
                respiratoryRate = original.respiratoryRate,
                heartRate = original.heartRate,
                capillaryRefillTime = original.capillaryRefillTime,
                hasRadialPulse = original.hasRadialPulse,
                oxygenSaturation = original.oxygenSaturation,
                systolicBloodPressure = original.systolicBloodPressure,
                diastolicBloodPressure = original.diastolicBloodPressure,
                canFollowCommands = original.canFollowCommands,
                canWalk = original.canWalk,
                isConscious = original.isConscious,
                glasgowComaScale = original.glasgowComaScale,
                bodyTemperature = original.bodyTemperature,
                hasVisibleBleeding = original.hasVisibleBleeding,
                hasFracture = original.hasFracture,
                hasBurns = original.hasBurns,
                painLevel = original.painLevel
            };
        }

        /// <summary>
        /// Obtient tous les dossiers
        /// </summary>
        public List<PatientRecord> GetAllRecords()
        {
            return new List<PatientRecord>(patientRecords);
        }

        /// <summary>
        /// Obtient les statistiques des dossiers
        /// </summary>
        public RecordStatistics GetStatistics()
        {
            return new RecordStatistics
            {
                totalRecords = patientRecords.Count,
                redCount = patientRecords.Count(r => r.triageCategory == StartCategory.Red),
                yellowCount = patientRecords.Count(r => r.triageCategory == StartCategory.Yellow),
                greenCount = patientRecords.Count(r => r.triageCategory == StartCategory.Green),
                blackCount = patientRecords.Count(r => r.triageCategory == StartCategory.Black),
                evacuatedCount = patientRecords.Count(r => r.isEvacuated),
                manualTriageCount = patientRecords.Count(r => r.wasManualTriage)
            };
        }
    }

    /// <summary>
    /// Dossier patient complet
    /// </summary>
    [System.Serializable]
    public class PatientRecord
    {
        public string patientId;
        public DateTime creationTime;
        
        // Informations personnelles
        public PersonalInfo personalInfo;
        
        // Triage
        public StartCategory triageCategory;
        public DateTime triageTime;
        public bool wasManualTriage;
        
        // Constantes
        public VitalSigns initialVitals;
        public VitalSigns triageVitals;
        public VitalSigns evacuationVitals;
        
        // Blessures
        public InjuryInfo injuryInfo;
        
        // Orientation
        public string assignedHospital;
        public DateTime hospitalAssignmentTime;
        public string assignedAmbulance;
        
        // Évacuation
        public bool isEvacuated;
        public DateTime evacuationTime;
        
        // Chronologie
        public List<RecordEntry> entries = new List<RecordEntry>();

        public void AddEntry(string description, string performer, EntryType type)
        {
            entries.Add(new RecordEntry
            {
                timestamp = DateTime.Now,
                description = description,
                performer = performer,
                entryType = type
            });
        }
    }

    [System.Serializable]
    public class PersonalInfo
    {
        public string firstName;
        public string lastName;
        public int age;
        public Gender gender;
        public string allergies;
        public string medicalHistory;
    }

    [System.Serializable]
    public class InjuryInfo
    {
        public InjuryType primaryInjury;
        public InjuryLocation location;
        public InjurySeverity severity;
        public string description;
    }

    [System.Serializable]
    public class RecordEntry
    {
        public DateTime timestamp;
        public string description;
        public string performer;
        public EntryType entryType;
    }

    public enum EntryType
    {
        System,
        Detection,
        Triage,
        Action,
        Assignment,
        Evacuation,
        Note
    }

    [System.Serializable]
    public class RecordStatistics
    {
        public int totalRecords;
        public int redCount;
        public int yellowCount;
        public int greenCount;
        public int blackCount;
        public int evacuatedCount;
        public int manualTriageCount;
    }
}
