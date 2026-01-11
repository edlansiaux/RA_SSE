// ============================================================================
// RASSE - Réalité Augmentée en Situations Sanitaires Exceptionnelles
// HospitalProfileSO.cs - ScriptableObject pour profils d'hôpitaux
// Conforme au rapport académique - Bloc 42: Système Information Hospitalier
// ============================================================================

using UnityEngine;
using System;
using System.Collections.Generic;

namespace RASSE.Data
{
    /// <summary>
    /// Type d'établissement hospitalier
    /// </summary>
    public enum HospitalType
    {
        CHU,                    // Centre Hospitalier Universitaire
        CHR,                    // Centre Hospitalier Régional
        CH,                     // Centre Hospitalier
        Clinique,               // Clinique privée
        HopitalMilitaire,       // Hôpital d'instruction des armées
        SMUR,                   // Service Mobile d'Urgence et de Réanimation
        PosteMedicalAvance      // PMA sur site
    }

    /// <summary>
    /// Spécialité médicale disponible
    /// </summary>
    [Serializable]
    public class MedicalSpecialty
    {
        public string specialtyName;
        public SpecialtyType type;
        public int availableBeds;
        public int totalBeds;
        public bool h24Available = true;        // Disponible 24h/24
        public int currentWaitTimeMinutes;
    }

    public enum SpecialtyType
    {
        Emergency,              // Urgences générales
        TraumaCenter,           // Centre de traumatologie
        BurnUnit,               // Unité grands brûlés
        Neurology,              // Neurologie/Neurochirurgie
        Cardiology,             // Cardiologie
        Orthopedics,            // Orthopédie
        Pediatrics,             // Pédiatrie
        IntensiveCare,          // Réanimation
        Surgery,                // Chirurgie générale
        Radiology,              // Imagerie médicale
        Toxicology,             // Toxicologie
        Psychiatry              // Psychiatrie de crise
    }

    /// <summary>
    /// Ressources matérielles de l'hôpital
    /// </summary>
    [Serializable]
    public class HospitalResources
    {
        [Header("Blocs Opératoires")]
        public int operatingRooms = 4;
        public int availableOperatingRooms = 2;
        
        [Header("Imagerie")]
        public int scanners = 2;
        public int irmMachines = 1;
        public int xrayRooms = 3;
        
        [Header("Réanimation")]
        public int icuBeds = 10;
        public int availableIcuBeds = 3;
        public int ventilators = 8;
        public int availableVentilators = 4;
        
        [Header("Sang")]
        public bool bloodBankAvailable = true;
        public Dictionary<string, int> bloodStockUnits = new Dictionary<string, int>
        {
            { "O-", 20 }, { "O+", 50 }, { "A-", 15 }, { "A+", 40 },
            { "B-", 10 }, { "B+", 25 }, { "AB-", 5 }, { "AB+", 15 }
        };
    }

    /// <summary>
    /// Contact d'urgence de l'hôpital
    /// </summary>
    [Serializable]
    public class EmergencyContact
    {
        public string role;                 // Ex: "Médecin régulateur"
        public string name;
        public string phoneNumber;
        public string email;
        public bool isAvailable = true;
    }

    /// <summary>
    /// ScriptableObject définissant un profil d'hôpital complet
    /// Conforme NFR-INT (interopérabilité) et Bloc 42
    /// </summary>
    [CreateAssetMenu(fileName = "NewHospital", menuName = "RASSE/Hospital Profile", order = 2)]
    public class HospitalProfileSO : ScriptableObject
    {
        [Header("=== IDENTIFICATION ===")]
        public string hospitalId;
        public string hospitalName = "Hôpital Central";
        public string finess;                   // N° FINESS (identifiant national)
        public HospitalType hospitalType = HospitalType.CH;
        public Sprite hospitalLogo;

        [Header("=== LOCALISATION ===")]
        public string address;
        public string city;
        public string postalCode;
        public Vector3 worldPosition;           // Position dans la scène Unity
        public Vector2 gpsCoordinates;          // Lat/Long réelles
        public float distanceFromSceneKm = 5f;  // Distance par défaut

        [Header("=== CAPACITÉS ===")]
        [Range(0, 1000)]
        public int totalBeds = 200;
        [Range(0, 500)]
        public int emergencyBeds = 30;
        [Range(0, 100)]
        public int availableBeds = 15;
        
        [Tooltip("Capacité afflux massif (Plan Blanc)")]
        public int surgeCapacity = 50;
        public bool planBlancActivated = false;

        [Header("=== SPÉCIALITÉS ===")]
        public List<MedicalSpecialty> specialties = new List<MedicalSpecialty>();

        [Header("=== RESSOURCES ===")]
        public HospitalResources resources = new HospitalResources();

        [Header("=== CONTACTS ===")]
        public string mainPhoneNumber;
        public string emergencyPhoneNumber;
        public string samuPhoneNumber = "15";
        public List<EmergencyContact> contacts = new List<EmergencyContact>();

