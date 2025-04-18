using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Asteria
{
    internal class AssetBundleDownloader : IDownloader<AssetBundle>
    {
        public class BundleParam
        {
            public readonly string filePath;
            public readonly string hash;
            public readonly uint crc;
            public readonly SecureData security;

            public BundleParam(string filePath, string hash, uint crc, SecureData security)
            {
                this.filePath = filePath;
                this.hash = hash;
                this.crc = crc;
                this.security = security;
            }
        }

        private const int DefaultRetryCount = 3;
        private const int RetryInterval = 500;

        private readonly string requestUrl;
        private readonly BundleParam param;
        private readonly IProgress<double> progress;
        private readonly bool downloadOnly;
        private readonly int maxRetryCount;

        private int retryCount;

        public string Key => param.filePath;

        public AssetBundleDownloader(string requestUrl, BundleParam param, IProgress<double> progress, bool downloadOnly, int maxRetryCount = DefaultRetryCount)
        {
            this.requestUrl = requestUrl;
            this.param = param;
            this.progress = progress;
            this.downloadOnly = downloadOnly;
            this.maxRetryCount = maxRetryCount;
        }

        public void Dispose()
        {
            // Dispose
        }

        public async UniTask<RequestResult<AssetBundle>> SendAsync(HttpClient client, string savePath, CancellationToken token)
        {
            Assert.IsNotNull(client, "http client is null.");
            Assert.IsFalse(string.IsNullOrEmpty(savePath), "save path is null or empty.");

            if (progress != null)
            {
                progress.Report(0);
            }

            var downloadPath = Path.Combine(savePath, Key);
            var result = await LoadAsync(client, downloadPath, token);

            if (progress != null)
            {
                progress.Report(100);
            }

            return result;
        }

        private async UniTask<RequestResult<AssetBundle>> LoadAsync(HttpClient client, string downloadPath, CancellationToken token)
        {
            if (!File.Exists(downloadPath))
            {
                if (Application.internetReachability is NetworkReachability.NotReachable)
                {
                    return new RequestResult<AssetBundle>(Key, null, 0, new SocketException(10051));
                }

                var statusCode = await client.DownloadAsync(requestUrl, downloadPath, progress, token);
                if (!statusCode.IsSuccessCode())
                {
                    if (statusCode.IsRetryCode())
                    {
                        throw new Exception();
                    }

                    return new RequestResult<AssetBundle>(Key, null, (uint) statusCode, new Exception($"Download failed. {requestUrl}"));
                }
            }

            if (downloadOnly)
            {
                return new RequestResult<AssetBundle>(Key, null, 200);
            }

            // NOTE: すでにメモリ上にAssetBundleが存在する場合は、それを返す
            var hit = AssetBundle.GetAllLoadedAssetBundles().FirstOrDefault(x => x.name == Key);
            if (hit != null)
            {
                return new RequestResult<AssetBundle>(Key, hit, 200);
            }

            var useSecurity = param.security != null && param.security.IsValid();
            if (!useSecurity)
            {
                hit = await AssetBundle.LoadFromFileAsync(downloadPath, param.crc).ToUniTask(cancellationToken: token);
            }
            else
            {
                // 8KBのバッファサイズを使用
                const int customBufferSize = 81920;
                var fileName = Path.GetFileName(requestUrl);
                var salt = Encoding.UTF8.GetBytes(fileName);
                var security = param.security;

                await using var fileStream = new FileStream(downloadPath, FileMode.Open, FileAccess.Read, FileShare.Read, customBufferSize, true);
                await using var aesStream = new SeekableAesStream(fileStream, security.Password.ToPlainText(), salt, security.EncriptionLength);

                hit = await AssetBundle.LoadFromStreamAsync(aesStream, param.crc).ToUniTask(cancellationToken: token);
            }

            if (hit == null)
            {
                throw new FileLoadException($"AssetBundle is null. {requestUrl}");
            }

            return new RequestResult<AssetBundle>(Key, hit, 200);
        }
    }
}
