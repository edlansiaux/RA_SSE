using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using VictimController = RASSE.Core.VictimController;

namespace RASSE.Zones
{
    /// <summary>
    /// Types de zones de triage selon le protocole START
    /// </summary>
    public enum TriageZoneType
    {
        Red = 0,        // Urgences absolues (UA) - Immediate
        Yellow = 1,     // Urgences relatives (UR) - Delayed
        Green = 2,      // Impliqués (blessés légers) - Minor
        Black = 3,      // Décédés/Dépassés - Deceased
        Waiting = 4,    // Zone d'attente pré-triage
        PMA = 5         // Poste Médical Avancé
    }

    /// <summary>
    /// Contrôleur des zones de triage pour la gestion des victimes.
    /// Conforme au protocole START et aux exigences REQ-3 (Classification).
    /// </summary>
    public class TriageZoneController : MonoBehaviour
    {
        [Header("Identification")]
        [SerializeField] private string zoneId;
        [SerializeField] private string zoneName;
        [SerializeField] private TriageZoneType zoneType = TriageZoneType.Red;
        
        [Header("Capacité")]
        [SerializeField] private int capacity = 20;
        [SerializeField] private int currentOccupancy = 0;
        [SerializeField] private bool allowOvercapacity = false;
        [SerializeField] private int overcapacityLimit = 5;
        
        [Header("Géométrie")]
        [SerializeField] private float zoneRadius = 10f;
        [SerializeField] private Vector3 zoneDimensions = new Vector3(20f, 2f, 20f);
        [SerializeField] private bool useBoxCollider = true;
        
        [Header("Apparence")]
        [SerializeField] private Color zoneColor = Color.red;
        [SerializeField] private Material zoneMaterial;
        [SerializeField] private bool showGroundMarking = true;
        [SerializeField] private bool showFlag = true;
        [SerializeField] private GameObject flagPrefab;
        
        [Header("Équipement Disponible")]
        [SerializeField] private List<string> availableEquipment = new List<string>();
        [SerializeField] private int medicalStaffCount = 0;
        [SerializeField] private bool hasOxygenSupply = false;
        [SerializeField] private bool hasDefibrillator = false;
        
        [Header("Événements")]
        public UnityEvent<GameObject> OnVictimEntered;
        public UnityEvent<GameObject> OnVictimExited;
        public UnityEvent OnZoneFull;
        public UnityEvent OnZoneOvercapacity;
        public UnityEvent<int> OnOccupancyChanged;
        
        // État interne
        private List<GameObject> victimsInZone = new List<GameObject>();
        private Dictionary<string, DateTime> victimEntryTimes = new Dictionary<string, DateTime>();
        private Collider zoneCollider;
        private GameObject groundMarking;
        private GameObject flagObject;
        
        // Propriétés publiques
        public string ZoneId => zoneId;
        public string ZoneName => zoneName;
        public TriageZoneType ZoneType => zoneType;
        public int Capacity => capacity;
        public int CurrentOccupancy => currentOccupancy;
        public int AvailableSpots => Mathf.Max(0, capacity - currentOccupancy);
        public bool IsFull => currentOccupancy >= capacity;
        public bool IsOvercapacity => currentOccupancy > capacity;
        public float OccupancyPercentage => capacity > 0 ? (float)currentOccupancy / capacity * 100f : 0f;
        public IReadOnlyList<GameObject> VictimsInZone => victimsInZone.AsReadOnly();
        
        private void Awake()
        {
            if (string.IsNullOrEmpty(zoneId))
            {
                zoneId = $"ZONE_{zoneType}_{Guid.NewGuid().ToString().Substring(0, 8)}";
            }
            
            if (string.IsNullOrEmpty(zoneName))
            {
                zoneName = GetDefaultZoneName();
            }
            
            SetupZoneColor();
            SetupCollider();
        }
        
        private void Start()
        {
            if (showGroundMarking)
            {
                CreateGroundMarking();
            }
            
            if (showFlag)
            {
                CreateFlag();
            }
            
            LogZoneCreation();
        }
        
