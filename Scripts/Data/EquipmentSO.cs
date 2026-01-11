// ============================================================================
// RASSE - Réalité Augmentée en Situations Sanitaires Exceptionnelles
// EquipmentSO.cs - ScriptableObject pour équipements médicaux
// Conforme au rapport académique - REQ-5: Consignes premiers secours
// ============================================================================

using UnityEngine;
using System;
using System.Collections.Generic;

namespace RASSE.Data
{
    /// <summary>
    /// Catégorie d'équipement médical
    /// </summary>
    public enum EquipmentCategory
    {
        Airway,             // Voies aériennes (canule, masque O2)
        Breathing,          // Respiration (BAVU, oxygène)
        Circulation,        // Circulation (garrot, pansement compressif)
        Immobilization,     // Immobilisation (attelles, collier cervical)
        Wound,              // Plaies (compresses, désinfectant)
        Burn,               // Brûlures (gel, pansements spéciaux)
        Monitoring,         // Monitoring (oxymètre, tensiomètre)
        Medication,         // Médicaments (adrénaline, morphine)
        Transport,          // Transport (brancard, couverture)
        Protection,         // Protection (gants, masque FFP2)
        Communication,      // Communication (radio, tablette)
        Other               // Autre
    }

    /// <summary>
    /// Niveau de compétence requis
    /// </summary>
    public enum RequiredSkillLevel
    {
        Basic,              // Tout public - PSC1
        Intermediate,       // Secouriste - PSE1/PSE2
        Advanced,           // Paramédical
        Medical             // Médecin uniquement
    }

    /// <summary>
    /// Compatibilité avec catégorie START
    /// </summary>
    [Serializable]
    public class CategoryCompatibility
    {
        public bool red = true;
        public bool yellow = true;
        public bool green = true;
        public bool black = false;
    }

    /// <summary>
    /// Instruction d'utilisation d'un équipement
    /// </summary>
    [Serializable]
    public class UsageInstruction
    {
        public int stepNumber;
        public string instruction;
        [TextArea(2, 3)]
        public string details;
        public Sprite illustration;
        public AudioClip voiceGuide;
        public float estimatedDuration;     // Secondes
    }

    /// <summary>
    /// ScriptableObject définissant un équipement médical
    /// Utilisé pour les consignes premiers secours (REQ-5)
    /// </summary>
    [CreateAssetMenu(fileName = "NewEquipment", menuName = "RASSE/Equipment", order = 5)]
    public class EquipmentSO : ScriptableObject
    {
        [Header("=== IDENTIFICATION ===")]
        public string equipmentId;
        public string equipmentName = "Nouvel Équipement";
        public string codeName;             // Code court (ex: "GARROT")
        [TextArea(2, 4)]
        public string description;
        public EquipmentCategory category = EquipmentCategory.Other;

        [Header("=== VISUEL ===")]
        public Sprite icon;
        public Sprite detailedImage;
        public GameObject prefab3D;
        public Color uiColor = Color.white;

        [Header("=== UTILISATION ===")]
        public RequiredSkillLevel requiredSkill = RequiredSkillLevel.Basic;
        public CategoryCompatibility compatibility = new CategoryCompatibility();
        public List<UsageInstruction> usageSteps = new List<UsageInstruction>();
        
        [Header("=== CARACTÉRISTIQUES ===")]
        [Tooltip("Consommable (usage unique)")]
        public bool isConsumable = true;
        [Tooltip("Réutilisable après nettoyage")]
        public bool isReusable = false;
        public float usageDurationSeconds = 30f;
        public int quantityPerKit = 1;

        [Header("=== INDICATIONS ===")]
        [TextArea(2, 4)]
        public string indications;          // Quand utiliser
        [TextArea(2, 4)]
        public string contraindications;    // Quand ne pas utiliser
        public string[] applicableInjuries; // Types de blessures

        [Header("=== AUDIO ===")]
        public AudioClip usageSound;
        public AudioClip completionSound;

        [Header("=== SIMULATION ===")]
        [Tooltip("Efficacité de l'équipement (0-1)")]
        [Range(0, 1)]
        public float effectiveness = 0.9f;
        [Tooltip("Points bonus si utilisé correctement")]
        public int correctUsagePoints = 20;

        /// <summary>
        /// Vérifie si l'équipement est applicable pour une catégorie
        /// </summary>
        public bool IsApplicableForCategory(string startCategory)
        {
            switch (startCategory.ToUpper())
            {
                case "RED": return compatibility.red;
                case "YELLOW": return compatibility.yellow;
                case "GREEN": return compatibility.green;
                case "BLACK": return compatibility.black;
                default: return false;
            }
        }

