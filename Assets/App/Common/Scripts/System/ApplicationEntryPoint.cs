using ContextSystem;

namespace Orca.Example
{
    public sealed class ApplicationEntryPoint : AbstractApplicationEntryPoint
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            
            ApplicationContext = new ApplicationContext();
        }
    }
}
