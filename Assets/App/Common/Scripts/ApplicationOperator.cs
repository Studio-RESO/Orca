using System;
using R3;
using UnityEngine;

namespace App.Common.Scripts
{
    public class ApplicationOperator : IDisposable, IApplicationProvider
    {
        public ISceneHandler SceneHandler { get; }
        
        public AudioSubsystem AudioSubsystem { get; }
        
        private readonly CompositeDisposable _disposables = new();
        
        public ApplicationOperator(ApplicationConfiguration appConfig)
        {
            // build environment
            BuildInfo = new BuildInformation(appConfig.BuildNo, appConfig.BuildKey, appConfig.BuildBranch);
            
            // TODO: Sceneの切り替えとロードを実装する。そのためのハンドラが必要。
            SceneHandler = new SceneHandler(this);
            
            // TODO: AudioをSubsystemではなくAudioSystemとして実装する
            AudioSubsystem = new AudioSubsystem();
            AudioSubsystem.Initialize();
        }
        
        public void ApplicationFocus()
        {
            
        }
        
        public void ApplicationPause()
        {
            
        }
        
        public void ApplicationQuit()
        {
            Application.Quit();
        }
        
        
        public void Dispose()
        {
            _disposables.Dispose();
        }
        
        public BuildInformation BuildInfo { get; }
    }
}
