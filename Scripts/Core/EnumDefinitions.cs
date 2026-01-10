using UnityEngine;

namespace RASSE.Core
{
    /// <summary>
    /// Catégories START pour le triage médical d'urgence
    /// </summary>
    public enum StartCategory
    {
        None = 0,       // Non évalué
        Green = 1,      // Mineur - peut marcher
        Yellow = 2,     // Différé - blessé mais stable
        Red = 3,        // Immédiat - urgence vitale
        Black = 4       // Décédé ou sans espoir
    }

    /// <summary>
    /// Types de blessures possibles
    /// </summary>
    public enum InjuryType
    {
        None,
        Hemorrhage,         // Hémorragie
        Fracture,           // Fracture
        Burn,               // Brûlure
        Concussion,         // Commotion cérébrale
        InternalBleeding,   // Hémorragie interne
        Crush,              // Écrasement
        Laceration,         // Lacération
        Puncture,           // Perforation
        Amputation,         // Amputation
        Asphyxiation,       // Asphyxie
        ChemicalExposure,   // Exposition chimique
        Electrocution,      // Électrocution
        Hypothermia,        // Hypothermie
        Hyperthermia,       // Hyperthermie
        CardiacArrest,      // Arrêt cardiaque
        RespiratoryDistress // Détresse respiratoire
    }

    /// <summary>
    /// Localisation des blessures sur le corps
    /// </summary>
    public enum InjuryLocation
    {
        Head,
        Neck,
        Chest,
        Abdomen,
        Back,
        Pelvis,
        LeftArm,
        RightArm,
        LeftLeg,
        RightLeg,
        LeftHand,
        RightHand,
        LeftFoot,
        RightFoot,
        Multiple
    }

    /// <summary>
    /// Sévérité des blessures
    /// </summary>
    public enum InjurySeverity
    {
        Minor,      // Mineure
        Moderate,   // Modérée
        Severe,     // Sévère
        Critical,   // Critique
        Fatal       // Fatale
    }

    /// <summary>
    /// Genre du patient
    /// </summary>
    public enum Gender
    {
        Male,
        Female,
        Unknown
    }

    /// <summary>
    /// État de conscience (Glasgow Coma Scale simplifié)
    /// </summary>
    public enum ConsciousnessLevel
    {
        Alert,          // Alerte et orienté
        Verbal,         // Répond aux stimuli verbaux
        Pain,           // Répond à la douleur
        Unresponsive    // Inconscient
    }

    /// <summary>
    /// État d'animation de la victime
    /// </summary>
    public enum VictimAnimationState
    {
        Unconscious,    // Allongé inconscient
        Sitting,        // Assis
        Standing,       // Debout
        InPain,         // En souffrance
        Walking,        // Marchant
        Crawling        // Rampant
    }

    /// <summary>
    /// État de l'ambulance
    /// </summary>
    public enum AmbulanceState
    {
        Available,          // Disponible à la base
        EnRouteToScene,     // En route vers la scène
        OnScene,            // Sur place
        EnRouteToHospital,  // En route vers l'hôpital
        AtHospital,         // À l'hôpital
        OutOfService        // Hors service
    }

    /// <summary>
    /// Type d'ambulance
    /// </summary>
    public enum AmbulanceType
    {
        Basic,      // VSAV - Transport basique
        Advanced,   // VLI - Véhicule Léger Infirmier
        SMUR        // SMUR - Service Mobile d'Urgence et Réanimation
    }

    /// <summary>
    /// Niveau de capacité hospitalière
    /// </summary>
    public enum HospitalCapacityLevel
    {
        Low,        // Capacité faible
        Medium,     // Capacité moyenne
        High        // Grande capacité
    }

    /// <summary>
    /// Spécialités hospitalières
    /// </summary>
    [System.Flags]
    public enum HospitalSpecialty
    {
        None = 0,
        TraumaCenter = 1 << 0,      // Centre de traumatologie
        BurnUnit = 1 << 1,          // Unité des grands brûlés
        CardiacCare = 1 << 2,       // Soins cardiaques
        Neurology = 1 << 3,         // Neurologie
        Pediatric = 1 << 4,         // Pédiatrie
        Obstetrics = 1 << 5,        // Obstétrique
        Orthopedic = 1 << 6,        // Orthopédie
        Toxicology = 1 << 7,        // Toxicologie
        Psychiatric = 1 << 8,       // Psychiatrie
        Hyperbaric = 1 << 9         // Caisson hyperbare
    }

