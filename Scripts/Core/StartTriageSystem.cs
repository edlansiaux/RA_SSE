using UnityEngine;
using System;

namespace RASSE.Core
{
    /// <summary>
    /// StartTriageSystem - Implémentation du protocole START (Simple Triage and Rapid Treatment)
    /// Classifie les victimes en 4 catégories: Rouge, Jaune, Vert, Noir
    /// </summary>
    public class StartTriageSystem : MonoBehaviour
    {
        [Header("=== SEUILS START ===")]
        [Tooltip("Seuil de fréquence respiratoire haute (>30 = critique)")]
        public int respiratoryRateHigh = 30;
        
        [Tooltip("Seuil de fréquence respiratoire basse (<10 = critique)")]
        public int respiratoryRateLow = 10;
        
        [Tooltip("Seuil de remplissage capillaire (>2s = critique)")]
        public float capillaryRefillThreshold = 2f;
        
        [Tooltip("Seuil de fréquence cardiaque haute")]
        public int heartRateHigh = 120;
        
        [Tooltip("Seuil de fréquence cardiaque basse")]
        public int heartRateLow = 60;

        [Header("=== TEMPS DE TRIAGE ===")]
        [Tooltip("Temps cible pour un triage (en secondes)")]
        public float targetTriageTime = 30f;

        public event Action<VictimController, StartCategory> OnTriageCompleted;
        public event Action<VictimController, StartCategory> OnTriageSuggested;

        /// <summary>
        /// Calcule la catégorie START basée sur les signes vitaux
        /// </summary>
        public StartCategory CalculateStartCategory(VitalSigns vitals)
        {
            // Étape 1: La victime respire-t-elle ?
            if (!vitals.isBreathing)
            {
                // Ouvrir les voies aériennes
                if (!vitals.breathingAfterAirwayManeuver)
                {
                    // Ne respire toujours pas = NOIR (décédé)
                    return StartCategory.Black;
                }
                else
                {
                    // Respire après manœuvre = ROUGE (immédiat)
                    return StartCategory.Red;
                }
            }

            // Étape 2: Vérifier la fréquence respiratoire
            if (vitals.respiratoryRate > respiratoryRateHigh || vitals.respiratoryRate < respiratoryRateLow)
            {
                return StartCategory.Red;
            }

            // Étape 3: Vérifier la perfusion (remplissage capillaire ou pouls radial)
            if (vitals.capillaryRefillTime > capillaryRefillThreshold || !vitals.hasRadialPulse)
            {
                return StartCategory.Red;
            }

            // Étape 4: Vérifier l'état mental (peut-il suivre des ordres simples ?)
            if (!vitals.canFollowCommands)
            {
                return StartCategory.Red;
            }

            // Étape 5: La victime peut-elle marcher ?
            if (vitals.canWalk)
            {
                return StartCategory.Green;
            }

            // Si peut suivre des ordres mais ne peut pas marcher = JAUNE
            return StartCategory.Yellow;
        }

        /// <summary>
        /// Effectue le triage automatique d'une victime
        /// </summary>
        public void PerformAutomaticTriage(VictimController victim)
        {
            if (victim == null) return;

            StartCategory suggestedCategory = CalculateStartCategory(victim.VitalSigns);
            
            // Notifier la suggestion
            OnTriageSuggested?.Invoke(victim, suggestedCategory);
            
            Debug.Log($"[StartTriage] Suggestion pour {victim.PatientId}: {suggestedCategory}");
            Debug.Log($"  - Respiration: {victim.VitalSigns.respiratoryRate}/min");
            Debug.Log($"  - Remplissage capillaire: {victim.VitalSigns.capillaryRefillTime}s");
            Debug.Log($"  - Suit les ordres: {victim.VitalSigns.canFollowCommands}");
            Debug.Log($"  - Peut marcher: {victim.VitalSigns.canWalk}");
        }

        /// <summary>
        /// Valide le triage (par commande vocale ou bouton)
        /// </summary>
        public void ValidateTriage(VictimController victim, StartCategory category, bool isManualOverride = false)
        {
            if (victim == null) return;

            victim.TriageCategory = category;
            victim.IsTriaged = true;
            victim.TriageTime = Time.time;
            victim.WasManuallyOverridden = isManualOverride;

            // Enregistrer dans le GameManager
            GameManager.Instance?.RegisterVictimTriage(victim, category);

            OnTriageCompleted?.Invoke(victim, category);

            Debug.Log($"[StartTriage] Triage validé pour {victim.PatientId}: {category}" + 
                     (isManualOverride ? " (manuel)" : " (automatique)"));
        }

