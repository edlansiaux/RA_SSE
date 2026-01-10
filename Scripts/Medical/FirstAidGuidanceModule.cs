using UnityEngine;
using System.Collections.Generic;

namespace RASSE.Medical
{
    /// <summary>
    /// FirstAidGuidanceModule - Module d'affichage des consignes de premiers secours (REQ-5)
    /// Fournit des instructions adaptées selon la catégorie START et le type de blessure
    /// </summary>
    public class FirstAidGuidanceModule : MonoBehaviour
    {
        public static FirstAidGuidanceModule Instance { get; private set; }

        [Header("=== PROTOCOLES ===")]
        [SerializeField] private List<FirstAidProtocol> protocols = new List<FirstAidProtocol>();

        [Header("=== CONSIGNES ÉVACUATION ===")]
        [SerializeField] private List<EvacuationGuideline> evacuationGuidelines = new List<EvacuationGuideline>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeProtocols();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Initialise les protocoles de premiers secours
        /// </summary>
        private void InitializeProtocols()
        {
            // === PROTOCOLES PAR CATÉGORIE START ===

            // ROUGE - Urgence Absolue
            protocols.Add(new FirstAidProtocol
            {
                category = Core.StartCategory.Red,
                protocolName = "Prise en charge ROUGE - Urgence Absolue",
                immediateActions = new List<string>
                {
                    "1. SÉCURISER la zone et la victime",
                    "2. LIBÉRER les voies aériennes (LVA)",
                    "3. CONTRÔLER les hémorragies (compression directe/garrot)",
                    "4. POSITION LATÉRALE DE SÉCURITÉ si inconscient",
                    "5. OXYGÈNE si disponible (15L/min au masque)",
                    "6. SURVEILLER constantes en continu"
                },
                doNotActions = new List<string>
                {
                    "NE PAS déplacer si suspicion trauma rachidien",
                    "NE PAS retirer d'objets pénétrants",
                    "NE PAS donner à boire",
                    "NE PAS laisser seul"
                },
                evacuationPriority = "IMMÉDIATE - Ambulance SMUR prioritaire",
                estimatedTimeOnScene = "< 10 min",
                hospitalRequirements = "Centre de trauma / Réanimation"
            });

            // JAUNE - Urgence Différée
            protocols.Add(new FirstAidProtocol
            {
                category = Core.StartCategory.Yellow,
                protocolName = "Prise en charge JAUNE - Urgence Différée",
                immediateActions = new List<string>
                {
                    "1. ÉVALUER la conscience et la respiration",
                    "2. IMMOBILISER les fractures suspectées",
                    "3. COUVRIR les plaies (pansements stériles)",
                    "4. POSITION confortable (semi-assise si dyspnée)",
                    "5. RÉCHAUFFER (couverture de survie)",
                    "6. RASSURER et surveiller"
                },
                doNotActions = new List<string>
                {
                    "NE PAS mobiliser le rachis si doute",
                    "NE PAS négliger la surveillance",
                    "NE PAS retarder l'évacuation si aggravation"
                },
                evacuationPriority = "DIFFÉRÉE - Après les cas ROUGES",
                estimatedTimeOnScene = "15-30 min",
                hospitalRequirements = "Service d'urgence / Traumatologie"
            });

            // VERT - Urgence Mineure
            protocols.Add(new FirstAidProtocol
            {
                category = Core.StartCategory.Green,
                protocolName = "Prise en charge VERT - Blessé Léger",
                immediateActions = new List<string>
                {
                    "1. RASSURER le patient",
                    "2. ÉVALUER les blessures visibles",
                    "3. NETTOYER et PROTÉGER les plaies mineures",
                    "4. DIRIGER vers le PRV (Point de Regroupement des Victimes)",
                    "5. FOURNIR couverture si nécessaire",
                    "6. INFORMER des procédures d'évacuation"
                },
                doNotActions = new List<string>
                {
                    "NE PAS négliger - peut évoluer en JAUNE",
                    "NE PAS laisser partir sans évaluation médicale"
                },
                evacuationPriority = "BASSE - PRV puis évacuation collective",
                estimatedTimeOnScene = "Variable",
                hospitalRequirements = "Consultation urgences ou retour domicile"
            });

            // NOIR - Décédé
            protocols.Add(new FirstAidProtocol
            {
                category = Core.StartCategory.Black,
                protocolName = "Constat NOIR - Décès",
                immediateActions = new List<string>
                {
                    "1. CONFIRMER l'absence de signes vitaux",
                    "2. NE PAS DÉBUTER de RCP (ressources limitées)",
                    "3. COUVRIR le corps dignement",
                    "4. BALISER l'emplacement",
                    "5. NOTER l'heure et les circonstances",
                    "6. POURSUIVRE vers les autres victimes"
                },
                doNotActions = new List<string>
                {
                    "NE PAS mobiliser le corps",
                    "NE PAS retirer les effets personnels",
                    "NE PAS communiquer l'identité avant confirmation"
                },
                evacuationPriority = "AUCUNE - Laisser sur place",
                estimatedTimeOnScene = "< 2 min",
                hospitalRequirements = "Morgue / Médecine légale"
            });

            // === PROTOCOLES PAR TYPE DE BLESSURE ===

            protocols.Add(new FirstAidProtocol
            {
                injuryType = Core.InjuryType.Hemorrhage,
                protocolName = "Hémorragie",
                immediateActions = new List<string>
                {
                    "1. COMPRESSION DIRECTE sur la plaie (10 min)",
                    "2. SURÉLEVER le membre si possible",
                    "3. PANSEMENT COMPRESSIF si inefficace",
                    "4. GARROT en dernier recours (membre + proximale)",
                    "5. NOTER l'heure de pose du garrot",
                    "6. NE JAMAIS desserrer le garrot"
                },
                equipmentNeeded = new List<string> { "Compresses stériles", "Bandes", "Garrot tactique" }
            });

            protocols.Add(new FirstAidProtocol
            {
                injuryType = Core.InjuryType.Burn,
                protocolName = "Brûlure",
                immediateActions = new List<string>
                {
                    "1. STOPPER le processus de brûlure",
                    "2. REFROIDIR à l'eau tiède (15-20°C) 10-20 min",
                    "3. RETIRER vêtements/bijoux non adhérents",
                    "4. COUVRIR de compresses humides stériles",
                    "5. ÉVALUER la surface (règle des 9)",
                    "6. PRÉVENIR l'hypothermie"
                },
                doNotActions = new List<string>
                {
                    "NE PAS appliquer de glace",
                    "NE PAS percer les phlyctènes",
                    "NE PAS appliquer de corps gras"
                },
                equipmentNeeded = new List<string> { "Eau propre", "Compresses stériles", "Couverture isotherme" }
            });

            protocols.Add(new FirstAidProtocol
            {
                injuryType = Core.InjuryType.Fracture,
                protocolName = "Fracture",
                immediateActions = new List<string>
                {
                    "1. IMMOBILISER le membre dans la position trouvée",
                    "2. NE PAS tenter de réduire",
                    "3. VÉRIFIER la sensibilité et le pouls distal",
                    "4. APPLIQUER une attelle si disponible",
                    "5. SURÉLEVER légèrement si possible",
                    "6. APPLIQUER froid (10 min max)"
                },
                equipmentNeeded = new List<string> { "Attelles", "Bandes", "Écharpe" }
            });

            protocols.Add(new FirstAidProtocol
            {
                injuryType = Core.InjuryType.Concussion,
                protocolName = "Traumatisme Crânien",
                immediateActions = new List<string>
                {
                    "1. MAINTENIR l'axe tête-cou-tronc",
                    "2. ÉVALUER le score de Glasgow",
                    "3. SURVEILLER les pupilles",
                    "4. POSITION à 30° si conscient",
                    "5. PLS si inconscient (avec maintien cervical)",
                    "6. SURVEILLANCE rapprochée (conscience, pupilles)"
                },
                warningSignsToWatch = new List<string>
                {
                    "Vomissements répétés",
                    "Céphalées croissantes",
                    "Confusion progressive",
                    "Anisocorie (pupilles inégales)",
                    "Déficit neurologique"
                }
            });

            protocols.Add(new FirstAidProtocol
            {
                injuryType = Core.InjuryType.RespiratoryDistress,
                protocolName = "Détresse Respiratoire",
                immediateActions = new List<string>
                {
                    "1. POSITION ASSISE ou semi-assise",
                    "2. LIBÉRER les voies aériennes",
                    "3. DESSERRER vêtements serrés",
                    "4. OXYGÈNE si disponible (6-15 L/min)",
                    "5. RASSURER et encourager respiration lente",
                    "6. PRÉPARER matériel d'aspiration"
                },
                criticalSigns = new List<string>
                {
                    "FR > 30 ou < 10/min",
                    "SpO2 < 90%",
                    "Cyanose",
                    "Tirage intercostal",
                    "Balancement thoraco-abdominal"
                }
            });

            // === CONSIGNES D'ÉVACUATION ===

            evacuationGuidelines.Add(new EvacuationGuideline
            {
                category = Core.StartCategory.Red,
                vehicleType = Core.AmbulanceType.SMUR,
                maxWaitTime = 10f,
                instructions = new List<string>
                {
                    "Évacuation IMMÉDIATE par SMUR",
                    "Destination: Centre de trauma le plus proche",
                    "Préavis hôpital OBLIGATOIRE",
                    "Accompagnement médical requis"
                }
            });

            evacuationGuidelines.Add(new EvacuationGuideline
            {
                category = Core.StartCategory.Yellow,
                vehicleType = Core.AmbulanceType.Advanced,
                maxWaitTime = 30f,
                instructions = new List<string>
                {
                    "Évacuation après les cas ROUGES",
                    "Ambulance médicalisée préférable",
                    "Réévaluation régulière (upgrade possible)",
                    "Destination selon pathologie"
                }
            });

            evacuationGuidelines.Add(new EvacuationGuideline
            {
                category = Core.StartCategory.Green,
                vehicleType = Core.AmbulanceType.Basic,
                maxWaitTime = 120f,
                instructions = new List<string>
                {
                    "Regroupement au PRV",
                    "Évacuation collective possible",
                    "Transport non urgent",
                    "Possibilité retour domicile après évaluation"
                }
            });

            Debug.Log($"[FirstAidGuidance] {protocols.Count} protocoles initialisés");
        }

