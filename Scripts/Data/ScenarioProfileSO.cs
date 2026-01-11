// ============================================================================
// RASSE - Réalité Augmentée en Situations Sanitaires Exceptionnelles
// ScenarioProfileSO.cs - ScriptableObject pour profils de scénarios
// Conforme au rapport académique - Bloc 3: Système RA SSE
// ============================================================================

using UnityEngine;
using System;
using System.Collections.Generic;

namespace RASSE.Data
{
    /// <summary>
    /// Type de catastrophe selon le rapport
    /// </summary>
    public enum DisasterType
    {
        TrainAccident,          // Accident ferroviaire
        IndustrialExplosion,    // Explosion industrielle
        BuildingCollapse,       // Effondrement bâtiment
        ChemicalSpill,          // Déversement chimique
        MassEvent,              // Événement de masse (concert, stade)
        TerroristAttack,        // Attaque terroriste
        NaturalDisaster,        // Catastrophe naturelle
        MultiVehicleAccident,   // Carambolage
        Fire,                   // Incendie
        Pandemic                // Situation pandémique
    }

    /// <summary>
    /// Niveau de difficulté du scénario
    /// </summary>
    public enum DifficultyLevel
    {
        Tutorial,       // Tutoriel guidé
        Easy,           // 5-10 victimes, conditions optimales
        Medium,         // 15-25 victimes, quelques difficultés
        Hard,           // 30-50 victimes, conditions dégradées
        Expert,         // 50+ victimes, conditions extrêmes
        Certification   // Évaluation certification
    }

    /// <summary>
    /// Configuration de distribution des victimes par catégorie START
    /// </summary>
    [Serializable]
    public class VictimDistribution
    {
        [Range(0, 100)] public int redPercentage = 15;      // Urgence absolue
        [Range(0, 100)] public int yellowPercentage = 30;   // Urgence relative
        [Range(0, 100)] public int greenPercentage = 50;    // Urgence dépassée (blessés légers)
        [Range(0, 100)] public int blackPercentage = 5;     // Décédés

        public void Normalize()
        {
            int total = redPercentage + yellowPercentage + greenPercentage + blackPercentage;
            if (total != 100 && total > 0)
            {
                float factor = 100f / total;
                redPercentage = Mathf.RoundToInt(redPercentage * factor);
                yellowPercentage = Mathf.RoundToInt(yellowPercentage * factor);
                greenPercentage = Mathf.RoundToInt(greenPercentage * factor);
                blackPercentage = 100 - redPercentage - yellowPercentage - greenPercentage;
            }
        }
    }

    /// <summary>
    /// Configuration des conditions environnementales (NFR-ROB)
    /// </summary>
    [Serializable]
    public class EnvironmentConfig
    {
        [Header("Éclairage")]
        [Range(0, 1)] public float lightLevel = 0.8f;       // 0=nuit noire, 1=plein jour
        public bool dynamicLighting = false;
        
        [Header("Conditions Météo")]
        [Range(0, 1)] public float rainIntensity = 0f;
        [Range(0, 1)] public float smokeLevel = 0f;
        [Range(0, 1)] public float dustLevel = 0f;
        
        [Header("Conditions Terrain")]
        public bool debrisPresent = false;
        public bool waterHazard = false;
        public bool fireHazard = false;
        public bool chemicalHazard = false;
        
        [Header("Interférences")]
        [Range(0, 1)] public float vibrationLevel = 0f;
        [Range(0, 1)] public float noiseLevel = 0.3f;
        public bool networkInterference = false;
    }

    /// <summary>
    /// Zone de spawn de victimes
    /// </summary>
    [Serializable]
    public class VictimSpawnZone
    {
        public string zoneName = "Zone A";
        public Vector3 center;
        public float radius = 10f;
        public int maxVictims = 10;
        public VictimDistribution localDistribution;
        public bool useGlobalDistribution = true;
    }

    /// <summary>
    /// Objectif de scénario
    /// </summary>
    [Serializable]
    public class ScenarioObjective
    {
        public string objectiveId;
        public string description;
        public ObjectiveType type;
        public float targetValue;
        public float timeLimit;         // Secondes, 0 = pas de limite
        public bool isRequired;         // Obligatoire pour réussir
        public int pointsValue = 100;
    }

    public enum ObjectiveType
    {
        TriageAllVictims,           // Trier toutes les victimes
        TriageWithinTime,           // Trier dans le temps imparti
        AchieveAccuracy,            // Atteindre un taux de précision
        EvacuateRedCategory,        // Évacuer tous les rouges
        MaintainBatteryAbove,       // Maintenir batterie au-dessus de X%
        UseVoiceCommands,           // Utiliser X commandes vocales
        MinimizeFalsePositives,     // Moins de X% faux positifs
        CompleteFirstAidProtocols,  // Appliquer protocoles premiers secours
        CoordinateWithHospitals,    // Coordonner avec hôpitaux
        ExportPatientRecords        // Exporter dossiers patients
    }

