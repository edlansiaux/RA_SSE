using UnityEngine;

namespace RASSE.Core
{
    /// <summary>
    /// Interface pour tous les objets interactifs dans le simulateur.
    /// Permet l'interaction mains-libres via commandes vocales ou regard.
    /// Conforme aux exigences NFR-UX (interaction mains-libres).
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Identifiant unique de l'objet interactif
        /// </summary>
        string InteractableId { get; }
        
        /// <summary>
        /// Nom affiché pour l'interaction
        /// </summary>
        string DisplayName { get; }
        
        /// <summary>
        /// Indique si l'interaction est actuellement possible
        /// </summary>
        bool CanInteract { get; }
        
        /// <summary>
        /// Distance maximale d'interaction en mètres
        /// </summary>
        float InteractionRange { get; }
        
        /// <summary>
        /// Priorité d'interaction (plus élevé = prioritaire)
        /// </summary>
        int InteractionPriority { get; }
        
        /// <summary>
        /// Actions disponibles pour cet objet
        /// </summary>
        InteractionAction[] AvailableActions { get; }
        
        /// <summary>
        /// Appelé quand le joueur commence à regarder l'objet
        /// </summary>
        void OnGazeEnter();
        
        /// <summary>
        /// Appelé quand le joueur arrête de regarder l'objet
        /// </summary>
        void OnGazeExit();
        
        /// <summary>
        /// Appelé quand le joueur interagit avec l'objet
        /// </summary>
        /// <param name="action">L'action à effectuer</param>
        /// <returns>True si l'interaction a réussi</returns>
        bool OnInteract(InteractionAction action);
        
        /// <summary>
        /// Obtient l'indication visuelle à afficher
        /// </summary>
        InteractionHint GetInteractionHint();
    }
    
    /// <summary>
    /// Types d'actions d'interaction
    /// </summary>
    public enum InteractionActionType
    {
        Examine,        // Examiner/Observer
        PickUp,         // Ramasser
        Use,            // Utiliser
        Talk,           // Parler (pour les victimes conscientes)
        Triage,         // Effectuer le triage
        Treat,          // Traiter/Soigner
        Move,           // Déplacer
        Tag,            // Poser une étiquette
        Call,           // Appeler (ambulance, renfort)
        Custom          // Action personnalisée
    }
    
    /// <summary>
    /// Action d'interaction
    /// </summary>
    [System.Serializable]
    public class InteractionAction
    {
        public string ActionId;
        public string ActionName;
        public string ActionNameFR;
        public InteractionActionType Type;
        public string VoiceCommand;
        public KeyCode ShortcutKey;
        public float Duration;
        public bool RequiresConfirmation;
        public string IconPath;
        
        public InteractionAction(InteractionActionType type, string name, string nameFR, string voiceCmd = null)
        {
            ActionId = System.Guid.NewGuid().ToString().Substring(0, 8);
            Type = type;
            ActionName = name;
            ActionNameFR = nameFR;
            VoiceCommand = voiceCmd ?? nameFR.ToUpper();
            Duration = 0f;
            RequiresConfirmation = false;
        }
    }
    
    /// <summary>
    /// Indication d'interaction pour l'UI
    /// </summary>
    [System.Serializable]
    public class InteractionHint
    {
        public string PrimaryText;
        public string SecondaryText;
        public Color HintColor;
        public bool ShowProgress;
        public float Progress;
        public Vector3 WorldPosition;
        
        public InteractionHint(string primary, string secondary = "", Color? color = null)
        {
            PrimaryText = primary;
            SecondaryText = secondary;
            HintColor = color ?? Color.white;
            ShowProgress = false;
            Progress = 0f;
        }
    }
    
    /// <summary>
    /// Classe de base pour les objets interactifs
    /// </summary>
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] protected string interactableId;
        [SerializeField] protected string displayName;
        [SerializeField] protected float interactionRange = 3f;
        [SerializeField] protected int interactionPriority = 0;
        [SerializeField] protected bool isInteractable = true;
        
        [Header("Visual Feedback")]
        [SerializeField] protected GameObject highlightEffect;
        [SerializeField] protected Color highlightColor = Color.yellow;
        
        protected bool isBeingLookedAt = false;
        protected InteractionAction[] actions;
        
        public virtual string InteractableId => interactableId;
        public virtual string DisplayName => displayName;
        public virtual bool CanInteract => isInteractable;
        public virtual float InteractionRange => interactionRange;
        public virtual int InteractionPriority => interactionPriority;
        public virtual InteractionAction[] AvailableActions => actions;
        
        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(interactableId))
            {
                interactableId = $"{GetType().Name}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
            }
            
            InitializeActions();
        }
        
        protected abstract void InitializeActions();
        
        public virtual void OnGazeEnter()
        {
            isBeingLookedAt = true;
            if (highlightEffect != null)
            {
                highlightEffect.SetActive(true);
            }
            OnHighlight(true);
        }
        
        public virtual void OnGazeExit()
        {
            isBeingLookedAt = false;
            if (highlightEffect != null)
            {
                highlightEffect.SetActive(false);
            }
            OnHighlight(false);
        }
        
        protected virtual void OnHighlight(bool highlighted)
        {
            // Surcharge pour effet de highlight personnalisé
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                // Appliquer un effet visuel
            }
        }
        
        public abstract bool OnInteract(InteractionAction action);
        
        public virtual InteractionHint GetInteractionHint()
        {
            if (!CanInteract)
            {
                return new InteractionHint("Non disponible", "", Color.gray);
            }
            
            string actionText = actions != null && actions.Length > 0 
                ? actions[0].ActionNameFR 
                : "Interagir";
                
            return new InteractionHint(displayName, $"Dire \"{actionText}\" ou appuyer sur E");
        }
        
        protected virtual void OnDrawGizmosSelected()
        {
            // Visualiser la portée d'interaction
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}