    /// <summary>
    /// Type de scénario
    /// </summary>
    public enum ScenarioType
    {
        TrafficAccident,        // Accident de la route
        IndustrialAccident,     // Accident industriel
        BuildingCollapse,       // Effondrement de bâtiment
        Fire,                   // Incendie
        Explosion,              // Explosion
        MassCasualty,           // Attaque terroriste/tuerie
        NaturalDisaster,        // Catastrophe naturelle
        ChemicalHazard,         // Risque chimique
        Training                // Entraînement
    }

    /// <summary>
    /// Niveau de difficulté
    /// </summary>
    public enum DifficultyLevel
    {
        Tutorial,   // Tutoriel guidé
        Easy,       // Facile
        Normal,     // Normal
        Hard,       // Difficile
        Expert      // Expert
    }

    /// <summary>
    /// État du scénario
    /// </summary>
    public enum ScenarioState
    {
        NotStarted,
        Initializing,
        Running,
        Paused,
        Completed,
        Failed
    }

    /// <summary>
    /// Types d'entrées dans le dossier patient
    /// </summary>
    public enum RecordEntryType
    {
        Detection,          // Détection de la victime
        InitialAssessment,  // Évaluation initiale
        Triage,             // Classification triage
        Treatment,          // Traitement appliqué
        VitalSignUpdate,    // Mise à jour signes vitaux
        HospitalAssignment, // Assignation hôpital
        AmbulanceAssignment,// Assignation ambulance
        Evacuation,         // Évacuation
        StatusChange,       // Changement d'état
        Note                // Note libre
    }

    /// <summary>
    /// Commandes vocales disponibles
    /// </summary>
    public enum VoiceCommand
    {
        None,
        // Triage
        Validate,       // Valider le triage suggéré
        Red,            // Forcer catégorie rouge
        Yellow,         // Forcer catégorie jaune
        Green,          // Forcer catégorie vert
        Black,          // Forcer catégorie noir
        // Actions
        ConfirmHospital,// Confirmer l'hôpital sélectionné
        SendHome,       // Retour à domicile (vert uniquement)
        CallAmbulance,  // Appeler ambulance
        // Navigation
        Next,           // Victime suivante
        Scan,           // Scanner la zone
        Navigate,       // Démarrer navigation
        // Système
        Help,           // Aide
        Status,         // Statut global
        Cancel,         // Annuler action
        Repeat          // Répéter dernière info
    }

    /// <summary>
    /// Direction de navigation
    /// </summary>
    public enum NavigationDirection
    {
        Straight,
        Left,
        Right,
        SharpLeft,
        SharpRight,
        UTurn,
        Arrived
    }

    /// <summary>
    /// Type de marqueur AR
    /// </summary>
    public enum ARMarkerType
    {
        Victim,
        TriagedVictim,
        Ambulance,
        Hospital,
        Waypoint,
        Destination,
        Hazard,
        SafeZone
    }

    /// <summary>
    /// Signes vitaux complets
    /// </summary>
    [System.Serializable]
    public struct VitalSigns
    {
        [Header("=== RESPIRATION ===")]
        public bool isBreathing;
        public int respiratoryRate;                 // Fréquence respiratoire (normal: 12-20)
        public bool breathingAfterAirwayManeuver;   // Respire après manœuvre voies aériennes
        
        [Header("=== CIRCULATION ===")]
        public int heartRate;                       // Fréquence cardiaque (normal: 60-100)
        public float capillaryRefillTime;           // Temps de remplissage capillaire (normal: <2s)
        public bool hasRadialPulse;                 // Présence du pouls radial
        public int systolicBP;                      // Pression systolique (normal: 90-140)
        public int diastolicBP;                     // Pression diastolique (normal: 60-90)
        
        [Header("=== NEUROLOGIQUE ===")]
        public bool canFollowCommands;              // Peut suivre des ordres simples
        public int glasgowComaScale;                // Score Glasgow (3-15, normal: 15)
        public ConsciousnessLevel consciousnessLevel;
        
        [Header("=== MOBILITÉ ===")]
        public bool canWalk;                        // Peut marcher
        public bool canMove;                        // Peut bouger
        
        [Header("=== AUTRES ===")]
        public float spO2;                          // Saturation en oxygène (normal: 95-100%)
        public float temperature;                   // Température corporelle (normal: 36.5-37.5°C)
        public float painLevel;                     // Niveau de douleur (0-10)

        /// <summary>
        /// Crée des signes vitaux normaux
        /// </summary>
        public static VitalSigns Normal()
        {
            return new VitalSigns
            {
                isBreathing = true,
                respiratoryRate = 16,
                breathingAfterAirwayManeuver = true,
                heartRate = 75,
                capillaryRefillTime = 1.5f,
                hasRadialPulse = true,
                systolicBP = 120,
                diastolicBP = 80,
                canFollowCommands = true,
                glasgowComaScale = 15,
                consciousnessLevel = ConsciousnessLevel.Alert,
                canWalk = true,
                canMove = true,
                spO2 = 98f,
                temperature = 37f,
                painLevel = 2f
            };
        }

