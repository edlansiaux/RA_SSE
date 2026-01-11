using System;
using System.Collections.Generic;
using UnityEngine;

namespace RASSE.Core
{
    /// <summary>
    /// Système d'événements global pour la communication découplée entre composants.
    /// Permet de souscrire et publier des événements typés sans couplage direct.
    /// </summary>
    public class EventManager : PersistentSingleton<EventManager>
    {
        // Dictionnaire des écouteurs par type d'événement
        private Dictionary<Type, List<Delegate>> eventListeners = new Dictionary<Type, List<Delegate>>();
        
        // File d'événements pour le traitement différé
        private Queue<Action> eventQueue = new Queue<Action>();
        private bool isProcessingQueue = false;
        
        #region Subscription Methods
        
        /// <summary>
        /// S'abonne à un type d'événement
        /// </summary>
        public void Subscribe<T>(Action<T> listener) where T : struct
        {
            Type eventType = typeof(T);
            
            if (!eventListeners.ContainsKey(eventType))
            {
                eventListeners[eventType] = new List<Delegate>();
            }
            
            if (!eventListeners[eventType].Contains(listener))
            {
                eventListeners[eventType].Add(listener);
            }
        }
        
        /// <summary>
        /// Se désabonne d'un type d'événement
        /// </summary>
        public void Unsubscribe<T>(Action<T> listener) where T : struct
        {
            Type eventType = typeof(T);
            
            if (eventListeners.ContainsKey(eventType))
            {
                eventListeners[eventType].Remove(listener);
            }
        }
        
        /// <summary>
        /// S'abonne à un événement sans paramètre
        /// </summary>
        public void Subscribe<T>(Action listener) where T : struct
        {
            Subscribe<T>(_ => listener());
        }
        
        #endregion
        
        #region Publishing Methods
        
