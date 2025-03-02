using System;
using UnityEngine.Audio;

namespace Orca
{
    [Serializable]
    public class Category
    {
        public string name;
        public int id;
        public AudioMixerGroup mixerGroup;
        public ThrottleType throttleType = ThrottleType.PriorityOrder;
        public int throttleLimit = 8;
    }
}