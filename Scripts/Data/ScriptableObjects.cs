using UnityEngine;

namespace RASSE.Core
{
    /// <summary>
    /// VictimData - ScriptableObject pour définir des victimes prédéfinies
    /// </summary>
    [CreateAssetMenu(fileName = "NewVictim", menuName = "RA SSE/Victim Data")]
    public class VictimData : ScriptableObject
    {
        [Header("=== IDENTIFICATION ===")]
        public string patientId;
        public int victimNumber;

        [Header("=== INFORMATIONS PERSONNELLES ===")]
        public string firstName;
        public string lastName;
        public int age;
        public Gender gender;
        public string allergies;
        public string medicalHistory;

        [Header("=== SIGNES VITAUX ===")]
        public VitalSigns vitalSigns;

        [Header("=== BLESSURES ===")]
        public InjuryType primaryInjury;
        public InjuryLocation injuryLocation;
        public InjurySeverity injurySeverity;
        public string injuryDescription;

        [Header("=== POSITION ===")]
        public Vector3 spawnPosition;
        public Quaternion spawnRotation;

        [Header("=== VISUEL ===")]
        public GameObject customModel;
        public Material skinMaterial;

        /// <summary>
        /// Calcule la catégorie START attendue
        /// </summary>
        public StartCategory GetExpectedCategory()
        {
            // Implémentation simplifiée du calcul START
            if (!vitalSigns.isBreathing && !vitalSigns.breathingAfterAirwayManeuver)
                return StartCategory.Black;
            
            if (!vitalSigns.isBreathing && vitalSigns.breathingAfterAirwayManeuver)
                return StartCategory.Red;
            
            if (vitalSigns.respiratoryRate > 30 || vitalSigns.respiratoryRate < 10)
                return StartCategory.Red;
            
            if (vitalSigns.capillaryRefillTime > 2f || !vitalSigns.hasRadialPulse)
                return StartCategory.Red;
            
            if (!vitalSigns.canFollowCommands)
                return StartCategory.Red;
            
            if (vitalSigns.canWalk)
                return StartCategory.Green;
            
            return StartCategory.Yellow;
        }
    }

    /// <summary>
    /// ScenarioData - ScriptableObject pour définir des scénarios complets
    /// </summary>
    [CreateAssetMenu(fileName = "NewScenario", menuName = "RA SSE/Scenario Data")]
    public class ScenarioData : ScriptableObject
    {
        [Header("=== INFORMATIONS SCÉNARIO ===")]
        public string scenarioName;
        public string description;
        public ScenarioType scenarioType;
        public DifficultyLevel difficulty;

        [Header("=== VICTIMES ===")]
        public VictimData[] predefinedVictims;
        public int randomVictimCount;
        public Vector3 spawnAreaCenter;
        public Vector3 spawnAreaSize;

        [Header("=== HÔPITAUX ===")]
        public HospitalData[] availableHospitals;

        [Header("=== AMBULANCES ===")]
        public int ambulanceCount = 5;
        public Transform[] ambulanceSpawnPoints;

        [Header("=== ENVIRONNEMENT ===")]
        public string sceneName;
        public Vector3 playerSpawnPosition;
        public float timeLimit; // 0 = pas de limite

        [Header("=== OBJECTIFS ===")]
        public int minimumEvacuations;
        public float maximumDeathRate;
        public float targetTriageAccuracy;

        [Header("=== CONDITIONS ===")]
        public EnvironmentCondition environmentCondition;
        public float visibilityDistance = 100f;
        public bool hasNetworkConnection = true;
    }

    /// <summary>
    /// Conditions environnementales
    /// </summary>
    [System.Serializable]
    public class EnvironmentCondition
    {
        public WeatherType weather = WeatherType.Clear;
        public TimeOfDay timeOfDay = TimeOfDay.Day;
        public float noiseLevel = 0.3f;
        public bool hasFire = false;
        public bool hasSmoke = false;
        public bool hasDebris = false;
    }

    public enum WeatherType
    {
        Clear,
        Rain,
        Snow,
        Fog,
        Storm
    }

    public enum TimeOfDay
    {
        Day,
        Dusk,
        Night,
        Dawn
    }
}
