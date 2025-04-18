using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Asteria
{
    public class AssetLoader<T> : IClearable, IAssetLoader<T> where T : Object
    {
        private readonly string prefix;
        private readonly IAddressableLoader addressableLoader;
        private readonly List<IDisposable> disposables = new();

        public AssetLoader(IAddressableLoader addressableLoader, string prefix)
        {
            Assert.IsNotNull(addressableLoader, "addressable loader is null.");

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

        protected string ToAddress(string assetName)
        {
            return $"{prefix}/{assetName.ToLower()}";
        }

        public virtual async UniTask<T> LoadAssetAsync(string assetName, CancellationToken cancellationToken = default)
        {
            var address = ToAddress(assetName);

            var disposableAsset = await addressableLoader.LoadAsync<T>(address, cancellationToken);
            if (disposableAsset.InnerException != null)
            {
                // 意図的にキャンセルした場合は無視
                if (disposableAsset.InnerException is not OperationCanceledException)
                {
                    Debug.LogError(disposableAsset.InnerException);
                }

                return null;
            }

            disposables.Add(disposableAsset);

            return disposableAsset.Asset;
        }

        public UniTask<IDisposableAsset<T>> LoadDisposableAssetAsync(string assetName, CancellationToken cancellationToken = default)
        {
            var address = ToAddress(assetName);

            return addressableLoader.LoadAsync<T>(address, cancellationToken);
        }
        
        public bool IsExist(string assetName)
        {
            var address = ToAddress(assetName);
            return addressableLoader.IsExist(address);
        }
    }
}