        /// <summary>
        /// Vérifie si l'équipement est applicable pour un type de blessure
        /// </summary>
        public bool IsApplicableForInjury(string injuryType)
        {
            if (applicableInjuries == null || applicableInjuries.Length == 0)
                return true;

            foreach (var injury in applicableInjuries)
            {
                if (injury.Equals(injuryType, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Génère le texte d'instructions complet
        /// </summary>
        public string GetFullInstructions()
        {
            string text = $"=== {equipmentName} ===\n\n";
            text += $"{description}\n\n";
            
            text += $"Catégorie: {category}\n";
            text += $"Niveau requis: {requiredSkill}\n\n";

            if (!string.IsNullOrEmpty(indications))
                text += $"INDICATIONS:\n{indications}\n\n";

            if (!string.IsNullOrEmpty(contraindications))
                text += $"CONTRE-INDICATIONS:\n{contraindications}\n\n";

            text += "MODE D'EMPLOI:\n";
            foreach (var step in usageSteps)
            {
                text += $"  {step.stepNumber}. {step.instruction}\n";
                if (!string.IsNullOrEmpty(step.details))
                    text += $"     → {step.details}\n";
            }

            return text;
        }

        /// <summary>
        /// Calcule le temps total d'utilisation estimé
        /// </summary>
        public float GetTotalEstimatedTime()
        {
            float total = 0f;
            foreach (var step in usageSteps)
            {
                total += step.estimatedDuration;
            }
            return total > 0 ? total : usageDurationSeconds;
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(equipmentId))
                equipmentId = "EQP-" + System.Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

            // Couleur par défaut selon catégorie
            switch (category)
            {
                case EquipmentCategory.Airway:
                    uiColor = new Color(0.5f, 0.8f, 1f);
                    break;
                case EquipmentCategory.Breathing:
                    uiColor = new Color(0.7f, 0.9f, 0.7f);
                    break;
                case EquipmentCategory.Circulation:
                    uiColor = new Color(1f, 0.6f, 0.6f);
                    break;
                case EquipmentCategory.Immobilization:
                    uiColor = new Color(1f, 0.9f, 0.5f);
                    break;
            }
        }
    }

    /// <summary>
    /// Collection d'équipements (Kit médical)
    /// </summary>
    [CreateAssetMenu(fileName = "NewMedicalKit", menuName = "RASSE/Medical Kit", order = 6)]
    public class MedicalKitSO : ScriptableObject
    {
        [Header("=== IDENTIFICATION ===")]
        public string kitId;
        public string kitName = "Kit Médical Standard";
        [TextArea(2, 3)]
        public string description;
        public Sprite kitIcon;

        [Header("=== CONTENU ===")]
        public List<KitContent> contents = new List<KitContent>();

        [Header("=== CARACTÉRISTIQUES ===")]
        public float totalWeight = 5f;          // kg
        public Vector3 dimensions;               // cm
        public bool isPortable = true;
        public RequiredSkillLevel minimumSkill = RequiredSkillLevel.Basic;

        [Serializable]
        public class KitContent
        {
            public EquipmentSO equipment;
            public int quantity = 1;
        }

        /// <summary>
        /// Vérifie si le kit contient un équipement donné
        /// </summary>
        public bool ContainsEquipment(string equipmentId)
        {
            return contents.Exists(c => c.equipment != null && 
                                       c.equipment.equipmentId == equipmentId);
        }

        /// <summary>
        /// Obtient la quantité d'un équipement dans le kit
        /// </summary>
        public int GetEquipmentQuantity(string equipmentId)
        {
            var content = contents.Find(c => c.equipment != null && 
                                            c.equipment.equipmentId == equipmentId);
            return content?.quantity ?? 0;
        }

        /// <summary>
        /// Liste tous les équipements par catégorie
        /// </summary>
        public Dictionary<EquipmentCategory, List<EquipmentSO>> GetEquipmentByCategory()
        {
            var result = new Dictionary<EquipmentCategory, List<EquipmentSO>>();
            
            foreach (var content in contents)
            {
                if (content.equipment == null) continue;
                
                var cat = content.equipment.category;
                if (!result.ContainsKey(cat))
                    result[cat] = new List<EquipmentSO>();
                
                result[cat].Add(content.equipment);
            }
            
            return result;
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(kitId))
                kitId = "KIT-" + System.Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
        }
    }
}
