using System.Collections.Generic;
using UnityEngine;

namespace Orca
{
    [CreateAssetMenu(fileName = "AudioSystemSettings", menuName = "Orca/Settings")]
    public class SoundKitSettings : ScriptableObject
    {
        public List<Category> categories = new List<Category>();
        public int defaultPoolSize = 10;
        public ThrottleType defaultThrottleType = ThrottleType.PriorityOrder;
        public int defaultThrottleLimit = 32;
    }
}