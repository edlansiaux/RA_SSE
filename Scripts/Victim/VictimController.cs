using UnityEngine;
using System;

namespace RASSE.Core
{
    /// <summary>
    /// VictimController - Gère une victime individuelle
    /// Contient les données médicales, l'état et les animations
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class VictimController : MonoBehaviour
    {
        [Header("=== IDENTIFICATION ===")]
        [SerializeField] private string patientId;
        public string PatientId => patientId;
        
        [SerializeField] private int victimNumber;
        public int VictimNumber => victimNumber;

        [Header("=== SIGNES VITAUX ===")]
        [SerializeField] private VitalSigns vitalSigns;
        public VitalSigns VitalSigns => vitalSigns;

        [Header("=== ÉTAT ===")]
        public bool IsDetected { get; set; }
        public bool IsTriaged { get; set; }
        public bool IsEvacuated { get; set; }
        public bool WasManuallyOverridden { get; set; }
        public float TriageTime { get; set; }
        public float DetectionTime { get; set; }
        
        [SerializeField] private StartCategory triageCategory = StartCategory.None;
        public StartCategory TriageCategory
        {
            get => triageCategory;
            set
            {
                triageCategory = value;
                UpdateVisualIndicator();
            }
        }

        [Header("=== BLESSURES ===")]
        public InjuryType primaryInjury = InjuryType.None;
        public InjuryLocation injuryLocation = InjuryLocation.None;
        public InjurySeverity injurySeverity = InjurySeverity.Minor;

        [Header("=== INFORMATIONS PATIENT ===")]
        public string firstName;
        public string lastName;
        public int age;
        public Gender gender;
        public string allergies;
        public string medicalHistory;

        [Header("=== VISUEL ===")]
        [SerializeField] private GameObject categoryIndicator;
        [SerializeField] private MeshRenderer indicatorRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private ParticleSystem bleedingEffect;

        [Header("=== AUDIO ===")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] painSounds;
        [SerializeField] private AudioClip[] helpSounds;

        // Événements
        public event Action<VictimController> OnDetected;
        public event Action<VictimController, StartCategory> OnTriaged;
        public event Action<VictimController> OnEvacuated;
        public event Action<VictimController> OnConditionChanged;

        // Détérioration de l'état
        private float deteriorationTimer;
        private float deteriorationRate = 0.1f; // Vitesse de détérioration

        private void Awake()
        {
            if (vitalSigns == null)
            {
                vitalSigns = new VitalSigns();
            }

            // Générer un ID unique si non défini
            if (string.IsNullOrEmpty(patientId))
            {
                patientId = GeneratePatientId();
            }
        }

        private void Start()
        {
            InitializeVisuals();
            UpdateAnimation();
        }

        private void Update()
        {
            // Simuler la détérioration de l'état si non traité
            if (!IsEvacuated && IsDetected)
            {
                SimulateConditionChange();
            }

            // Jouer des sons périodiquement
            if (!IsEvacuated && vitalSigns.isConscious && Time.time % 10 < 0.1f)
            {
                PlayRandomSound();
            }
        }

        /// <summary>
        /// Initialise la victime avec des paramètres aléatoires
        /// </summary>
        public void InitializeRandom(int number, DifficultyLevel difficulty)
        {
            victimNumber = number;
            patientId = GeneratePatientId();

            // Générer des données personnelles aléatoires
            GeneratePersonalData();

            // Générer des signes vitaux basés sur la difficulté
            GenerateVitalSigns(difficulty);

            // Générer des blessures
            GenerateInjuries(difficulty);

            UpdateVisualIndicator();
            UpdateAnimation();
        }

        /// <summary>
        /// Initialise à partir de données prédéfinies
        /// </summary>
        public void InitializeFromData(VictimData data)
        {
            if (data == null) return;

            patientId = data.patientId;
            firstName = data.firstName;
            lastName = data.lastName;
            age = data.age;
            gender = data.gender;
            
            vitalSigns = data.vitalSigns;
            primaryInjury = data.primaryInjury;
            injuryLocation = data.injuryLocation;
            injurySeverity = data.injurySeverity;

            UpdateVisualIndicator();
            UpdateAnimation();
        }

        private void GeneratePersonalData()
        {
            string[] maleNames = { "Jean", "Pierre", "Michel", "François", "Philippe", "Alain", "Nicolas", "David" };
            string[] femaleNames = { "Marie", "Sophie", "Isabelle", "Nathalie", "Christine", "Sylvie", "Catherine", "Valérie" };
            string[] lastNames = { "Martin", "Bernard", "Dubois", "Thomas", "Robert", "Richard", "Petit", "Durand" };

            gender = UnityEngine.Random.value > 0.5f ? Gender.Male : Gender.Female;
            firstName = gender == Gender.Male 
                ? maleNames[UnityEngine.Random.Range(0, maleNames.Length)]
                : femaleNames[UnityEngine.Random.Range(0, femaleNames.Length)];
            lastName = lastNames[UnityEngine.Random.Range(0, lastNames.Length)];
            age = UnityEngine.Random.Range(18, 80);
        }

        private void GenerateVitalSigns(DifficultyLevel difficulty)
        {
            // Déterminer aléatoirement la gravité
            float severityRoll = UnityEngine.Random.value;
            
            // Ajuster selon la difficulté
            float criticalChance = difficulty switch
            {
                DifficultyLevel.Easy => 0.2f,
                DifficultyLevel.Normal => 0.35f,
                DifficultyLevel.Hard => 0.5f,
                DifficultyLevel.Expert => 0.65f,
                _ => 0.35f
            };

            if (severityRoll < 0.1f) // 10% décès
            {
                GenerateCriticalVitals(true);
            }
            else if (severityRoll < criticalChance) // Rouge
            {
                GenerateCriticalVitals(false);
            }
            else if (severityRoll < criticalChance + 0.3f) // Jaune
            {
                GenerateModerateVitals();
            }
            else // Vert
            {
                GenerateMinorVitals();
            }
        }

        private void GenerateCriticalVitals(bool deceased)
        {
            if (deceased)
            {
                vitalSigns.isBreathing = false;
                vitalSigns.breathingAfterAirwayManeuver = false;
                vitalSigns.heartRate = 0;
                vitalSigns.isConscious = false;
                vitalSigns.glasgowComaScale = 3;
            }
            else
            {
                vitalSigns.isBreathing = UnityEngine.Random.value > 0.3f;
                vitalSigns.breathingAfterAirwayManeuver = !vitalSigns.isBreathing;
                vitalSigns.respiratoryRate = UnityEngine.Random.Range(6, 12);
                if (UnityEngine.Random.value > 0.5f)
                    vitalSigns.respiratoryRate = UnityEngine.Random.Range(32, 45);
                vitalSigns.heartRate = UnityEngine.Random.Range(40, 60);
                if (UnityEngine.Random.value > 0.5f)
                    vitalSigns.heartRate = UnityEngine.Random.Range(130, 180);
                vitalSigns.capillaryRefillTime = UnityEngine.Random.Range(3f, 6f);
                vitalSigns.hasRadialPulse = UnityEngine.Random.value > 0.6f;
                vitalSigns.canFollowCommands = false;
                vitalSigns.canWalk = false;
                vitalSigns.oxygenSaturation = UnityEngine.Random.Range(70, 88);
                vitalSigns.glasgowComaScale = UnityEngine.Random.Range(3, 9);
            }
        }

        private void GenerateModerateVitals()
        {
            vitalSigns.isBreathing = true;
            vitalSigns.respiratoryRate = UnityEngine.Random.Range(14, 28);
            vitalSigns.heartRate = UnityEngine.Random.Range(70, 110);
            vitalSigns.capillaryRefillTime = UnityEngine.Random.Range(1f, 2f);
            vitalSigns.hasRadialPulse = true;
            vitalSigns.canFollowCommands = true;
            vitalSigns.canWalk = false;
            vitalSigns.oxygenSaturation = UnityEngine.Random.Range(92, 97);
            vitalSigns.glasgowComaScale = UnityEngine.Random.Range(12, 15);
            vitalSigns.isConscious = true;
        }

        private void GenerateMinorVitals()
        {
            vitalSigns.isBreathing = true;
            vitalSigns.respiratoryRate = UnityEngine.Random.Range(12, 20);
            vitalSigns.heartRate = UnityEngine.Random.Range(65, 100);
            vitalSigns.capillaryRefillTime = UnityEngine.Random.Range(0.5f, 1.5f);
            vitalSigns.hasRadialPulse = true;
            vitalSigns.canFollowCommands = true;
            vitalSigns.canWalk = true;
            vitalSigns.oxygenSaturation = UnityEngine.Random.Range(96, 100);
            vitalSigns.glasgowComaScale = 15;
            vitalSigns.isConscious = true;
        }

        private void GenerateInjuries(DifficultyLevel difficulty)
        {
            InjuryType[] injuries = (InjuryType[])Enum.GetValues(typeof(InjuryType));
            InjuryLocation[] locations = (InjuryLocation[])Enum.GetValues(typeof(InjuryLocation));

            primaryInjury = injuries[UnityEngine.Random.Range(1, injuries.Length)];
            injuryLocation = locations[UnityEngine.Random.Range(1, locations.Length)];

            // Déterminer la sévérité basée sur les vitaux
            StartCategory estimatedCategory = GameManager.Instance?.triageSystem?.CalculateStartCategory(vitalSigns) 
                ?? StartCategory.Yellow;

            injurySeverity = estimatedCategory switch
            {
                StartCategory.Red => InjurySeverity.Critical,
                StartCategory.Yellow => InjurySeverity.Serious,
                StartCategory.Green => InjurySeverity.Minor,
                StartCategory.Black => InjurySeverity.Fatal,
                _ => InjurySeverity.Moderate
            };

            // Activer les effets visuels
            if (primaryInjury == InjuryType.Hemorrhage && bleedingEffect != null)
            {
                bleedingEffect.Play();
            }
        }

        private void SimulateConditionChange()
        {
            if (TriageCategory == StartCategory.Black) return;

            deteriorationTimer += Time.deltaTime;

            // Les victimes non traitées se détériorent
            if (deteriorationTimer >= 60f && !IsTriaged)
            {
                deteriorationTimer = 0f;
                
                // Légère détérioration des signes vitaux
                if (vitalSigns.oxygenSaturation > 80)
                    vitalSigns.oxygenSaturation -= 1;
                
                if (vitalSigns.systolicBloodPressure > 80)
                    vitalSigns.systolicBloodPressure -= 2;

                OnConditionChanged?.Invoke(this);
            }
        }

        private void InitializeVisuals()
        {
            if (categoryIndicator == null)
            {
                // Créer un indicateur simple au-dessus de la victime
                categoryIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                categoryIndicator.transform.SetParent(transform);
                categoryIndicator.transform.localPosition = new Vector3(0, 2f, 0);
                categoryIndicator.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                
                // Désactiver le collider de l'indicateur
                Destroy(categoryIndicator.GetComponent<Collider>());
                
                indicatorRenderer = categoryIndicator.GetComponent<MeshRenderer>();
                categoryIndicator.SetActive(false);
            }
        }

        private void UpdateVisualIndicator()
        {
            if (categoryIndicator == null || indicatorRenderer == null) return;

            if (IsTriaged && TriageCategory != StartCategory.None)
            {
                categoryIndicator.SetActive(true);
                indicatorRenderer.material.color = StartTriageSystem.GetCategoryColor(TriageCategory);
                
                // Ajouter un effet émissif
                indicatorRenderer.material.EnableKeyword("_EMISSION");
                indicatorRenderer.material.SetColor("_EmissionColor", 
                    StartTriageSystem.GetCategoryColor(TriageCategory) * 2f);
            }
            else
            {
                categoryIndicator.SetActive(false);
            }
        }

        private void UpdateAnimation()
        {
            if (animator == null) return;

            // Définir l'animation selon l'état
            if (!vitalSigns.isConscious || TriageCategory == StartCategory.Black)
            {
                animator.SetTrigger("Unconscious");
            }
            else if (!vitalSigns.canWalk)
            {
                animator.SetTrigger("Sitting");
            }
            else
            {
                animator.SetTrigger("Standing");
            }

            // Animation de douleur
            if (vitalSigns.painLevel > 5)
            {
                animator.SetBool("InPain", true);
            }
        }

        private void PlayRandomSound()
        {
            if (audioSource == null) return;

            AudioClip[] clips = vitalSigns.painLevel > 5 ? painSounds : helpSounds;
            if (clips != null && clips.Length > 0)
            {
                audioSource.PlayOneShot(clips[UnityEngine.Random.Range(0, clips.Length)]);
            }
        }

        private string GeneratePatientId()
        {
            return $"PAT-{DateTime.Now:yyyyMMdd}-{UnityEngine.Random.Range(1000, 9999)}";
        }

        /// <summary>
        /// Obtient un résumé de l'état de la victime
        /// </summary>
        public string GetStatusSummary()
        {
            return $"ID: {patientId}\n" +
                   $"Nom: {firstName} {lastName}, {age} ans\n" +
                   $"Catégorie: {TriageCategory}\n" +
                   $"FR: {vitalSigns.respiratoryRate}/min | FC: {vitalSigns.heartRate} bpm\n" +
                   $"SpO2: {vitalSigns.oxygenSaturation}% | RC: {vitalSigns.capillaryRefillTime}s\n" +
                   $"GCS: {vitalSigns.glasgowComaScale} | Marche: {(vitalSigns.canWalk ? "Oui" : "Non")}";
        }

        private void OnTriggerEnter(Collider other)
        {
            // Détection par le secouriste
            if (other.CompareTag("Rescuer") && !IsDetected)
            {
                IsDetected = true;
                DetectionTime = Time.time;
                OnDetected?.Invoke(this);
                GameManager.Instance?.RegisterVictimDetection(this);
            }
        }

        private void OnDrawGizmos()
        {
            // Dessiner un gizmo pour visualiser la victime dans l'éditeur
            Gizmos.color = IsTriaged 
                ? StartTriageSystem.GetCategoryColor(TriageCategory) 
                : Color.gray;
            Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.5f);
        }
    }

    // Enums pour les blessures
    public enum InjuryType
    {
        None,
        Hemorrhage,         // Hémorragie
        Fracture,           // Fracture
        Burn,               // Brûlure
        Concussion,         // Commotion
        InternalBleeding,   // Hémorragie interne
        Crush,              // Écrasement
        Laceration,         // Lacération
        Amputation,         // Amputation
        SmokeInhalation,    // Inhalation de fumée
        Shock               // État de choc
    }

    public enum InjuryLocation
    {
        None,
        Head,
        Neck,
        Chest,
        Abdomen,
        UpperLimb,
        LowerLimb,
        Spine,
        Pelvis,
        Multiple
    }

    public enum InjurySeverity
    {
        Minor,
        Moderate,
        Serious,
        Critical,
        Fatal
    }

    public enum Gender
    {
        Male,
        Female,
        Other
    }
}
