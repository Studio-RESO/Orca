using System.Collections.Generic;
using UnityEngine;

namespace Orca.Runtime.Core.Models
{
    [CreateAssetMenu(fileName = "AudioSystemSettings", menuName = "Orca/Settings")]
    public class SoundKitSettings : ScriptableObject
    {
        public List<Category> Categories = new List<Category>();
        public int DefaultPoolSize = 10;
        public ThrottleType DefaultThrottleType = ThrottleType.PriorityOrder;
        public int DefaultThrottleLimit = 32;
    }
}
