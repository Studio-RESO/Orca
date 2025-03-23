using System;
using System.Collections.Generic;

namespace Orca.Runtime.Core.Models
{
    [Serializable]
    public class Cue
    {
        public string Id = Guid.NewGuid().ToString();
        public string Name;
        public int CategoryId;
        public PlayType PlayType = PlayType.Sequential;
        public List<Track> Tracks = new List<Track>();
        public float Volume = 1f;
        public float VolumeRange = 0f;
        public float Pitch = 1f;
        public float PitchRange = 0f;
    }
}
