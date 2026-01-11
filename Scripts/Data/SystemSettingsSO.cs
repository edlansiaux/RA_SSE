// ============================================================================
// RASSE - Réalité Augmentée en Situations Sanitaires Exceptionnelles
// SystemSettingsSO.cs - ScriptableObject pour paramètres système
// Conforme au rapport académique - NFR-*: Exigences non-fonctionnelles
// ============================================================================

using UnityEngine;
using System;

namespace RASSE.Data
{
    /// <summary>
    /// Configuration des lunettes RA (Bloc 1: Hardware)
    /// </summary>
    [Serializable]
    public class ARGlassesConfig
    {
        [Header("Modèle Lunettes")]
        public string model = "RealWear Navigator 520";
        public string manufacturer = "RealWear";
        
        [Header("Caractéristiques Écran")]
        public Vector2 displayResolution = new Vector2(1920, 1080);
        public float fieldOfView = 20f;             // Degrés
        public float refreshRate = 60f;             // Hz
        
        [Header("Caméras (Bloc 13, 14)")]
        public bool hasRGBCamera = true;
        public Vector2 rgbResolution = new Vector2(1920, 1080);
        public bool hasThermalCamera = true;
        public Vector2 thermalResolution = new Vector2(640, 480);
        
        [Header("Batterie (NFR-AVA)")]
        public int batteryCapacityMAh = 5000;
        public float targetAutonomyHours = 8f;
        
        [Header("Protection")]
        public string ipRating = "IP66";
        public string dropResistance = "MIL-STD-810H";
    }

    /// <summary>
    /// Configuration de précision (NFR-ACC)
    /// </summary>
    [Serializable]
    public class AccuracyConfig
    {
        [Header("Détection (REQ-1)")]
        [Range(0, 100)]
        public float targetDetectionAccuracy = 95f;     // ≥ 95%
        [Range(0, 100)]
        public float maxFalsePositiveRate = 5f;         // ≤ 5%
        
        [Header("Classification (REQ-3)")]
        [Range(0, 100)]
        public float targetTriageAccuracy = 95f;
        
        [Header("Localisation (NFR-LOC)")]
        public float maxGuidancePrecisionMeters = 2f;   // ≤ 2m
        
        [Header("Constantes Vitales (REQ-2)")]
        public float respiratoryRateToleranceBPM = 2f;
        public float heartRateToleranceBPM = 5f;
        public float temperatureToleranceC = 0.5f;
        public float spO2TolerancePercent = 2f;
    }

    /// <summary>
    /// Configuration de vitesse (NFR-VIT)
    /// </summary>
    [Serializable]
    public class PerformanceConfig
    {
        [Header("Latences Cibles")]
        public float maxLatencyOfflineSeconds = 30f;    // ≤ 30s offline
        public float maxLatencyOnlineSeconds = 10f;     // ≤ 10s online
        
        [Header("Analyse d'Image")]
        public float rgbAnalysisDuration = 2f;          // Secondes
        public float thermalAnalysisDuration = 2f;
        public float fusionDuration = 1f;
        public float validationDuration = 0.5f;
        
        [Header("Temps de Réponse UI")]
        public float maxUIResponseMs = 100f;
        public float animationDuration = 0.3f;
    }

    /// <summary>
    /// Configuration de robustesse (NFR-ROB)
    /// </summary>
    [Serializable]
    public class RobustnessConfig
    {
        [Header("Conditions Lumineuses")]
        [Range(0, 1)]
        public float minLightLevel = 0.1f;              // Fonctionne jusqu'à 10% lumière
        
        [Header("Conditions Environnementales")]
        [Range(0, 1)]
        public float maxSmokeLevel = 0.7f;              // Tolère jusqu'à 70% fumée
        [Range(0, 1)]
        public float maxRainIntensity = 0.8f;
        
        [Header("Dégradation Acceptable")]
        [Range(0, 1)]
        public float maxAccuracyDegradation = 0.15f;    // Max 15% perte précision
        
        [Header("Tentatives Reconnexion")]
        public int maxReconnectionAttempts = 5;
        public float reconnectionDelaySeconds = 5f;
    }

    /// <summary>
    /// Configuration de sécurité (NFR-SEC)
    /// </summary>
    [Serializable]
    public class SecurityConfig
    {
        [Header("Chiffrement")]
        public string encryptionAlgorithm = "AES-256";
        public string communicationProtocol = "TLS 1.3";
        public bool enforceEncryption = true;
        
