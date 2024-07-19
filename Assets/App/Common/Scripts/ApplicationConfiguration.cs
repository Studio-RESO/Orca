using UnityEngine;

namespace App.Common.Scripts
{
    [CreateAssetMenu(fileName = "AppConfig", menuName = "ScriptableObjects/AppConfig")]
    public sealed class ApplicationConfiguration : ScriptableObject
    {
        public string Id;
        public string Name = "Orca";
    }
}