        /// <summary>
        /// Obtient les consignes pour une catégorie START
        /// </summary>
        public FirstAidProtocol GetProtocolForCategory(Core.StartCategory category)
        {
            return protocols.Find(p => p.category == category && p.injuryType == Core.InjuryType.None);
        }

        /// <summary>
        /// Obtient les consignes pour un type de blessure
        /// </summary>
        public FirstAidProtocol GetProtocolForInjury(Core.InjuryType injuryType)
        {
            return protocols.Find(p => p.injuryType == injuryType);
        }

        /// <summary>
        /// Obtient les consignes d'évacuation
        /// </summary>
        public EvacuationGuideline GetEvacuationGuideline(Core.StartCategory category)
        {
            return evacuationGuidelines.Find(g => g.category == category);
        }

        /// <summary>
        /// Génère le texte complet des consignes pour affichage AR
        /// </summary>
        public string GetFullGuidanceText(Core.VictimController victim)
        {
            if (victim == null) return "";

            var categoryProtocol = GetProtocolForCategory(victim.TriageCategory);
            var injuryProtocol = GetProtocolForInjury(victim.primaryInjury);
            var evacGuideline = GetEvacuationGuideline(victim.TriageCategory);

            string text = $"═══ CONSIGNES {victim.TriageCategory.GetFrenchName()} ═══\n\n";

            if (categoryProtocol != null)
            {
                text += "▶ ACTIONS IMMÉDIATES:\n";
                foreach (var action in categoryProtocol.immediateActions)
                {
                    text += $"  {action}\n";
                }

                if (categoryProtocol.doNotActions.Count > 0)
                {
                    text += "\n⚠ À NE PAS FAIRE:\n";
                    foreach (var action in categoryProtocol.doNotActions)
                    {
                        text += $"  • {action}\n";
                    }
                }
            }

            if (injuryProtocol != null && victim.primaryInjury != Core.InjuryType.None)
            {
                text += $"\n═══ SPÉCIFIQUE: {injuryProtocol.protocolName.ToUpper()} ═══\n";
                foreach (var action in injuryProtocol.immediateActions)
                {
                    text += $"  {action}\n";
                }
            }

            if (evacGuideline != null)
            {
                text += "\n═══ ÉVACUATION ═══\n";
                text += $"Priorité: {categoryProtocol?.evacuationPriority}\n";
                text += $"Véhicule: {evacGuideline.vehicleType}\n";
                text += $"Délai max: {evacGuideline.maxWaitTime} min\n";
            }

            return text;
        }

