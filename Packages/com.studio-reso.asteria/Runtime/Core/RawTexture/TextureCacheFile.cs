using System;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Asteria
{
    internal class TextureCacheFile : IDisposableAsset<Texture2D>
    {
        private readonly string key;
        private readonly Action<string> dispose;
        private int referenceCount;

        public Texture2D Asset
        {
            get;
            private set;
        }

        public Exception InnerException
        {
            get;
        }

        public bool Disposed
        {
            get;
            private set;
        }

        public TextureCacheFile(string key, Action<string> dispose, Exception innerException = null)
        {
            this.key = key;
            this.dispose = dispose;
            InnerException = innerException;
        }

        public void Update(Texture2D asset)
        {
            Assert.IsFalse(Disposed, "Disposed");
            Asset = asset;
        }

        public void AddReference()
        {
            Assert.IsFalse(Disposed, "Disposed");
            referenceCount++;
        }

        public void Dispose()
        {
            if (Disposed)
            {
                return;
            }
            referenceCount--;

            if (referenceCount > 0)
            {
                return;
            }
            Disposed = true;
            Object.Destroy(Asset);
            dispose(key);
        }
    }
}
