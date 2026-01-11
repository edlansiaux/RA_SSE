// ============================================================================
// MedicalKitSO.cs - ScriptableObject pour les Kits Médicaux
// Projet RA-SSE : Simulateur de Triage Médical en Réalité Augmentée
// Conforme au rapport académique - Bloc SysML 5 (Équipements Médicaux)
// ============================================================================

using UnityEngine;
using System;
using System.Collections.Generic;

namespace RASSE.Data
{
    /// <summary>
    /// Type de kit médical selon les normes françaises de secourisme
    /// </summary>
    public enum MedicalKitType
    {
        PSE1,           // Premiers Secours en Équipe niveau 1
        PSE2,           // Premiers Secours en Équipe niveau 2
        DSA,            // Défibrillateur Semi-Automatique
        SacUrgence,     // Sac d'urgence complet
        KitHemorragie,  // Kit hémorragie massive
        KitBrulure,     // Kit brûlures
        KitImmobilisation, // Kit immobilisation/contention
        KitOxygene,     // Kit oxygénothérapie
        KitPediatrique, // Kit pédiatrique
        KitChimique,    // Kit décontamination chimique
        KitTrauma       // Kit trauma avancé
    }

    /// <summary>
    /// Niveau de compétence requis pour utiliser le kit
    /// </summary>
    public enum RequiredCompetencyLevel
    {
        Secouriste,     // PSC1/SST
        EquipierPSE1,   // PSE1
        EquipierPSE2,   // PSE2
        Infirmier,      // IDE
        Medecin,        // Médecin
        MedecinUrgentiste // Médecin urgentiste SAMU
    }

    /// <summary>
    /// Contenu d'un kit médical - item individuel
    /// </summary>
    [Serializable]
    public class MedicalKitItem
    {
        [Tooltip("Nom de l'équipement")]
        public string itemName;
        
        [Tooltip("Quantité dans le kit")]
        public int quantity;
        
        [Tooltip("Référence à l'EquipmentSO si disponible")]
        public EquipmentSO equipmentReference;
        
        [Tooltip("Poids unitaire en grammes")]
        public float unitWeightGrams;
        
        [Tooltip("Consommable ou réutilisable")]
        public bool isConsumable;
        
        [Tooltip("Date de péremption applicable")]
        public bool hasExpiryDate;
        
        [Tooltip("Durée de vie en mois (si péremption)")]
        public int shelfLifeMonths;
    }

    /// <summary>
    /// Protocole associé au kit médical
    /// </summary>
    [Serializable]
    public class KitProtocol
    {
        [Tooltip("Nom du protocole")]
        public string protocolName;
        
        [Tooltip("Description du protocole")]
        [TextArea(2, 4)]
        public string description;
        
        [Tooltip("Étapes du protocole")]
        public List<string> steps = new List<string>();
        
        [Tooltip("Catégories de triage concernées")]
        public List<string> applicableTriageCategories = new List<string>();
    }

    /// <summary>
    /// ScriptableObject définissant un Kit Médical complet
    /// Utilisé pour équiper les secouristes dans le simulateur RA-SSE
    /// </summary>
    [CreateAssetMenu(fileName = "New_MedicalKit", menuName = "RA-SSE/Medical/Medical Kit", order = 2)]
    public class MedicalKitSO : ScriptableObject
    {
        // ===== IDENTIFICATION =====
        [Header("=== IDENTIFICATION ===")]
        
        [Tooltip("Identifiant unique du kit")]
        public string kitId;
        
        [Tooltip("Nom complet du kit")]
        public string kitName;
        
        [Tooltip("Type de kit médical")]
        public MedicalKitType kitType;
        
        [Tooltip("Description du kit et de son utilisation")]
        [TextArea(3, 6)]
        public string description;
        
        [Tooltip("Icône du kit pour l'interface")]
        public Sprite kitIcon;
        
