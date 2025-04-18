using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using UnityEngine;

namespace Asteria
{
    public class LiteDatabaseHandler<T> : IDataRepository<T> where T : class, new()
    {
        private readonly LiteDatabase db;
        private readonly string collectionName;

        public LiteDatabaseHandler(string collectionName, LiteDatabase db)
        {
            this.collectionName = collectionName;
            this.db = db;
        }

        public T Get(string id)
        {
            return Get(new BsonValue(id));
        }

        public T Get(BsonValue id)
        {
            var collection = GetCollection();

            return collection.FindById(id);
        }

        public IEnumerable<T> GetAll()
        {
            var collection = GetCollection();

            return collection.FindAll();
        }

        public IEnumerable<T> Find(BsonExpression query)
        {
            var collection = GetCollection();

            return collection.Find(query);
        }

        public IEnumerable<T> FindAll()
        {
            var collection = GetCollection();

            return collection.FindAll();
        }

        public void Insert(T trace)
        {
            UseTransaction(() =>
            {
                var collection = GetCollection();
                collection.Insert(trace);
            });
        }

        public void BulkInsert(IEnumerable<T> traces)
        {
            UseTransaction(() =>
            {
                var collection = GetCollection();
                var ts = traces as T[] ?? traces.ToArray();
                collection.InsertBulk(ts, ts.Count());
            });
        }

        public void Update(T trace)
        {
            UseTransaction(() =>
            {
                var collection = GetCollection();
                collection.Update(trace);
            });
        }

        public void Update(IEnumerable<T> traces)
        {
            UseTransaction(() =>
            {
                var collection = GetCollection();
                collection.Update(traces);
            });
        }

        public void Upsert(T trace)
        {
            UseTransaction(() =>
            {
                var collection = GetCollection();
                collection.Upsert(trace);
            });
        }

        public void Upsert(IEnumerable<T> traces)
        {
            UseTransaction(() =>
            {
                var collection = GetCollection();
                collection.Upsert(traces);
            });
        }

        public void Delete(string id)
        {
            UseTransaction(() =>
            {
                var collection = GetCollection();
                collection.Delete(id);
            });
        }

        public void Delete(BsonValue id)
        {
            UseTransaction(() =>
            {
                var collection = GetCollection();
                collection.Delete(id);
            });
        }

        public void DeleteAll()
        {
            UseTransaction(() =>
            {
                var collection = GetCollection();
                collection.DeleteAll();
            });
        }

        protected ILiteCollection<T> GetCollection()
        {
            return db.GetCollection<T>(collectionName);
        }

        private void UseTransaction(Action action)
        {
            db.BeginTrans();

            try
            {
                action();

                db.Commit();
            }
            catch (LiteException liteDbEx)
            {
                // LiteDBに関するエラーが発生した場合の処理
                db.Rollback();

                Debug.LogError($"LiteDB error occurred: {liteDbEx.Message}");
            }
            catch (IOException ioEx)
            {
                // 入出力エラーの場合の処理
                db.Rollback();

                Debug.LogError($"IO error occurred: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException authEx)
            {
                // アクセス権限エラーの場合の処理
                db.Rollback();

                Debug.LogError($"Unauthorized access: {authEx.Message}");
            }
            catch (Exception ex)
            {
                // 他の全般的な例外処理
                db.Rollback();

                Debug.LogError($"Error occurred: {ex.Message}");
            }
        }
    }
}
