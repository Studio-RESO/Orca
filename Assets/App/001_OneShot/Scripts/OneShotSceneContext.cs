using ContextSystem;
using R3;

namespace Orca.Example
{
    public class OneShotSceneContext : ISceneContext
    {
        public string SceneName { get; set; }

        public ReactiveProperty<int> Score { get; set; } = new ReactiveProperty<int>(0);
        
        private CompositeDisposable compositeDisposable;
        
        public void Dispose()
        {
            compositeDisposable?.Dispose();
        }
    }
}
