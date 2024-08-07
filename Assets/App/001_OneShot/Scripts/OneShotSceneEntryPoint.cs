using ContextSystem;

namespace Orca.Example
{
    internal sealed class OneShotSceneEntryPoint : AbstractSceneEntryPoint<OneShotSceneContext>
    {
        private ApplicationContext AppContext => applicationEntryPoint.ApplicationContext;
        
        private string SceneName
        {
            get => SceneContext.SceneName;
            set => SceneContext.SceneName = value;
        }
        
        protected override void Awake()
        {
            base.Awake();

            SceneName = "OneShot";
        }

        protected override void InjectContexts(ContextInjectableBehaviour injectable)
        {
            injectable.InjectContext(AppContext);
            injectable.InjectContext(SceneContext);
        }
    }
}
