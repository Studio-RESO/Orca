using System;
using System.Collections.Generic;
using LiteDB;

namespace Asteria
{
    public class FileAccessTraceService
    {
        private readonly IDataRepository<FileAccessTrace> fileAccessTraceRepository;

        public FileAccessTraceService(IDataRepository<FileAccessTrace> fileAccessTraceRepository)
        {
            this.fileAccessTraceRepository = fileAccessTraceRepository;
        }

        /// <summary>
        /// ファイルアクセスの履歴を取得
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public FileAccessTrace GetFileAccessTrace(string fileName)
        {
            return fileAccessTraceRepository.Get(fileName);
        }

        /// <summary>
        /// 期限が切れたファイルアクセスの履歴を取得
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FileAccessTrace> GetExpiredFileAccessTraces(DateTime expiredDateTime)
        {
            var query = Query.LT("TimeStamp", expiredDateTime);

            return fileAccessTraceRepository.Find(query);
        }

        /// <summary>
        /// ファイルアクセスの履歴を更新
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="eTag"></param>
        public void UpdateAccessTrace(string fileName, string eTag)
        {
            var trace = new FileAccessTrace { FileName = fileName, TimeStamp = DateTime.Now, ETag = eTag };
            fileAccessTraceRepository.Upsert(trace);
        }

        /// <summary>
        /// ファイルアクセスの履歴を削除
        /// </summary>
        /// <param name="fileName"></param>
        public void RemoveAccessTrace(string fileName)
        {
            fileAccessTraceRepository.Delete(fileName);
        }

        /// <summary>
        /// ファイルアクセスの履歴を全削除
        /// </summary>
        public void RemoveAllAccessTrace()
        {
            fileAccessTraceRepository.DeleteAll();
        }
    }

}
