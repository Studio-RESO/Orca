using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Asteria
{
    public interface IWebRequest<T> : IDisposable
    {
        void Cancel();
        UniTask<RequestResult<T>> SendAsync(IDownloader<T> downloader, CancellationToken token);
    }
}
