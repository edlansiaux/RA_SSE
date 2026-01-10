using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RASSE.ImageAnalysis
{
    /// <summary>
    /// ImageAnalysisModule - Module d'Analyse d'Image (Bloc 19 du rapport)
    /// Simule l'analyse des flux caméra RGB et thermique pour:
    /// - Localisation des victimes (Bloc 191)
    /// - Analyse des constantes vitales (Bloc 192)
    /// Conformité: REQ-2, NFR-ACC (≥95% exactitude), NFR-VIT (≤30s latence)
    /// </summary>
    public class ImageAnalysisModule : MonoBehaviour
    {
        public static ImageAnalysisModule Instance { get; private set; }

        [Header("=== CONFIGURATION CAMÉRAS ===")]
        [SerializeField] private Camera rgbCamera;
        [SerializeField] private Camera thermalCamera;
        [SerializeField] private float analysisInterval = 0.5f;
        [SerializeField] private float detectionRange = 15f;
        [SerializeField] private float detectionAngle = 60f;
        [SerializeField] private LayerMask victimLayer;

        [Header("=== PARAMÈTRES ANALYSE ===")]
        [Tooltip("Précision de détection (NFR-ACC: ≥95%)")]
        [Range(0.9f, 1f)]
        [SerializeField] private float detectionAccuracy = 0.96f;
        
        [Tooltip("Taux de faux positifs (NFR-ACC: ≤5%)")]
        [Range(0f, 0.1f)]
        [SerializeField] private float falsePositiveRate = 0.03f;
        
        [Tooltip("Temps d'analyse en secondes (NFR-VIT: ≤30s)")]
        [Range(1f, 30f)]
        [SerializeField] private float analysisTime = 5f;

        [Header("=== CONDITIONS ENVIRONNEMENTALES ===")]
        [SerializeField] private EnvironmentConditions currentConditions;

        [Header("=== SORTIE ANALYSE ===")]
        [SerializeField] private List<VictimDetectionResult> detectedVictims = new List<VictimDetectionResult>();

        // Événements
        public event Action<VictimDetectionResult> OnVictimDetected;
        public event Action<VictimDetectionResult> OnVitalsAnalyzed;
        public event Action<int> OnMultiTargetDetection;

        // État
        private bool isAnalyzing;
        private Coroutine analysisCoroutine;
        private Transform cameraTransform;

        // Métriques de performance (NFR-ACC)
        public AnalysisMetrics Metrics { get; private set; } = new AnalysisMetrics();

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

            currentConditions = new EnvironmentConditions();
        }

        private void Start()
        {
            if (rgbCamera == null)
                rgbCamera = Camera.main;
            
            cameraTransform = rgbCamera?.transform ?? transform;

            // Démarrer l'analyse continue
            StartContinuousAnalysis();
        }

        /// <summary>
        /// Démarre l'analyse continue du flux vidéo
        /// </summary>
        public void StartContinuousAnalysis()
        {
            if (analysisCoroutine != null)
                StopCoroutine(analysisCoroutine);
            
            analysisCoroutine = StartCoroutine(ContinuousAnalysisLoop());
            Debug.Log("[ImageAnalysis] Analyse continue démarrée");
        }

        /// <summary>
        /// Arrête l'analyse continue
        /// </summary>
        public void StopContinuousAnalysis()
        {
            if (analysisCoroutine != null)
            {
                StopCoroutine(analysisCoroutine);
                analysisCoroutine = null;
            }
            Debug.Log("[ImageAnalysis] Analyse continue arrêtée");
        }

        /// <summary>
        /// Boucle d'analyse continue simulant le traitement vidéo
        /// </summary>
        private IEnumerator ContinuousAnalysisLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(analysisInterval);

                // Simuler l'analyse du frame actuel
                PerformFrameAnalysis();
            }
        }

        /// <summary>
        /// Analyse un frame - Détection multi-cibles (REQ-1)
        /// </summary>
        private void PerformFrameAnalysis()
        {
            if (cameraTransform == null) return;

            // Appliquer le facteur de dégradation environnemental
            float environmentFactor = CalculateEnvironmentFactor();

            // Détecter les victimes dans le champ de vision
            Collider[] colliders = Physics.OverlapSphere(
                cameraTransform.position, 
                detectionRange * environmentFactor, 
                victimLayer
            );

            int newDetections = 0;
            List<VictimDetectionResult> frameResults = new List<VictimDetectionResult>();

            foreach (var collider in colliders)
            {
                // Vérifier l'angle de vue
                Vector3 directionToTarget = (collider.transform.position - cameraTransform.position).normalized;
                float angle = Vector3.Angle(cameraTransform.forward, directionToTarget);

                if (angle > detectionAngle / 2) continue;

                // Vérifier la ligne de vue
                if (!HasLineOfSight(collider.transform.position)) continue;

                // Simuler la probabilité de détection (NFR-ACC)
                float adjustedAccuracy = detectionAccuracy * environmentFactor;
                if (UnityEngine.Random.value > adjustedAccuracy)
                {
                    Metrics.missedDetections++;
                    continue;
                }

                // Simuler les faux positifs
                if (UnityEngine.Random.value < falsePositiveRate)
                {
                    Metrics.falsePositives++;
                    continue;
                }

                var victimController = collider.GetComponent<Core.VictimController>();
                if (victimController == null) continue;

                // Créer ou mettre à jour le résultat de détection
                VictimDetectionResult result = GetOrCreateDetectionResult(victimController);
                result.lastDetectionTime = Time.time;
                result.distanceFromCamera = Vector3.Distance(cameraTransform.position, collider.transform.position);
                result.angleFromCenter = angle;
                result.confidence = CalculateDetectionConfidence(result, environmentFactor);

                if (!result.hasBeenReported)
                {
                    result.hasBeenReported = true;
                    newDetections++;
                    Metrics.totalDetections++;
                    OnVictimDetected?.Invoke(result);
                }

                frameResults.Add(result);
            }

            // Notifier si multi-détection
            if (newDetections > 1)
            {
                OnMultiTargetDetection?.Invoke(newDetections);
            }

            // Mettre à jour les métriques
            Metrics.lastFrameTime = Time.time;
            Metrics.victimsInView = frameResults.Count;
        }

        /// <summary>
        /// Analyse complète des constantes vitales d'une victime (REQ-2)
        /// Simule l'analyse par caméra RGB et thermique
        /// </summary>
        public IEnumerator AnalyzeVictimVitals(Core.VictimController victim, Action<VitalsAnalysisResult> onComplete)
        {
            if (victim == null)
            {
                onComplete?.Invoke(null);
                yield break;
            }

            isAnalyzing = true;
            float startTime = Time.time;

            VitalsAnalysisResult result = new VitalsAnalysisResult
            {
                patientId = victim.PatientId,
                analysisStartTime = DateTime.Now,
                environmentConditions = currentConditions.Clone()
            };

            // Phase 1: Analyse RGB - Détection visuelle (2s)
            yield return new WaitForSeconds(analysisTime * 0.3f);
            result.rgbAnalysis = PerformRGBAnalysis(victim);

            // Phase 2: Analyse Thermique - Température et circulation (2s)
            yield return new WaitForSeconds(analysisTime * 0.3f);
            result.thermalAnalysis = PerformThermalAnalysis(victim);

            // Phase 3: Fusion et estimation des constantes (1s)
            yield return new WaitForSeconds(analysisTime * 0.2f);
            result.estimatedVitals = FuseAnalysisResults(result.rgbAnalysis, result.thermalAnalysis, victim);

            // Phase 4: Validation et calcul de confiance (0.5s)
            yield return new WaitForSeconds(analysisTime * 0.2f);
            result.overallConfidence = CalculateOverallConfidence(result);
            result.analysisEndTime = DateTime.Now;
            result.analysisLatency = Time.time - startTime;

            // Vérifier NFR-VIT
            if (result.analysisLatency > 30f)
            {
                Debug.LogWarning($"[ImageAnalysis] Latence analyse ({result.analysisLatency:F1}s) > NFR-VIT (30s)");
            }

            isAnalyzing = false;
            Metrics.totalAnalyses++;
            Metrics.averageLatency = (Metrics.averageLatency * (Metrics.totalAnalyses - 1) + result.analysisLatency) / Metrics.totalAnalyses;

            // Notifier et retourner
            OnVitalsAnalyzed?.Invoke(GetOrCreateDetectionResult(victim));
            onComplete?.Invoke(result);

            Debug.Log($"[ImageAnalysis] Analyse complète pour {victim.PatientId} en {result.analysisLatency:F1}s (confiance: {result.overallConfidence:P0})");
        }

        /// <summary>
        /// Analyse RGB simulée - Détecte mouvements, couleur peau, respiration visible
        /// </summary>
        private RGBAnalysisData PerformRGBAnalysis(Core.VictimController victim)
        {
            var vitals = victim.VitalSigns;
            float environmentFactor = CalculateEnvironmentFactor();

            return new RGBAnalysisData
            {
                // Détection de mouvement thoracique (respiration)
                respiratoryMotionDetected = vitals.isBreathing,
                estimatedRespiratoryRate = vitals.respiratoryRate + UnityEngine.Random.Range(-2, 3),
                respiratoryConfidence = 0.85f * environmentFactor,

                // Analyse couleur peau (cyanose, pâleur)
                skinColorAnalysis = AnalyzeSkinColor(vitals),
                cyanosisDetected = vitals.oxygenSaturation < 90,
                pallorDetected = vitals.systolicBloodPressure < 90,

                // Détection de mouvement général
                movementDetected = vitals.canMove,
                consciousnessEstimate = vitals.canFollowCommands ? "Conscient" : "Inconscient",

                // Détection de saignement visible
                visibleBleedingDetected = vitals.hasVisibleBleeding,
                estimatedBleedingSeverity = vitals.hasVisibleBleeding ? "Modéré" : "Aucun"
            };
        }

        /// <summary>
        /// Analyse thermique simulée - Température, circulation périphérique
        /// </summary>
        private ThermalAnalysisData PerformThermalAnalysis(Core.VictimController victim)
        {
            var vitals = victim.VitalSigns;
            float environmentFactor = CalculateEnvironmentFactor();

            // Simuler la lecture thermique avec bruit
            float measuredTemp = vitals.bodyTemperature + UnityEngine.Random.Range(-0.3f, 0.3f);
            float capillaryIndicator = vitals.capillaryRefillTime;

            return new ThermalAnalysisData
            {
                // Température corporelle
                coreTemperature = measuredTemp,
                temperatureConfidence = 0.9f * environmentFactor,
                hypothermiaRisk = measuredTemp < 35f,
                hyperthermiaRisk = measuredTemp > 38.5f,

                // Circulation périphérique (différence thermique extrémités)
                peripheralCirculationIndex = capillaryIndicator < 2f ? 1f : (1f / capillaryIndicator),
                extremitiesTemperature = measuredTemp - (capillaryIndicator > 2f ? 3f : 1f),

                // Détection de zones froides (possible hémorragie interne)
                coldZonesDetected = capillaryIndicator > 3f,
                estimatedPerfusionStatus = capillaryIndicator <= 2f ? "Normal" : "Diminué",

                // Fréquence cardiaque par thermographie (simulation)
                estimatedHeartRateFromThermal = vitals.heartRate + UnityEngine.Random.Range(-5, 6),
                heartRateConfidence = 0.75f * environmentFactor
            };
        }

        /// <summary>
        /// Fusionne les résultats RGB et thermique pour estimer les constantes
        /// </summary>
        private EstimatedVitalSigns FuseAnalysisResults(RGBAnalysisData rgb, ThermalAnalysisData thermal, Core.VictimController victim)
        {
            var actualVitals = victim.VitalSigns;

            // Fusion pondérée des estimations
            return new EstimatedVitalSigns
            {
                // Respiration: priorité à l'analyse RGB
                isBreathing = rgb.respiratoryMotionDetected,
                respiratoryRate = rgb.estimatedRespiratoryRate,
                respiratoryRateConfidence = rgb.respiratoryConfidence,

                // Fréquence cardiaque: fusion RGB (mouvement) + Thermique
                heartRate = thermal.estimatedHeartRateFromThermal,
                heartRateConfidence = thermal.heartRateConfidence,

                // Circulation: analyse thermique
                capillaryRefillEstimate = thermal.peripheralCirculationIndex < 0.5f ? ">2s" : "≤2s",
                circulationStatus = thermal.estimatedPerfusionStatus,

                // Température: thermique direct
                bodyTemperature = thermal.coreTemperature,
                temperatureConfidence = thermal.temperatureConfidence,

                // Neurologique: analyse RGB
                consciousnessLevel = rgb.consciousnessEstimate,
                canFollowCommands = rgb.movementDetected && rgb.consciousnessEstimate == "Conscient",

                // Mobilité: RGB
                canWalk = actualVitals.canWalk, // Nécessite observation directe
                canMove = rgb.movementDetected,

                // SpO2 estimé via couleur peau
                estimatedSpO2 = rgb.cyanosisDetected ? 85f : (rgb.pallorDetected ? 92f : 97f),
                spO2Confidence = 0.7f,

                // Saignement
                visibleBleeding = rgb.visibleBleedingDetected,
                bleedingSeverity = rgb.estimatedBleedingSeverity
            };
        }

        /// <summary>
        /// Calcule la confiance globale de l'analyse
        /// </summary>
        private float CalculateOverallConfidence(VitalsAnalysisResult result)
        {
            float rgbConfidence = result.rgbAnalysis.respiratoryConfidence;
            float thermalConfidence = result.thermalAnalysis.temperatureConfidence;
            float environmentPenalty = 1f - (1f - CalculateEnvironmentFactor()) * 0.5f;

            return (rgbConfidence * 0.4f + thermalConfidence * 0.4f + result.estimatedVitals.respiratoryRateConfidence * 0.2f) * environmentPenalty;
        }

        /// <summary>
        /// Analyse la couleur de peau
        /// </summary>
        private string AnalyzeSkinColor(Core.VitalSigns vitals)
        {
            if (vitals.oxygenSaturation < 85) return "Cyanosé";
            if (vitals.systolicBloodPressure < 80) return "Pâle/Marbré";
            if (vitals.bodyTemperature > 38.5f) return "Rouge/Chaud";
            return "Normal";
        }

        /// <summary>
        /// Calcule le facteur de dégradation environnemental (NFR-ROB)
        /// </summary>
        private float CalculateEnvironmentFactor()
        {
            float factor = 1f;

            // Luminosité
            factor *= currentConditions.lightLevel switch
            {
                LightLevel.Bright => 1f,
                LightLevel.Normal => 0.95f,
                LightLevel.Dim => 0.85f,
                LightLevel.Dark => 0.6f,
                _ => 1f
            };

            // Fumée
            if (currentConditions.smokePresent)
                factor *= 0.7f;

            // Pluie
            if (currentConditions.rainPresent)
                factor *= 0.85f;

            // Tremblements/vibrations
            if (currentConditions.vibrations)
                factor *= 0.9f;

            return Mathf.Clamp(factor, 0.3f, 1f);
        }

        /// <summary>
        /// Vérifie la ligne de vue vers une position
        /// </summary>
        private bool HasLineOfSight(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - cameraTransform.position;
            return !Physics.Raycast(cameraTransform.position, direction.normalized, direction.magnitude, ~victimLayer);
        }

        /// <summary>
        /// Calcule la confiance de détection
        /// </summary>
        private float CalculateDetectionConfidence(VictimDetectionResult result, float envFactor)
        {
            float distanceFactor = 1f - (result.distanceFromCamera / detectionRange);
            float angleFactor = 1f - (result.angleFromCenter / (detectionAngle / 2));
            return Mathf.Clamp01(distanceFactor * 0.4f + angleFactor * 0.3f + envFactor * 0.3f);
        }

        /// <summary>
        /// Obtient ou crée un résultat de détection
        /// </summary>
        private VictimDetectionResult GetOrCreateDetectionResult(Core.VictimController victim)
        {
            var existing = detectedVictims.Find(v => v.victimController == victim);
            if (existing != null) return existing;

            var newResult = new VictimDetectionResult
            {
                victimController = victim,
                patientId = victim.PatientId,
                firstDetectionTime = Time.time
            };
            detectedVictims.Add(newResult);
            return newResult;
        }

        /// <summary>
        /// Met à jour les conditions environnementales
        /// </summary>
        public void SetEnvironmentConditions(EnvironmentConditions conditions)
        {
            currentConditions = conditions;
            Debug.Log($"[ImageAnalysis] Conditions mises à jour - Facteur: {CalculateEnvironmentFactor():P0}");
        }

        /// <summary>
        /// Obtient les métriques de performance (pour NFR-ACC)
        /// </summary>
        public string GetPerformanceReport()
        {
            float accuracy = Metrics.totalDetections > 0 
                ? (float)(Metrics.totalDetections - Metrics.falsePositives) / (Metrics.totalDetections + Metrics.missedDetections)
                : 0f;

            float falsePositiveRateActual = Metrics.totalDetections > 0
                ? (float)Metrics.falsePositives / Metrics.totalDetections
                : 0f;

            return $"=== MÉTRIQUES IMAGE ANALYSIS ===\n" +
                   $"Détections totales: {Metrics.totalDetections}\n" +
                   $"Faux positifs: {Metrics.falsePositives} ({falsePositiveRateActual:P1})\n" +
                   $"Détections manquées: {Metrics.missedDetections}\n" +
                   $"Exactitude: {accuracy:P1} (NFR-ACC: ≥95%)\n" +
                   $"Analyses complètes: {Metrics.totalAnalyses}\n" +
                   $"Latence moyenne: {Metrics.averageLatency:F1}s (NFR-VIT: ≤30s)\n" +
                   $"Victimes en vue: {Metrics.victimsInView}\n" +
                   $"Facteur environnement: {CalculateEnvironmentFactor():P0}";
        }
    }

    #region Data Structures

    /// <summary>
    /// Résultat de détection d'une victime
    /// </summary>
    [Serializable]
    public class VictimDetectionResult
    {
        public Core.VictimController victimController;
        public string patientId;
        public float firstDetectionTime;
        public float lastDetectionTime;
        public float distanceFromCamera;
        public float angleFromCenter;
        public float confidence;
        public bool hasBeenReported;
        public Vector3 lastKnownPosition;
    }

    /// <summary>
    /// Résultat complet d'analyse des constantes
    /// </summary>
    [Serializable]
    public class VitalsAnalysisResult
    {
        public string patientId;
        public DateTime analysisStartTime;
        public DateTime analysisEndTime;
        public float analysisLatency;
        public RGBAnalysisData rgbAnalysis;
        public ThermalAnalysisData thermalAnalysis;
        public EstimatedVitalSigns estimatedVitals;
        public float overallConfidence;
        public EnvironmentConditions environmentConditions;
    }

    /// <summary>
    /// Données d'analyse RGB
    /// </summary>
    [Serializable]
    public class RGBAnalysisData
    {
        public bool respiratoryMotionDetected;
        public int estimatedRespiratoryRate;
        public float respiratoryConfidence;
        public string skinColorAnalysis;
        public bool cyanosisDetected;
        public bool pallorDetected;
        public bool movementDetected;
        public string consciousnessEstimate;
        public bool visibleBleedingDetected;
        public string estimatedBleedingSeverity;
    }

    /// <summary>
    /// Données d'analyse thermique
    /// </summary>
    [Serializable]
    public class ThermalAnalysisData
    {
        public float coreTemperature;
        public float temperatureConfidence;
        public bool hypothermiaRisk;
        public bool hyperthermiaRisk;
        public float peripheralCirculationIndex;
        public float extremitiesTemperature;
        public bool coldZonesDetected;
        public string estimatedPerfusionStatus;
        public int estimatedHeartRateFromThermal;
        public float heartRateConfidence;
    }

    /// <summary>
    /// Constantes vitales estimées
    /// </summary>
    [Serializable]
    public class EstimatedVitalSigns
    {
        public bool isBreathing;
        public int respiratoryRate;
        public float respiratoryRateConfidence;
        public int heartRate;
        public float heartRateConfidence;
        public string capillaryRefillEstimate;
        public string circulationStatus;
        public float bodyTemperature;
        public float temperatureConfidence;
        public string consciousnessLevel;
        public bool canFollowCommands;
        public bool canWalk;
        public bool canMove;
        public float estimatedSpO2;
        public float spO2Confidence;
        public bool visibleBleeding;
        public string bleedingSeverity;
    }

    /// <summary>
    /// Conditions environnementales (NFR-ROB)
    /// </summary>
    [Serializable]
    public class EnvironmentConditions
    {
        public LightLevel lightLevel = LightLevel.Normal;
        public bool smokePresent = false;
        public bool rainPresent = false;
        public bool vibrations = false;
        public float visibility = 100f; // en mètres

        public EnvironmentConditions Clone()
        {
            return new EnvironmentConditions
            {
                lightLevel = lightLevel,
                smokePresent = smokePresent,
                rainPresent = rainPresent,
                vibrations = vibrations,
                visibility = visibility
            };
        }
    }

    public enum LightLevel
    {
        Bright,     // Plein jour
        Normal,     // Intérieur éclairé
        Dim,        // Faible luminosité
        Dark        // Obscurité
    }

    /// <summary>
    /// Métriques de performance du module
    /// </summary>
    [Serializable]
    public class AnalysisMetrics
    {
        public int totalDetections;
        public int falsePositives;
        public int missedDetections;
        public int totalAnalyses;
        public float averageLatency;
        public int victimsInView;
        public float lastFrameTime;
    }

    #endregion
}
