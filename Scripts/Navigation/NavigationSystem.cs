using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

namespace RASSE.Core
{
    /// <summary>
    /// NavigationSystem - Gère le guidage AR vers les destinations
    /// Utilise NavMesh pour le pathfinding et affiche le chemin en AR
    /// </summary>
    public class NavigationSystem : MonoBehaviour
    {
        [Header("=== CONFIGURATION ===")]
        [SerializeField] private float pathUpdateInterval = 0.5f;
        [SerializeField] private float waypointThreshold = 2f;
        [SerializeField] private float arrivalThreshold = 1.5f;

        [Header("=== AFFICHAGE CHEMIN ===")]
        [SerializeField] private LineRenderer pathLineRenderer;
        [SerializeField] private GameObject waypointMarkerPrefab;
        [SerializeField] private Color pathColor = new Color(0, 0.8f, 1f, 0.8f);
        [SerializeField] private float pathWidth = 0.3f;
        [SerializeField] private float pathHeight = 0.5f;

        [Header("=== MARQUEURS ===")]
        [SerializeField] private GameObject destinationMarkerPrefab;
        [SerializeField] private float markerPulseSpeed = 2f;

        [Header("=== AUDIO ===")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip directionChangeSound;
        [SerializeField] private AudioClip arrivalSound;
        [SerializeField] private AudioClip waypointSound;

        // État de la navigation
        public bool IsNavigating { get; private set; }
        public Transform CurrentDestination { get; private set; }
        public string DestinationName { get; private set; }
        public float DistanceToDestination { get; private set; }
        public float EstimatedTime { get; private set; }

        // Chemin
        private NavMeshPath currentPath;
        private List<Vector3> pathPoints = new List<Vector3>();
        private int currentWaypointIndex;
        private List<GameObject> waypointMarkers = new List<GameObject>();
        private GameObject destinationMarker;
        private Coroutine navigationCoroutine;

        // Directions
        public NavigationDirection CurrentDirection { get; private set; }
        private NavigationDirection lastDirection;

        private void Awake()
        {
            currentPath = new NavMeshPath();
            SetupLineRenderer();
        }

        private void SetupLineRenderer()
        {
            if (pathLineRenderer == null)
            {
                GameObject lineObj = new GameObject("PathLine");
                lineObj.transform.SetParent(transform);
                pathLineRenderer = lineObj.AddComponent<LineRenderer>();
            }

            pathLineRenderer.startWidth = pathWidth;
            pathLineRenderer.endWidth = pathWidth;
            pathLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            pathLineRenderer.startColor = pathColor;
            pathLineRenderer.endColor = pathColor;
            pathLineRenderer.positionCount = 0;
        }

        /// <summary>
        /// Démarre la navigation vers une destination
        /// </summary>
        public void StartNavigation(Transform destination, string name)
        {
            if (destination == null)
            {
                Debug.LogWarning("[Navigation] Destination null!");
                return;
            }

            CurrentDestination = destination;
            DestinationName = name;
            IsNavigating = true;
            currentWaypointIndex = 0;

            // Créer le marqueur de destination
            CreateDestinationMarker();

            // Calculer le chemin initial
            CalculatePath();

            // Démarrer la coroutine de mise à jour
            if (navigationCoroutine != null)
                StopCoroutine(navigationCoroutine);
            navigationCoroutine = StartCoroutine(NavigationUpdate());

            Debug.Log($"[Navigation] Navigation démarrée vers {name}");
        }

        /// <summary>
        /// Arrête la navigation
        /// </summary>
        public void StopNavigation()
        {
            IsNavigating = false;
            CurrentDestination = null;
            DestinationName = "";

            if (navigationCoroutine != null)
            {
                StopCoroutine(navigationCoroutine);
                navigationCoroutine = null;
            }

            ClearPathVisualization();
            DestroyDestinationMarker();

            Debug.Log("[Navigation] Navigation arrêtée");
        }

        /// <summary>
        /// Coroutine de mise à jour de la navigation
        /// </summary>
        private IEnumerator NavigationUpdate()
        {
            while (IsNavigating && CurrentDestination != null)
            {
                // Recalculer le chemin périodiquement
                CalculatePath();

                // Mettre à jour la distance
                DistanceToDestination = CalculatePathDistance();
                EstimatedTime = DistanceToDestination / 5f; // Vitesse de marche ~5m/s

                // Vérifier l'arrivée
                if (DistanceToDestination < arrivalThreshold)
                {
                    OnArrival();
                    yield break;
                }

                // Mettre à jour la direction
                UpdateDirection();

                // Mettre à jour la visualisation
                UpdatePathVisualization();

                yield return new WaitForSeconds(pathUpdateInterval);
            }
        }

        /// <summary>
        /// Calcule le chemin NavMesh vers la destination
        /// </summary>
        private void CalculatePath()
        {
            if (CurrentDestination == null) return;

            Vector3 startPos = transform.position;
            Vector3 endPos = CurrentDestination.position;

            // Trouver les points NavMesh les plus proches
            NavMeshHit startHit, endHit;
            if (!NavMesh.SamplePosition(startPos, out startHit, 10f, NavMesh.AllAreas) ||
                !NavMesh.SamplePosition(endPos, out endHit, 10f, NavMesh.AllAreas))
            {
                // Fallback: ligne directe
                pathPoints.Clear();
                pathPoints.Add(startPos);
                pathPoints.Add(endPos);
                return;
            }

            // Calculer le chemin
            if (NavMesh.CalculatePath(startHit.position, endHit.position, NavMesh.AllAreas, currentPath))
            {
                pathPoints.Clear();
                pathPoints.AddRange(currentPath.corners);
            }
        }

        /// <summary>
        /// Calcule la distance totale du chemin
        /// </summary>
        private float CalculatePathDistance()
        {
            float distance = 0f;
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                distance += Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
            }
            return distance;
        }

