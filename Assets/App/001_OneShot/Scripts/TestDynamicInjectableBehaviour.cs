using System;
using ContextSystem;
using R3;
using UnityEngine;

namespace Orca.Example
{
    internal sealed class TestDynamicInjectableBehaviour : ContextInjectableBehaviour
    {
        private ApplicationContext ApplicationContext { get; set; }
        
        private string AppName => ApplicationContext.AppName;
        
        private OneShotSceneContext SceneContext { get; set; }

        private string SceneName => SceneContext.SceneName;
        
        private ReactiveProperty<int> Score => SceneContext.Score;
        
        private CompositeDisposable compositeDisposable { get; } = new CompositeDisposable();
        
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
            Debug.Log("TestDynamicInjectableBehaviour Awake");
        }

        private void OnDestroy()
        {
            compositeDisposable?.Dispose();
        }

        public void Initialize()
        {
            SceneContext.Score.Subscribe(OnChangeScore).AddTo(compositeDisposable);
        }

        private void OnChangeScore(int score)
        {
            Debug.Log($"Score : {Score.Value}");
        }
    }
}
