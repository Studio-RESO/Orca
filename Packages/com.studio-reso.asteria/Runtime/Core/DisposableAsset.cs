using System;

namespace Asteria
{
    public interface IDisposableAsset<out T> : IDisposable
    {
        T Asset
        {
            get;
        }

        Exception InnerException
        {
            get;
        }
    }

    public class DisposableAsset<T> : IDisposableAsset<T>
    {
        public T Asset
        {
            get;
        }

        public Exception InnerException
        {
            get;
        }

        private bool disposed;

        private readonly Action disposeAction;

        public DisposableAsset(T asset, Action dispose, Exception innerException = null)
        {
            Asset = asset;
            InnerException = innerException;

            disposeAction = dispose;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            disposeAction?.Invoke();
        }
    }
}