        /// <summary>
        /// Obtient les consignes courtes pour affichage compact
        /// </summary>
        public List<string> GetShortGuidance(Core.StartCategory category)
        {
            var protocol = GetProtocolForCategory(category);
            if (protocol == null) return new List<string>();

            // Retourner les 3 premières actions
            return protocol.immediateActions.GetRange(0, Mathf.Min(3, protocol.immediateActions.Count));
        }
    }

    #region Data Structures

    /// <summary>
    /// Protocole de premiers secours
    /// </summary>
    [System.Serializable]
    public class FirstAidProtocol
    {
        public Core.StartCategory category = Core.StartCategory.None;
        public Core.InjuryType injuryType = Core.InjuryType.None;
        public string protocolName;
        public List<string> immediateActions = new List<string>();
        public List<string> doNotActions = new List<string>();
        public List<string> equipmentNeeded = new List<string>();
        public List<string> warningSignsToWatch = new List<string>();
        public List<string> criticalSigns = new List<string>();
        public string evacuationPriority;
        public string estimatedTimeOnScene;
        public string hospitalRequirements;
    }

    /// <summary>
    /// Consignes d'évacuation
    /// </summary>
    [System.Serializable]
    public class EvacuationGuideline
    {
        public Core.StartCategory category;
        public Core.AmbulanceType vehicleType;
        public float maxWaitTime; // en minutes
        public List<string> instructions = new List<string>();
    }

    #endregion
}
