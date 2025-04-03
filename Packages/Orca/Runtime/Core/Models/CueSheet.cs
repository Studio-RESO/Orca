using System;
using System.Collections.Generic;
using UnityEngine;

namespace Orca.Runtime.Core.Models
{
    [CreateAssetMenu(fileName = "CueSheet", menuName = "Orca/CueSheet")]
    public class CueSheet : ScriptableObject
    {
        public string id = Guid.NewGuid().ToString();
        public string DisplayName;
        public List<Cue> cues = new List<Cue>();
        public float Volume = 1f;
        public float Pitch = 1f;
    }
}
