// ============================================================================
// RASSE - Réalité Augmentée en Situations Sanitaires Exceptionnelles
// TriageProtocolSO.cs - ScriptableObject pour protocoles de triage
// Conforme au rapport académique - REQ-3, REQ-5: Classification et consignes
// ============================================================================

using UnityEngine;
using System;
using System.Collections.Generic;

namespace RASSE.Data
{
    /// <summary>
    /// Étape d'évaluation dans le protocole START
    /// </summary>
    [Serializable]
    public class TriageStep
    {
        public int stepNumber;
        public string stepName;
        [TextArea(2, 4)]
        public string instruction;
        [TextArea(2, 3)]
        public string evaluationCriteria;
        public string yesOutcome;           // Si critère rempli
        public string noOutcome;            // Si critère non rempli
        public float recommendedDuration;   // Secondes recommandées
        public AudioClip voiceInstruction;
        public Sprite stepIllustration;
    }

    /// <summary>
    /// Action de premiers secours
    /// </summary>
    [Serializable]
    public class FirstAidAction
    {
        public string actionId;
        public string actionName;
        [TextArea(2, 4)]
        public string description;
        public string[] requiredEquipment;
        public float estimatedDuration;     // Secondes
        public int priorityOrder;
        public bool isCritical;
        public AudioClip voiceGuide;
        public Sprite actionIllustration;
    }

    /// <summary>
    /// Contre-indication (à ne pas faire)
    /// </summary>
    [Serializable]
    public class Contraindication
    {
        public string description;
        public string reason;
        public string category;             // Catégorie START concernée
    }

    /// <summary>
    /// Protocole complet pour une catégorie START
    /// </summary>
    [Serializable]
    public class CategoryProtocol
    {
        public string categoryCode;         // RED, YELLOW, GREEN, BLACK
        public string categoryName;         // Nom complet
        public Color categoryColor;
        
        [Header("Définition")]
        [TextArea(3, 5)]
        public string definition;
        public string[] criteria;
        
        [Header("Actions Immédiates (REQ-5)")]
        public List<FirstAidAction> immediateActions = new List<FirstAidAction>();
        
        [Header("Consignes Évacuation")]
        public string evacuationPriority;
        public int maxEvacuationTimeMinutes;
        public string transportType;        // SMUR, VSAV, etc.
        public string destinationType;      // CHU, Trauma Center, etc.
        
        [Header("Contre-indications")]
        public List<Contraindication> contraindications = new List<Contraindication>();
        
        [Header("Communication")]
        public string[] alertMessages;
        public string radioCode;            // Ex: "DELTA" pour ROUGE
    }

    /// <summary>
    /// ScriptableObject définissant le protocole START complet
    /// Conforme aux exigences REQ-3 (classification) et REQ-5 (consignes)
    /// </summary>
    [CreateAssetMenu(fileName = "TriageProtocol_START", menuName = "RASSE/Triage Protocol", order = 4)]
    public class TriageProtocolSO : ScriptableObject
    {
        [Header("=== IDENTIFICATION PROTOCOLE ===")]
        public string protocolId = "START";
        public string protocolName = "Simple Triage And Rapid Treatment";
        public string version = "1.0";
        public string lastUpdate = "2024";
        [TextArea(3, 5)]
        public string description;
        public string sourceReference;      // Référence officielle

        [Header("=== ÉTAPES D'ÉVALUATION (REQ-3) ===")]
        public List<TriageStep> evaluationSteps = new List<TriageStep>();

        [Header("=== PROTOCOLES PAR CATÉGORIE ===")]
        public CategoryProtocol redProtocol = new CategoryProtocol();
        public CategoryProtocol yellowProtocol = new CategoryProtocol();
        public CategoryProtocol greenProtocol = new CategoryProtocol();
        public CategoryProtocol blackProtocol = new CategoryProtocol();

        [Header("=== SEUILS CRITIQUES ===")]
        [Tooltip("FR > ce seuil = ROUGE")]
        public int respiratoryRateThreshold = 30;
        [Tooltip("TRC > ce seuil (sec) = ROUGE")]
        public float capillaryRefillThreshold = 2f;
        [Tooltip("Glasgow < ce seuil = ROUGE")]
        public int glasgowThreshold = 14;
        [Tooltip("PAS < ce seuil = ROUGE")]
        public int systolicBPThreshold = 90;

