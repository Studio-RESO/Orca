using System.Collections.Generic;
using LiteDB;

namespace Asteria
{
    public interface IDataRepository<T> where T : class
    {
        T Get(string id);
        IEnumerable<T> Find(BsonExpression query);
        IEnumerable<T> FindAll();

        void Insert(T trace);
        void Update(T trace);
        void Upsert(T trace);
        void Delete(string id);
        void DeleteAll();
    }
}