        /// <summary>
        /// Publie un événement immédiatement
        /// </summary>
        public void Publish<T>(T eventData) where T : struct
        {
            Type eventType = typeof(T);
            
            if (eventListeners.ContainsKey(eventType))
            {
                // Copie de la liste pour éviter les modifications pendant l'itération
                var listeners = new List<Delegate>(eventListeners[eventType]);
                
                foreach (var listener in listeners)
                {
                    try
                    {
                        ((Action<T>)listener)?.Invoke(eventData);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EventManager] Erreur lors de la publication de {eventType.Name}: {ex.Message}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Publie un événement sans données
        /// </summary>
        public void Publish<T>() where T : struct, new()
        {
            Publish(new T());
        }
        
        /// <summary>
        /// Met un événement en file d'attente pour traitement différé
        /// </summary>
        public void QueueEvent<T>(T eventData) where T : struct
        {
            eventQueue.Enqueue(() => Publish(eventData));
        }
        
        #endregion
        
        #region Queue Processing
        
        private void Update()
        {
            ProcessEventQueue();
        }
        
        private void ProcessEventQueue()
        {
            if (isProcessingQueue) return;
            
            isProcessingQueue = true;
            
            int processedCount = 0;
            int maxEventsPerFrame = 10;
            
            while (eventQueue.Count > 0 && processedCount < maxEventsPerFrame)
            {
                var eventAction = eventQueue.Dequeue();
                try
                {
                    eventAction?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[EventManager] Erreur traitement file: {ex.Message}");
                }
                processedCount++;
            }
            
            isProcessingQueue = false;
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Supprime tous les écouteurs pour un type d'événement
        /// </summary>
        public void ClearListeners<T>() where T : struct
        {
            Type eventType = typeof(T);
            if (eventListeners.ContainsKey(eventType))
            {
                eventListeners[eventType].Clear();
            }
        }
        
        /// <summary>
        /// Supprime tous les écouteurs
        /// </summary>
        public void ClearAllListeners()
        {
            eventListeners.Clear();
            eventQueue.Clear();
        }
        
        /// <summary>
        /// Obtient le nombre d'écouteurs pour un type d'événement
        /// </summary>
        public int GetListenerCount<T>() where T : struct
        {
            Type eventType = typeof(T);
            return eventListeners.ContainsKey(eventType) ? eventListeners[eventType].Count : 0;
        }
        
        #endregion
        
        protected override void OnDestroy()
        {
            ClearAllListeners();
            base.OnDestroy();
        }
    }
    
    #region Event Definitions
    
    // ===== ÉVÉNEMENTS DE JEU =====
    
    /// <summary>Événement: Démarrage du scénario</summary>
    public struct ScenarioStartedEvent
    {
        public string ScenarioId;
        public string ScenarioName;
        public int MaxVictims;
    }
    
    /// <summary>Événement: Fin du scénario</summary>
    public struct ScenarioEndedEvent
    {
        public string ScenarioId;
        public float Duration;
        public int VictimsTriaged;
        public int VictimsEvacuated;
        public float Score;
    }
    
    /// <summary>Événement: Pause/Reprise</summary>
    public struct GamePausedEvent
    {
        public bool IsPaused;
    }
    
    // ===== ÉVÉNEMENTS DE TRIAGE =====
    
    /// <summary>Événement: Victime détectée</summary>
    public struct VictimDetectedEvent
    {
        public string VictimId;
        public Vector3 Position;
        public float ConfidenceLevel;
    }
    
    /// <summary>Événement: Triage effectué</summary>
    public struct TriageCompletedEvent
    {
        public string VictimId;
        public RASSE.Zones.TriageCategory AssignedCategory;
        public RASSE.Zones.TriageCategory? PreviousCategory;
        public float TriageDuration;
        public string TriagedBy;
    }
    
    /// <summary>Événement: Catégorie modifiée</summary>
    public struct CategoryChangedEvent
    {
        public string VictimId;
        public RASSE.Zones.TriageCategory OldCategory;
        public RASSE.Zones.TriageCategory NewCategory;
        public string Reason;
    }
    
    // ===== ÉVÉNEMENTS DE VICTIME =====
    
    /// <summary>Événement: État de victime changé</summary>
    public struct VictimStateChangedEvent
    {
        public string VictimId;
        public string PreviousState;
        public string NewState;
    }
    
    /// <summary>Événement: Victime décédée</summary>
    public struct VictimDeceasedEvent
    {
        public string VictimId;
        public string CauseOfDeath;
        public float TimeOfDeath;
    }
    
    /// <summary>Événement: Victime traitée</summary>
    public struct VictimTreatedEvent
    {
        public string VictimId;
        public string TreatmentType;
        public string EquipmentUsed;
    }
    
    // ===== ÉVÉNEMENTS DE ZONE =====
    
    /// <summary>Événement: Entrée dans zone de danger</summary>
    public struct DangerZoneEnteredEvent
    {
        public string EntityId;
        public string ZoneId;
        public RASSE.Environment.DangerType DangerType;
        public int DangerLevel;
    }
    
    /// <summary>Événement: Sortie de zone de danger</summary>
    public struct DangerZoneExitedEvent
    {
        public string EntityId;
        public string ZoneId;
    }
    
    /// <summary>Événement: Zone de triage pleine</summary>
    public struct TriageZoneFullEvent
    {
        public string ZoneId;
        public RASSE.Zones.TriageZoneType ZoneType;
        public int Capacity;
    }
    
    // ===== ÉVÉNEMENTS D'ÉVACUATION =====
    
    /// <summary>Événement: Ambulance dispatchée</summary>
    public struct AmbulanceDispatchedEvent
    {
        public string AmbulanceId;
        public string CallSign;
        public Vector3 Destination;
    }
    
    /// <summary>Événement: Patient chargé</summary>
    public struct PatientLoadedEvent
    {
        public string AmbulanceId;
        public string PatientId;
        public int CurrentLoad;
        public int MaxCapacity;
    }
    
    /// <summary>Événement: Transport terminé</summary>
    public struct TransportCompletedEvent
    {
        public string AmbulanceId;
        public string HospitalId;
        public int PatientsDelivered;
    }
    
    // ===== ÉVÉNEMENTS SYSTÈME =====
    
    /// <summary>Événement: Batterie faible</summary>
    public struct BatteryLowEvent
    {
        public float BatteryLevel;
        public float EstimatedTimeRemaining;
    }
    
    /// <summary>Événement: Connexion perdue</summary>
    public struct ConnectionLostEvent
    {
        public string ServiceName;
        public bool SwitchingToOffline;
    }
    
    /// <summary>Événement: Connexion rétablie</summary>
    public struct ConnectionRestoredEvent
    {
        public string ServiceName;
    }
    
    // ===== ÉVÉNEMENTS UI =====
    
    /// <summary>Événement: Notification à afficher</summary>
    public struct ShowNotificationEvent
    {
        public string Title;
        public string Message;
        public NotificationType Type;
        public float Duration;
    }
    
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error,
        Critical
    }
    
    /// <summary>Événement: Commande vocale reçue</summary>
    public struct VoiceCommandReceivedEvent
    {
        public string Command;
        public float Confidence;
        public bool Executed;
    }
    
    #endregion
}