        private string GetDefaultZoneName()
        {
            switch (zoneType)
            {
                case TriageZoneType.Red: return "Zone Rouge - Urgences Absolues";
                case TriageZoneType.Yellow: return "Zone Jaune - Urgences Relatives";
                case TriageZoneType.Green: return "Zone Verte - Impliqués";
                case TriageZoneType.Black: return "Zone Noire - Décédés";
                case TriageZoneType.Waiting: return "Zone d'Attente";
                case TriageZoneType.PMA: return "Poste Médical Avancé";
                default: return "Zone de Triage";
            }
        }
        
        private void SetupZoneColor()
        {
            switch (zoneType)
            {
                case TriageZoneType.Red:
                    zoneColor = new Color(0.9f, 0.1f, 0.1f, 0.5f);
                    break;
                case TriageZoneType.Yellow:
                    zoneColor = new Color(0.9f, 0.9f, 0.1f, 0.5f);
                    break;
                case TriageZoneType.Green:
                    zoneColor = new Color(0.1f, 0.8f, 0.1f, 0.5f);
                    break;
                case TriageZoneType.Black:
                    zoneColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
                    break;
                case TriageZoneType.Waiting:
                    zoneColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    break;
                case TriageZoneType.PMA:
                    zoneColor = new Color(0.1f, 0.1f, 0.9f, 0.5f);
                    break;
            }
        }
        
        private void SetupCollider()
        {
            if (useBoxCollider)
            {
                BoxCollider box = gameObject.GetComponent<BoxCollider>();
                if (box == null)
                {
                    box = gameObject.AddComponent<BoxCollider>();
                }
                box.isTrigger = true;
                box.size = zoneDimensions;
                box.center = new Vector3(0, zoneDimensions.y / 2, 0);
                zoneCollider = box;
            }
            else
            {
                SphereCollider sphere = gameObject.GetComponent<SphereCollider>();
                if (sphere == null)
                {
                    sphere = gameObject.AddComponent<SphereCollider>();
                }
                sphere.isTrigger = true;
                sphere.radius = zoneRadius;
                zoneCollider = sphere;
            }
        }
        
        private void CreateGroundMarking()
        {
            groundMarking = GameObject.CreatePrimitive(PrimitiveType.Quad);
            groundMarking.name = "GroundMarking";
            groundMarking.transform.SetParent(transform);
            groundMarking.transform.localPosition = new Vector3(0, 0.02f, 0);
            groundMarking.transform.localRotation = Quaternion.Euler(90, 0, 0);
            
            if (useBoxCollider)
            {
                groundMarking.transform.localScale = new Vector3(zoneDimensions.x, zoneDimensions.z, 1);
            }
            else
            {
                groundMarking.transform.localScale = new Vector3(zoneRadius * 2, zoneRadius * 2, 1);
            }
            
            // Supprimer le collider du quad
            Destroy(groundMarking.GetComponent<Collider>());
            
            // Appliquer le matériau
            Renderer renderer = groundMarking.GetComponent<Renderer>();
            if (zoneMaterial != null)
            {
                renderer.material = zoneMaterial;
            }
            else
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = zoneColor;
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                renderer.material = mat;
            }
        }
        
