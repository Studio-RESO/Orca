using UnityEngine;

namespace App.Common.Scripts
{
    public abstract class BaseScene : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] protected SceneConfiguration sceneConfig;
        
        [SerializeField] protected Canvas hudRoot;
        
        protected GameMode GameMode { get; set; }
        
        protected virtual void Enter()
        {
            Debug.Log($"Scene - Enter : {sceneConfig.Name}");
        }
        
        protected virtual void Leave()
        {
            Debug.Log($"Scene - Leave : {sceneConfig.Name}");
        }
        
        protected virtual void ApplicationFocus()
        {
            Debug.Log($"Scene - App Focus : {sceneConfig.Name}");
        }
        
        protected virtual void ApplicationPause()
        {
            Debug.Log($"Scene - App Pause : {sceneConfig.Name}");
        }
        
        protected virtual void ApplicationQuit()
        {
            Debug.Log($"Scene - App Quit : {sceneConfig.Name}");
        }
    }
}
