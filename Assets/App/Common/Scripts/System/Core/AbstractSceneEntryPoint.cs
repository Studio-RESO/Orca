using System;
using Orca.Example;
using UnityEngine;

namespace ContextSystem
{
    /// <summary>
    /// シーンのエントリーポイントの抽象クラス
    /// </summary>
    /// <typeparam name="T">SceneContextの型</typeparam>
    [DefaultExecutionOrder(-Int32.MaxValue + 1)]
    public abstract class AbstractSceneEntryPoint<T> : AbstractEntryPoint where T : ISceneContext, new()
    {
        [SerializeField] protected AbstractApplicationEntryPoint applicationEntryPoint;
        protected T SceneContext { get; private set; }

        protected virtual void Awake()
        {
            SceneContext = new T();
            
            var injectables = GetComponentsByType<ContextInjectableBehaviour>(FindObjectsSortMode.None);
            
            foreach (var injectable in injectables)
            {
                // injectable.InjectContext<ApplicationContext>(applicationEntryPoint.ApplicationContext);
                // injectable.InjectContext<T>(SceneContext);
                InjectContexts(injectable);
            }
        }
        
        protected abstract void InjectContexts(ContextInjectableBehaviour injectable);
    }
}
