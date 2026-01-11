// ============================================================================
// RASSE - Réalité Augmentée en Situations Sanitaires Exceptionnelles
// VictimProfileSO.cs - ScriptableObject pour profils de victimes
// Conforme au rapport académique - REQ-2, REQ-3: Analyse et classification
// ============================================================================

using UnityEngine;
using System;
using System.Collections.Generic;

namespace RASSE.Data
{
    /// <summary>
    /// Type de blessure principale
    /// </summary>
    public enum InjuryType
    {
        None,                   // Pas de blessure visible
        Hemorrhage,             // Hémorragie
        Fracture,               // Fracture
        Burn,                   // Brûlure
        HeadTrauma,             // Traumatisme crânien
        ChestTrauma,            // Traumatisme thoracique
        AbdominalTrauma,        // Traumatisme abdominal
        SpinalInjury,           // Lésion médullaire
        Amputation,             // Amputation
        Crush,                  // Écrasement
        Inhalation,             // Inhalation fumée/gaz
        ChemicalExposure,       // Exposition chimique
        Hypothermia,            // Hypothermie
        Shock,                  // État de choc
        CardiacArrest,          // Arrêt cardiaque
        Multiple                // Polytraumatisme
    }

    /// <summary>
    /// Sévérité de la blessure
    /// </summary>
    public enum InjurySeverity
    {
        Minor,          // Mineur - soins simples
        Moderate,       // Modéré - soins médicaux nécessaires
        Severe,         // Sévère - chirurgie possible
        Critical,       // Critique - vital engagé
        Fatal           // Fatal - décès imminent/constaté
    }

    /// <summary>
    /// Détail d'une blessure spécifique
    /// </summary>
    [Serializable]
    public class InjuryDetail
    {
        public InjuryType type;
        public InjurySeverity severity;
        public string bodyLocation;         // "Tête", "Thorax", "Bras droit", etc.
        [TextArea(2, 3)]
        public string visualDescription;    // Description pour l'affichage AR
        public bool isVisible = true;       // Visible sans examen
        public bool requiresThermal = false; // Nécessite caméra thermique
        public float bleedingRate = 0f;     // ml/min si hémorragie
    }

    /// <summary>
    /// Constantes vitales d'un profil
    /// </summary>
    [Serializable]
    public class VitalSignsProfile
    {
        [Header("Respiration (REQ-2)")]
        [Range(0, 60)]
        public int respiratoryRate = 16;        // Respirations/min
        public bool respiratoryDistress = false;
        public bool airwayObstructed = false;

        [Header("Circulation")]
        [Range(0, 250)]
        public int heartRate = 80;              // BPM
        [Range(0, 300)]
        public int systolicBP = 120;            // mmHg
        [Range(0, 200)]
        public int diastolicBP = 80;            // mmHg
        [Range(0, 10)]
        public float capillaryRefillTime = 2f; // Secondes

        [Header("Neurologique")]
        [Range(3, 15)]
        public int glasgowScore = 15;           // Score Glasgow
        public bool pupilsReactive = true;
        public bool conscious = true;
        public bool responds = true;

        [Header("Autres")]
        [Range(20, 45)]
        public float temperature = 37f;         // °C
        [Range(0, 100)]
        public float spO2 = 98f;               // %
        public string skinColor = "Normal";     // Normal, Pâle, Cyanosé, Rouge

        /// <summary>
        /// Calcule la catégorie START basée sur ces constantes
        /// Conforme au protocole START du rapport
        /// </summary>
        public string CalculateStartCategory()
        {
            // 1. Marche possible ? → VERT (non applicable ici, géré par comportement)
            
            // 2. Respiration absente ?
            if (respiratoryRate == 0)
            {
                // Après libération voies aériennes
                if (airwayObstructed)
                    return "RED";       // Libérer VA → respire → ROUGE
                else
                    return "BLACK";     // Ne respire toujours pas → NOIR
            }

            // 3. Fréquence respiratoire
            if (respiratoryRate > 30)
                return "RED";

            // 4. Perfusion (TRC ou pouls radial)
            if (capillaryRefillTime > 2f || systolicBP < 90)
                return "RED";

            // 5. État neurologique
            if (!responds || glasgowScore < 14)
                return "RED";

            // Sinon
            return "YELLOW";
        }

        /// <summary>
        /// Obtient un résumé textuel des constantes
        /// </summary>
        public string GetSummary()
        {
            return $"FR:{respiratoryRate}/min | FC:{heartRate}bpm | PA:{systolicBP}/{diastolicBP} | " +
                   $"SpO2:{spO2}% | T°:{temperature}°C | Glasgow:{glasgowScore}";
        }
    }

