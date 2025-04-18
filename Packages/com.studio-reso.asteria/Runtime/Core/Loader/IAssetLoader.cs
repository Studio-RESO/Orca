using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Asteria
{
    public interface IClearable
    {
        void Clear();
    }

    public interface IAddressableLoader
    {
        UniTask<IDisposableAsset<T>> LoadAsync<T>(string address, CancellationToken cancellationToken = default) where T : Object;
        UniTask<IDisposableAsset<SceneInstance>> LoadSceneAsync(string address, LoadSceneMode loadMode, CancellationToken cancellationToken = default);
        bool IsExist(string address);
    }

    public interface IAssetLoader<T> where T : Object
    {
        /// <summary>
        /// AssetNameを指定してアセットをロードする
        /// Dispose処理はLoader自体で管理
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        UniTask<T> LoadAssetAsync(string assetName, CancellationToken cancellationToken = default);

        /// <summary>
        /// AssetNameを指定してアセットをロードする
        /// 呼び出し側でDispose処理を行う必要がある
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        UniTask<IDisposableAsset<T>> LoadDisposableAssetAsync(string assetName, CancellationToken cancellationToken = default);

        /// <summary>
        /// 指定したアドレスが存在するか確認する
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        bool IsExist(string assetName);
    }
}
