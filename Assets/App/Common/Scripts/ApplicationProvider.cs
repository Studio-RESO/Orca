namespace App.Common.Scripts
{
    public interface IApplicationProvider
    {
        public BuildInformation BuildInfo { get; }
        
        public ISceneHandler SceneHandler { get; }
        
        public AudioSubsystem AudioSubsystem { get; }
    }
}
