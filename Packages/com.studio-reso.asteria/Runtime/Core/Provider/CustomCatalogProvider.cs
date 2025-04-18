using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets.ResourceProviders;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.Scripting;

namespace Asteria
{
    [DisplayName("Custom Content Catalog Provider")]
    [Preserve]
    internal class CustomCatalogProvider : ContentCatalogProvider
    {
        private class CacheData
        {
            public string hash;
            public ContentCatalogData data;
            public DateTime lastAccessTime;
        }

        private readonly Dictionary<string, CacheData> cache = new();

        [Preserve]
        public CustomCatalogProvider(ResourceManager resourceManager) : base(resourceManager) { }

        private bool IsPackedPlayMode()
        {
#if UNITY_EDITOR
            return AddressablesUtility.IsPackedPlayMode();
#else
            return true;
#endif
        }

        public override async void Provide(ProvideHandle providerInterface)
        {
            try
            {
                var url = providerInterface.Location.InternalId;

                // NOTE: Addressablesの初期化時にLocalに存在するCatalogを読み込もうとしてリクエストが勝手に飛んでくる
                if (File.Exists(url))
                {
                    url = "file:///" + Path.GetFullPath(url);
                }

                // NOTE: PackedPlayMode(本番)の場合はRemoteにあるカタログだけを参照したいので、Localのカタログは無視する
                if (IsPackedPlayMode() && !url.StartsWith("https://"))
                {
                    url = InternalContext.Context.Remote.CatalogUrl;
                }

                if (cache.TryGetValue(url, out var check) && check.data != null && DateTime.UtcNow - check.lastAccessTime < TimeSpan.FromMinutes(3))
                {
                    providerInterface.Complete(check.data, true, null);

                    return;
                }

                var hash = "";
                var catalogCompress = url.CustomEndsWith(".bin");

                // HashのチェックをするのはPackedPlayModeの場合のみ
                if (IsPackedPlayMode())
                {
                    var extension = catalogCompress ? ".bin" : ".json";
                    var hashUrl = url.Replace(extension, ".hash");
                    if (!hashUrl.CustomEndsWith(".hash"))
                    {
                        hashUrl += ".hash";
                    }
                    hash = await LoadTextAsync(hashUrl, false);

                    if (cache.TryGetValue(url, out var hit) && hit.hash == hash)
                    {
                        hit.lastAccessTime = DateTime.UtcNow;
                        providerInterface.Complete(hit.data, true, null);

                        return;
                    }
                }

                var catalogJson = await LoadTextAsync(url, catalogCompress);

                var data = JsonUtility.FromJson<ContentCatalogData>(catalogJson);

                var type = typeof(ContentCatalogData);

                type.GetField("location", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(data, providerInterface.Location);
                type.GetField("localHash", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(data, hash);

                cache[url] = new CacheData
                {
                    hash = hash,
                    data = data,
                    lastAccessTime = DateTime.UtcNow,
                };

                providerInterface.Complete(data, true, null);
            }
            catch (Exception e)
            {
                providerInterface.Complete<ContentCatalogData>(null, false, new Exception($"url: {providerInterface.Location.InternalId}\n{e.Message}\n{e.StackTrace}"));
            }
        }

        private async UniTask<string> LoadTextAsync(string url, bool decompress)
        {
            var req = UnityWebRequest.Get(url);
            await req.SendWebRequest().ToUniTask();

            if (req.IsError())
            {
                throw new Exception($"url: {url}\n{req.error}");
            }

            if (!decompress)
            {
                return req.downloadHandler.text;
            }

            using var inputStream = new MemoryStream(req.downloadHandler.data);
            await using var brotliStream = new BrotliStream(inputStream, CompressionMode.Decompress);

            using var outputStream = new MemoryStream();
            await brotliStream.CopyToAsync(outputStream);

            var decompressedBytes = outputStream.ToArray();

            return Encoding.UTF8.GetString(decompressedBytes);
        }

        public override void Release(IResourceLocation location, object obj)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            var key = location.InternalId;
            if (!key.StartsWith("https://"))
            {
                return;
            }

            if (cache.ContainsKey(key))
            {
                cache.Remove(key);
            }
        }
    }
}
