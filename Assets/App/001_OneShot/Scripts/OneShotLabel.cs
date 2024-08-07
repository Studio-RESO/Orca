using ContextSystem;
using TMPro;
using UnityEngine;
using R3;

namespace Orca.Example
{
    public class OneShotLabel : ContextInjectableBehaviour
    {
        [SerializeField] private TMP_Text label;
        
        private ApplicationContext ApplicationContext { get; set; }
        
        private string AppName => ApplicationContext.AppName;
        
        private OneShotSceneContext SceneContext { get; set; }

        private string SceneName => SceneContext.SceneName;
        
        private ReactiveProperty<int> Score => SceneContext.Score;
        
        public override void InjectContext<T>(T context)
        {
            if (context is ApplicationContext appContext)
            {
                ApplicationContext = appContext;
            }
            else if (context is OneShotSceneContext sceneContext)
            {
                SceneContext = sceneContext;
            }
            else
            {
                Debug.LogError($"Failed to inject context: {context.GetType().Name}");
            }
        }

        private void Awake()
        {
            SceneContext.Score.Subscribe(OnChangeScore);
        }
        
        private void OnChangeScore(int score)
        {
            label.text = $"Score : {score}";
            Debug.Log($"Score : {Score.Value}");
        }
    }
}
