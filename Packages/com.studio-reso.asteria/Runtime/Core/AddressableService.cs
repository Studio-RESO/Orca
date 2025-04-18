using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Asteria
{
    public class AddressableService : IAddressableLoader, IDisposable
    {
        private readonly AddressablesCatalog catalogEntity;
        private readonly CancellationTokenSource loadCancellationTokenSource = new();
        private readonly AddressablesCacheTraceService cacheTraceService;

        private float networkUnreachableReceiveTime;

        private readonly long cleanupThreshold = 1 * 1024 * 1024 * 1024; // キャッシュを整理する閾値 : 1GB

        public bool Initialized
        {
            get;
            private set;
        }

        public IContext Context
        {
            get;
            private set;
        }

        public IAddressablesCatalog Catalog => catalogEntity;

        public event Action<Exception> OnNetworkUnreachableException = delegate { };

        public AddressableService(IContext context, AddressablesCacheTraceService cacheTraceService)
        {
            Assert.IsNotNull(context, "context is null.");
            Assert.IsNotNull(cacheTraceService, "databaseOperator is null.");

            this.cacheTraceService = cacheTraceService;

            catalogEntity = new AddressablesCatalog(cacheTraceService);

            UpdateContext(context);

#if UNITY_EDITOR
            if (AddressablesUtility.IsPackedPlayMode())
#endif
            {
                // NOTE: RemoteのAssetBundleを使用するときはCustomCatalogProviderを追加する
                var rm = Addressables.ResourceManager;
                rm.ResourceProviders.Add(new CustomCatalogProvider(rm));
            }
            ResourceManager.ExceptionHandler += ExceptionCallBack;
        }

        public void Dispose()
        {
            ResourceManager.ExceptionHandler -= ExceptionCallBack;

            catalogEntity.Dispose();
            Context.Dispose();
        }

        private void ExceptionCallBack(AsyncOperationHandle handle, Exception e)
        {
            if (!Initialized)
            {
                return;
            }

            if (e is OperationCanceledException)
            {
                return;
            }

            if (e is SocketException { ErrorCode: 10051 })
            {
                OnNetworkUnreachableException(e);
            }
        }

        public void Cancel()
        {
            if (!loadCancellationTokenSource.IsCancellationRequested)
            {
                loadCancellationTokenSource.Cancel();
            }
            Context.WebRequest.Cancel();
        }

        public void UpdateContext(IContext newContext)
        {
            Assert.IsNotNull(newContext, "context is null.");

            Context?.Dispose();
            Context = newContext;
            InternalContext.Context = Context;
            InternalCaching.CachePath = Context.CachePath;
            
            if (!Directory.Exists(Context.CachePath))
            {
                Directory.CreateDirectory(Context.CachePath);
            }

            Initialized = false;
        }

        public async UniTask<bool> InitializeAsync(CancellationToken cancellationToken)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogError("No internet connection.");

                return false;
            }

            if (Initialized)
            {
                return true;
            }

            try
            {
                await Addressables.InitializeAsync().ToUniTask(cancellationToken: cancellationToken);

                catalogEntity.Unload();
                await catalogEntity.LoadAsync(Context.Remote.CatalogUrl, cancellationToken);

                foreach (var url in Context.Remote.SubCatalogUrls)
                {
                    await catalogEntity.LoadAsync(url, cancellationToken);
                }

                // キャッシュの整理
                RemoveStaleCacheEntries();
                RemoveExceededFiles();

                Initialized = true;

                return true;
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception e)
            {
                Debug.LogError($"AddressableService Initialize failed. {e}");
            }

            Initialized = false;

            return false;
        }

        public async UniTask LoadCatalogAsync(string url, CancellationToken cancellationToken)
        {
            if (!Initialized)
            {
                await InitializeAsync(cancellationToken);
            }
            Assert.IsTrue(Initialized, "AddressableService is not initialized.");

            await catalogEntity.LoadAsync(url, cancellationToken);
        }

        public async UniTask UpdateCatalogAsync(CancellationToken cancellationToken)
        {
            if (!Initialized)
            {
                await InitializeAsync(cancellationToken);
            }
            Assert.IsTrue(Initialized, "AddressableService is not initialized.");

            await catalogEntity.UpdateAsync(cancellationToken);
        }

        /// <summary>
        /// ダウンロード済みのAssetBundleを全て削除
        /// </summary>
        public async UniTask<bool> CacheClearAsync(CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.RunOnThreadPool(() =>
                {
                    var savePath = Context.CachePath;
                    if (Directory.Exists(savePath))
                    {
                        Directory.Delete(savePath, true);
                        Directory.CreateDirectory(savePath);
                    }
                }, cancellationToken: cancellationToken);

                return true;
            }
            catch (Exception e)
            {
                Debug.Log($"Cache clear failed. {e}");

                return false;
            }
        }

        /// <summary>
        /// アセットバンドルの読み込み履歴をDBに保存
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="bundleSize"></param>
        private void UpdateAssetBundleLoadingTrace(string filePath, long bundleSize)
        {
            var upsert = new AssetBundleLoadingTrace
            {
                FilePath = filePath,
                BundleSize = bundleSize,
                TimeStamp = DateTime.Now,
            };
            cacheTraceService.UpdateAccess(upsert);
        }

        /// <summary>
        /// キャッシュしたアセットバンドルの合計サイズをDBに保存
        /// </summary>
        /// <param name="bundleSize"></param>
        private void UpdateAssetBundleTotalCaches(long bundleSize)
        {
            var latestCachesInfo = cacheTraceService.GetTotalCachesLatest();

            if (latestCachesInfo == null)
            {
                var insert = new AssetBundleTotalCaches
                {
                    TotalBundleSize = bundleSize,
                    Count = 1,
                    CreatedAt = DateTime.Now,
                };
                cacheTraceService.InsertTotalCaches(insert);

                return;
            }

            latestCachesInfo.TotalBundleSize += bundleSize;
            latestCachesInfo.Count += 1;
            latestCachesInfo.UpdatedAt = DateTime.Now;

            cacheTraceService.UpdateTotalCaches(latestCachesInfo);
        }

        /// <summary>
        /// キャッシュ合計サイズが閾値を超えた場合の削除対象となるキャッシュ情報を抽出
        /// </summary>
        /// <param name="traces">キャッシュ情報</param>
        /// <param name="totalBundleSize">合計バンドルサイズ</param>
        /// <returns>抽出されたキャッシュ情報</returns>
        private IEnumerable<AssetBundleLoadingTrace> ExtractTracesUntilTotalSizeExceeds(IEnumerable<AssetBundleLoadingTrace> traces, long totalBundleSize)
        {
            var cleanupSize = 0f;

            foreach (var trace in traces)
            {
                cleanupSize += trace.BundleSize;

                // Debug.Log($"削除後の合計バンドルサイズ : {totalBundleSize - cleanupSize}");
                // Debug.Log($"目標の合計バンドルサイズ : {cleanupThreshold}");
                if (totalBundleSize - cleanupSize < cleanupThreshold)
                {
                    yield return trace;
                }
            }
        }

        /// <summary>
        /// 閾値を超えないように古い順からキャッシュを削除
        /// </summary>
        private void RemoveExceededFiles()
        {
            var latestCachesInfo = cacheTraceService.GetTotalCachesLatest();
            if (latestCachesInfo != null && latestCachesInfo.TotalBundleSize > cleanupThreshold)
            {
                // NOTE: キャッシュしているアセットバンドルの合計サイズが、cleanupThresholdを超えたら古い順からcleanupSize分だけ削除
                var traces = cacheTraceService.GetTraceOrderByTimeStamp();
                // tracesのログを確認
                var filteredTraces = ExtractTracesUntilTotalSizeExceeds(traces, latestCachesInfo.TotalBundleSize);
                foreach (var trace in filteredTraces)
                {
                    var filePath = trace.FilePath;

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    cacheTraceService.DeleteAccess(filePath);
                }

                // NOTE: DBの合計キャッシュ情報の元データを登録 - LoadAsyncでキャッシュしたアセットバンドルの合計サイズがDBに保存される
                cacheTraceService.DeleteAllTotalCaches();
            }
        }

        /// <summary>
        /// DBに存在しないキャッシュファイルを削除
        /// </summary>
        private void RemoveStaleCacheEntries()
        {
            foreach (var filePath in Directory.EnumerateFiles(InternalCaching.CachePath, "*", SearchOption.AllDirectories))
            {
                if (!cacheTraceService.IsCached(filePath))
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
            }
        }
        
        /// <summary>
        /// アドレスからDBを更新
        /// </summary>
        /// <param name="address"></param>
        private void UpdateDatabaseWithAddress(string address)
        {
            if (catalogEntity.TryGetAddressInfo(address, out var fileName, out var options))
            {
                // NOTE: アドレスからキャッシュのファイルパスとバンドルサイズを取得して、DBを更新
                var filePath = InternalCaching.FileNameToCachePath(fileName, options.Hash);
                var bundleSize = options.BundleSize;

                // Debug.Log($"FilePath : {filePath} - BundleSize : {bundleSize}");

                // アセットバンドルのキャッシュ情報をDBに保存
                UpdateAssetBundleLoadingTrace(filePath, bundleSize);

                // キャッシュしたアセットバンドルの合計サイズをDBに保存
                UpdateAssetBundleTotalCaches(bundleSize);
            }
        }

        /// <summary>
        /// アセットをロード
        /// 必ずIDisposableAssetをDisposeする必要あり
        /// DisposeしないとAssetBundleが解放されない
        /// </summary>
        public async UniTask<IDisposableAsset<T>> LoadAsync<T>(string address, CancellationToken cancellationToken = default) where T : Object
        {
            Assert.IsTrue(Initialized, "AddressableService is not initialized.");
            Assert.IsFalse(string.IsNullOrEmpty(address), "key is null or empty.");

            if (!catalogEntity.IsValid)
            {
                return new DisposableAsset<T>(null, delegate { }, new Exception("Catalog is not valid."));
            }

            try
            {
                address = address.ToLower();

                // NOTE: カタログ情報を正しく使っているときだけチェック
#if UNITY_EDITOR
                if (AddressablesUtility.IsPackedPlayMode())
#endif
                {
                    var exist = catalogEntity.IsExists(address);
                    if (!exist)
                    {
                        return new DisposableAsset<T>(null, delegate { }, new KeyNotFoundException("Not found address. " + address));
                    }
                }

                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, loadCancellationTokenSource.Token);

                var handle = Addressables.LoadAssetAsync<T>(address);
                await handle.ToUniTask(cancellationToken: source.Token);

                if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
                {
                    if (handle.IsValid())
                    {
                        Addressables.Release(handle);
                    }

                    var e = handle.OperationException;
                    if (source.Token.IsCancellationRequested || e is OperationCanceledException || e is TimeoutException)
                    {
                        return new DisposableAsset<T>(null, delegate { }, new OperationCanceledException());
                    }

                    if (e is SocketException { ErrorCode: 10051 })
                    {
                        // 複数のリクエストがすべてSocketExceptionで帰って来るのでまとめて通知する
                        if (Time.realtimeSinceStartup - networkUnreachableReceiveTime > 1f)
                        {
                            OnNetworkUnreachableException(e);
                        }
                        networkUnreachableReceiveTime = Time.realtimeSinceStartup;

                        return new DisposableAsset<T>(null, delegate { }, new OperationCanceledException());
                    }

                    return new DisposableAsset<T>(null, delegate { }, new Exception("Failed to load asset. " + address, e));
                }

                UpdateDatabaseWithAddress(address);

                return new DisposableAsset<T>(handle.Result, () => Addressables.Release(handle));
            }
            catch (OperationCanceledException cancel)
            {
                return new DisposableAsset<T>(null, delegate { }, cancel);
            }
            catch (Exception e)
            {
                // 発生したErrorはExceptionCallBackでハンドリングされる
                return new DisposableAsset<T>(null, delegate { }, e);
            }
        }

        /// <summary>
        /// Sceneアセットをロード
        /// 必ずIDisposableAssetをDisposeする必要あり
        /// DisposeしないとAssetBundleが解放されない
        /// </summary>
        public async UniTask<IDisposableAsset<SceneInstance>> LoadSceneAsync(string address, LoadSceneMode loadMode, CancellationToken cancellationToken = default)
        {
            Assert.IsTrue(Initialized, "AddressableService is not initialized.");
            Assert.IsFalse(string.IsNullOrEmpty(address), "key is null or empty.");

            if (!catalogEntity.IsValid)
            {
                return new DisposableAsset<SceneInstance>(default, delegate { }, new Exception("Catalog is not valid."));
            }

            try
            {
                address = address.ToLower();

                // NOTE: カタログ情報を正しく使っているときだけチェック
#if UNITY_EDITOR
                if (AddressablesUtility.IsPackedPlayMode())
#endif
                {
                    var exist = catalogEntity.IsExists(address);
                    if (!exist)
                    {
                        return new DisposableAsset<SceneInstance>(default, delegate { }, new KeyNotFoundException("Not found address. " + address));
                    }
                }

                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, loadCancellationTokenSource.Token);

                var handle = Addressables.LoadSceneAsync(address, loadMode);
                await handle.ToUniTask(cancellationToken: source.Token);

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    if (handle.IsValid())
                    {
                        Addressables.Release(handle);
                    }

                    var e = handle.OperationException;
                    if (source.Token.IsCancellationRequested || e is OperationCanceledException || e is TimeoutException)
                    {
                        return new DisposableAsset<SceneInstance>(default, delegate { }, new OperationCanceledException());
                    }

                    if (e is SocketException { ErrorCode: 10051 })
                    {
                        // 複数のリクエストがすべてSocketExceptionで帰って来るのでまとめて通知する
                        if (Time.realtimeSinceStartup - networkUnreachableReceiveTime > 1f)
                        {
                            OnNetworkUnreachableException(e);
                        }
                        networkUnreachableReceiveTime = Time.realtimeSinceStartup;

                        return new DisposableAsset<SceneInstance>(default, delegate { }, new OperationCanceledException());
                    }

                    return new DisposableAsset<SceneInstance>(default, delegate { }, new Exception("Failed to load asset. " + address, e));
                }

                UpdateDatabaseWithAddress(address);

                return new DisposableAsset<SceneInstance>(handle.Result, () => Addressables.UnloadSceneAsync(handle));
            }
            catch (OperationCanceledException cancel)
            {
                return new DisposableAsset<SceneInstance>(default, delegate { }, cancel);
            }
            catch (Exception e)
            {
                // 発生したErrorはExceptionCallBackでハンドリングされる
                return new DisposableAsset<SceneInstance>(default, delegate { }, e);
            }
        }

        /// <summary>
        /// 指定したアドレスとアドレスに依存するアセットをダウンロード
        /// </summary>
        /// <param name="addresses"></param>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        public async UniTask PreloadAssetAsync(IEnumerable<string> addresses, IProgress<double> progress, CancellationToken cancellationToken)
        {
            Assert.IsTrue(Initialized, "AddressableService is not initialized.");

            AsyncOperationHandle handle = default;

            if (!catalogEntity.IsValid)
            {
                Debug.LogWarning("Catalog is not valid.");

                return;
            }

            try
            {
                var requests = (await catalogEntity.GetNonCachedAddressesAsync(addresses, cancellationToken)).ToArray();
                if (requests.Length == 0)
                {
                    return;
                }

                handle = Addressables.DownloadDependenciesAsync((IEnumerable) requests, Addressables.MergeMode.Union, true);
                if (progress == null)
                {
                    await handle.ToUniTask(cancellationToken: cancellationToken);

                    return;
                }

                var progressValue = 0f;
                while (!handle.IsDone)
                {
                    if (!handle.IsValid() || cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (!Mathf.Approximately(progressValue, handle.PercentComplete))
                    {
                        progressValue = handle.PercentComplete;

                        // NOTE: なぜか0.75fから1fまでの値が返ってくるのでそれを0fから1fに変換
                        // TODO: 0~1の値が返ってくるように修正
                        const float min = 0.75f, max = 1f;

                        var value = (progressValue - min) / (max - min);
                        progress.Report(value);
                    }

                    await UniTask.Yield();
                }
                
                // ダウンロード完了後にDB登録
                foreach (var address in requests)
                {
                    UpdateDatabaseWithAddress(address);
                }
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            finally
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
        }

        /// <summary>
        /// 指定したアドレスが存在するか確認する
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool IsExist(string address)
        {
#if UNITY_EDITOR
            if (!AddressablesUtility.IsPackedPlayMode())
            {
                return true;
            }
#endif
            return catalogEntity.IsExists(address);
        }
    }

}
