using System;
using LiteDB;
using UnityEngine.Scripting;

namespace Asteria
{
    [Preserve]
    [Serializable]
    public class AssetBundleTotalCaches
    {
        [Preserve]
        [BsonId]
        public ObjectId Id { get; set; }
        public long TotalBundleSize { get; set; } // キャッシュされたアセットバンドルの合計サイズ
        public int Count { get; set; } // キャッシュされたアセットバンドルの総数
        public DateTime CreatedAt { get; set; } // 作成日時
        public DateTime UpdatedAt { get; set; } // 更新日時
    }
}
