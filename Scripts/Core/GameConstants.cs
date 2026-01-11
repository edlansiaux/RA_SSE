using UnityEngine;

namespace RASSE.Core
{
    /// <summary>
    /// Constantes globales du simulateur RA-SSE.
    /// Centralise toutes les valeurs de configuration utilisées dans le projet.
    /// </summary>
    public static class GameConstants
    {
        #region Tags
        
        public const string TAG_VICTIM = "Victim";
        public const string TAG_RESCUER = "Rescuer";
        public const string TAG_EQUIPMENT = "Equipment";
        public const string TAG_TRIAGE_ZONE = "TriageZone";
        public const string TAG_DANGER_ZONE = "DangerZone";
        public const string TAG_HOSPITAL = "Hospital";
        public const string TAG_AMBULANCE = "Ambulance";
        public const string TAG_PMA = "PMA";
        public const string TAG_SPAWN_POINT = "SpawnPoint";
        
        #endregion

        #region Layers
        
        public const int LAYER_DEFAULT = 0;
        public const int LAYER_VICTIM = 8;
        public const int LAYER_RESCUER = 9;
        public const int LAYER_EQUIPMENT = 10;
        public const int LAYER_TRIAGE_ZONE = 11;
        public const int LAYER_DANGER_ZONE = 12;
        public const int LAYER_AR_OVERLAY = 13;
        public const int LAYER_NAVIGATION = 14;
        public const int LAYER_TRIGGER = 15;
        public const int LAYER_GROUND = 16;
        
        #endregion

        #region Triage Colors (START Protocol)
        
        public static readonly Color COLOR_RED = new Color(0.9f, 0.1f, 0.1f, 1f);
        public static readonly Color COLOR_YELLOW = new Color(0.95f, 0.85f, 0.1f, 1f);
        public static readonly Color COLOR_GREEN = new Color(0.1f, 0.8f, 0.1f, 1f);
        public static readonly Color COLOR_BLACK = new Color(0.1f, 0.1f, 0.1f, 1f);
        public static readonly Color COLOR_UNKNOWN = new Color(0.5f, 0.5f, 0.5f, 1f);
        
        #endregion

        #region Vital Signs Thresholds (REQ-2)
        
        // Fréquence respiratoire (respirations/min)
        public const int RESPIRATORY_RATE_LOW = 10;
        public const int RESPIRATORY_RATE_HIGH = 30;
        public const int RESPIRATORY_RATE_CRITICAL = 6;
        
        // Saturation en oxygène (%)
        public const float SPO2_NORMAL = 95f;
        public const float SPO2_LOW = 90f;
        public const float SPO2_CRITICAL = 85f;
        
        // Fréquence cardiaque (bpm)
        public const int HEART_RATE_LOW = 60;
        public const int HEART_RATE_HIGH = 100;
        public const int HEART_RATE_CRITICAL_LOW = 40;
        public const int HEART_RATE_CRITICAL_HIGH = 150;
        
        // Pression artérielle systolique (mmHg)
        public const int BP_SYSTOLIC_LOW = 90;
        public const int BP_SYSTOLIC_HIGH = 140;
        public const int BP_SYSTOLIC_CRITICAL = 70;
        
        // Température (°C)
        public const float TEMP_LOW = 36f;
        public const float TEMP_HIGH = 38f;
        public const float TEMP_CRITICAL_LOW = 35f;
        public const float TEMP_CRITICAL_HIGH = 40f;
        
        #endregion

        #region Performance Requirements (NFR-*)
        
        // NFR-ACC: Précision ≥95%
        public const float DETECTION_ACCURACY_TARGET = 0.95f;
        public const float TRIAGE_ACCURACY_TARGET = 0.95f;
        public const float FALSE_POSITIVE_MAX = 0.05f;
        
        // NFR-VIT: Latence
        public const float LATENCY_OFFLINE_MAX_SECONDS = 30f;
        public const float LATENCY_ONLINE_MAX_SECONDS = 10f;
        public const float ANALYSIS_RGB_MAX_SECONDS = 2f;
        public const float ANALYSIS_THERMAL_MAX_SECONDS = 2f;
        public const float FUSION_MAX_SECONDS = 1f;
        public const float UI_RESPONSE_MAX_MS = 100f;
        
