using ContextSystem;

namespace Orca.Example
{
    /// <summary>
    /// アプリケーションのエントリーポイント
    /// </summary>
    public sealed class ApplicationEntryPoint : AbstractApplicationEntryPoint
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            
            ApplicationContext = new ApplicationContext();
        }
    }
}
