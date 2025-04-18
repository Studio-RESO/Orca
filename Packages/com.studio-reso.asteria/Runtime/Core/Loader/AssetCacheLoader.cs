using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Asteria
{
    public class AssetCacheLoader<T> : AssetLoader<T> where T : Object
    {
        private readonly Dictionary<string, T> assetCache = new();

        public AssetCacheLoader(IAddressableLoader addressableLoader, string prefix) : base(addressableLoader, prefix) { }

        public override void Clear()
        {
            base.Clear();
            assetCache.Clear();
        }

        public override async UniTask<T> LoadAssetAsync(string assetName, CancellationToken cancellationToken = default)
        {
            var address = ToAddress(assetName);

            if (assetCache.TryGetValue(address, out var asset))
            {
                return asset;
            }

            asset = await base.LoadAssetAsync(address, cancellationToken);
            if (asset == null)
            {
                return null;
            }

            assetCache[address] = asset;

            return asset;
        }
    }
}