        /// <summary>
        /// Obtient la couleur associée à une catégorie START
        /// </summary>
        public static Color GetCategoryColor(StartCategory category)
        {
            switch (category)
            {
                case StartCategory.Red:
                    return new Color(0.9f, 0.1f, 0.1f); // Rouge vif
                case StartCategory.Yellow:
                    return new Color(1f, 0.9f, 0f);     // Jaune vif
                case StartCategory.Green:
                    return new Color(0.1f, 0.8f, 0.1f); // Vert
                case StartCategory.Black:
                    return new Color(0.1f, 0.1f, 0.1f); // Noir
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// Obtient la description de la catégorie
        /// </summary>
        public static string GetCategoryDescription(StartCategory category)
        {
            switch (category)
            {
                case StartCategory.Red:
                    return "IMMÉDIAT - Urgence vitale, évacuation prioritaire";
                case StartCategory.Yellow:
                    return "DIFFÉRÉ - Blessé sérieux, peut attendre";
                case StartCategory.Green:
                    return "MINEUR - Blessé léger, ambulatoire";
                case StartCategory.Black:
                    return "DÉCÉDÉ - Aucune intervention";
                default:
                    return "NON CLASSIFIÉ";
            }
        }

        /// <summary>
        /// Obtient l'action recommandée pour la catégorie
        /// </summary>
        public static string GetRecommendedAction(StartCategory category)
        {
            switch (category)
            {
                case StartCategory.Red:
                    return "Évacuation immédiate vers hôpital avec service de réanimation";
                case StartCategory.Yellow:
                    return "Évacuation vers hôpital après les cas rouges";
                case StartCategory.Green:
                    return "Regroupement au PRV, évacuation différée ou retour domicile";
                case StartCategory.Black:
                    return "Laisser sur place, signaler aux autorités";
                default:
                    return "Effectuer le triage";
            }
        }

        /// <summary>
        /// Obtient la priorité d'évacuation (1 = plus urgent)
        /// </summary>
        public static int GetEvacuationPriority(StartCategory category)
        {
            switch (category)
            {
                case StartCategory.Red:
                    return 1;
                case StartCategory.Yellow:
                    return 2;
                case StartCategory.Green:
                    return 3;
                case StartCategory.Black:
                    return 4;
                default:
                    return 5;
            }
        }
    }

    /// <summary>
    /// Catégories START
    /// </summary>
    public enum StartCategory
    {
        None,       // Non classifié
        Red,        // Immédiat (urgence absolue)
        Yellow,     // Différé (urgence relative)
        Green,      // Mineur (impliqué)
        Black       // Décédé
    }

    /// <summary>
    /// Structure des signes vitaux pour le triage
    /// </summary>
    [System.Serializable]
    public class VitalSigns
    {
        [Header("=== RESPIRATION ===")]
        public bool isBreathing = true;
        public bool breathingAfterAirwayManeuver = false;
        
        [Range(0, 60)]
        public int respiratoryRate = 16;

        [Header("=== CIRCULATION ===")]
        [Range(0, 200)]
        public int heartRate = 80;
        
        [Range(0f, 10f)]
        public float capillaryRefillTime = 2f;
        
        public bool hasRadialPulse = true;
        
        [Range(0, 100)]
        public int oxygenSaturation = 98;
        
        [Range(60, 200)]
        public int systolicBloodPressure = 120;
        
        [Range(40, 120)]
        public int diastolicBloodPressure = 80;

        [Header("=== NEUROLOGIQUE ===")]
        public bool canFollowCommands = true;
        public bool canWalk = true;
        public bool isConscious = true;
        
        [Range(3, 15)]
        public int glasgowComaScale = 15;

        [Header("=== TEMPÉRATURE ===")]
        [Range(30f, 42f)]
        public float bodyTemperature = 37f;

        [Header("=== AUTRES ===")]
        public bool hasVisibleBleeding = false;
        public bool hasFracture = false;
        public bool hasBurns = false;
        public float painLevel = 0f; // 0-10
    }
}
