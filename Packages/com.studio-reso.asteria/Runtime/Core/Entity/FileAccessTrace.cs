using System;
using LiteDB;
using UnityEngine.Scripting;

namespace Asteria
{
    [Preserve]
    [Serializable]
    public class FileAccessTrace
    {
        [Preserve]
        [BsonId]
        public string FileName { get; set; }
        public DateTime TimeStamp { get; set; }
        public string ETag { get; set; }
    }
}
