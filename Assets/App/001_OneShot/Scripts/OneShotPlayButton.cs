using System.Threading;
using ContextSystem;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Orca.Example
{
    internal sealed class OneShotPlayButton : ContextInjectableBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private AudioCueSheetAsset audioCueSheetAsset;
        [SerializeField] private AudioCuePlayback audioCuePlayback;
        [SerializeField] private TestDynamicInjectableBehaviour testDynamicInjectableBehaviour;
        
        private ApplicationContext ApplicationContext { get; set; }
        
        private string AppName => ApplicationContext.AppName;
        
        private OneShotSceneContext SceneContext { get; set; }

        private string SceneName => SceneContext.SceneName;
        
        private ReactiveProperty<int> Score => SceneContext.Score;
        
        private CancellationToken _cancellationToken;
        
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
        
        private async void Awake()
        {
            _cancellationToken = new CancellationToken();
            
            button.onClick.AddListener(OnClick);
        }
        
        private async void Start()
        {
            // bool isLoaded = await audioCuePlayback.Setup(
            //     audioCueSheetAsset.audioCueSheet.audioCueList[0],
            //     _cancellationToken);
            //
            // if (isLoaded)
            // {
            //     Debug.Log("AudioCue loaded.");
            // }
            // else
            // {
            //     Debug.LogError("Failed to load AudioCue.");
            // }
            
        }
        
        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnClick);
        }
        
        private void OnClick()
        {
            ContextInjectableBehaviourFactory
                .Create(testDynamicInjectableBehaviour, transform, new IContext[] {ApplicationContext, SceneContext})
                .Initialize();
            
            Debug.Log(AppName);
            Debug.Log(SceneName);

            Score.Value++;

            // audioCuePlayback.Play();
        }
        
    }
}
