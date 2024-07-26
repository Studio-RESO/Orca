namespace App.Common.Scripts
{
    internal interface IApplicationProvider
    {
        public BuildInformation BuildInfo { get; }
        
        public ISceneHandler SceneHandler { get; }
        
        public AudioSubsystem AudioSubsystem { get; }
    }
}
