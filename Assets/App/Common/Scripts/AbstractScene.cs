using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace App.Common.Scripts
{
    public abstract class AbstractScene : MonoBehaviour
    {
        [SerializeField] public SceneConfiguration sceneConfig;
        [SerializeField] protected Canvas hudRoot;
        
        protected GameMode GameMode { get; set; }
        
        private IApplicationProvider _provider;
        
        public virtual void Open()
        {
            // Debug.Log($"Scene - Enter : {sceneConfig.sceneName}");
        }
        
        public abstract UniTask<bool> Load();
        public abstract UniTask Unload();
        
        public virtual void ApplicationFocus()
        {
            // Debug.Log($"Scene - App Focus : {sceneConfig.sceneName}");
        }
        
        public virtual void ApplicationPause()
        {
            // Debug.Log($"Scene - App Pause : {sceneConfig.sceneName}");
        }
        
        public virtual void ApplicationQuit()
        {
            // Debug.Log($"Scene - App Quit : {sceneConfig.sceneName}");
        }
    }
}
