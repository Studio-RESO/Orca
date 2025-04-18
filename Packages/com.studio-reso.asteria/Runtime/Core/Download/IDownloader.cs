using System;
using System.Net.Http;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Asteria
{
    public interface IDownloader<T> : IDisposable
    {
        string Key
        {
            get;
        }

        UniTask<RequestResult<T>> SendAsync(HttpClient client, string savePath, CancellationToken token);
    }
}