        [Header("=== PARAMÈTRES TEMPORELS ===")]
        [Tooltip("Temps max par victime (sec)")]
        public float maxTimePerVictim = 60f;
        [Tooltip("Intervalle réévaluation (min)")]
        public float reevaluationIntervalMinutes = 15f;

        [Header("=== RESSOURCES AUDIO ===")]
        public AudioClip protocolIntroduction;
        public AudioClip[] stepInstructions;

        /// <summary>
        /// Évalue une victime selon le protocole START
        /// </summary>
        public string EvaluateVictim(
            bool canWalk,
            int respiratoryRate,
            bool breathingAfterAirway,
            float capillaryRefillTime,
            bool respondsToCommands)
        {
            // Étape 1: Peut marcher ?
            if (canWalk)
                return "GREEN";

            // Étape 2: Respire ?
            if (respiratoryRate == 0)
            {
                // Libérer voies aériennes
                if (breathingAfterAirway)
                    return "RED";
                else
                    return "BLACK";
            }

            // Étape 3: FR > 30 ?
            if (respiratoryRate > respiratoryRateThreshold)
                return "RED";

            // Étape 4: Perfusion (TRC)
            if (capillaryRefillTime > capillaryRefillThreshold)
                return "RED";

            // Étape 5: Répond aux ordres simples ?
            if (!respondsToCommands)
                return "RED";

            // Sinon
            return "YELLOW";
        }

        /// <summary>
        /// Obtient le protocole pour une catégorie
        /// </summary>
        public CategoryProtocol GetProtocolForCategory(string category)
        {
            switch (category.ToUpper())
            {
                case "RED": return redProtocol;
                case "YELLOW": return yellowProtocol;
                case "GREEN": return greenProtocol;
                case "BLACK": return blackProtocol;
                default: return null;
            }
        }

        /// <summary>
        /// Obtient la couleur d'une catégorie
        /// </summary>
        public Color GetCategoryColor(string category)
        {
            var protocol = GetProtocolForCategory(category);
            return protocol?.categoryColor ?? Color.gray;
        }

        /// <summary>
        /// Obtient les actions immédiates pour une catégorie
        /// </summary>
        public List<FirstAidAction> GetImmediateActions(string category)
        {
            var protocol = GetProtocolForCategory(category);
            return protocol?.immediateActions ?? new List<FirstAidAction>();
        }

        /// <summary>
        /// Génère le texte de guidage complet pour une catégorie
        /// </summary>
        public string GenerateGuidanceText(string category)
        {
            var protocol = GetProtocolForCategory(category);
            if (protocol == null) return "Catégorie inconnue";

            string text = $"=== {protocol.categoryName} ({protocol.categoryCode}) ===\n\n";
            text += $"{protocol.definition}\n\n";

            text += "ACTIONS IMMÉDIATES:\n";
            foreach (var action in protocol.immediateActions)
            {
                text += $"  {action.priorityOrder}. {action.actionName}";
                if (action.isCritical) text += " [CRITIQUE]";
                text += $"\n     {action.description}\n";
            }

            text += $"\nÉVACUATION:\n";
            text += $"  Priorité: {protocol.evacuationPriority}\n";
            text += $"  Délai max: {protocol.maxEvacuationTimeMinutes} min\n";
            text += $"  Transport: {protocol.transportType}\n";
            text += $"  Destination: {protocol.destinationType}\n";

            if (protocol.contraindications.Count > 0)
            {
                text += "\nÀ NE PAS FAIRE:\n";
                foreach (var ci in protocol.contraindications)
                {
                    text += $"  ⚠ {ci.description}\n";
                }
            }

            return text;
        }