    /// <summary>
    /// Comportement ambulatoire de la victime
    /// </summary>
    [Serializable]
    public class VictimBehavior
    {
        public bool canWalk = false;            // Peut marcher → VERT dans START
        public bool canSpeak = true;
        public bool isAgitated = false;
        public bool isUnconscious = false;
        public bool isTrapped = false;          // Coincée sous débris
        
        [Range(0, 1)]
        public float cooperationLevel = 0.8f;   // Niveau de coopération
        
        [TextArea(2, 3)]
        public string verbalResponse;           // Ce que la victime dit
    }

    /// <summary>
    /// Évolution temporelle du profil
    /// </summary>
    [Serializable]
    public class ConditionEvolution
    {
        [Tooltip("Temps en secondes avant détérioration")]
        public float timeToDeterioration = 300f;    // 5 minutes
        
        [Tooltip("Variation FR par minute")]
        public float respiratoryRateChange = 0f;
        
        [Tooltip("Variation FC par minute")]
        public float heartRateChange = 0f;
        
        [Tooltip("Perte de sang ml/min")]
        public float bloodLossRate = 0f;
        
        [Tooltip("Variation Glasgow par 5 min")]
        public int glasgowChange = 0;
        
        [Tooltip("Catégorie finale si non traité")]
        public string deterioratedCategory = "BLACK";
    }

    /// <summary>
    /// ScriptableObject définissant un profil de victime complet
    /// Conforme aux exigences REQ-2 (analyse) et REQ-3 (classification START)
    /// </summary>
    [CreateAssetMenu(fileName = "NewVictimProfile", menuName = "RASSE/Victim Profile", order = 3)]
    public class VictimProfileSO : ScriptableObject
    {
        [Header("=== IDENTIFICATION ===")]
        public string profileId;
        public string profileName = "Victime Type";
        [TextArea(2, 4)]
        public string description;
        
        [Header("=== DONNÉES DÉMOGRAPHIQUES ===")]
        public string defaultName = "Inconnu";
        public Gender gender = Gender.Unknown;
        [Range(0, 120)]
        public int ageMin = 20;
        [Range(0, 120)]
        public int ageMax = 50;
        
        [Header("=== CATÉGORIE START (REQ-3) ===")]
        public string startCategory = "YELLOW";     // RED, YELLOW, GREEN, BLACK
        public Color categoryColor = Color.yellow;
        [Range(1, 10)]
        public int triagePriority = 5;              // 1 = plus urgent

        [Header("=== BLESSURES ===")]
        public InjuryType primaryInjury = InjuryType.None;
        public InjurySeverity primarySeverity = InjurySeverity.Moderate;
        public List<InjuryDetail> injuries = new List<InjuryDetail>();

        [Header("=== CONSTANTES VITALES (REQ-2) ===")]
        public VitalSignsProfile vitalSigns = new VitalSignsProfile();

        [Header("=== COMPORTEMENT ===")]
        public VictimBehavior behavior = new VictimBehavior();

        [Header("=== ÉVOLUTION ===")]
        public ConditionEvolution evolution = new ConditionEvolution();
        public bool canDeteriorate = true;
        public bool canImproveWithTreatment = true;

        [Header("=== APPARENCE VISUELLE ===")]
        public Color skinTint = Color.white;
        public bool hasVisibleBlood = false;
        public bool hasVisibleBurns = false;
        public bool hasVisibleFracture = false;
        [Range(0, 1)]
        public float dirtLevel = 0.3f;

        [Header("=== AUDIO ===")]
        public AudioClip[] painSounds;
        public AudioClip[] breathingSounds;
        public AudioClip[] voiceLines;
        [Range(0, 1)]
        public float vocalFrequency = 0.3f;         // Fréquence des sons

        [Header("=== SCORING ===")]
        public int correctTriagePoints = 100;
        public int incorrectTriagePenalty = -50;
        public int missedEvacuationPenalty = -100;

        public enum Gender { Male, Female, Unknown }

        /// <summary>
        /// Génère un âge aléatoire dans la plage
        /// </summary>
        public int GenerateRandomAge()
        {
            return UnityEngine.Random.Range(ageMin, ageMax + 1);
        }

        /// <summary>
        /// Vérifie si le triage donné est correct
        /// </summary>
        public bool IsTriageCorrect(string assignedCategory)
        {
            return assignedCategory == startCategory;
        }

