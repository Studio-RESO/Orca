using System;

namespace Asteria
{
    public interface IDataTraceOperator : IDisposable
    {
        public AddressablesCacheTraceService CreateAddressablesCacheTraceService();

        // Texture用
        public IDataRepository<FileAccessTrace> TextureFileAccessTrace
        {
            get;
        }

        // Addressable用
        public IDataRepository<AssetBundleLoadingTrace> AssetBundleLoadingTrace
        {
            get;
        }

        public IDataRepository<AssetBundleTotalCaches> AssetBundleTotalCaches
        {
            get;
        }
    }
}