        /// <summary>
        /// Crée des signes vitaux critiques
        /// </summary>
        public static VitalSigns Critical()
        {
            return new VitalSigns
            {
                isBreathing = true,
                respiratoryRate = 32,
                breathingAfterAirwayManeuver = true,
                heartRate = 130,
                capillaryRefillTime = 4f,
                hasRadialPulse = false,
                systolicBP = 70,
                diastolicBP = 40,
                canFollowCommands = false,
                glasgowComaScale = 8,
                consciousnessLevel = ConsciousnessLevel.Pain,
                canWalk = false,
                canMove = false,
                spO2 = 82f,
                temperature = 35.5f,
                painLevel = 9f
            };
        }

        /// <summary>
        /// Vérifie si les signes vitaux sont dans les normes
        /// </summary>
        public bool IsNormal()
        {
            return isBreathing &&
                   respiratoryRate >= 12 && respiratoryRate <= 20 &&
                   heartRate >= 60 && heartRate <= 100 &&
                   capillaryRefillTime <= 2f &&
                   hasRadialPulse &&
                   canFollowCommands &&
                   glasgowComaScale >= 13 &&
                   spO2 >= 95f;
        }
    }

    /// <summary>
    /// Données hospitalières pour ScriptableObject
    /// </summary>
    [System.Serializable]
    public class HospitalData
    {
        public string hospitalName;
        public string hospitalId;
        public float distanceKm;
        public HospitalSpecialty specialties;
        public int totalBeds;
        public int availableBeds;
        public HospitalCapacityLevel capacityLevel;
        public Vector3 position;
        public float averageWaitTime;
        public bool isTraumaCenter;
        public string phoneNumber;
    }

    /// <summary>
    /// Extensions utilitaires pour les enums
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Retourne la couleur associée à une catégorie START
        /// </summary>
        public static Color GetColor(this StartCategory category)
        {
            switch (category)
            {
                case StartCategory.Green: return new Color(0.2f, 0.8f, 0.2f);
                case StartCategory.Yellow: return new Color(1f, 0.9f, 0.2f);
                case StartCategory.Red: return new Color(0.9f, 0.2f, 0.2f);
                case StartCategory.Black: return Color.black;
                default: return Color.gray;
            }
        }

        /// <summary>
        /// Retourne le nom français d'une catégorie START
        /// </summary>
        public static string GetFrenchName(this StartCategory category)
        {
            switch (category)
            {
                case StartCategory.Green: return "VERT - Mineur";
                case StartCategory.Yellow: return "JAUNE - Différé";
                case StartCategory.Red: return "ROUGE - Immédiat";
                case StartCategory.Black: return "NOIR - Décédé";
                default: return "Non évalué";
            }
        }

        /// <summary>
        /// Retourne la priorité (1 = plus haute)
        /// </summary>
        public static int GetPriority(this StartCategory category)
        {
            switch (category)
            {
                case StartCategory.Red: return 1;
                case StartCategory.Yellow: return 2;
                case StartCategory.Green: return 3;
                case StartCategory.Black: return 4;
                default: return 5;
            }
        }

        /// <summary>
        /// Retourne le nom français d'un type de blessure
        /// </summary>
        public static string GetFrenchName(this InjuryType injury)
        {
            switch (injury)
            {
                case InjuryType.Hemorrhage: return "Hémorragie";
                case InjuryType.Fracture: return "Fracture";
                case InjuryType.Burn: return "Brûlure";
                case InjuryType.Concussion: return "Commotion cérébrale";
                case InjuryType.InternalBleeding: return "Hémorragie interne";
                case InjuryType.Crush: return "Écrasement";
                case InjuryType.Laceration: return "Lacération";
                case InjuryType.Puncture: return "Perforation";
                case InjuryType.Amputation: return "Amputation";
                case InjuryType.Asphyxiation: return "Asphyxie";
                case InjuryType.ChemicalExposure: return "Exposition chimique";
                case InjuryType.Electrocution: return "Électrocution";
                case InjuryType.Hypothermia: return "Hypothermie";
                case InjuryType.Hyperthermia: return "Hyperthermie";
                case InjuryType.CardiacArrest: return "Arrêt cardiaque";
                case InjuryType.RespiratoryDistress: return "Détresse respiratoire";
                default: return "Inconnue";
            }
        }
    }
}