        /// <summary>
        /// Met à jour la direction à suivre
        /// </summary>
        private void UpdateDirection()
        {
            if (pathPoints.Count < 2) return;

            // Trouver le prochain waypoint
            while (currentWaypointIndex < pathPoints.Count - 1)
            {
                if (Vector3.Distance(transform.position, pathPoints[currentWaypointIndex]) > waypointThreshold)
                    break;
                currentWaypointIndex++;
                PlaySound(waypointSound);
            }

            Vector3 targetPoint = pathPoints[Mathf.Min(currentWaypointIndex, pathPoints.Count - 1)];
            Vector3 direction = targetPoint - transform.position;
            direction.y = 0;

            float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);

            // Déterminer la direction
            if (Mathf.Abs(angle) < 20f)
                CurrentDirection = NavigationDirection.Straight;
            else if (angle > 60f)
                CurrentDirection = NavigationDirection.SharpRight;
            else if (angle > 20f)
                CurrentDirection = NavigationDirection.Right;
            else if (angle < -60f)
                CurrentDirection = NavigationDirection.SharpLeft;
            else
                CurrentDirection = NavigationDirection.Left;

            // Jouer un son si la direction change
            if (CurrentDirection != lastDirection)
            {
                lastDirection = CurrentDirection;
                PlaySound(directionChangeSound);
            }
        }

        /// <summary>
        /// Met à jour la visualisation du chemin
        /// </summary>
        private void UpdatePathVisualization()
        {
            if (pathLineRenderer == null || pathPoints.Count == 0) return;

            // Mettre à jour le LineRenderer
            Vector3[] elevatedPoints = new Vector3[pathPoints.Count];
            for (int i = 0; i < pathPoints.Count; i++)
            {
                elevatedPoints[i] = pathPoints[i] + Vector3.up * pathHeight;
            }

            pathLineRenderer.positionCount = elevatedPoints.Length;
            pathLineRenderer.SetPositions(elevatedPoints);

            // Mettre à jour les marqueurs de waypoint
            UpdateWaypointMarkers();

            // Animer le marqueur de destination
            if (destinationMarker != null)
            {
                float scale = 1f + 0.2f * Mathf.Sin(Time.time * markerPulseSpeed);
                destinationMarker.transform.localScale = Vector3.one * scale;
            }
        }