        private void CreateFlag()
        {
            if (flagPrefab != null)
            {
                flagObject = Instantiate(flagPrefab, transform);
                flagObject.transform.localPosition = new Vector3(0, 0, 0);
            }
            else
            {
                // Créer un drapeau simple
                flagObject = new GameObject("Flag");
                flagObject.transform.SetParent(transform);
                flagObject.transform.localPosition = Vector3.zero;
                
                // Mât
                GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.name = "Pole";
                pole.transform.SetParent(flagObject.transform);
                pole.transform.localPosition = new Vector3(0, 1.5f, 0);
                pole.transform.localScale = new Vector3(0.1f, 3f, 0.1f);
                Destroy(pole.GetComponent<Collider>());
                
                // Drapeau
                GameObject flag = GameObject.CreatePrimitive(PrimitiveType.Cube);
                flag.name = "FlagCloth";
                flag.transform.SetParent(flagObject.transform);
                flag.transform.localPosition = new Vector3(0.5f, 2.5f, 0);
                flag.transform.localScale = new Vector3(1f, 0.6f, 0.05f);
                Destroy(flag.GetComponent<Collider>());
                
                // Appliquer la couleur
                Renderer flagRenderer = flag.GetComponent<Renderer>();
                Material flagMat = new Material(Shader.Find("Standard"));
                flagMat.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 1f);
                flagRenderer.material = flagMat;
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Vérifier si c'est une victime
            var victimController = other.GetComponent<RASSE.Victim.VictimController>();
            if (victimController != null)
            {
                AddVictim(other.gameObject);
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            var victimController = other.GetComponent<RASSE.Victim.VictimController>();
            if (victimController != null)
            {
                RemoveVictim(other.gameObject);
            }
        }
        
        /// <summary>
        /// Ajoute une victime à la zone
        /// </summary>
        public bool AddVictim(GameObject victim)
        {
            if (victim == null) return false;
            
            // Vérifier si la victime est déjà dans la zone
            if (victimsInZone.Contains(victim)) return true;
            
            // Vérifier la capacité
            if (IsFull && !allowOvercapacity)
            {
                Debug.LogWarning($"[TriageZone] Zone {zoneId} pleine - Impossible d'ajouter {victim.name}");
                OnZoneFull?.Invoke();
                return false;
            }
            
            if (IsOvercapacity && currentOccupancy >= capacity + overcapacityLimit)
            {
                Debug.LogError($"[TriageZone] Zone {zoneId} en surcapacité maximale!");
                return false;
            }
            
            // Ajouter la victime
            victimsInZone.Add(victim);
            currentOccupancy = victimsInZone.Count;
            
            string victimId = victim.GetInstanceID().ToString();
            victimEntryTimes[victimId] = DateTime.Now;
            
            // Mettre à jour le contrôleur de victime
            var victimController = victim.GetComponent<RASSE.Victim.VictimController>();
            if (victimController != null)
            {
                victimController.SetCurrentZone(this);
            }
            
            OnVictimEntered?.Invoke(victim);
            OnOccupancyChanged?.Invoke(currentOccupancy);
            
            if (IsOvercapacity)
            {
                OnZoneOvercapacity?.Invoke();
            }
            
            Debug.Log($"[TriageZone] {victim.name} ajouté à {zoneName} ({currentOccupancy}/{capacity})");
            return true;
        }
        
        /// <summary>
        /// Retire une victime de la zone
        /// </summary>
        public bool RemoveVictim(GameObject victim)
        {
            if (victim == null) return false;
            
            if (!victimsInZone.Contains(victim)) return false;
            
            victimsInZone.Remove(victim);
            currentOccupancy = victimsInZone.Count;
            
            string victimId = victim.GetInstanceID().ToString();
            victimEntryTimes.Remove(victimId);
            
            // Mettre à jour le contrôleur de victime
            var victimController = victim.GetComponent<RASSE.Victim.VictimController>();
            if (victimController != null)
            {
                victimController.ClearCurrentZone();
            }
            
            OnVictimExited?.Invoke(victim);
            OnOccupancyChanged?.Invoke(currentOccupancy);
            
            Debug.Log($"[TriageZone] {victim.name} retiré de {zoneName} ({currentOccupancy}/{capacity})");
            return true;
        }
        
        /// <summary>
        /// Obtient le temps passé par une victime dans la zone
        /// </summary>
        public TimeSpan GetVictimTimeInZone(GameObject victim)
        {
            string victimId = victim.GetInstanceID().ToString();
            if (victimEntryTimes.TryGetValue(victimId, out DateTime entryTime))
            {
                return DateTime.Now - entryTime;
            }
            return TimeSpan.Zero;
        }
        
        /// <summary>
        /// Vérifie si la zone peut accepter une victime de la catégorie donnée
        /// </summary>
        public bool CanAcceptVictimCategory(TriageCategory category)
        {
            switch (zoneType)
            {
                case TriageZoneType.Red:
                    return category == TriageCategory.Red;
                case TriageZoneType.Yellow:
                    return category == TriageCategory.Yellow;
                case TriageZoneType.Green:
                    return category == TriageCategory.Green;
                case TriageZoneType.Black:
                    return category == TriageCategory.Black;
                case TriageZoneType.Waiting:
                case TriageZoneType.PMA:
                    return true;
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Obtient les statistiques de la zone
        /// </summary>
        public TriageZoneStats GetStats()
        {
            var stats = new TriageZoneStats
            {
                ZoneId = zoneId,
                ZoneName = zoneName,
                ZoneType = zoneType,
                Capacity = capacity,
                CurrentOccupancy = currentOccupancy,
                AvailableSpots = AvailableSpots,
                OccupancyPercentage = OccupancyPercentage,
                IsFull = IsFull,
                IsOvercapacity = IsOvercapacity,
                MedicalStaffCount = medicalStaffCount,
                HasOxygenSupply = hasOxygenSupply,
                HasDefibrillator = hasDefibrillator,
                VictimIds = victimsInZone.Select(v => v.GetInstanceID().ToString()).ToList()
            };
            
            // Calculer le temps moyen en zone
            if (victimEntryTimes.Count > 0)
            {
                double totalSeconds = victimEntryTimes.Values
                    .Select(t => (DateTime.Now - t).TotalSeconds)
                    .Average();
                stats.AverageTimeInZoneSeconds = totalSeconds;
            }
            
            return stats;
        }
        
        /// <summary>
        /// Met à jour la capacité de la zone
        /// </summary>
        public void SetCapacity(int newCapacity)
        {
            capacity = Mathf.Max(1, newCapacity);
            OnOccupancyChanged?.Invoke(currentOccupancy);
        }
        
        /// <summary>
        /// Ajoute du personnel médical à la zone
        /// </summary>
        public void AddMedicalStaff(int count = 1)
        {
            medicalStaffCount += count;
            Debug.Log($"[TriageZone] {count} personnel médical ajouté à {zoneName}. Total: {medicalStaffCount}");
        }
        
        /// <summary>
        /// Retire du personnel médical de la zone
        /// </summary>
        public void RemoveMedicalStaff(int count = 1)
        {
            medicalStaffCount = Mathf.Max(0, medicalStaffCount - count);
        }
        
        /// <summary>
        /// Obtient la victime avec le temps d'attente le plus long
        /// </summary>
        public GameObject GetLongestWaitingVictim()
        {
            if (victimsInZone.Count == 0) return null;
            
            string longestWaitingId = victimEntryTimes
                .OrderBy(kvp => kvp.Value)
                .FirstOrDefault().Key;
            
            return victimsInZone.FirstOrDefault(v => v.GetInstanceID().ToString() == longestWaitingId);
        }
        
        /// <summary>
        /// Obtient la position d'un emplacement libre dans la zone
        /// </summary>
        public Vector3 GetFreeSpotPosition()
        {
            // Calculer une position basée sur l'occupation actuelle
            int row = currentOccupancy / 5;
            int col = currentOccupancy % 5;
            
            float spacing = 2f;
            Vector3 offset = new Vector3(
                (col - 2) * spacing,
                0,
                (row - 2) * spacing
            );
            
            return transform.position + offset;
        }
        
        private void LogZoneCreation()
        {
            Debug.Log($"[TriageZone] Zone créée: {zoneName} ({zoneId}) | Type: {zoneType} | Capacité: {capacity}");
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 0.3f);
            
            if (useBoxCollider)
            {
                Gizmos.DrawWireCube(transform.position + new Vector3(0, zoneDimensions.y / 2, 0), zoneDimensions);
            }
            else
            {
                Gizmos.DrawWireSphere(transform.position, zoneRadius);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 0.5f);
            
            if (useBoxCollider)
            {
                Gizmos.DrawCube(transform.position + new Vector3(0, zoneDimensions.y / 2, 0), zoneDimensions);
            }
            else
            {
                Gizmos.DrawSphere(transform.position, zoneRadius);
            }
            
            // Afficher le nom de la zone
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3, $"{zoneName}\n{currentOccupancy}/{capacity}");
            #endif
        }
    }
    
    /// <summary>
    /// Catégories de triage START
    /// </summary>
    public enum TriageCategory
    {
        Red = 0,
        Yellow = 1,
        Green = 2,
        Black = 3,
        Unknown = 4
    }
    
    /// <summary>
    /// Statistiques d'une zone de triage
    /// </summary>
    [Serializable]
    public class TriageZoneStats
    {
        public string ZoneId;
        public string ZoneName;
        public TriageZoneType ZoneType;
        public int Capacity;
        public int CurrentOccupancy;
        public int AvailableSpots;
        public float OccupancyPercentage;
        public bool IsFull;
        public bool IsOvercapacity;
        public int MedicalStaffCount;
        public bool HasOxygenSupply;
        public bool HasDefibrillator;
        public double AverageTimeInZoneSeconds;
        public List<string> VictimIds;
    }
}