        /// <summary>
        /// Valide le triage effectué par l'utilisateur
        /// </summary>
        public TriageValidationResult ValidateTriage(
            string assignedCategory,
            bool victimCanWalk,
            int respiratoryRate,
            float capillaryRefillTime,
            bool respondsToCommands)
        {
            string correctCategory = EvaluateVictim(
                victimCanWalk,
                respiratoryRate,
                true, // Assume airways cleared
                capillaryRefillTime,
                respondsToCommands
            );

            var result = new TriageValidationResult
            {
                assignedCategory = assignedCategory,
                correctCategory = correctCategory,
                isCorrect = assignedCategory == correctCategory
            };

            if (!result.isCorrect)
            {
                // Déterminer le type d'erreur
                int assignedPriority = GetCategoryPriority(assignedCategory);
                int correctPriority = GetCategoryPriority(correctCategory);

                if (assignedPriority > correctPriority)
                {
                    result.errorType = TriageErrorType.UnderTriage;
                    result.feedback = $"Sous-triage: Cette victime devrait être {correctCategory}, " +
                                    $"pas {assignedCategory}. Risque de retard de prise en charge.";
                }
                else
                {
                    result.errorType = TriageErrorType.OverTriage;
                    result.feedback = $"Sur-triage: Cette victime devrait être {correctCategory}, " +
                                    $"pas {assignedCategory}. Gaspillage potentiel de ressources.";
                }
            }
            else
            {
                result.errorType = TriageErrorType.None;
                result.feedback = "Triage correct. Bien joué!";
            }

            return result;
        }

        private int GetCategoryPriority(string category)
        {
            switch (category.ToUpper())
            {
                case "RED": return 1;
                case "YELLOW": return 2;
                case "GREEN": return 3;
                case "BLACK": return 4;
                default: return 5;
            }
        }

        private void OnValidate()
        {
            // Initialiser les protocoles par défaut si vides
            if (string.IsNullOrEmpty(redProtocol.categoryCode))
                InitializeDefaultProtocols();
        }

        private void InitializeDefaultProtocols()
        {
            // ROUGE
            redProtocol = new CategoryProtocol
            {
                categoryCode = "RED",
                categoryName = "Urgence Absolue",
                categoryColor = Color.red,
                definition = "Victime dont le pronostic vital est engagé mais qui peut être sauvée avec une prise en charge immédiate.",
                criteria = new[] { "FR > 30/min", "TRC > 2s", "Ne répond pas aux ordres", "Respire après LVA" },
                evacuationPriority = "IMMÉDIATE",
                maxEvacuationTimeMinutes = 10,
                transportType = "SMUR / Hélicoptère",
                destinationType = "Trauma Center / CHU",
                radioCode = "DELTA"
            };

            // JAUNE
            yellowProtocol = new CategoryProtocol
            {
                categoryCode = "YELLOW",
                categoryName = "Urgence Relative",
                categoryColor = Color.yellow,
                definition = "Victime dont l'état est sérieux mais stable, nécessitant une prise en charge médicale différée.",
                criteria = new[] { "Ne peut pas marcher", "FR ≤ 30/min", "TRC ≤ 2s", "Répond aux ordres" },
                evacuationPriority = "DIFFÉRÉE",
                maxEvacuationTimeMinutes = 30,
                transportType = "VSAV / Ambulance",
                destinationType = "Centre Hospitalier",
                radioCode = "BRAVO"
            };

            // VERT
            greenProtocol = new CategoryProtocol
            {
                categoryCode = "GREEN",
                categoryName = "Urgence Dépassée (Blessé Léger)",
                categoryColor = Color.green,
                definition = "Victime valide pouvant marcher, blessures légères ne mettant pas en jeu le pronostic vital.",
                criteria = new[] { "Peut marcher sans aide" },
                evacuationPriority = "BASSE",
                maxEvacuationTimeMinutes = 120,
                transportType = "Transport collectif / Marche",
                destinationType = "PRV / Centre de soins",
                radioCode = "ALPHA"
            };

            // NOIR
            blackProtocol = new CategoryProtocol
            {
                categoryCode = "BLACK",
                categoryName = "Décédé / Dépassé",
                categoryColor = Color.black,
                definition = "Victime décédée ou dont le pronostic est dépassé compte tenu des ressources disponibles.",
                criteria = new[] { "Absence de respiration après LVA", "Blessures incompatibles avec la vie" },
                evacuationPriority = "AUCUNE",
                maxEvacuationTimeMinutes = 0,
                transportType = "Aucun",
                destinationType = "Zone de regroupement des corps",
                radioCode = "OMEGA"
            };
        }
    }

    /// <summary>
    /// Résultat de validation d'un triage
    /// </summary>
    [Serializable]
    public class TriageValidationResult
    {
        public string assignedCategory;
        public string correctCategory;
        public bool isCorrect;
        public TriageErrorType errorType;
        public string feedback;
    }

    public enum TriageErrorType
    {
        None,
        UnderTriage,    // Sous-estimation de la gravité
        OverTriage      // Sur-estimation de la gravité
    }
}
