using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Asteria
{
    public interface ITextureWebRequest : IDisposable
    {
        IUniTaskAsyncEnumerable<IDisposableAsset<Texture2D>> SendAsync(string url, CancellationToken cancellationToken);
    }
}
