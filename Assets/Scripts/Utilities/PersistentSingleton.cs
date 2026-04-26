using UnityEngine;

namespace Utilities
{
    public class PersistentSingleton <T> : MonoBehaviour where T : Component
    {
        public bool autoUnparentOnAwake = true;

        protected static T _instance;
        
        protected static bool _applicationIsQuitting = false;
        
        public static bool HasInstance => _instance != null;

        public static T TryGetInstance() => HasInstance ? _instance : null;

        public static T Instance
        {
            get
            {
                // Prevent recreation during shutdown
                if (_applicationIsQuitting)
                    return null;
                
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<T>();
                    if (_instance == null)
                    {
                        GameObject go = new(typeof(T).Name);
                        _instance = go.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            InitializeSingleton();
        }

        protected virtual void InitializeSingleton()
        {
            if (!Application.isPlaying) return;

            if (autoUnparentOnAwake)
            {
                transform.SetParent(null);
            }

            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                if (_instance != this)
                    Destroy(gameObject);
            }
        }

        protected void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (_applicationIsQuitting)
                return;
            
            if (_instance == this)
                _instance = null;
        }
    }
}