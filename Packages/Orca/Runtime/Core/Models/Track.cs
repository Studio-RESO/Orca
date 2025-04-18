using System;
using UnityEngine;

namespace Orca.Runtime.Core.Models
{
    [Serializable]
    public class Track
    {
        public string Id = Guid.NewGuid().ToString();
        public string Name;
        public AudioClip Clip;
        public float Volume = 1f;
        public float VolumeRange = 0f;
        public float Pitch = 1f;
        public float PitchRange = 0f;
        public bool Loop = false;
        public int StartSample = 0;
        public int EndSample = 0;
        public int LoopStartSample = 0;
        public int RandomWeight = 1;
        public float FadeTime = 0.1f;
    }
}
