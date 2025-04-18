using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.ResourceProviders;
using Debug = UnityEngine.Debug;
using UniTask = Cysharp.Threading.Tasks.UniTask;

namespace Asteria
{
    public interface IAddressablesCatalog
    {
        string[] GetAddressesForLabel(IEnumerable<string> labels);
        UniTask<string[]> GetNonCachedAddressesAsync(IEnumerable<string> addresses, CancellationToken cancellationToken);
        UniTask<long> GetBundleSizeAsync(IEnumerable<string> address, CancellationToken cancellationToken);
        string[] FindAddresses(string serchKey);
    }

    internal class AddressablesCatalog : IDisposable, IAddressablesCatalog
    {
        private readonly Dictionary<string, ResourceLocationMap> locationMaps = new();

        private readonly AddressablesCacheTraceService cacheTraceService;

        public bool IsValid
        {
            get
            {
#if UNITY_EDITOR
                if (!AddressablesUtility.IsPackedPlayMode())
                {
                    return true;
                }
#endif
                return locationMaps.Count > 0;
            }
        }

        public AddressablesCatalog(AddressablesCacheTraceService cacheTraceService)
        {
            this.cacheTraceService = cacheTraceService;
        }

        public void Dispose()
        {
            locationMaps.Clear();
        }

        public void Unload()
        {
            Dispose();
        }

        public bool TryGetAddressInfo(string address, out string fileName, out AssetBundleRequestOptions result)
        {
            Assert.IsFalse(string.IsNullOrEmpty(address), "address is null or empty.");

            foreach (var map in locationMaps.Values)
            {
                if (!map.Locate(address, null, out var locations) || locations.Count == 0)
                {
                    continue;
                }

                var locate = locations[0];
                if (!locate.HasDependencies)
                {
                    continue;
                }

                foreach (var d in locate.Dependencies)
                {
                    if (d.Data is AssetBundleRequestOptions options)
                    {
                        fileName = d.PrimaryKey;
                        result = options;

                        return true;
                    }
                }
            }

            fileName = null;
            result = null;

            return false;
        }

        private long GetDownloadSize(string address)
        {
            if (TryGetAddressInfo(address, out var fileName, out var options))
            {
                var filePath = InternalCaching.FileNameToCachePath(fileName, options.Hash);

                // キャッシュされている場合はダウンロードサイズ0
                if (cacheTraceService.IsCached(filePath))
                {
                    return 0;
                }

                return options.BundleSize;
            }

            return 0;
        }

        private bool IsCached(string address)
        {
            if (TryGetAddressInfo(address, out var fileName, out var options))
            {
                var filePath = InternalCaching.FileNameToCachePath(fileName, options.Hash);

                return cacheTraceService.IsCached(filePath);
            }

            return false;
        }

        public bool IsExists(string address)
        {
            var key = address.ToLower();
            foreach (var map in locationMaps.Values)
            {
                return map.Locate(key, null, out _);
            }

            return false;
        }