        /// <summary>
        /// Met à jour les marqueurs de waypoint
        /// </summary>
        private void UpdateWaypointMarkers()
        {
            if (waypointMarkerPrefab == null) return;

            // Supprimer les anciens marqueurs
            foreach (var marker in waypointMarkers)
            {
                if (marker != null)
                    Destroy(marker);
            }
            waypointMarkers.Clear();

            // Créer les nouveaux marqueurs pour les corners importants
            for (int i = currentWaypointIndex; i < pathPoints.Count - 1; i++)
            {
                Vector3 markerPos = pathPoints[i] + Vector3.up * 0.5f;
                GameObject marker = Instantiate(waypointMarkerPrefab, markerPos, Quaternion.identity);
                marker.transform.SetParent(transform);
                waypointMarkers.Add(marker);
            }
        }

        /// <summary>
        /// Crée le marqueur de destination
        /// </summary>
        private void CreateDestinationMarker()
        {
            DestroyDestinationMarker();

            if (destinationMarkerPrefab != null && CurrentDestination != null)
            {
                Vector3 markerPos = CurrentDestination.position + Vector3.up * 2f;
                destinationMarker = Instantiate(destinationMarkerPrefab, markerPos, Quaternion.identity);
            }
        }

        /// <summary>
        /// Détruit le marqueur de destination
        /// </summary>
        private void DestroyDestinationMarker()
        {
            if (destinationMarker != null)
            {
                Destroy(destinationMarker);
                destinationMarker = null;
            }
        }

        /// <summary>
        /// Efface la visualisation du chemin
        /// </summary>
        private void ClearPathVisualization()
        {
            if (pathLineRenderer != null)
                pathLineRenderer.positionCount = 0;

            foreach (var marker in waypointMarkers)
            {
                if (marker != null)
                    Destroy(marker);
            }
            waypointMarkers.Clear();

            pathPoints.Clear();
        }

        /// <summary>
        /// Appelé à l'arrivée à destination
        /// </summary>
        private void OnArrival()
        {
            Debug.Log($"[Navigation] Arrivée à {DestinationName}");
            
            PlaySound(arrivalSound);

            // Notifier l'interface AR
            GameManager.Instance?.arInterface?.ShowAlert(
                $"Arrivée: {DestinationName}", Color.green, 3f);

            StopNavigation();
        }

        /// <summary>
        /// Joue un son
        /// </summary>
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// Obtient la direction en texte
        /// </summary>
        public string GetDirectionText()
        {
            return CurrentDirection switch
            {
                NavigationDirection.Straight => "↑ TOUT DROIT",
                NavigationDirection.Left => "← GAUCHE",
                NavigationDirection.Right => "→ DROITE",
                NavigationDirection.SharpLeft => "↰ FORTE GAUCHE",
                NavigationDirection.SharpRight => "↱ FORTE DROITE",
                NavigationDirection.UTurn => "↺ DEMI-TOUR",
                _ => ""
            };
        }

        /// <summary>
        /// Obtient les informations de navigation formatées
        /// </summary>
        public string GetNavigationInfo()
        {
            if (!IsNavigating) return "";

            return $"Destination: {DestinationName}\n" +
                   $"Distance: {DistanceToDestination:F0}m\n" +
                   $"Temps estimé: {EstimatedTime:F0}s\n" +
                   $"Direction: {GetDirectionText()}";
        }

        private void OnDrawGizmosSelected()
        {
            // Dessiner le chemin dans l'éditeur
            if (pathPoints.Count > 1)
            {
                Gizmos.color = pathColor;
                for (int i = 0; i < pathPoints.Count - 1; i++)
                {
                    Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
                }
            }
        }
    }

    public enum NavigationDirection
    {
        None,
        Straight,
        Left,
        Right,
        SharpLeft,
        SharpRight,
        UTurn
    }
}
