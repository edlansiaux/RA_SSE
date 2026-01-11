using UnityEngine;

namespace RASSE.Core
{
    /// <summary>
    /// Classe de base pour implémenter le pattern Singleton dans Unity.
    /// Garantit qu'une seule instance du type T existe dans la scène.
    /// </summary>
    /// <typeparam name="T">Le type du composant singleton</typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        /// <summary>
        /// Accès à l'instance unique du singleton
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance de '{typeof(T)}' déjà détruite. Retourne null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Chercher une instance existante
                        _instance = FindObjectOfType<T>();

                        if (_instance == null)
                        {
                            // Créer un nouveau GameObject avec le composant
                            GameObject singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = $"[Singleton] {typeof(T)}";

                            // Persister entre les scènes
                            DontDestroyOnLoad(singletonObject);

                            Debug.Log($"[Singleton] Instance de '{typeof(T)}' créée automatiquement.");
                        }
                    }

                    return _instance;
                }
            }
        }

        /// <summary>
        /// Vérifie si une instance existe sans en créer une nouvelle
        /// </summary>
        public static bool HasInstance => _instance != null;

        /// <summary>
        /// Appelé à l'initialisation du composant
        /// </summary>
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
                OnSingletonAwake();
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[Singleton] Instance dupliquée de '{typeof(T)}' détruite sur {gameObject.name}");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Appelé après l'initialisation du singleton (à surcharger)
        /// </summary>
        protected virtual void OnSingletonAwake() { }

        /// <summary>
        /// Appelé lorsque l'application quitte
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }

        /// <summary>
        /// Appelé lors de la destruction du composant
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }

    /// <summary>
    /// Singleton persistant qui survit aux changements de scène.
    /// Utiliser pour les managers globaux (GameManager, AudioManager, etc.)
    /// </summary>
    public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();
            transform.SetParent(null);
        }
    }

    /// <summary>
    /// Singleton de scène qui est détruit lors du changement de scène.
    /// Utiliser pour les managers spécifiques à une scène.
    /// </summary>
    public abstract class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    
                    if (_instance == null)
                    {
                        Debug.LogError($"[SceneSingleton] Aucune instance de '{typeof(T)}' trouvée dans la scène!");
                    }
                }
                return _instance;
            }
        }

        public static bool HasInstance => _instance != null;

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                OnSceneSingletonAwake();
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[SceneSingleton] Instance dupliquée de '{typeof(T)}' détruite");
                Destroy(gameObject);
            }
        }

        protected virtual void OnSceneSingletonAwake() { }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
