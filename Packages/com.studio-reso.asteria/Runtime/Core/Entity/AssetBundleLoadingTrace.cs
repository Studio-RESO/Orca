using System;
using LiteDB;
using UnityEngine.Scripting;

namespace Asteria
{
    [Preserve]
    [Serializable]
    public class AssetBundleLoadingTrace
    {
        [Preserve]
        [BsonId]
        public string FilePath { get; set; } // キャッシュされたアセットバンドルのファイルパス
        public long BundleSize { get; set; } // アセットバンドルのサイズ
        public DateTime TimeStamp { get; set; } // ファイルアクセスした日時
    }
}
