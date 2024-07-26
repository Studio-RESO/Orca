using UnityEngine;

namespace App.Common.Scripts
{
    public class AudioSubsystem : ISubsystem
    {
        public void Initialize()
        {
            Debug.Log("Initialized Audio Subsystem.");
        }
        
        public void Shutdown()
        {
            Debug.Log("Shutdown Audio Subsystem.");
        }
    }
}
