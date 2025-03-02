using System;
using UnityEngine;

namespace Orca
{
    [Serializable]
    public class Track
    {
        public string id = Guid.NewGuid().ToString();
        public string name;
        public AudioClip clip;
        public float volume = 1f;
        public float volumeRange = 0f;
        public float pitch = 1f;
        public float pitchRange = 0f;
        public bool loop = false;
        public int startSample = 0;
        public int endSample = 0;
        public int loopStartSample = 0;
        public int randomWeight = 1;
        public float fadeTime = 0.1f;
    }
}