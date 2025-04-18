using System;
using System.IO;
using System.Net.Http;
using System.Security;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Asteria
{
    public interface IContext : IDisposable
    {
        string CachePath
        {
            get;
        }

        RemoteAccessData Remote
        {
            get;
        }

        SecureData Security
        {
            get;
        }

        IWebRequest<AssetBundle> WebRequest
        {
            get;
        }
    }

    public class RemoteAccessData
    {
        private readonly string defaultCatalogName;
        public string BaseUrl
        {
            get;
        }

        public int MaxConcurrent
        {
            get;
        }

        public string CatalogUrl => Path.Combine(BaseUrl, defaultCatalogName);

        public string[] SubCatalogUrls
        {
            get;
        }

        public RemoteAccessData(string baseUrl, string defaultCatalogName, int maxConcurrent, params string[] subCtalogUrls)
        {
            Assert.IsFalse(string.IsNullOrEmpty(baseUrl), "base url is null or empty.");
            Assert.IsFalse(string.IsNullOrEmpty(defaultCatalogName), "catalog name is null or empty.");

            var os = AddressablesUtility.GetPlatformName(Application.platform);
            Assert.IsFalse(string.IsNullOrEmpty(os), "not supported platform.");

            this.defaultCatalogName = defaultCatalogName;

            BaseUrl = Path.Combine(baseUrl, os);
            MaxConcurrent = maxConcurrent;
            SubCatalogUrls = subCtalogUrls;
        }
    }

    public class SecureData : IDisposable
    {
        public SecureString Password
        {
            get;
        }

        public int EncriptionLength
        {
            get;
        }

        public SecureData(SecureString password, int encriptionLength)
        {
            Password = password;
            EncriptionLength = encriptionLength;
        }

        public void Dispose()
        {
            Password?.Dispose();
        }

        public bool IsValid()
        {
            return Password is { Length: > 0 } && EncriptionLength > 0;
        }
    }

    public class DefaultContext : IContext
    {
        private readonly string url;

        public string CachePath
        {
            get;
        }

        public RemoteAccessData Remote
        {
            get;
        }

        public SecureData Security
        {
            get;
        }

        public IWebRequest<AssetBundle> WebRequest
        {
            get;
        }

        public DefaultContext(HttpClient httpClient, string cachePath, RemoteAccessData remoteData, SecureData security)
        {
            Assert.IsNotNull(httpClient, "http client is null.");
            Assert.IsFalse(string.IsNullOrEmpty(cachePath), "cache path is null or empty.");
            Assert.IsNotNull(remoteData, "remote data is null.");

            CachePath = cachePath;
            Remote = remoteData;
            Security = security;

            url = Path.Combine(Remote.BaseUrl, "bundle");

            WebRequest = new FileWebRequest<AssetBundle>(httpClient, Remote.MaxConcurrent, cachePath);

            Addressables.ResourceManager.InternalIdTransformFunc += ReplaceBundleUrl;
        }

        public void Dispose()
        {
            Addressables.ResourceManager.InternalIdTransformFunc += ReplaceBundleUrl;

            Security?.Dispose();
            WebRequest.Dispose();
        }

        private string ReplaceBundleUrl(IResourceLocation location)
        {
            return location.InternalId.Replace("https://Host", url);
        }
    }
}
