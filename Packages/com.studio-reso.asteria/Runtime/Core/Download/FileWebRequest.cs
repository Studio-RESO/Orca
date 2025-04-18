using System;
using System.Net.Http;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Assertions;

namespace Asteria
{
    public class FileWebRequest<T> : IWebRequest<T> where T : class
    {
        private CancellationTokenSource cts = new();

        private readonly HttpClient httpClient;
        private readonly int maxConcurrent;

        private int loadings;

        private readonly string cachePath;

        /// <param name="httpClient">共通で使用するHttpClient</param>
        /// <param name="maxConcurrent">並列で処理する最大数</param>
        /// <param name="cachePath">ダウンロードしたファイルの保存先Directory</param>
        public FileWebRequest(HttpClient httpClient, int maxConcurrent, string cachePath)
        {
            Assert.IsNotNull(httpClient, "http client is null.");
            Assert.IsFalse(string.IsNullOrEmpty(cachePath), "cache path is null or empty.");
            Assert.IsFalse(maxConcurrent <= 0, "max parallel is less than or equal to 0.");

            this.httpClient = httpClient;
            this.cachePath = cachePath;
            Interlocked.Exchange(ref this.maxConcurrent, maxConcurrent);
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

        public async UniTask<RequestResult<T>> SendAsync(IDownloader<T> downloader, CancellationToken token)
        {
            Assert.IsNotNull(downloader, "downloader is null.");

            using var source = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, token);

            try
            {
                if (loadings >= maxConcurrent && maxConcurrent > 0)
                {
                    while (loadings >= maxConcurrent)
                    {
                        await UniTask.Yield(source.Token);
                    }
                }

                Interlocked.Increment(ref loadings);

                return await downloader.SendAsync(httpClient, cachePath, source.Token);
            }
            catch (Exception e)
            {
                return new RequestResult<T>(downloader.Key, null, 0, e);
            }
            finally
            {
                Interlocked.Decrement(ref loadings);
            }
        }
    }
}
