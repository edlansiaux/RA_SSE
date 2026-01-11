using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RASSE.Environment
{
    /// <summary>
    /// Types de zones dangereuses conformes aux scénarios SSE
    /// </summary>
    public enum DangerType
    {
        Fire = 0,           // Incendie
        Gas = 1,            // Fuite de gaz
        Chemical = 2,       // Contamination chimique
        Radiation = 3,      // Zone radiologique
        Collapse = 4,       // Risque d'effondrement
        Electrical = 5,     // Risque électrique
        Explosion = 6,      // Risque d'explosion
        Flood = 7           // Inondation
    }

    /// <summary>
    /// Contrôleur des zones de danger pour les scénarios SSE.
    /// Gère la détection, les alertes et les effets sur les entités.
    /// Conforme aux blocs SysML 32-35 (Environnement et Dangers).
    /// </summary>
    public class DangerZoneController : MonoBehaviour
    {
        [Header("Configuration de la Zone")]
        [SerializeField] private string zoneId;
        [SerializeField] private DangerType dangerType = DangerType.Fire;
        [SerializeField] [Range(1, 5)] private int dangerLevel = 3;
        [SerializeField] private string description;
        
        [Header("Paramètres Géométriques")]
        [SerializeField] private float warningRadius = 20f;
        [SerializeField] private float dangerRadius = 10f;
        [SerializeField] private float lethalRadius = 3f;
        [SerializeField] private bool isDynamic = false;
        [SerializeField] private float expansionRate = 0f;
        
        [Header("Effets sur les Entités")]
        [SerializeField] private float damagePerSecond = 10f;
        [SerializeField] private bool affectsVictims = true;
        [SerializeField] private bool affectsRescuers = true;
        [SerializeField] private bool requiresProtection = false;
        [SerializeField] private string requiredProtectionEquipment;
        
        [Header("Paramètres Visuels")]
        [SerializeField] private Color warningColor = new Color(1f, 0.8f, 0f, 0.3f);
        [SerializeField] private Color dangerColor = new Color(1f, 0.4f, 0f, 0.5f);
        [SerializeField] private Color lethalColor = new Color(1f, 0f, 0f, 0.7f);
        [SerializeField] private bool showVisualIndicators = true;
        [SerializeField] private ParticleSystem dangerParticles;
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip warningSound;
        [SerializeField] private AudioClip dangerSound;
        [SerializeField] private bool loopSound = true;
        
        [Header("Événements")]
        public UnityEvent<GameObject> OnEntityEnterWarningZone;
        public UnityEvent<GameObject> OnEntityEnterDangerZone;
        public UnityEvent<GameObject> OnEntityEnterLethalZone;
        public UnityEvent<GameObject> OnEntityExitZone;
        public UnityEvent OnZoneNeutralized;
        
        // État interne
        private HashSet<GameObject> entitiesInWarningZone = new HashSet<GameObject>();
        private HashSet<GameObject> entitiesInDangerZone = new HashSet<GameObject>();
        private HashSet<GameObject> entitiesInLethalZone = new HashSet<GameObject>();
        private bool isActive = true;
        private float currentDangerRadius;
        private float elapsedTime;
        
        // Composants
        private SphereCollider warningCollider;
        private SphereCollider dangerCollider;
        private SphereCollider lethalCollider;
        
        // Propriétés publiques
        public string ZoneId => zoneId;
        public DangerType Type => dangerType;
        public int DangerLevel => dangerLevel;
        public float WarningRadius => warningRadius;
        public float DangerRadius => currentDangerRadius;
        public float LethalRadius => lethalRadius;
        public bool IsActive => isActive;
        public int EntitiesAtRisk => entitiesInDangerZone.Count + entitiesInLethalZone.Count;
        
        private void Awake()
        {
            if (string.IsNullOrEmpty(zoneId))
            {
                zoneId = $"DANGER_{dangerType}_{Guid.NewGuid().ToString().Substring(0, 8)}";
            }
            
            currentDangerRadius = dangerRadius;
            SetupColliders();
            SetupAudio();
        }
        
        private void Start()
        {
            if (showVisualIndicators)
            {
                CreateVisualIndicators();
            }
            
            LogZoneCreation();
        }
        
        private void Update()
        {
            if (!isActive) return;
            
            elapsedTime += Time.deltaTime;
            
            // Expansion dynamique de la zone
            if (isDynamic && expansionRate > 0)
            {
                ExpandZone();
            }
            
            // Appliquer les dégâts aux entités dans les zones
            ApplyDamageToEntities();
            
            // Mettre à jour les effets visuels
            UpdateVisualEffects();
        }
        
        private void SetupColliders()
        {
            // Collider zone d'avertissement
            GameObject warningObj = new GameObject("WarningZone");
            warningObj.transform.SetParent(transform);
            warningObj.transform.localPosition = Vector3.zero;
            warningObj.layer = LayerMask.NameToLayer("Trigger");
            warningCollider = warningObj.AddComponent<SphereCollider>();
            warningCollider.isTrigger = true;
            warningCollider.radius = warningRadius;
            DangerZoneTrigger warningTrigger = warningObj.AddComponent<DangerZoneTrigger>();
            warningTrigger.Initialize(this, ZoneLevel.Warning);
            
            // Collider zone de danger
            GameObject dangerObj = new GameObject("DangerZone");
            dangerObj.transform.SetParent(transform);
            dangerObj.transform.localPosition = Vector3.zero;
            dangerObj.layer = LayerMask.NameToLayer("Trigger");
            dangerCollider = dangerObj.AddComponent<SphereCollider>();
            dangerCollider.isTrigger = true;
            dangerCollider.radius = dangerRadius;
            DangerZoneTrigger dangerTrigger = dangerObj.AddComponent<DangerZoneTrigger>();
            dangerTrigger.Initialize(this, ZoneLevel.Danger);
            
            // Collider zone létale
            if (lethalRadius > 0)
            {
                GameObject lethalObj = new GameObject("LethalZone");
                lethalObj.transform.SetParent(transform);
                lethalObj.transform.localPosition = Vector3.zero;
                lethalObj.layer = LayerMask.NameToLayer("Trigger");
                lethalCollider = lethalObj.AddComponent<SphereCollider>();
                lethalCollider.isTrigger = true;
                lethalCollider.radius = lethalRadius;
                DangerZoneTrigger lethalTrigger = lethalObj.AddComponent<DangerZoneTrigger>();
                lethalTrigger.Initialize(this, ZoneLevel.Lethal);
            }
        }
        
        private void SetupAudio()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
            
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = lethalRadius;
            audioSource.maxDistance = warningRadius;
            audioSource.loop = loopSound;
            audioSource.playOnAwake = false;
        }
        
        private void CreateVisualIndicators()
        {
            // Créer des indicateurs visuels pour les zones
            CreateZoneIndicator("WarningIndicator", warningRadius, warningColor);
            CreateZoneIndicator("DangerIndicator", dangerRadius, dangerColor);
            if (lethalRadius > 0)
            {
                CreateZoneIndicator("LethalIndicator", lethalRadius, lethalColor);
            }
        }
        
        private void CreateZoneIndicator(string name, float radius, Color color)
        {
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            indicator.name = name;
            indicator.transform.SetParent(transform);
            indicator.transform.localPosition = new Vector3(0, 0.01f, 0);
            indicator.transform.localScale = new Vector3(radius * 2, 0.01f, radius * 2);
            
            // Supprimer le collider du cylindre
            Destroy(indicator.GetComponent<Collider>());
            
            // Configurer le matériau
            Renderer renderer = indicator.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.SetFloat("_Mode", 3); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            renderer.material = mat;
        }
        
        private void ExpandZone()
        {
            currentDangerRadius += expansionRate * Time.deltaTime;
            
            if (dangerCollider != null)
            {
                dangerCollider.radius = currentDangerRadius;
            }
            
            // Mettre à jour les indicateurs visuels
            Transform dangerIndicator = transform.Find("DangerIndicator");
            if (dangerIndicator != null)
            {
                dangerIndicator.localScale = new Vector3(currentDangerRadius * 2, 0.01f, currentDangerRadius * 2);
            }
        }
        
        private void ApplyDamageToEntities()
        {
            if (damagePerSecond <= 0) return;
            
            float deltaTime = Time.deltaTime;
            
            // Dégâts dans la zone létale (dégâts x3)
            foreach (var entity in entitiesInLethalZone)
            {
                if (entity != null)
                {
                    ApplyDamageToEntity(entity, damagePerSecond * 3f * deltaTime);
                }
            }
            
            // Dégâts dans la zone de danger (dégâts normaux)
            foreach (var entity in entitiesInDangerZone)
            {
                if (entity != null && !entitiesInLethalZone.Contains(entity))
                {
                    ApplyDamageToEntity(entity, damagePerSecond * deltaTime);
                }
            }
        }
        
        private void ApplyDamageToEntity(GameObject entity, float damage)
        {
            // Vérifier si l'entité a une protection
            if (requiresProtection)
            {
                var protectionComponent = entity.GetComponent<ProtectionEquipment>();
                if (protectionComponent != null && protectionComponent.HasProtectionAgainst(dangerType))
                {
                    damage *= 0.1f; // Réduction de 90% avec protection
                }
            }
            
            // Appliquer les dégâts à une victime
            var victimController = entity.GetComponent<RASSE.Core.VictimController>();
            if (victimController != null && affectsVictims)
            {
                victimController.TakeDamage(damage, dangerType.ToString());
                return;
            }
            
            // Appliquer les dégâts à un secouriste
            var rescuerController = entity.GetComponent<RASSE.Core.RescuerController>();
            if (rescuerController != null && affectsRescuers)
            {
                rescuerController.TakeDamage(damage);
            }
        }
        
        private void UpdateVisualEffects()
        {
            if (dangerParticles != null && !dangerParticles.isPlaying && isActive)
            {
                dangerParticles.Play();
            }
        }
        
        /// <summary>
        /// Appelé par les triggers enfants quand une entité entre dans une zone
        /// </summary>
        public void OnEntityEnter(GameObject entity, ZoneLevel level)
        {
            if (entity == null) return;
            
            switch (level)
            {
                case ZoneLevel.Warning:
                    if (entitiesInWarningZone.Add(entity))
                    {
                        OnEntityEnterWarningZone?.Invoke(entity);
                        PlayWarningSound();
                        Debug.Log($"[DangerZone] {entity.name} entre dans la zone d'avertissement de {zoneId}");
                    }
                    break;
                    
                case ZoneLevel.Danger:
                    if (entitiesInDangerZone.Add(entity))
                    {
                        OnEntityEnterDangerZone?.Invoke(entity);
                        PlayDangerSound();
                        Debug.LogWarning($"[DangerZone] {entity.name} entre dans la zone de danger de {zoneId}!");
                    }
                    break;
                    
                case ZoneLevel.Lethal:
                    if (entitiesInLethalZone.Add(entity))
                    {
                        OnEntityEnterLethalZone?.Invoke(entity);
                        Debug.LogError($"[DangerZone] {entity.name} entre dans la zone létale de {zoneId}!!");
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Appelé par les triggers enfants quand une entité quitte une zone
        /// </summary>
        public void OnEntityExit(GameObject entity, ZoneLevel level)
        {
            if (entity == null) return;
            
            switch (level)
            {
                case ZoneLevel.Warning:
                    if (entitiesInWarningZone.Remove(entity))
                    {
                        // Vérifier si l'entité a quitté toutes les zones
                        if (!entitiesInDangerZone.Contains(entity) && !entitiesInLethalZone.Contains(entity))
                        {
                            OnEntityExitZone?.Invoke(entity);
                        }
                    }
                    break;
                    
                case ZoneLevel.Danger:
                    entitiesInDangerZone.Remove(entity);
                    break;
                    
                case ZoneLevel.Lethal:
                    entitiesInLethalZone.Remove(entity);
                    break;
            }
        }
        
        private void PlayWarningSound()
        {
            if (audioSource != null && warningSound != null)
            {
                audioSource.PlayOneShot(warningSound);
            }
        }
        
        private void PlayDangerSound()
        {
            if (audioSource != null && dangerSound != null)
            {
                if (loopSound)
                {
                    audioSource.clip = dangerSound;
                    audioSource.Play();
                }
                else
                {
                    audioSource.PlayOneShot(dangerSound);
                }
            }
        }
        
        /// <summary>
        /// Neutralise la zone de danger (ex: incendie éteint)
        /// </summary>
        public void Neutralize()
        {
            isActive = false;
            
            if (audioSource != null)
            {
                audioSource.Stop();
            }
            
            if (dangerParticles != null)
            {
                dangerParticles.Stop();
            }
            
            // Désactiver les indicateurs visuels
            foreach (Transform child in transform)
            {
                if (child.name.Contains("Indicator"))
                {
                    child.gameObject.SetActive(false);
                }
            }
            
            OnZoneNeutralized?.Invoke();
            Debug.Log($"[DangerZone] Zone {zoneId} neutralisée");
        }
        
        /// <summary>
        /// Réactive la zone de danger
        /// </summary>
        public void Reactivate()
        {
            isActive = true;
            
            foreach (Transform child in transform)
            {
                if (child.name.Contains("Indicator"))
                {
                    child.gameObject.SetActive(true);
                }
            }
            
            Debug.Log($"[DangerZone] Zone {zoneId} réactivée");
        }
        
        /// <summary>
        /// Vérifie si une position est dans la zone de danger
        /// </summary>
        public bool IsPositionInDanger(Vector3 position)
        {
            float distance = Vector3.Distance(transform.position, position);
            return distance <= currentDangerRadius;
        }
        
        /// <summary>
        /// Obtient le niveau de danger pour une position donnée
        /// </summary>
        public ZoneLevel GetDangerLevelAtPosition(Vector3 position)
        {
            float distance = Vector3.Distance(transform.position, position);
            
            if (distance <= lethalRadius) return ZoneLevel.Lethal;
            if (distance <= currentDangerRadius) return ZoneLevel.Danger;
            if (distance <= warningRadius) return ZoneLevel.Warning;
            return ZoneLevel.Safe;
        }
        
        /// <summary>
        /// Obtient les données de la zone pour l'export
        /// </summary>
        public DangerZoneData GetZoneData()
        {
            return new DangerZoneData
            {
                ZoneId = zoneId,
                Type = dangerType,
                Level = dangerLevel,
                Position = transform.position,
                WarningRadius = warningRadius,
                DangerRadius = currentDangerRadius,
                LethalRadius = lethalRadius,
                IsActive = isActive,
                EntitiesAtRisk = EntitiesAtRisk,
                Description = description
            };
        }
        
        private void LogZoneCreation()
        {
            Debug.Log($"[DangerZone] Zone créée: {zoneId} | Type: {dangerType} | Niveau: {dangerLevel} | Position: {transform.position}");
        }
        
        private void OnDrawGizmos()
        {
            // Zone d'avertissement (jaune)
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, warningRadius);
            
            // Zone de danger (orange)
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, isDynamic ? currentDangerRadius : dangerRadius);
            
            // Zone létale (rouge)
            if (lethalRadius > 0)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
                Gizmos.DrawWireSphere(transform.position, lethalRadius);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Afficher les zones avec des sphères pleines quand sélectionné
            Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
            Gizmos.DrawSphere(transform.position, warningRadius);
            
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.15f);
            Gizmos.DrawSphere(transform.position, isDynamic ? currentDangerRadius : dangerRadius);
            
            if (lethalRadius > 0)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
                Gizmos.DrawSphere(transform.position, lethalRadius);
            }
        }
    }
    
    /// <summary>
    /// Niveaux de danger dans une zone
    /// </summary>
    public enum ZoneLevel
    {
        Safe,
        Warning,
        Danger,
        Lethal
    }
    
    /// <summary>
    /// Composant trigger pour les sous-zones
    /// </summary>
    public class DangerZoneTrigger : MonoBehaviour
    {
        private DangerZoneController parentZone;
        private ZoneLevel zoneLevel;
        
        public void Initialize(DangerZoneController parent, ZoneLevel level)
        {
            parentZone = parent;
            zoneLevel = level;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (parentZone != null)
            {
                parentZone.OnEntityEnter(other.gameObject, zoneLevel);
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (parentZone != null)
            {
                parentZone.OnEntityExit(other.gameObject, zoneLevel);
            }
        }
    }
    
    /// <summary>
    /// Interface pour les équipements de protection
    /// </summary>
    public class ProtectionEquipment : MonoBehaviour
    {
        [SerializeField] private List<DangerType> protectedAgainst = new List<DangerType>();
        
        public bool HasProtectionAgainst(DangerType dangerType)
        {
            return protectedAgainst.Contains(dangerType);
        }
    }
    
    /// <summary>
    /// Données exportables d'une zone de danger
    /// </summary>
    [Serializable]
    public class DangerZoneData
    {
        public string ZoneId;
        public DangerType Type;
        public int Level;
        public Vector3 Position;
        public float WarningRadius;
        public float DangerRadius;
        public float LethalRadius;
        public bool IsActive;
        public int EntitiesAtRisk;
        public string Description;
    }
}
