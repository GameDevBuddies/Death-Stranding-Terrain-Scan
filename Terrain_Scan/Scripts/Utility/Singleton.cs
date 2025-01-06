using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameDevBuddies
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance;
        private bool _isInitialized = false;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GetObjectOfType();
                    if (_instance == null)
                    {
                        Debug.LogError($"{typeof(T)}:: Singleton instance not found.");
                    }
                    else
                    {
                        Debug.LogWarning($"{typeof(T)}:: Singleton instance not initialized yet, " +
                            $"don't reference singletons in \"Awake\" or earlier. " +
                            $"Calling \"{nameof(Initialize)}\" now, might have unintended effects...");
                        if (!_instance._isInitialized)
                        {
                            _instance._isInitialized = true;
                            _instance.Initialize();
                        }
                    }
                }

                return _instance;
            }

        }

        protected virtual void Initialize()
        {
        }

        protected virtual void Cleanup()
        {
        }

        protected void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (!_isInitialized)
                {
                    _isInitialized = true;
                    Initialize();
                }
            }
            else if (_instance != this)
            {
                Debug.LogError($"{typeof(T)}:: Duplicate singleton instance! Destroying {typeof(T)} " +
                    $"on object [{name}] but object itself stays alive, leaving " +
                    $"object [{_instance.name}] intact.");
                Destroy(this);
            }
        }

        protected void OnDestroy()
        {
            if (_instance == this)
            {
                Cleanup();
                _instance = null;
                _isInitialized = false;
            }
        }

        private static T GetObjectOfType()
        {
            int numberOfScenes = SceneManager.sceneCount;

            for (int i = 0; i < numberOfScenes; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                GameObject[] rootObjects = scene.GetRootGameObjects();

                foreach (GameObject rootObject in rootObjects)
                {
                    T foundComponent = FindTypeInChildren(rootObject.transform);
                    if (foundComponent != null)
                    {
                        return foundComponent;
                    }
                }
            }

            return null;
        }

        private static T FindTypeInChildren(Transform transformObject)
        {
            if (transformObject.GetComponent<T>() != null)
            {
                return transformObject.GetComponent<T>();
            }

            foreach (Transform child in transformObject)
            {
                T component = FindTypeInChildren(child);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }
    }
}