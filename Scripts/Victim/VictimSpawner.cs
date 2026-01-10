using UnityEngine;
using System.Collections.Generic;

namespace RASSE.Core
{
    /// <summary>
    /// VictimSpawner - Génère les victimes sur la scène
    /// </summary>
    public class VictimSpawner : MonoBehaviour
    {
        [Header("=== CONFIGURATION ===")]
        [SerializeField] private GameObject victimPrefab;
        [SerializeField] private Transform spawnParent;
        
        [Header("=== ZONE DE SPAWN ===")]
        [SerializeField] private Vector3 spawnAreaCenter;
        [SerializeField] private Vector3 spawnAreaSize = new Vector3(20f, 0f, 20f);
        [SerializeField] private float minDistanceBetweenVictims = 2f;
        
        [Header("=== POINTS DE SPAWN PRÉDÉFINIS ===")]
        [SerializeField] private Transform[] predefinedSpawnPoints;
        [SerializeField] private bool usePredefinedPoints = false;

        private List<Vector3> usedPositions = new List<Vector3>();

        /// <summary>
        /// Génère un nombre spécifié de victimes
        /// </summary>
        public List<VictimController> SpawnVictims(int count, DifficultyLevel difficulty)
        {
            List<VictimController> victims = new List<VictimController>();
            usedPositions.Clear();

            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPosition = GetSpawnPosition(i);
                VictimController victim = SpawnVictim(spawnPosition, i, difficulty);
                
                if (victim != null)
                {
                    victims.Add(victim);
                    usedPositions.Add(spawnPosition);
                }
            }

            Debug.Log($"[VictimSpawner] {victims.Count} victimes générées");
            return victims;
        }

        /// <summary>
        /// Génère une victime à une position donnée
        /// </summary>
        private VictimController SpawnVictim(Vector3 position, int number, DifficultyLevel difficulty)
        {
            if (victimPrefab == null)
            {
                Debug.LogError("[VictimSpawner] Prefab de victime non défini!");
                return null;
            }

            GameObject victimObj = Instantiate(victimPrefab, position, Quaternion.identity, spawnParent);
            victimObj.name = $"Victim_{number:D3}";

            VictimController victim = victimObj.GetComponent<VictimController>();
            if (victim == null)
            {
                victim = victimObj.AddComponent<VictimController>();
            }

            victim.InitializeRandom(number, difficulty);

            return victim;
        }

        /// <summary>
        /// Obtient une position de spawn valide
        /// </summary>
        private Vector3 GetSpawnPosition(int index)
        {
            if (usePredefinedPoints && predefinedSpawnPoints != null && 
                index < predefinedSpawnPoints.Length && predefinedSpawnPoints[index] != null)
            {
                return predefinedSpawnPoints[index].position;
            }

            // Génération aléatoire dans la zone
            int maxAttempts = 50;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector3 randomPosition = spawnAreaCenter + new Vector3(
                    Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                    0f,
                    Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
                );

                // Vérifier la distance avec les autres victimes
                if (IsPositionValid(randomPosition))
                {
                    // Raycast pour trouver le sol
                    if (Physics.Raycast(randomPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
                    {
                        return hit.point;
                    }
                    return randomPosition;
                }
            }

            // Fallback: position dans la zone
            return spawnAreaCenter + new Vector3(
                Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                0f,
                Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
            );
        }

        private bool IsPositionValid(Vector3 position)
        {
            foreach (Vector3 usedPos in usedPositions)
            {
                if (Vector3.Distance(position, usedPos) < minDistanceBetweenVictims)
                {
                    return false;
                }
            }
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            // Dessiner la zone de spawn
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawCube(spawnAreaCenter, spawnAreaSize);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);
        }
    }
}
