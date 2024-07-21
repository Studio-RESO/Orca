using UnityEngine;

namespace App.Common.Scripts
{
    [CreateAssetMenu(fileName = "AppConfig", menuName = "ScriptableObjects/AppConfig")]
    public sealed class ApplicationConfiguration : ScriptableObject
    {
        
        
        [Header("Common")]
        
        [SerializeField] public string Id;
        
        [SerializeField] public string Name = "Orca";
        
        [SerializeField] private SceneDefines defaultScene;
        
        [SerializeField] private string buildNo;
        
        [SerializeField] private ClientBuildKey buildKey;
        
        [SerializeField] private string buildBranch;
        
        public SceneDefines DefaultScene
        {
            get => defaultScene;
#if UNITY_EDITOR
            set
            {
                defaultScene = value;
            }
#endif
        }
        
        public string BuildNo
        {
            get => buildNo;
#if UNITY_EDITOR
            set
            {
                buildNo = value;
            }
#endif
        }
        
        public ClientBuildKey BuildKey
        {
            get => buildKey;
#if UNITY_EDITOR
            set
            {
                buildKey = value;
            }
#endif
        }
        
        public string BuildBranch
        {
            get => buildBranch;
#if UNITY_EDITOR
            set
            {
                buildBranch = value;
            }
#endif
        }
    }
    
    public enum ClientBuildKey
    {
        Production = 0,
        Beta,
        Alpha,
        Develop,
        Daily,
    }
}
