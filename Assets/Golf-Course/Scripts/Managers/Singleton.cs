using UnityEngine;

namespace Golf_Course.Scripts.Managers
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new();
        private static bool _applicationIsQuitting;
        protected virtual bool DontDestroyOnLoadEnabled => false;

        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) + "' is already destroyed. Returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = (T)FindObjectOfType(typeof(T));

                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {
                            Debug.LogError(
                                "[Singleton] There should never be more than one singleton instance! Destroying additional instances.");
                            return _instance;
                        }

                        if (_instance == null)
                        {
                            var singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = typeof(T) + " (Manager)";
                            
                            if ((singletonObject.GetComponent<T>() as Singleton<T>)!.DontDestroyOnLoadEnabled)
                            {
                                DontDestroyOnLoad(singletonObject);
                            }
                        }
                        else
                        {
                            Debug.Log("[Singleton] Using already created instance: " + _instance.gameObject.name);
                        }
                    }

                    return _instance;
                }
            }
        }
        
        public static void DestroyInstance()
        {
            if (_instance == null)
            {
                return;
            }

            Destroy(_instance.gameObject);
            _instance = null;
            _applicationIsQuitting = false;
        }

        public virtual void OnDestroy()
        {
            _applicationIsQuitting = true;
        }
    }
}