        // ===== CARACTÉRISTIQUES PHYSIQUES =====
        [Header("=== CARACTÉRISTIQUES PHYSIQUES ===")]
        
        [Tooltip("Poids total du kit en kg")]
        [Range(0.5f, 30f)]
        public float totalWeightKg = 5f;
        
        [Tooltip("Dimensions du kit (L x l x h) en cm")]
        public Vector3 dimensionsCm = new Vector3(40, 30, 20);
        
        [Tooltip("Couleur du kit (pour identification visuelle)")]
        public Color kitColor = Color.red;
        
        [Tooltip("Résistant à l'eau")]
        public bool isWaterResistant;
        
        [Tooltip("Résistant aux chocs")]
        public bool isShockResistant;
        
        // ===== CONTENU DU KIT =====
        [Header("=== CONTENU DU KIT ===")]
        
        [Tooltip("Liste des équipements contenus dans le kit")]
        public List<MedicalKitItem> contents = new List<MedicalKitItem>();
        
        // ===== UTILISATION =====
        [Header("=== UTILISATION ===")]
        
        [Tooltip("Niveau de compétence requis")]
        public RequiredCompetencyLevel requiredCompetency = RequiredCompetencyLevel.EquipierPSE1;
        
        [Tooltip("Nombre maximum de victimes traitables")]
        [Range(1, 50)]
        public int maxVictimsTreatable = 10;
        
        [Tooltip("Adapté pour usage pédiatrique")]
        public bool suitableForPediatric;
        
        [Tooltip("Adapté pour usage adulte")]
        public bool suitableForAdult = true;
        
        [Tooltip("Nécessite conditions de stockage spéciales")]
        public bool requiresSpecialStorage;
        
        [Tooltip("Température de stockage min/max en °C")]
        public Vector2 storageTemperatureRange = new Vector2(5, 25);
        
        // ===== PROTOCOLES ASSOCIÉS =====
        [Header("=== PROTOCOLES ASSOCIÉS ===")]
        
        [Tooltip("Protocoles d'utilisation associés au kit")]
        public List<KitProtocol> associatedProtocols = new List<KitProtocol>();
        
        // ===== CATÉGORIES DE TRIAGE =====
        [Header("=== TRIAGE ===")]
        
        [Tooltip("Applicable aux victimes ROUGES (urgence absolue)")]
        public bool applicableToRed = true;
        
        [Tooltip("Applicable aux victimes JAUNES (urgence relative)")]
        public bool applicableToYellow = true;
        
        [Tooltip("Applicable aux victimes VERTES (blessés légers)")]
        public bool applicableToGreen = true;
        
        [Tooltip("Applicable aux victimes NOIRES (décédés)")]
        public bool applicableToBlack;
        
        // ===== MAINTENANCE =====
        [Header("=== MAINTENANCE ===")]
        
        [Tooltip("Fréquence de vérification en jours")]
        [Range(1, 365)]
        public int checkFrequencyDays = 30;
        
        [Tooltip("Coût de remplacement complet en euros")]
        public float replacementCostEuros = 500f;
        
        [Tooltip("Référence fournisseur")]
        public string supplierReference;
        
        // ===== SIMULATION =====
        [Header("=== PARAMÈTRES SIMULATION ===")]
        
        [Tooltip("Temps moyen d'utilisation par victime en secondes")]
        [Range(10f, 600f)]
        public float avgUsageTimeSeconds = 60f;
        
        [Tooltip("Efficacité du traitement (0-1)")]
        [Range(0f, 1f)]
        public float treatmentEfficiency = 0.85f;
        
        [Tooltip("Peut être utilisé en conditions dégradées")]
        public bool usableInDegradedConditions;
        
        [Tooltip("Prefab 3D du kit pour la scène")]
        public GameObject kitPrefab;
        
        // ===== MÉTHODES UTILITAIRES =====
        
