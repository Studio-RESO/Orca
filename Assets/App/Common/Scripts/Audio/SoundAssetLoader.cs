using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SoundAssetLoader
{
    public async UniTask<AudioClip> LoadFromResourcesAsync(string path, CancellationToken cancellationToken = default)
    {
        var request = Resources.LoadAsync<AudioClip>(path);
        
        await UniTask.WaitUntil(() => request.isDone, cancellationToken: cancellationToken);
        
        AudioClip clip = request.asset as AudioClip;
        
        if (clip == null)
        {
            Debug.LogError($"Failed to load AudioClip from Resources : {path}");
            return null;
        }
        else
        {
            return clip;
        }
    }
    
    public async UniTask<AudioClip> LoadFromAddressableAsync(string path, CancellationToken cancellationToken = default)
    {
        AsyncOperationHandle<AudioClip> handle = Addressables.LoadAssetAsync<AudioClip>(path);
        
        await handle.ToUniTask(cancellationToken: cancellationToken);
        
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result;
        }
        else
        {
            Debug.LogError($"Audio clip at path {path} not found in Addressables.");
            return null;
        }
    }
}