        // NFR-DIS: Disponibilité
        public const float BATTERY_CAPACITY_MAH = 5000f;
        public const float BATTERY_AUTONOMY_HOURS = 8f;
        
        // NFR-LOC: Localisation
        public const float LOCALIZATION_ACCURACY_METERS = 2f;
        
        // NFR-ROB: Robustesse
        public const float MIN_LIGHT_PERCENT = 10f;
        public const float MAX_SMOKE_PERCENT = 70f;
        public const float MAX_RAIN_PERCENT = 80f;
        public const float MAX_DEGRADATION_PERCENT = 15f;
        public const int MAX_RECONNECTION_ATTEMPTS = 5;
        
        #endregion

        #region Scenario Limits
        
        public const int MAX_VICTIMS_PER_SCENARIO = 100;
        public const int MAX_RESCUERS = 10;
        public const int MAX_AMBULANCES = 20;
        public const int MAX_HOSPITALS = 10;
        
        #endregion

        #region UI Constants
        
        public const float UI_FADE_DURATION = 0.3f;
        public const float NOTIFICATION_DURATION = 3f;
        public const float TOOLTIP_DELAY = 0.5f;
        
        #endregion

        #region Audio
        
        public const float AUDIO_MASTER_VOLUME = 1f;
        public const float AUDIO_SFX_VOLUME = 0.8f;
        public const float AUDIO_VOICE_VOLUME = 1f;
        public const float AUDIO_AMBIENT_VOLUME = 0.5f;
        
        #endregion

        #region Scene Names
        
        public const string SCENE_MAIN_MENU = "MainMenu";
        public const string SCENE_TRAINING = "TrainingScene";
        public const string SCENE_INDUSTRIAL = "Scenario_IndustrialExplosion";
        public const string SCENE_TRAIN = "Scenario_TrainAccident";
        public const string SCENE_BUILDING = "Scenario_BuildingCollapse";
        
        #endregion

        #region PlayerPrefs Keys
        
        public const string PREF_MASTER_VOLUME = "MasterVolume";
        public const string PREF_SFX_VOLUME = "SFXVolume";
        public const string PREF_VOICE_COMMANDS = "VoiceCommandsEnabled";
        public const string PREF_LANGUAGE = "Language";
        public const string PREF_DIFFICULTY = "Difficulty";
        public const string PREF_HIGH_SCORE = "HighScore_";
        
        #endregion

        #region Wake Word
        
        public const string WAKE_WORD = "RASSE";
        public const string VOICE_LANGUAGE = "fr-FR";
        
        #endregion

        #region Helper Methods
        
        /// <summary>
        /// Obtient la couleur correspondant à une catégorie START
        /// </summary>
        public static Color GetTriageColor(StartCategory category)
        {
            return category switch
            {
                StartCategory.Red => COLOR_RED,
                StartCategory.Yellow => COLOR_YELLOW,
                StartCategory.Green => COLOR_GREEN,
                StartCategory.Black => COLOR_BLACK,
                _ => COLOR_UNKNOWN
            };
        }
        
        /// <summary>
        /// Convertit une catégorie START en nom français
        /// </summary>
        public static string GetTriageCategoryName(StartCategory category)
        {
            return category switch
            {
                StartCategory.Red => "Urgence Absolue",
                StartCategory.Yellow => "Urgence Relative",
                StartCategory.Green => "Impliqué",
                StartCategory.Black => "Décédé",
                _ => "Non Classé"
            };
        }
        
        /// <summary>
        /// Vérifie si des signes vitaux sont dans les limites normales
        /// </summary>
        public static bool AreVitalSignsNormal(float heartRate, float respiratoryRate, float spo2, float bpSystolic)
        {
            return heartRate >= HEART_RATE_LOW && heartRate <= HEART_RATE_HIGH &&
                   respiratoryRate >= RESPIRATORY_RATE_LOW && respiratoryRate <= RESPIRATORY_RATE_HIGH &&
                   spo2 >= SPO2_NORMAL &&
                   bpSystolic >= BP_SYSTOLIC_LOW && bpSystolic <= BP_SYSTOLIC_HIGH;
        }
        
        #endregion
    }
}