        [Header("Authentification")]
        public bool requireAuthentication = true;
        public int sessionTimeoutMinutes = 30;
        public int maxLoginAttempts = 3;
        
        [Header("Audit (NFR-SEC)")]
        public bool enableAuditLogging = true;
        public int auditRetentionDays = 365;
        
        [Header("RGPD")]
        public bool anonymizeExports = false;
        public bool requireConsent = true;
        public int dataRetentionDays = 30;
    }

    /// <summary>
    /// Configuration d'interopérabilité (NFR-INT)
    /// </summary>
    [Serializable]
    public class InteroperabilityConfig
    {
        [Header("FHIR")]
        public bool enableFHIR = true;
        public string fhirVersion = "R4";
        public string defaultFHIREndpoint = "https://hospital.local/fhir";
        
        [Header("HL7")]
        public bool enableHL7 = true;
        public string hl7Version = "2.5";
        public int defaultHL7Port = 2575;
        
        [Header("Export")]
        public bool autoExportOnEvacuation = true;
        public string exportFormat = "FHIR";            // FHIR ou HL7
    }

    /// <summary>
    /// Configuration UX (NFR-UX)
    /// </summary>
    [Serializable]
    public class UXConfig
    {
        [Header("Mode Mains Libres")]
        public bool enableHandsFreeMode = true;
        public bool enableVoiceCommands = true;
        public bool enableGestures = false;
        
        [Header("Commandes Vocales")]
        public string voiceLanguage = "fr-FR";
        public float voiceActivationThreshold = 0.7f;
        public string wakeWord = "RASSE";
        
        [Header("Feedback")]
        public bool enableHapticFeedback = true;
        public bool enableAudioFeedback = true;
        public float feedbackVolume = 0.8f;
        
        [Header("Accessibilité")]
        public float textScaleFactor = 1f;
        public bool highContrastMode = false;
        public bool colorBlindMode = false;
    }

    /// <summary>
    /// Configuration réglementaire (NFR-REG)
    /// </summary>
    [Serializable]
    public class RegulatoryConfig
    {
        [Header("Normes")]
        public string isoCompliance = "ISO 14971";      // Gestion risques DM
        public string iecCompliance = "IEC 62304";       // Cycle vie logiciel DM
        public string mdRegulation = "MDR 2017/745";     // Règlement DM UE
        
        [Header("Classification")]
        public string deviceClass = "IIa";               // Classe DM
        public string intendedUse = "Aide au triage médical en situation de catastrophe";
        
        [Header("Documentation")]
        public string technicalFileVersion = "1.0";
        public string riskAnalysisVersion = "1.0";
    }

    /// <summary>
    /// ScriptableObject pour tous les paramètres système
    /// Centralise la configuration conforme aux NFR du rapport
    /// </summary>
    [CreateAssetMenu(fileName = "SystemSettings", menuName = "RASSE/System Settings", order = 10)]
    public class SystemSettingsSO : ScriptableObject
    {
        [Header("=== IDENTIFICATION ===")]
        public string systemVersion = "1.0.0";
        public string buildDate = "2024";
        public string configurationName = "Default";

        [Header("=== MATÉRIEL (Bloc 1) ===")]
        public ARGlassesConfig arGlasses = new ARGlassesConfig();

        [Header("=== PRÉCISION (NFR-ACC) ===")]
        public AccuracyConfig accuracy = new AccuracyConfig();

        [Header("=== PERFORMANCE (NFR-VIT) ===")]
        public PerformanceConfig performance = new PerformanceConfig();

        [Header("=== ROBUSTESSE (NFR-ROB) ===")]
        public RobustnessConfig robustness = new RobustnessConfig();

        [Header("=== SÉCURITÉ (NFR-SEC) ===")]
        public SecurityConfig security = new SecurityConfig();

        [Header("=== INTEROPÉRABILITÉ (NFR-INT) ===")]
        public InteroperabilityConfig interoperability = new InteroperabilityConfig();

        [Header("=== EXPÉRIENCE UTILISATEUR (NFR-UX) ===")]
        public UXConfig ux = new UXConfig();

        [Header("=== RÉGLEMENTATION (NFR-REG) ===")]
        public RegulatoryConfig regulatory = new RegulatoryConfig();

        [Header("=== SIMULATION ===")]
        [Tooltip("Multiplicateur de temps pour la simulation")]
        [Range(0.1f, 10f)]
        public float timeScale = 1f;
        
