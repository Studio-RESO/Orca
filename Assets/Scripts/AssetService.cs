using System;
using System.IO;
using System.Net.Http;
using System.Security;
using System.Threading;
using Asteria;
using Cysharp.Net.Http;
using Cysharp.Threading.Tasks;
using UnityEngine;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace Orca.Example
{
    public class AssetService : IDisposable
    {
        private const string DBName = "caches.db";
        private const string DBPassword = "Kd!h+2zseg~E";
        private const int MaxConcurrent = 8;
        
        private readonly AddressableService addressableService;
        private readonly LiteDatabaseOperator databaseOperator;
        
        private readonly HttpClient httpClient;
        
        private float networkUnreachableReceiveTime;
        
        public WwiseAssetLoader WwiseAssetLoader { get; private set; }

        public AssetService(string baseUrl, string catalogName, SecureData secureData = null)
        {
            Debug.Log($"AssetService: {baseUrl}, {catalogName}");
            var cachePath = Application.persistentDataPath;

#if UNITY_EDITOR
            if (Application.installMode is ApplicationInstallMode.Editor)
            {
                cachePath = Path.Combine(Application.dataPath, "../cache");
            }
#endif

            var bundleCachePath = Path.Combine(cachePath, "aa");

#if UNITY_IOS
            Device.SetNoBackupFlag(cachePath);
#endif
            
            var handler = new YetAnotherHttpHandler { Http2Only = true };
            httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(10),
            };
            
            var secureString = new SecureString();
            foreach (var c in DBPassword)
            {
                secureString.AppendChar(c);
            }
            secureString.MakeReadOnly();
            
            databaseOperator = new LiteDatabaseOperator(DBName, cachePath, secureString);
            
            var remote = new RemoteAccessData(baseUrl, catalogName, MaxConcurrent);
            var context = new DefaultContext(httpClient, bundleCachePath, remote, secureData);
            
            Debug.Log(context.Remote.CatalogUrl);
            addressableService = new AddressableService(context, databaseOperator.CreateAddressablesCacheTraceService());
            
            WwiseAssetLoader = new WwiseAssetLoader(addressableService);
            
            addressableService.OnNetworkUnreachableException += OnNetworkUnreachableException;
        }
        
        public void Dispose()
        {
            
        }
        
        public UniTask<bool> InitializeAsync(CancellationToken cancellationToken)
        {
            if (addressableService.Initialized)
            {
                return UniTask.FromResult(true);
            }

            return addressableService.InitializeAsync(cancellationToken);
        }
        
        public UniTask UpdateCatalogAsync(CancellationToken cancellationToken)
        {
            return addressableService.UpdateCatalogAsync(cancellationToken);
        }
        
        private void OnNetworkUnreachableException(Exception e)
        {
            if (Time.realtimeSinceStartup - networkUnreachableReceiveTime < 1)
            {
                return;
            }
            networkUnreachableReceiveTime = Time.realtimeSinceStartup;

            addressableService.Cancel();

            Debug.LogWarning(e);
        }
    }
}
