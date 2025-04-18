using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Asteria
{
    internal class TextureDownloader
    {
        private const int DefaultRetryCount = 3;
        private const int RetryInterval = 500;

        private readonly string requestUrl;
        private readonly string saveFilePath;
        private readonly string eTag;
        private readonly IProgress<double> progress;
        private readonly bool downloadOnly;
        private readonly int maxRetryCount;

        private int retryCount;

        public string Key
        {
            get;
        }

        public TextureDownloader(string requestUrl, string saveFilePath, string eTag, IProgress<double> progress, bool downloadOnly, int maxRetryCount = DefaultRetryCount)
        {
            Assert.IsFalse(string.IsNullOrEmpty(saveFilePath), "save file path is null or empty.");

            Key = Path.GetFileName(saveFilePath);
            this.requestUrl = requestUrl;
            this.saveFilePath = saveFilePath;
            this.eTag = eTag;
            this.progress = progress;
            this.downloadOnly = downloadOnly;
            this.maxRetryCount = maxRetryCount;
        }

        public void Dispose()
        {
            // Dispose
        }

        internal async UniTask SendAsync(HttpClient client, IAsyncWriter<TextureRequestResult> writer, CancellationToken cancellationToken)
        {
            Assert.IsNotNull(client, "http client is null.");

            while (retryCount < maxRetryCount)
            {
                try
                {
                    if (retryCount > 0)
                    {
                        await UniTask.Delay(RetryInterval, cancellationToken: cancellationToken);
                    }

                    if (await LoadAsync(writer, client, cancellationToken))
                    {
                        if (progress != null)
                        {
                            progress.Report(100);
                        }

                        break;
                    }
                }
                catch (OperationCanceledException cancel)
                {
                    await writer.YieldAsync(new TextureRequestResult(Key, null, "", 0, cancel));

                    break;
                }
                catch (HttpRequestException http)
                {
                    // リトライして成功する可能性があるエラーの場合
                    if (http.InnerException is TimeoutException || http.InnerException is SocketException || http.Message.Contains("timed out"))
                    {
                        retryCount++;

                        continue;
                    }

                    await writer.YieldAsync(new TextureRequestResult(Key, null, "", 0, http));

                    break;
                }
                catch (Exception e)
                {
                    retryCount++;

                    Debug.LogWarning(e);
                    var filePath = $"{saveFilePath}.{eTag}";
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
            }

            if (retryCount >= DefaultRetryCount)
            {
                await writer.YieldAsync(new TextureRequestResult(Key, null, "", 408, new TimeoutException("Retry count over.")));
            }
        }

        private async UniTask<bool> LoadAsync(IAsyncWriter<TextureRequestResult> writer, HttpClient client, CancellationToken cancellationToken)
        {
            Texture2D result;

            // eTag がある場合は、ローカルのテクスチャを読み込む
            if (!string.IsNullOrEmpty(eTag))
            {
                var filePath = $"{saveFilePath}.{eTag}";
                if (File.Exists(filePath))
                {
                    if (downloadOnly)
                    {
                        return true;
                    }

                    result = await LoadLocalTexture(filePath, cancellationToken);
                    if (result != null)
                    {
                        await writer.YieldAsync(new TextureRequestResult(Key, result, eTag, 200));
                    }
                }
            }

            if (Application.internetReachability is NetworkReachability.NotReachable)
            {
                await writer.YieldAsync(new TextureRequestResult(Key, null, "", 0, new SocketException(10051)));

                return false;
            }

            // 差分があるかをチェックし、差分があれば再度Textureを返す
            var (code, newEtag) = await DownloadTexture(client, cancellationToken);

            if (code.IsSuccessCode() || code == HttpStatusCode.NotModified)
            {
                if (!string.IsNullOrEmpty(eTag) && !string.IsNullOrEmpty(newEtag) && newEtag == eTag)
                {
                    return true;
                }

                var filePath = $"{saveFilePath}.{newEtag}";
                if (File.Exists(filePath))
                {
                    if (downloadOnly)
                    {
                        return true;
                    }

                    result = await LoadLocalTexture(filePath, cancellationToken);
                    if (result != null)
                    {
                        await writer.YieldAsync(new TextureRequestResult(Key, result, newEtag, (uint) code));

                        return true;
                    }
                }
            }

            if (code.IsRetryCode())
            {
                return false;
            }

            await writer.YieldAsync(new TextureRequestResult(Key, null, "", 0, new Exception("Download failed.")));

            return true;
        }

        private async UniTask<Texture2D> LoadLocalTexture(string filePath, CancellationToken cancellationToken)
        {
            try
            {
                var data = await File.ReadAllBytesAsync(filePath, cancellationToken);
                var tex = new Texture2D(2, 2)
                {
                    name = Key,
                };
                tex.LoadImage(data);

                return tex;
            }
            catch (Exception)
            {
                // ファイルが壊れている場合は削除
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            return null;
        }

        private async UniTask<(HttpStatusCode code, string newEtag)> DownloadTexture(HttpClient client, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            // eTag がある場合は、If-None-Match ヘッダを付与してリクエストを送信
            if (!string.IsNullOrEmpty(eTag))
            {
                request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue($"\"{eTag}\""));
            }

            using var response = await client.SendAsync(request, cancellationToken);

            // 304 NotModified の場合は、差分がないため終了
            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                return (response.StatusCode, eTag);
            }

            if (response.IsSuccessStatusCode && response.Headers.ETag != null)
            {
                var tag = response.Headers.ETag.Tag.Replace("\"", "");
                var savePath = $"{saveFilePath}.{tag}";
                var code = await response.SaveAsync(savePath, progress, cancellationToken);

                return (code, tag);
            }

            return (response.StatusCode, "");
        }
    }

}
