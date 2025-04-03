using System;
using UnityEngine.Audio;

namespace Orca.Runtime.Core.Models
{
    [Serializable]
    public class Category
    {
        public string Name;
        public int Id;
        public AudioMixerGroup MixerGroup;
        public ThrottleType ThrottleType = ThrottleType.PriorityOrder;
        public int ThrottleLimit = 8;
    }
}
