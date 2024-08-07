using ContextSystem;

namespace Orca.Example
{
    internal sealed class OneShotSceneEntryPoint : AbstractSceneEntryPoint<OneShotSceneContext>
    {
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
    }
}
