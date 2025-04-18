using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Asteria
{
    public class FileDownloader : IDownloader<TextAsset>
    {
        private const int DefaultRetryCount = 3;
        private const int RetryInterval = 500;

        private readonly string requestUrl;
        private readonly IProgress<double> progress;
        private readonly bool downloadOnly;
        private readonly int maxRetryCount;

        private int retryCount;

        public string Key
        {
            get;
        }

        public FileDownloader(string requestUrl, string filePath, IProgress<double> progress, bool downloadOnly, int maxRetryCount = DefaultRetryCount)
        {
            Key = filePath;
            this.requestUrl = requestUrl;
            this.progress = progress;
            this.downloadOnly = downloadOnly;
            this.maxRetryCount = maxRetryCount;
        }

        public void Dispose()
        {
            // Dispose
        }

        public async UniTask<RequestResult<TextAsset>> SendAsync(HttpClient client, string savePath, CancellationToken token)
        {
            Assert.IsNotNull(client, "http client is null.");
            Assert.IsFalse(string.IsNullOrEmpty(savePath), "save path is null or empty.");

            var downloadPath = Path.Combine(savePath, Key);

            while (retryCount < maxRetryCount)
            {
                try
                {
                    if (retryCount > 0)
                    {
                        await UniTask.Delay(RetryInterval, cancellationToken: token);
                    }

                    var result = await LoadAsync(client, downloadPath, token);

                    if (progress != null)
                    {
                        progress.Report(100);
                    }

                    return result;
                }
                catch (OperationCanceledException cancel)
                {
                    return new RequestResult<TextAsset>(Key, null, 0, cancel);
                }
                catch (HttpRequestException http)
                {
                    // リトライして成功する可能性があるエラーの場合
                    if (http.InnerException is TimeoutException || http.InnerException is SocketException || http.Message.Contains("timed out"))
                    {
                        retryCount++;

                        continue;
                    }

                    return new RequestResult<TextAsset>(Key, null, 0, http);
                }
                catch (Exception)
                {
                    retryCount++;
                    File.Delete(downloadPath);
                }
            }

            return new RequestResult<TextAsset>(Key, null, 408, new TimeoutException("Retry count over."));
        }

        private async UniTask<RequestResult<TextAsset>> LoadAsync(HttpClient client, string downloadPath, CancellationToken token)
        {
            if (!File.Exists(downloadPath))
            {
                if (Application.internetReachability is NetworkReachability.NotReachable)
                {
                    return new RequestResult<TextAsset>(Key, null, 0, new SocketException(10051));
                }
                var statusCode = await client.DownloadAsync(requestUrl, downloadPath, progress, token);
                if (!statusCode.IsSuccessCode())
                {
                    if (statusCode.IsRetryCode())
                    {
                        throw new Exception();
                    }

                    return new RequestResult<TextAsset>(Key, null, (uint) statusCode, new Exception("Download failed."));
                }
            }

            if (downloadOnly)
            {
                return new RequestResult<TextAsset>(Key, null, 200);
            }

            var text = await File.ReadAllTextAsync(downloadPath, token);
            if (string.IsNullOrEmpty(text))
            {
                return new RequestResult<TextAsset>(Key, null, 405, new FileNotFoundException($"File is empty: {downloadPath}"));
            }

            return new RequestResult<TextAsset>(Key, new TextAsset(text), 200);
        }
    }
}
