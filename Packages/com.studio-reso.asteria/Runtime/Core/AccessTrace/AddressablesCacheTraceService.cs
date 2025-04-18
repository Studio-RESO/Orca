using System;
using System.Collections.Generic;
using System.Linq;

namespace Asteria
{
    public class AddressablesCacheTraceService
    {
        private readonly IDataRepository<AssetBundleLoadingTrace> loadingTraceRepository;
        private readonly IDataRepository<AssetBundleTotalCaches> totalCachesRepository;

        public AddressablesCacheTraceService(IDataRepository<AssetBundleLoadingTrace> loadingTraceRepository, IDataRepository<AssetBundleTotalCaches> totalCachesRepository)
        {
            this.loadingTraceRepository = loadingTraceRepository;
            this.totalCachesRepository = totalCachesRepository;
        }

        public bool IsCached(string id)
        {
            return loadingTraceRepository.Get(id) != null;
        }

        public void UpdateAccess(AssetBundleLoadingTrace trace)
        {
            loadingTraceRepository.Upsert(trace);
        }

        public void DeleteAccess(string id)
        {
            loadingTraceRepository.Delete(id);
        }

        public IEnumerable<AssetBundleLoadingTrace> GetTraceOrderByTimeStamp()
        {
            return loadingTraceRepository.FindAll().OrderBy(t => t.TimeStamp);
        }

        public void InsertTotalCaches(AssetBundleTotalCaches totalCaches)
        {
            totalCachesRepository.Insert(totalCaches);
        }

        public void UpdateTotalCaches(AssetBundleTotalCaches totalCaches)
        {
            totalCachesRepository.Update(totalCaches);
        }

        public void DeleteAllTotalCaches()
        {
            totalCachesRepository.DeleteAll();
            totalCachesRepository.Insert(new AssetBundleTotalCaches { TotalBundleSize = 0, Count = 0, CreatedAt = DateTime.Now });
        }

        public AssetBundleTotalCaches GetTotalCachesLatest()
        {
            return totalCachesRepository.FindAll().OrderByDescending(t => t.CreatedAt).FirstOrDefault();
        }

    }
}
