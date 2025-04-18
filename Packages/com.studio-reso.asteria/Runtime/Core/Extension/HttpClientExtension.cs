using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Asteria
{
    internal static class HttpClientExtension
    {
        public static async UniTask<HttpStatusCode> DownloadAsync(this HttpClient client, string url, string savePath, IProgress<double> progress, CancellationToken token)
        {
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);

            return await response.SaveAsync(savePath, progress, token);
        }

        public static async UniTask<HttpStatusCode> SaveAsync(this HttpResponseMessage response, string savePath, IProgress<double> progress, CancellationToken token)
        {
            if (!response.IsSuccessStatusCode || response.Content.Headers.ContentLength == null)
            {
                return response.StatusCode;
            }

            var dir = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (progress == null)
            {
                await using var httpStream = await response.Content.ReadAsStreamAsync();
                await using var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
                await httpStream.CopyToAsync(fileStream, token);
            }
            else
            {
                progress.Report(0);

                var totalBytes = response.Content.Headers.ContentLength;

                await using var httpStream = await response.Content.ReadAsStreamAsync();
                await using var progressStream = new ProgressStream(httpStream, progress, totalBytes.GetValueOrDefault());
                await using var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);

                await httpStream.CopyToAsync(fileStream, token);

                progress.Report(100);
            }

            return response.StatusCode;
        }

        public static bool IsSuccessCode(this HttpStatusCode code)
        {
            var codeValue = (int) code;

            return codeValue is >= 200 and <= 299;
        }

        public static bool IsRetryCode(this HttpStatusCode code)
        {
            // 408 RequestTimeout
            // 429 TooManyRequests
            // 503 ServiceUnavailable
            // 500 InternalServerError
            // 504 GatewayTimeout
            // 502 BadGateway
            return code is HttpStatusCode.RequestTimeout
                           or HttpStatusCode.TooManyRequests
                           or HttpStatusCode.ServiceUnavailable
                           or HttpStatusCode.InternalServerError
                           or HttpStatusCode.GatewayTimeout
                           or HttpStatusCode.BadGateway;
        }
    }
}
