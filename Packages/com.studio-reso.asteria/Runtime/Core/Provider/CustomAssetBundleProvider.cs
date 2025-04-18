using System;
using System.ComponentModel;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.Scripting;

namespace Asteria
{
    [Preserve]
    [DisplayName("Custom AssetBundle Provider")]
    public class CustomAssetBundleProvider : ResourceProviderBase
    {
        /// <summary>
        /// ProvideHandleに入っている情報が示すAssetBundleを読み込む処理
        /// </summary>
        public override void Provide(ProvideHandle providerInterface)
        {
            var res = new CustomAssetBundleResource(providerInterface);
            res.Load();
        }

        /// <summary>
        /// 読み込み結果としてAssetロード用のProviderに渡すための型を返却
        /// Assets from Bundles Providerを使う場合にはIAssetBundleResourceを指定
        /// </summary>
        public override Type GetDefaultType(IResourceLocation location)
        {
            return typeof(IAssetBundleResource);
        }

        /// <summary>
        /// 解放処理
        /// </summary>
        public override void Release(IResourceLocation location, object asset)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }
            if (asset == null)
            {
                return;
            }
            if (asset is CustomAssetBundleResource bundle)
            {
                bundle.Unload();
            }
        }
    }
}