        [Header("=== PARAMÈTRES SIMULATION ===")]
        [Range(0, 120)]
        public int averageWaitTimeMinutes = 15;
        [Range(0, 60)]
        public int estimatedTravelTimeMinutes = 10;
        
        [Tooltip("Score de priorité (plus élevé = plus adapté pour urgences graves)")]
        [Range(0, 100)]
        public int traumaPriorityScore = 50;
        
        public bool acceptsHelicopter = false;
        public bool h24Emergency = true;

        [Header("=== INTEROPÉRABILITÉ (NFR-INT) ===")]
        public bool fhirEnabled = true;
        public bool hl7Enabled = true;
        public string fhirEndpoint;
        public string hl7EndpointIP;
        public int hl7Port = 2575;

        [Header("=== COULEUR UI ===")]
        public Color hospitalColor = Color.blue;
        public Color lowCapacityColor = Color.yellow;
        public Color noCapacityColor = Color.red;

        /// <summary>
        /// Vérifie si l'hôpital peut accepter une catégorie START
        /// </summary>
        public bool CanAcceptCategory(string startCategory)
        {
            if (availableBeds <= 0) return false;

            switch (startCategory)
            {
                case "RED":
                    // Rouge nécessite réa/trauma
                    return HasSpecialty(SpecialtyType.TraumaCenter) || 
                           HasSpecialty(SpecialtyType.IntensiveCare);
                case "YELLOW":
                    // Jaune nécessite urgences ou chirurgie
                    return HasSpecialty(SpecialtyType.Emergency) || 
                           HasSpecialty(SpecialtyType.Surgery);
                case "GREEN":
                    // Vert peut aller partout avec des lits
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Vérifie si l'hôpital a une spécialité donnée
        /// </summary>
        public bool HasSpecialty(SpecialtyType type)
        {
            return specialties.Exists(s => s.type == type && s.availableBeds > 0);
        }

        /// <summary>
        /// Obtient le nombre de lits disponibles pour une spécialité
        /// </summary>
        public int GetAvailableBedsForSpecialty(SpecialtyType type)
        {
            var specialty = specialties.Find(s => s.type == type);
            return specialty?.availableBeds ?? 0;
        }

        /// <summary>
        /// Calcule le score d'adéquation pour un patient
        /// </summary>
        public float CalculateMatchScore(string startCategory, string injuryType, float distance)
        {
            float score = 100f;

            // Pénalité distance (25% du score)
            score -= (distance / 50f) * 25f;

            // Bonus/Malus capacité (30% du score)
            float capacityRatio = (float)availableBeds / Mathf.Max(totalBeds, 1);
            score += (capacityRatio - 0.5f) * 30f;

            // Bonus spécialité (20% du score)
            if (startCategory == "RED" && HasSpecialty(SpecialtyType.TraumaCenter))
                score += 20f;
            if (injuryType == "Burn" && HasSpecialty(SpecialtyType.BurnUnit))
                score += 20f;
            if (injuryType == "HeadTrauma" && HasSpecialty(SpecialtyType.Neurology))
                score += 20f;

            // Pénalité temps d'attente (15% du score)
            score -= (averageWaitTimeMinutes / 60f) * 15f;

            // Bonus type établissement (10% du score)
            if (hospitalType == HospitalType.CHU)
                score += 10f;
            else if (hospitalType == HospitalType.PosteMedicalAvance)
                score += 5f; // Proximité

            return Mathf.Clamp(score, 0f, 100f);
        }

        /// <summary>
        /// Réserve un lit et retourne true si succès
        /// </summary>
        public bool ReserveBed(SpecialtyType preferredSpecialty = SpecialtyType.Emergency)
        {
            if (availableBeds <= 0) return false;

            var specialty = specialties.Find(s => s.type == preferredSpecialty && s.availableBeds > 0);
            if (specialty != null)
            {
                specialty.availableBeds--;
            }

            availableBeds--;
            return true;
        }

        /// <summary>
        /// Libère un lit
        /// </summary>
        public void ReleaseBed(SpecialtyType specialty = SpecialtyType.Emergency)
        {
            var spec = specialties.Find(s => s.type == specialty);
            if (spec != null && spec.availableBeds < spec.totalBeds)
            {
                spec.availableBeds++;
            }

            if (availableBeds < totalBeds)
            {
                availableBeds++;
            }
        }

        /// <summary>
        /// Génère l'identifiant FHIR de l'organisation
        /// </summary>
        public string GetFHIROrganizationId()
        {
            return $"Organization/{finess ?? hospitalId}";
        }

        /// <summary>
        /// Obtient la couleur selon la capacité actuelle
        /// </summary>
        public Color GetCapacityColor()
        {
            float ratio = (float)availableBeds / Mathf.Max(emergencyBeds, 1);
            if (ratio > 0.5f) return hospitalColor;
            if (ratio > 0.2f) return lowCapacityColor;
            return noCapacityColor;
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(hospitalId))
                hospitalId = "HOP-" + System.Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
            
            availableBeds = Mathf.Min(availableBeds, totalBeds);
        }
    }
}
