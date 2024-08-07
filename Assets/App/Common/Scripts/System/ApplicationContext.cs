using ContextSystem;

namespace Orca.Example
{
    /// <summary>
    /// アプリケーションコンテキスト
    /// </summary>
    public class ApplicationContext : IApplicationContext
    {
        public const string AppName = "Orca";

        public AudioSystem AudioSystem { get; } = new AudioSystem();
    }
}
