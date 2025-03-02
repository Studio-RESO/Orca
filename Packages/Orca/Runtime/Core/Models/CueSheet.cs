using System;
using System.Collections.Generic;
using UnityEngine;

namespace Orca
{
    [CreateAssetMenu(fileName = "CueSheet", menuName = "Orca/CueSheet")]
    public class CueSheet : ScriptableObject
    {
        public string id = Guid.NewGuid().ToString();
        public string displayName;
        public List<Cue> cues = new List<Cue>();
        public float volume = 1f;
        public float pitch = 1f;
    }
}