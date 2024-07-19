using System;
using R3;
using UnityEngine;

namespace App.Common.Scripts
{
    public class ApplicationOperator : IDisposable
    {
        public AudioSubsystem AudioSubsystem
        {
            get;
        }
        
        private readonly CompositeDisposable _disposables = new();
        
        public ApplicationOperator(ApplicationConfiguration appConfig)
        {
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
    }
}