        /// <summary>
        /// ラベルが設定されているアセットバンドルのアドレスを取得
        /// </summary>
        /// <param name="labels"></param>
        /// <returns></returns>
        public string[] GetAddressesForLabel(IEnumerable<string> labels)
        {
            var requests = labels.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToArray();
            if (requests.Length == 0)
            {
                return Array.Empty<string>();
            }

            var result = new HashSet<string>();

            foreach (var map in locationMaps.Values)
            {
                foreach (var label in requests)
                {
                    if (map.Locate(label, null, out var locations))
                    {
                        foreach (var location in locations)
                        {
                            if (location.HasDependencies && location.Dependencies[0].Data is AssetBundleRequestOptions)
                            {
                                result.Add(location.PrimaryKey);
                            }
                        }
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// カタログの取得
        /// </summary>
        public async UniTask LoadAsync(string url, CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            if (!AddressablesUtility.IsPackedPlayMode())
            {
                return;
            }
#endif
            try
            {
                Assert.IsFalse(string.IsNullOrEmpty(url), "key is null or empty.");

                var locator = await Addressables.LoadContentCatalogAsync(url, true).ToUniTask(cancellationToken: cancellationToken);
                if (locator == null)
                {
                    return;
                }

                if (locator is ResourceLocationMap map)
                {
                    locationMaps[url] = map;
                    Debug.Log($"catalog loaded: {url}");
                }
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception exception)
            {
                Debug.Log($"LoadAsync {exception}");
            }
        }

        /// <summary>
        /// 現在Cacheしているすべてのカタログを更新
        /// </summary>
        /// <param name="cancellationToken"></param>
        public async UniTask UpdateAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await UniTask.WhenAll(Enumerable.Select(locationMaps.Keys, x => LoadAsync(x, cancellationToken)));
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception exception)
            {
                Debug.Log($"cache clear {exception}");
            }
        }

        /// <summary>
        /// Localにキャッシュされてないアセットバンドルのアドレスを取得
        /// </summary>
        /// <param name="addresses"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async UniTask<string[]> GetNonCachedAddressesAsync(IEnumerable<string> addresses, CancellationToken cancellationToken)
        {
#if UNITY_EDITOR
            if (!AddressablesUtility.IsPackedPlayMode())
            {
                return Array.Empty<string>();
            }
#endif

            Assert.IsTrue(locationMaps.Count > 0, "location maps is empty.");

            try
            {
                return await UniTask.RunOnThreadPool(() =>
                {
                    var result = new List<string>();

                    var check = addresses.Where(x => !string.IsNullOrEmpty(x)).Select(x => x.ToLower()).Distinct().ToArray();
                    foreach (var c in check)
                    {
                        if (IsCached(c))
                        {
                            continue;
                        }
                        result.Add(c);
                    }

                    return result.ToArray();
                }, cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return Array.Empty<string>();
            }
            catch (Exception exception)
            {
                Debug.Log($"GetNonCachedAddressesAsync {exception}");

                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Localにキャッシュのないアセットバンドルのサイズを取得
        /// </summary>
        /// <param name="addresses"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async UniTask<long> GetBundleSizeAsync(IEnumerable<string> addresses, CancellationToken cancellationToken)
        {
#if UNITY_EDITOR
            if (!AddressablesUtility.IsPackedPlayMode())
            {
                return 0;
            }
#endif
            Assert.IsTrue(locationMaps.Count > 0, "location maps is empty.");

            try
            {
                return await UniTask.RunOnThreadPool(() =>
                {
                    var total = 0L;

                    var requests = addresses.Where(x => !string.IsNullOrEmpty(x)).Select(x => x.ToLower()).ToArray();
                    foreach (var a in requests)
                    {
                        if (IsCached(a))
                        {
                            continue;
                        }
                        total += GetDownloadSize(a);
                    }

                    return total;
                }, cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return 0;
            }
            catch (Exception exception)
            {
                Debug.Log($"GetBundleSizeAsync {exception}");

                return 0;
            }
        }

        public string[] GetAllAddresses()
        {
            var result = new HashSet<string>();

            foreach (var map in locationMaps.Values)
            {
                foreach (var locations in map.Locations.Values)
                {
                    foreach (var location in locations)
                    {
                        if (location.HasDependencies && location.Dependencies[0].Data is AssetBundleRequestOptions)
                        {
                            result.Add(location.PrimaryKey);

                            break;
                        }
                    }
                }
            }

            return result.ToArray();
        }

        public string[] FindAddresses(string serchKey)
        {
            var result = new HashSet<string>();

            foreach (var map in locationMaps.Values)
            {
                foreach (var locations in map.Locations.Values)
                {
                    foreach (var location in locations)
                    {
                        if (location.HasDependencies && location.Dependencies[0].Data is AssetBundleRequestOptions)
                        {
                            if (location.PrimaryKey.CustomStartsWith(serchKey))
                            {
                                result.Add(location.PrimaryKey);
                            }
                        }
                    }
                }
            }

            return result.ToArray();
        }
    }

}
