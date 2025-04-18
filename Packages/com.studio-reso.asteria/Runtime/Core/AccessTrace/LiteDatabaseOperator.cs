using System.IO;
using System.Security;
using LiteDB;
using UnityEngine;

namespace Asteria
{
    public sealed class LiteDatabaseOperator : IDataTraceOperator
    {
        private readonly LiteDatabase db;

        // NOTE: データベースの初期化は、MonoBehaviourクラスのフィールド初期化子で行わないこと。エディタ確認の場合、複数回初期化されるためLiteDBがIOExceptionをスローする。
        public LiteDatabaseOperator(string databaseName, string databasePath, SecureString password)
        {
            if (!Directory.Exists(databasePath))
            {
                Directory.CreateDirectory(databasePath);
            }
            
            var connectionStr = $"Filename={Path.Combine(databasePath, databaseName)};Password={password.ToPlainText()}";

            try
            {
                db = new LiteDatabase(connectionStr);
            }
            catch (LiteException e)
            {
                // パスワードが間違っている場合、データベースを削除して再作成する
                if (e.ErrorCode == LiteException.INVALID_PASSWORD)
                {
                    if (File.Exists(databasePath))
                    {
                        File.Delete(databasePath);
                    }

                    db = new LiteDatabase(connectionStr);
                }
                else
                {
                    throw;
                }
            }
        }

        public void Dispose()
        {
            db?.Dispose();
        }

        public AddressablesCacheTraceService CreateAddressablesCacheTraceService()
        {
            return new AddressablesCacheTraceService(AssetBundleLoadingTrace, AssetBundleTotalCaches);
        }

        // Texture用
        private LiteDatabaseHandler<FileAccessTrace> textureFileAccessTrace;
        public IDataRepository<FileAccessTrace> TextureFileAccessTrace
        {
            get
            {
                return textureFileAccessTrace ??= new LiteDatabaseHandler<FileAccessTrace>(CollectionNames.TextureAssets.ToString(), db);
            }
        }

        // Addressable用
        private LiteDatabaseHandler<AssetBundleLoadingTrace> assetBundleLoadingTrace;
        public IDataRepository<AssetBundleLoadingTrace> AssetBundleLoadingTrace
        {
            get
            {
                return assetBundleLoadingTrace ??= new LiteDatabaseHandler<AssetBundleLoadingTrace>(CollectionNames.AssetBundleLoadingTrace.ToString(), db);
            }
        }

        private LiteDatabaseHandler<AssetBundleTotalCaches> assetBundleTotalCaches;
        public IDataRepository<AssetBundleTotalCaches> AssetBundleTotalCaches
        {
            get
            {
                return assetBundleTotalCaches ??= new LiteDatabaseHandler<AssetBundleTotalCaches>(CollectionNames.AssetBundleTotalCaches.ToString(), db);
            }
        }

        // NOTE: 最終的にstringでコレクション名が管理されるのでEnum名は変更しないこと
        private enum CollectionNames
        {
            TextureAssets,
            AssetBundleLoadingTrace,
            AssetBundleTotalCaches,
        }
    }
}