        /// <summary>
        /// Calcule le poids total basé sur le contenu
        /// </summary>
        public float CalculateContentWeight()
        {
            float totalWeight = 0f;
            foreach (var item in contents)
            {
                totalWeight += item.unitWeightGrams * item.quantity;
            }
            return totalWeight / 1000f; // Convertir en kg
        }
        
        /// <summary>
        /// Vérifie si le kit peut traiter une catégorie de triage donnée
        /// </summary>
        public bool CanTreatCategory(string triageCategory)
        {
            switch (triageCategory.ToUpper())
            {
                case "RED":
                case "ROUGE":
                    return applicableToRed;
                case "YELLOW":
                case "JAUNE":
                    return applicableToYellow;
                case "GREEN":
                case "VERT":
                    return applicableToGreen;
                case "BLACK":
                case "NOIR":
                    return applicableToBlack;
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Vérifie si un utilisateur a le niveau de compétence requis
        /// </summary>
        public bool HasRequiredCompetency(RequiredCompetencyLevel userLevel)
        {
            return userLevel >= requiredCompetency;
        }
        
        /// <summary>
        /// Obtient la liste des équipements consommables
        /// </summary>
        public List<MedicalKitItem> GetConsumableItems()
        {
            return contents.FindAll(item => item.isConsumable);
        }
        
        /// <summary>
        /// Obtient la liste des équipements avec date de péremption
        /// </summary>
        public List<MedicalKitItem> GetPerishableItems()
        {
            return contents.FindAll(item => item.hasExpiryDate);
        }
        
        /// <summary>
        /// Calcule le nombre de victimes restantes traitables
        /// basé sur les consommables restants
        /// </summary>
        public int EstimateRemainingCapacity(Dictionary<string, int> usedItems)
        {
            int minCapacity = maxVictimsTreatable;
            
            foreach (var item in GetConsumableItems())
            {
                if (usedItems.TryGetValue(item.itemName, out int used))
                {
                    int remaining = item.quantity - used;
                    int capacityForItem = remaining; // 1 item = 1 victime par défaut
                    minCapacity = Mathf.Min(minCapacity, capacityForItem);
                }
            }
            
            return Mathf.Max(0, minCapacity);
        }
        
        /// <summary>
        /// Génère un rapport d'inventaire du kit
        /// </summary>
        public string GenerateInventoryReport()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"=== INVENTAIRE KIT: {kitName} ===");
            sb.AppendLine($"Type: {kitType}");
            sb.AppendLine($"Poids total: {totalWeightKg} kg");
            sb.AppendLine($"Compétence requise: {requiredCompetency}");
            sb.AppendLine();
            sb.AppendLine("CONTENU:");
            
            foreach (var item in contents)
            {
                string consumable = item.isConsumable ? "[C]" : "[R]";
                string perishable = item.hasExpiryDate ? $"(péremption: {item.shelfLifeMonths} mois)" : "";
                sb.AppendLine($"  - {item.itemName} x{item.quantity} {consumable} {perishable}");
            }
            
            sb.AppendLine();
            sb.AppendLine($"Capacité max: {maxVictimsTreatable} victimes");
            sb.AppendLine($"Applicable: Rouge={applicableToRed}, Jaune={applicableToYellow}, Vert={applicableToGreen}");
            
            return sb.ToString();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Validation automatique
            if (string.IsNullOrEmpty(kitId))
            {
                kitId = $"KIT_{kitType}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
            }
            
            // Recalculer le poids si nécessaire
            if (contents.Count > 0)
            {
                float calculatedWeight = CalculateContentWeight();
                if (Mathf.Abs(calculatedWeight - totalWeightKg) > 1f)
                {
                    Debug.LogWarning($"[MedicalKitSO] Poids déclaré ({totalWeightKg}kg) différent du poids calculé ({calculatedWeight}kg) pour {kitName}");
                }
            }
        }
#endif
    }
}
