using System;
using System.Threading;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.Scripting;

namespace Asteria
{
    [Preserve]
    internal class CustomAssetBundleResource : IAssetBundleResource
    {
        private ProvideHandle provideHandle;
        private readonly AssetBundleRequestOptions options;
        private readonly CancellationTokenSource cancellationTokenSource = new();

        private double progressValue;
        private DownloadStatus downloadStatus;
        private RequestResult<AssetBundle> resultCache;

        public CustomAssetBundleResource(ProvideHandle provideHandle)
        {
            this.provideHandle = provideHandle;
            options = this.provideHandle.Location.Data as AssetBundleRequestOptions;

            provideHandle.SetProgressCallback(GetProgress);
            provideHandle.SetDownloadProgressCallbacks(GetDownloadStatus);
        }

        public async void Load()
        {
            var location = provideHandle.Location;
            var context = InternalContext.Context;

            var filePath = InternalCaching.FileNameToCachePath(location.PrimaryKey, options.Hash);
            var param = new AssetBundleDownloader.BundleParam(filePath, options.Hash, options.Crc, context.Security);

            var url = provideHandle.ResourceManager.TransformInternalId(location);

            IProgress<double> progress = new Progress<double>(UpdateProgress);

            var downloadOnly = location.ToString().CustomEndsWith("DownloadOnlyLocation");
            var downloader = new AssetBundleDownloader(url, param, progress, downloadOnly, options.RetryCount);

            resultCache = await context.WebRequest.SendAsync(downloader, cancellationTokenSource.Token);

            progress.Report(100);

            provideHandle.Complete(this, resultCache.IsSuccess, resultCache.InnerException);
        }

        public void Unload()
        {
            resultCache?.Result?.Unload(true);
            cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// ロード・ダウンロードされたAssetBundleを取得する
        /// </summary>
        public AssetBundle GetAssetBundle()
        {
            return resultCache?.Result;
        }

        private void UpdateProgress(double progress)
        {
            progressValue = progress;
            downloadStatus = new DownloadStatus { TotalBytes = 100, DownloadedBytes = (int) progressValue, IsDone = progressValue >= 100 };
        }

        private float GetProgress()
        {
            if (progressValue <= 0)
            {
                return 0;
            }

            return (float) (progressValue / 100);
        }

        private DownloadStatus GetDownloadStatus()
        {
            return downloadStatus;
        }
    }
}
