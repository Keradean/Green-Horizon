using UnityEngine;

namespace Dennis.Manager
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance {get; private set;}
        
        protected virtual bool PersistAcrossScenes => true;
        ////////////////////////////////////////////////////////////////////////////////////////////////
        protected virtual void Awake()
        {
            if (!Instance)
            {
                Instance = this as T;
                if (!PersistAcrossScenes) return;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}