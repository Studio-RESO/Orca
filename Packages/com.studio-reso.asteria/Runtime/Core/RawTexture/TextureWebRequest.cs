using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Asteria
{
    public class TextureWebRequest : ITextureWebRequest
    {
        private const int CacheDeleteDays = 14;

        private readonly string cachePath;
        private readonly FileAccessTraceService accessTraceService;

        private CancellationTokenSource cts = new();

        private HttpClient httpClient;
        private int maxConcurrent;
        
        private int loadings;

        private readonly ConcurrentDictionary<string, TextureCacheFile> fileCache = new();
        private float networkUnreachableReceiveTime;

        /// <summary>
        /// Load処理が完了した数をリアルタイムで取得するためのイベント
        /// ResetLoadedCount()を呼び出すと0にリセットされる
        /// </summary>
        public event Action<int> OnChangedLoaded = delegate { };
        public event Action<Exception> OnNetworkUnreachableException = delegate { };

        /// <param name="httpClient">共通で使用するHttpClient</param>
        /// <param name="maxConcurrent">並列で処理する最大数(UpdateMaxParallelで更新可)</param>
        /// <param name="cachePath">ダウンロードしたファイルの保存先Directory</param>
        /// <param name="accessTraceService">ローカルデータベースを扱うためのインターフェース</param>
        public TextureWebRequest(HttpClient httpClient, int maxConcurrent, string cachePath, FileAccessTraceService accessTraceService)
        {
            Assert.IsFalse(string.IsNullOrEmpty(cachePath), "cache path is null or empty.");

            this.cachePath = cachePath;
            this.accessTraceService = accessTraceService;
            
            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }

            UpdateHttpClient(httpClient);
            UpdateMaxConcurrent(maxConcurrent);

            CleanupCache();
        }

        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();
        }

        public void Cancel()
        {
            cts.Cancel();
            cts = new CancellationTokenSource();
        }

        public void CleanupCache()
        {
            if (!Directory.Exists(cachePath))
            {
                return;
            }
            
            // アクセス履歴が指定した期間より経過していたらキャッシュを削除
            var accessTraces = accessTraceService.GetExpiredFileAccessTraces(DateTime.Now.AddDays(-CacheDeleteDays));
            foreach (var trace in accessTraces)
            {
                var path = Path.Combine(cachePath, $"{trace.FileName[0]}/{trace.FileName}.{trace.ETag}");
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                accessTraceService.RemoveAccessTrace(trace.FileName);
            }
        }

        public void UpdateHttpClient(HttpClient client)
        {
            Assert.IsNotNull(client, "http client is null.");
            httpClient = client;
        }

        /// <summary>
        /// WebRequestを並列で処理する最大数を更新
        /// </summary>
        /// <param name="max"></param>
        public void UpdateMaxConcurrent(int max)
        {
            Assert.IsFalse(max <= 0, "max concurrent is less than or equal to 0.");
            Interlocked.Exchange(ref maxConcurrent, max);
        }

        /// <summary>
        /// URLのTextureを取得
        ///
        /// Localにキャッシュが存在しない場合
        /// 1. URLからTextureを取得し、Localにキャッシュを作成して返す
        ///
        /// Localにキャッシュが存在する場合
        /// 1. Localにあるキャッシュを返す
        /// 2. キャッシュのEtagとURLのEtagが異なる場合はキャッシュを更新
        /// 3. Etagが同じ場合は終了、異なる場合は再度Textuerを返す
        ///
        /// IDisposableAssetのDisposeを呼び出すとキャッシュを削除
        /// ※ キャッシュはDisposeされるまで削除されないので必ずDisposeを呼ぶこと
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public IUniTaskAsyncEnumerable<IDisposableAsset<Texture2D>> SendAsync(string url, CancellationToken cancellationToken)
        {
            Assert.IsFalse(string.IsNullOrEmpty(url), "url is null or empty.");

            // NOTE: URLからHashを生成してキャッシュファイル名とする
            // NOTE: {hash}.{etag} の形式でLocalに保存

            // https://hoge.com/file_path
            var split = url.Split('/', '\\');
            var relativePath = string.Join('/', split, 3, split.Length - 3);
            var fileName = HashCreator.ComputeMD5Hash(relativePath);

            if (fileCache.TryGetValue(fileName, out var cache))
            {
                if (cache.Disposed)
                {
                    fileCache.Remove(fileName, out _);
                }
                else
                {
                    cache.AddReference();
                    if (cache.Asset != null)
                    {
                        return UniTaskAsyncEnumerable.Return(cache);
                    }
                }
            }

            // NOTE: 1つのディレクトリに大量のファイルがある場合に対応するため、1文字目のディレクトリを作成
            var filePath = $"{fileName[0]}/{fileName}";

            var eTag = accessTraceService.GetFileAccessTrace(fileName)?.ETag;

            var downloader = new TextureDownloader(url, Path.Combine(cachePath, filePath), eTag, null, false);

            return UniTaskAsyncEnumerable.Create<TextureRequestResult>(async (writer, ct) =>
            {
                using var source = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken, ct);

                // 並列数が上限に達している場合は待機
                try
                {
                    if (loadings >= maxConcurrent && maxConcurrent > 0)
                    {
                        while (loadings >= maxConcurrent)
                        {
                            await UniTask.Yield(source.Token);
                        }
                    }
                }
                catch (OperationCanceledException cancel)
                {
                    await writer.YieldAsync(new TextureRequestResult(downloader.Key, null, "", 0, cancel));
                }

                await DownloadAsync(writer, downloader, source.Token);
            }).Select(UpdateCache);
        }

        private async UniTask DownloadAsync(IAsyncWriter<TextureRequestResult> writer, TextureDownloader downloader, CancellationToken cancellationToken)
        {
            try
            {
                Interlocked.Increment(ref loadings);
                await downloader.SendAsync(httpClient, writer, cancellationToken);
            }
            catch (OperationCanceledException cancel)
            {
                await writer.YieldAsync(new TextureRequestResult(downloader.Key, null, "", 0, cancel));
            }
            catch (Exception e)
            {
                await writer.YieldAsync(new TextureRequestResult(downloader.Key, null, "", 0, e));
            }
            finally
            {
                Interlocked.Decrement(ref loadings);
            }
        }

        private IDisposableAsset<Texture2D> UpdateCache(TextureRequestResult result)
        {
            if (fileCache.TryGetValue(result.Key, out var cache))
            {
                if (!result.IsSuccess || result.InnerException != null)
                {
                    cache.Dispose();

                    if (result.InnerException is SocketException { ErrorCode: 10051 } socketException)
                    {
                        if (Time.realtimeSinceStartup - networkUnreachableReceiveTime > 1)
                        {
                            OnNetworkUnreachableException(socketException);
                        }
                        networkUnreachableReceiveTime = Time.realtimeSinceStartup;

                        return new TextureCacheFile(null, delegate { }, new OperationCanceledException());
                    }

                    return new TextureCacheFile(null, delegate { }, result.InnerException);
                }
            }
            else
            {
                cache = new TextureCacheFile(result.Key, RemoveCache, result.InnerException);

                // ローカルDB更新
                accessTraceService.UpdateAccessTrace(result.Key, result.Etag);

                fileCache[result.Key] = cache;
            }

            cache.AddReference();
            cache.Update(result.Result);

            // ETagを比較して異なる場合はキャッシュを削除
            var eTag = accessTraceService.GetFileAccessTrace(result.Key)?.ETag;
            if (result.Etag != eTag)
            {
                var path = Path.Combine(cachePath, $"{result.Key[0]}/{result.Key}");
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            return cache;
        }

        private void RemoveCache(string key)
        {
            fileCache.Remove(key, out _);
        }

        public async UniTask<bool> CacheClearAsync(CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.RunOnThreadPool(() =>
                {
                    accessTraceService.RemoveAllAccessTrace();

                    if (Directory.Exists(cachePath))
                    {
                        Directory.Delete(cachePath, true);
                        Directory.CreateDirectory(cachePath);
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
    }
}
