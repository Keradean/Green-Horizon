using UnityEngine;

namespace Extra
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
                if(PersistAcrossScenes) DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}