    /// <summary>
    /// ScriptableObject définissant un scénario complet
    /// Conforme aux exigences REQ-1 à REQ-7 et NFR-*
    /// </summary>
    [CreateAssetMenu(fileName = "NewScenario", menuName = "RASSE/Scenario Profile", order = 1)]
    public class ScenarioProfileSO : ScriptableObject
    {
        [Header("=== INFORMATIONS GÉNÉRALES ===")]
        public string scenarioId;
        public string scenarioName = "Nouveau Scénario";
        [TextArea(3, 5)]
        public string description;
        public DisasterType disasterType = DisasterType.TrainAccident;
        public DifficultyLevel difficulty = DifficultyLevel.Medium;
        public Sprite scenarioThumbnail;

        [Header("=== CONFIGURATION VICTIMES ===")]
        [Range(1, 200)]
        public int totalVictims = 20;
        public VictimDistribution victimDistribution = new VictimDistribution();
        public List<VictimSpawnZone> spawnZones = new List<VictimSpawnZone>();
        
        [Header("=== TIMING ===")]
        [Tooltip("Durée maximale en minutes (0 = illimité)")]
        public float maxDurationMinutes = 30f;
        [Tooltip("Temps entre détériorations des victimes (secondes)")]
        public float deteriorationInterval = 60f;
        [Tooltip("Intervalle de spawn progressif (0 = tous au départ)")]
        public float progressiveSpawnInterval = 0f;

        [Header("=== ENVIRONNEMENT (NFR-ROB) ===")]
        public EnvironmentConfig environment = new EnvironmentConfig();
        public string sceneName = "MainScene";
        public Vector3 playerStartPosition = Vector3.zero;
        public Vector3 playerStartRotation = Vector3.zero;

        [Header("=== RESSOURCES DISPONIBLES ===")]
        [Range(1, 20)]
        public int availableAmbulances = 5;
        [Range(1, 10)]
        public int availableHospitals = 3;
        [Range(50, 100)]
        public float initialBatteryPercent = 100f;
        public bool offlineModeEnabled = true;
        public bool voiceCommandsEnabled = true;

        [Header("=== OBJECTIFS ===")]
        public List<ScenarioObjective> objectives = new List<ScenarioObjective>();
        
        [Header("=== SCORING ===")]
        public int perfectScoreThreshold = 95;
        public int goodScoreThreshold = 80;
        public int passScoreThreshold = 60;
        
        [Header("=== PARAMÈTRES AVANCÉS ===")]
        public bool enableTutorialHints = false;
        public bool allowPause = true;
        public bool allowRestart = true;
        public float timeScaleMultiplier = 1f;

        [Header("=== CONFORMITÉ RAPPORT ===")]
        [Tooltip("Exigences fonctionnelles testées")]
        public List<string> testedRequirements = new List<string>();
        [Tooltip("Blocs SysML concernés")]
        public List<string> relatedSysMLBlocks = new List<string>();

        /// <summary>
        /// Calcule le nombre de victimes par catégorie
        /// </summary>
        public Dictionary<string, int> GetVictimCounts()
        {
            victimDistribution.Normalize();
            return new Dictionary<string, int>
            {
                { "RED", Mathf.RoundToInt(totalVictims * victimDistribution.redPercentage / 100f) },
                { "YELLOW", Mathf.RoundToInt(totalVictims * victimDistribution.yellowPercentage / 100f) },
                { "GREEN", Mathf.RoundToInt(totalVictims * victimDistribution.greenPercentage / 100f) },
                { "BLACK", Mathf.RoundToInt(totalVictims * victimDistribution.blackPercentage / 100f) }
            };
        }

        /// <summary>
        /// Obtient le facteur de dégradation environnementale
        /// </summary>
        public float GetEnvironmentDegradationFactor()
        {
            float factor = 1f;
            factor -= (1f - environment.lightLevel) * 0.15f;
            factor -= environment.smokeLevel * 0.2f;
            factor -= environment.rainIntensity * 0.1f;
            factor -= environment.dustLevel * 0.1f;
            factor -= environment.vibrationLevel * 0.1f;
            return Mathf.Clamp(factor, 0.5f, 1f);
        }

        /// <summary>
        /// Valide la configuration du scénario
        /// </summary>
        public bool Validate(out List<string> errors)
        {
            errors = new List<string>();

            if (string.IsNullOrEmpty(scenarioId))
                errors.Add("L'ID du scénario est requis");

            if (totalVictims < 1)
                errors.Add("Le nombre de victimes doit être >= 1");

            if (objectives.Count == 0)
                errors.Add("Au moins un objectif est requis");

            int requiredObjectives = objectives.FindAll(o => o.isRequired).Count;
            if (requiredObjectives == 0)
                errors.Add("Au moins un objectif obligatoire est requis");

            return errors.Count == 0;
        }

        private void OnValidate()
        {
            victimDistribution.Normalize();
            
            if (string.IsNullOrEmpty(scenarioId))
                scenarioId = System.Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        }
    }
}