        /// <summary>
        /// Calcule le score pour un triage
        /// </summary>
        public int CalculateTriageScore(string assignedCategory, float timeToTriage)
        {
            int score = 0;

            if (assignedCategory == startCategory)
            {
                score = correctTriagePoints;
                
                // Bonus rapidité (basé sur catégorie)
                float maxTime = startCategory == "RED" ? 60f : 120f;
                if (timeToTriage < maxTime)
                {
                    score += Mathf.RoundToInt((1f - timeToTriage / maxTime) * 50f);
                }
            }
            else
            {
                score = incorrectTriagePenalty;
                
                // Pénalité supplémentaire si sur-triage ou sous-triage dangereux
                if (startCategory == "RED" && assignedCategory == "GREEN")
                    score -= 100; // Sous-triage critique
                if (startCategory == "BLACK" && assignedCategory == "RED")
                    score -= 50;  // Gaspillage ressources
            }

            return score;
        }

        /// <summary>
        /// Obtient la description complète pour l'affichage AR
        /// </summary>
        public string GetARDescription()
        {
            string desc = $"<b>{profileName}</b>\n";
            desc += $"Catégorie: <color=#{ColorUtility.ToHtmlStringRGB(categoryColor)}>{startCategory}</color>\n\n";
            
            if (injuries.Count > 0)
            {
                desc += "<b>Blessures:</b>\n";
                foreach (var injury in injuries)
                {
                    if (injury.isVisible)
                    {
                        desc += $"• {injury.type} ({injury.bodyLocation})\n";
                    }
                }
            }

            desc += $"\n<b>Constantes:</b>\n{vitalSigns.GetSummary()}";
            
            return desc;
        }

        /// <summary>
        /// Applique une détérioration temporelle
        /// </summary>
        public VitalSignsProfile GetDeterioratedVitals(float elapsedMinutes)
        {
            if (!canDeteriorate || elapsedMinutes < evolution.timeToDeterioration / 60f)
                return vitalSigns;

            var deteriorated = new VitalSignsProfile
            {
                respiratoryRate = Mathf.Max(0, vitalSigns.respiratoryRate + 
                    Mathf.RoundToInt(evolution.respiratoryRateChange * elapsedMinutes)),
                heartRate = Mathf.Max(0, vitalSigns.heartRate + 
                    Mathf.RoundToInt(evolution.heartRateChange * elapsedMinutes)),
                glasgowScore = Mathf.Clamp(vitalSigns.glasgowScore + 
                    Mathf.RoundToInt(evolution.glasgowChange * elapsedMinutes / 5f), 3, 15),
                systolicBP = vitalSigns.systolicBP,
                diastolicBP = vitalSigns.diastolicBP,
                temperature = vitalSigns.temperature,
                spO2 = Mathf.Max(0, vitalSigns.spO2 - elapsedMinutes * 0.5f),
                capillaryRefillTime = vitalSigns.capillaryRefillTime,
                conscious = vitalSigns.conscious,
                responds = vitalSigns.responds,
                pupilsReactive = vitalSigns.pupilsReactive,
                respiratoryDistress = vitalSigns.respiratoryDistress,
                airwayObstructed = vitalSigns.airwayObstructed,
                skinColor = vitalSigns.skinColor
            };

            // Ajuster selon perte de sang
            if (evolution.bloodLossRate > 0)
            {
                float bloodLoss = evolution.bloodLossRate * elapsedMinutes;
                if (bloodLoss > 500)
                {
                    deteriorated.heartRate += 20;
                    deteriorated.systolicBP -= 20;
                }
                if (bloodLoss > 1000)
                {
                    deteriorated.heartRate += 40;
                    deteriorated.systolicBP -= 40;
                    deteriorated.skinColor = "Pâle";
                }
                if (bloodLoss > 1500)
                {
                    deteriorated.conscious = false;
                    deteriorated.responds = false;
                }
            }

            return deteriorated;
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(profileId))
                profileId = "VIC-" + System.Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

            // Définir couleur selon catégorie
            switch (startCategory)
            {
                case "RED": categoryColor = Color.red; triagePriority = 1; break;
                case "YELLOW": categoryColor = Color.yellow; triagePriority = 3; break;
                case "GREEN": categoryColor = Color.green; triagePriority = 5; break;
                case "BLACK": categoryColor = Color.black; triagePriority = 10; break;
            }

            ageMax = Mathf.Max(ageMin, ageMax);
        }
    }
}
