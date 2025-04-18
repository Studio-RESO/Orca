using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Asteria
{
    public class SceneLoader : IClearable
    {
        private readonly string prefix;
        private readonly IAddressableLoader addressableLoader;
        private readonly List<IDisposable> disposables = new();

        public SceneLoader(IAddressableLoader addressableLoader, string prefix)
        {
            Assert.IsNotNull(addressableLoader, "addressable scene loader is null.");

            this.addressableLoader = addressableLoader;
            this.prefix = prefix;
        }

        public virtual void Clear()
        {
            foreach (var disposable in disposables)
            {
                disposable?.Dispose();
            }
            disposables.Clear();
        }

        private string ToAddress(string assetName)
        {
            return $"{prefix}/{assetName.ToLower()}";
        }

        public async UniTask<Scene> LoadSceneAsync(string assetName, LoadSceneMode sceneMode, CancellationToken cancellationToken)
        {
            var address = ToAddress(assetName);

            var disposableAsset = await addressableLoader.LoadSceneAsync(address, sceneMode, cancellationToken);
            if (disposableAsset.InnerException != null)
            {
                // 意図的にキャンセルした場合は無視
                if (disposableAsset.InnerException is not OperationCanceledException)
                {
                    Debug.LogError(disposableAsset.InnerException);
                }

                return default;
            }

            disposables.Add(disposableAsset);

            return disposableAsset.Asset.Scene;
        }
    }
}
