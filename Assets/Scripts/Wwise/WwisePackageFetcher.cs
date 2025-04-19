using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Wwise
{
    public sealed class WwisePackageFetcher
    {
        private S3DataLoader s3DataLoader;

        public WwisePackageFetcher()
        {
            s3DataLoader = new S3DataLoader();
        }

        public async UniTask<bool> Fetch(string packageName)
        {
            var fileName = packageName + ".pck";
            var path = GetBasePath();
            
            var data = await s3DataLoader.Download(fileName);
            if (data == null)
            {
                return false;
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var dist = Path.Combine(GetBasePath(), fileName);
            await File.WriteAllBytesAsync(dist, data);
            
            return true;
        }

        public string GetBasePath()
        {
#if UNITY_IPHONE
            var basePath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
            return basePath.Substring(0, basePath.LastIndexOf('/')) + "/Documents/Audio/Packages";
#elif UNITY_ANDROID
            return Path.Combine(Application.persistentDataPath, "Audio", "Packages");
#elif UNITY_EDITOR
            return Path.Combine(Application.streamingAssetsPath, "Audio", "Packages");
#else
            return Path.Combine(Application.dataPath, "Audio", "Packages");
#endif
        }
    }
}
