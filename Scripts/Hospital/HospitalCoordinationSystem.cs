using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace RASSE.Core
{
    /// <summary>
    /// HospitalCoordinationSystem - Gère la coordination avec les hôpitaux
    /// Sélection du meilleur hôpital selon la pathologie et la capacité
    /// </summary>
    public class HospitalCoordinationSystem : MonoBehaviour
    {
        [Header("=== HÔPITAUX DISPONIBLES ===")]
        [SerializeField] private List<HospitalData> hospitals = new List<HospitalData>();

        [Header("=== CONFIGURATION ===")]
        [SerializeField] private float maxDistanceKm = 50f;
        [SerializeField] private float updateInterval = 30f;

        [Header("=== COMMUNICATION ===")]
        [SerializeField] private float communicationDelay = 0.5f;
        [SerializeField] private bool simulateNetworkLatency = true;

        // État
        private float lastUpdateTime;
        private Dictionary<string, int> patientAssignments = new Dictionary<string, int>();

        public void Initialize()
        {
            Debug.Log("[HospitalSystem] Initialisation du système de coordination hospitalière");
            
            if (hospitals.Count == 0)
            {
                GenerateDefaultHospitals();
            }

            UpdateHospitalStatus();
        }

        private void Update()
        {
            // Mise à jour périodique des capacités
            if (Time.time - lastUpdateTime > updateInterval)
            {
                UpdateHospitalStatus();
                lastUpdateTime = Time.time;
            }
        }

        /// <summary>
        /// Génère des hôpitaux par défaut pour la simulation
        /// </summary>
        private void GenerateDefaultHospitals()
        {
            hospitals.Add(new HospitalData
            {
                hospitalId = "CHU-001",
                hospitalName = "CHU Lille - Centre Hospitalier Universitaire",
                hospitalType = HospitalType.CHU,
                distanceKm = 5.2f,
                hasTraumaCenter = true,
                hasBurnUnit = true,
                hasCardiology = true,
                hasNeurology = true,
                hasPediatrics = true,
                availableBeds = 45,
                totalBeds = 120,
                emergencyCapacity = EmergencyCapacity.High,
                estimatedWaitTime = 15
            });

            hospitals.Add(new HospitalData
            {
                hospitalId = "CH-002",
                hospitalName = "Centre Hospitalier de Roubaix",
                hospitalType = HospitalType.GeneralHospital,
                distanceKm = 8.7f,
                hasTraumaCenter = true,
                hasBurnUnit = false,
                hasCardiology = true,
                hasNeurology = false,
                hasPediatrics = true,
                availableBeds = 22,
                totalBeds = 80,
                emergencyCapacity = EmergencyCapacity.Medium,
                estimatedWaitTime = 25
            });

            hospitals.Add(new HospitalData
            {
                hospitalId = "CLIN-003",
                hospitalName = "Clinique du Parc",
                hospitalType = HospitalType.Clinic,
                distanceKm = 3.1f,
                hasTraumaCenter = false,
                hasBurnUnit = false,
                hasCardiology = false,
                hasNeurology = false,
                hasPediatrics = false,
                availableBeds = 15,
                totalBeds = 40,
                emergencyCapacity = EmergencyCapacity.Low,
                estimatedWaitTime = 10
            });

            hospitals.Add(new HospitalData
            {
                hospitalId = "CHU-004",
                hospitalName = "Hôpital Roger Salengro (CHRU Lille)",
                hospitalType = HospitalType.CHU,
                distanceKm = 6.5f,
                hasTraumaCenter = true,
                hasBurnUnit = true,
                hasCardiology = true,
                hasNeurology = true,
                hasPediatrics = true,
                availableBeds = 38,
                totalBeds = 150,
                emergencyCapacity = EmergencyCapacity.High,
                estimatedWaitTime = 20
            });

            hospitals.Add(new HospitalData
            {
                hospitalId = "CH-005",
                hospitalName = "Centre Hospitalier de Tourcoing",
                hospitalType = HospitalType.GeneralHospital,
                distanceKm = 12.3f,
                hasTraumaCenter = true,
                hasBurnUnit = false,
                hasCardiology = true,
                hasNeurology = true,
                hasPediatrics = true,
                availableBeds = 28,
                totalBeds = 90,
                emergencyCapacity = EmergencyCapacity.Medium,
                estimatedWaitTime = 18
            });
        }

        /// <summary>
        /// Obtient le meilleur hôpital pour une victime
        /// </summary>
        public HospitalData GetBestHospital(StartCategory category, Vector3 currentPosition)
        {
            return GetBestHospital(category, InjuryType.None, currentPosition);
        }

        /// <summary>
        /// Obtient le meilleur hôpital selon la catégorie et le type de blessure
        /// </summary>
        public HospitalData GetBestHospital(StartCategory category, InjuryType injuryType, Vector3 currentPosition)
        {
            List<HospitalData> eligibleHospitals = hospitals
                .Where(h => h.availableBeds > 0)
                .Where(h => h.distanceKm <= maxDistanceKm)
                .Where(h => IsHospitalSuitable(h, category, injuryType))
                .ToList();

            if (eligibleHospitals.Count == 0)
            {
                Debug.LogWarning("[HospitalSystem] Aucun hôpital disponible!");
                return hospitals.FirstOrDefault();
            }

            // Calculer un score pour chaque hôpital
            return eligibleHospitals
                .OrderByDescending(h => CalculateHospitalScore(h, category, injuryType))
                .First();
        }

        /// <summary>
        /// Vérifie si un hôpital est adapté pour le cas
        /// </summary>
        private bool IsHospitalSuitable(HospitalData hospital, StartCategory category, InjuryType injury)
        {
            // Les cas rouges nécessitent un centre de trauma
            if (category == StartCategory.Red)
            {
                if (!hospital.hasTraumaCenter && hospital.emergencyCapacity != EmergencyCapacity.High)
                    return false;
            }

            // Vérifier les spécialités requises
            switch (injury)
            {
                case InjuryType.Burn:
                    return hospital.hasBurnUnit || hospital.hospitalType == HospitalType.CHU;
                case InjuryType.Concussion:
                    return hospital.hasNeurology || hospital.hasTraumaCenter;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Calcule le score d'un hôpital pour un cas donné
        /// </summary>
        private float CalculateHospitalScore(HospitalData hospital, StartCategory category, InjuryType injury)
        {
            float score = 0f;

            // Score de capacité (plus de lits = meilleur)
            score += (float)hospital.availableBeds / hospital.totalBeds * 30f;

            // Score de distance (plus proche = meilleur)
            score += (maxDistanceKm - hospital.distanceKm) / maxDistanceKm * 25f;

            // Score de temps d'attente (moins = meilleur)
            score += Mathf.Max(0, 60 - hospital.estimatedWaitTime) / 60f * 20f;

            // Bonus pour les spécialités
            if (hospital.hasTraumaCenter && category == StartCategory.Red)
                score += 15f;

            if (hospital.hospitalType == HospitalType.CHU)
                score += 10f;

            // Pénalité si la capacité est faible
            if (hospital.emergencyCapacity == EmergencyCapacity.Low && category == StartCategory.Red)
                score -= 20f;

            return score;
        }

        /// <summary>
        /// Assigne un patient à un hôpital
        /// </summary>
        public bool AssignPatientToHospital(VictimController victim, HospitalData hospital)
        {
            if (hospital.availableBeds <= 0)
            {
                Debug.LogWarning($"[HospitalSystem] Pas de lit disponible à {hospital.hospitalName}");
                return false;
            }

            // Réserver un lit
            hospital.availableBeds--;
            patientAssignments[victim.PatientId] = hospitals.IndexOf(hospital);

            Debug.Log($"[HospitalSystem] Patient {victim.PatientId} assigné à {hospital.hospitalName}");
            return true;
        }

        /// <summary>
        /// Libère un lit d'hôpital
        /// </summary>
        public void ReleaseBed(string patientId)
        {
            if (patientAssignments.TryGetValue(patientId, out int hospitalIndex))
            {
                hospitals[hospitalIndex].availableBeds++;
                patientAssignments.Remove(patientId);
            }
        }

        /// <summary>
        /// Obtient tous les hôpitaux disponibles
        /// </summary>
        public List<HospitalData> GetAvailableHospitals()
        {
            return hospitals.Where(h => h.availableBeds > 0).ToList();
        }

        /// <summary>
        /// Met à jour le statut des hôpitaux (simulation)
        /// </summary>
        private void UpdateHospitalStatus()
        {
            foreach (var hospital in hospitals)
            {
                // Simuler des variations de capacité
                if (Random.value < 0.3f)
                {
                    int change = Random.Range(-2, 3);
                    hospital.availableBeds = Mathf.Clamp(
                        hospital.availableBeds + change,
                        0,
                        hospital.totalBeds
                    );
                }

                // Mettre à jour le temps d'attente
                hospital.estimatedWaitTime = Mathf.Max(5, hospital.estimatedWaitTime + Random.Range(-5, 6));
            }

            Debug.Log("[HospitalSystem] Statut des hôpitaux mis à jour");
        }

        /// <summary>
        /// Obtient un résumé de la capacité hospitalière
        /// </summary>
        public string GetCapacitySummary()
        {
            int totalBeds = hospitals.Sum(h => h.totalBeds);
            int availableBeds = hospitals.Sum(h => h.availableBeds);
            
            return $"Capacité totale: {availableBeds}/{totalBeds} lits disponibles\n" +
                   $"Hôpitaux actifs: {hospitals.Count(h => h.availableBeds > 0)}/{hospitals.Count}";
        }
    }

    /// <summary>
    /// Données d'un hôpital
    /// </summary>
    [System.Serializable]
    public class HospitalData
    {
        public string hospitalId;
        public string hospitalName;
        public HospitalType hospitalType;
        public float distanceKm;
        
        [Header("Services")]
        public bool hasTraumaCenter;
        public bool hasBurnUnit;
        public bool hasCardiology;
        public bool hasNeurology;
        public bool hasPediatrics;
        
        [Header("Capacité")]
        public int availableBeds;
        public int totalBeds;
        public EmergencyCapacity emergencyCapacity;
        public int estimatedWaitTime; // en minutes

        [Header("Position")]
        public Vector3 position;
        public string address;
    }

    public enum HospitalType
    {
        CHU,            // Centre Hospitalier Universitaire
        GeneralHospital,// Hôpital général
        Clinic,         // Clinique
        FieldHospital   // Hôpital de campagne
    }

    public enum EmergencyCapacity
    {
        Low,
        Medium,
        High,
        Critical
    }
}