        [Tooltip("Mode debug avec informations supplémentaires")]
        public bool debugMode = false;
        
        [Tooltip("Sauvegarde automatique des sessions")]
        public bool autoSave = true;
        public float autoSaveIntervalMinutes = 5f;

        /// <summary>
        /// Valide la configuration contre les exigences du rapport
        /// </summary>
        public bool ValidateConfiguration(out string[] violations)
        {
            var violationList = new System.Collections.Generic.List<string>();

            // NFR-ACC
            if (accuracy.targetDetectionAccuracy < 95f)
                violationList.Add("NFR-ACC: Précision détection doit être ≥ 95%");
            if (accuracy.maxFalsePositiveRate > 5f)
                violationList.Add("NFR-ACC: Taux faux positifs doit être ≤ 5%");

            // NFR-VIT
            if (performance.maxLatencyOfflineSeconds > 30f)
                violationList.Add("NFR-VIT: Latence offline doit être ≤ 30s");
            if (performance.maxLatencyOnlineSeconds > 10f)
                violationList.Add("NFR-VIT: Latence online doit être ≤ 10s");

            // NFR-AVA
            if (arGlasses.targetAutonomyHours < 8f)
                violationList.Add("NFR-AVA: Autonomie doit être ≥ 8h");

            // NFR-LOC
            if (accuracy.maxGuidancePrecisionMeters > 2f)
                violationList.Add("NFR-LOC: Précision guidage doit être ≤ 2m");

            // NFR-INT
            if (!interoperability.enableFHIR && !interoperability.enableHL7)
                violationList.Add("NFR-INT: Au moins FHIR ou HL7 doit être activé");

            // NFR-SEC
            if (!security.enableAuditLogging)
                violationList.Add("NFR-SEC: Audit logging doit être activé");

            violations = violationList.ToArray();
            return violationList.Count == 0;
        }

        /// <summary>
        /// Génère un rapport de configuration
        /// </summary>
        public string GenerateConfigurationReport()
        {
            string report = $"=== RAPPORT CONFIGURATION RASSE v{systemVersion} ===\n\n";
            
            report += "MATÉRIEL:\n";
            report += $"  Lunettes: {arGlasses.model}\n";
            report += $"  Caméra RGB: {arGlasses.rgbResolution.x}x{arGlasses.rgbResolution.y}\n";
            report += $"  Caméra Thermique: {(arGlasses.hasThermalCamera ? "Oui" : "Non")}\n";
            report += $"  Autonomie cible: {arGlasses.targetAutonomyHours}h\n\n";
            
            report += "EXIGENCES NON-FONCTIONNELLES:\n";
            report += $"  NFR-ACC: Précision ≥{accuracy.targetDetectionAccuracy}%, FP ≤{accuracy.maxFalsePositiveRate}%\n";
            report += $"  NFR-VIT: Latence ≤{performance.maxLatencyOfflineSeconds}s (offline), ≤{performance.maxLatencyOnlineSeconds}s (online)\n";
            report += $"  NFR-LOC: Précision ≤{accuracy.maxGuidancePrecisionMeters}m\n";
            report += $"  NFR-INT: FHIR={interoperability.enableFHIR}, HL7={interoperability.enableHL7}\n";
            report += $"  NFR-SEC: Chiffrement={security.encryptionAlgorithm}, Audit={security.enableAuditLogging}\n";
            report += $"  NFR-UX: Mains libres={ux.enableHandsFreeMode}, Vocal={ux.enableVoiceCommands}\n";
            report += $"  NFR-REG: {regulatory.isoCompliance}, {regulatory.iecCompliance}\n\n";
            
            ValidateConfiguration(out string[] violations);
            report += $"CONFORMITÉ: {(violations.Length == 0 ? "✓ CONFORME" : $"✗ {violations.Length} VIOLATION(S)")}\n";
            
            foreach (var v in violations)
            {
                report += $"  ⚠ {v}\n";
            }
            
            return report;
        }

        private void OnValidate()
        {
            // Assurer des valeurs minimales
            performance.maxLatencyOfflineSeconds = Mathf.Max(1f, performance.maxLatencyOfflineSeconds);
            performance.maxLatencyOnlineSeconds = Mathf.Max(0.5f, performance.maxLatencyOnlineSeconds);
            arGlasses.targetAutonomyHours = Mathf.Max(1f, arGlasses.targetAutonomyHours);
        }
    }
}
