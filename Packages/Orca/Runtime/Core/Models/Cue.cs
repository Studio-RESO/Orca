using System;
using System.Collections.Generic;

namespace Orca
{
    [Serializable]
    public class Cue
    {
        public string id = Guid.NewGuid().ToString();
        public string name;
        public int categoryId;
        public PlayType playType = PlayType.Sequential;
        public List<Track> tracks = new List<Track>();
        public float volume = 1f;
        public float volumeRange = 0f;
        public float pitch = 1f;
        public float pitchRange = 0f;
    }
}