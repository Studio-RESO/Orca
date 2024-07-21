using UnityEngine;

namespace App.Common.Scripts
{
    [CreateAssetMenu(fileName = "SceneConfig", menuName = "ScriptableObjects/SceneConfig")]
    public class SceneConfiguration : ScriptableObject
    {
        [Header("Common")]
        [SerializeField] public string sceneId;
        [SerializeField] public string sceneName;
    }
}
