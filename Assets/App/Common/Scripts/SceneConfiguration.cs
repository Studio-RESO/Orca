using UnityEngine;

namespace App.Common.Scripts
{
    [CreateAssetMenu(fileName = "SceneConfig", menuName = "ScriptableObjects/SceneConfig")]
    public class SceneConfiguration : ScriptableObject
    {
        public string Id;
        public string Name;
    }
}
