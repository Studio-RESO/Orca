using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;

namespace Asteria.Editor
{
    public static class AddressableBuilder
    {
        public static void Build(bool catalogCompress = true)
        {
            Build(catalogCompress, false).Forget();
        }

        public static void BuildCrypto(bool catalogCompress = true)
        {
            Build(catalogCompress, true).Forget();
        }

        private static async UniTask Build(bool catalogCompress, bool crypto)
        {
            var cancellationToken = new CancellationTokenSource().Token;

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            using var scope = OverrideSettings(settings);

            try
            {
                AddressableAssetSettings.BuildPlayerContent();

                if (catalogCompress)
                {
                    await CompressCatalogAsync(settings, cancellationToken);
                }

                await CleanUnnecessaryBundle(settings, cancellationToken);

                if (crypto)
                {
                    await EncryptBundle(settings, cancellationToken);
                }

                Debug.Log("Build Complete");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private static IDisposable OverrideSettings(AddressableAssetSettings settings)
        {
            var profileId = settings.activeProfileId;
            var remoteBuildPath = settings.profileSettings.GetValueByName(profileId, AddressableAssetSettings.kRemoteBuildPath);

            settings.DisableCatalogUpdateOnStartup = true;
            settings.OverridePlayerVersion = "0.0.0";

            settings.profileSettings.SetValue(profileId, AddressableAssetSettings.kRemoteBuildPath, $"ServerData/{AddressablesUtility.GetPlatform()}");

            return Disposable.Create((settings, profileId, remoteBuildPath), state =>
            {
                var (set, id, path) = state;
                set.profileSettings.SetValue(id, AddressableAssetSettings.kRemoteBuildPath, path);
            });
        }

        private static async UniTask CompressCatalogAsync(AddressableAssetSettings settings, CancellationToken cancellationToken)
        {
            var directory = settings.RemoteCatalogBuildPath.GetValue(settings);

            var fileName = string.IsNullOrEmpty(settings.OverridePlayerVersion) ? "catalog" : $"catalog_{settings.OverridePlayerVersion}";

            var originPath = Path.Combine(directory, $"{fileName}.json");
            var outputPath = Path.Combine(directory, $"{Path.GetFileNameWithoutExtension(originPath)}.bin");

            var json = await File.ReadAllTextAsync(originPath, cancellationToken);

            // カタログが平文なので圧縮
            var byteArray = Encoding.UTF8.GetBytes(json);
            await using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            await using var compressionStream = new BrotliStream(fileStream, CompressionMode.Compress);

            compressionStream.Write(byteArray, 0, byteArray.Length);

            Debug.Log($"Compress catalog: {outputPath}");
        }

        private static async UniTask<ContentCatalogData> LoadCatalogAsync(AddressableAssetSettings settings, CancellationToken cancellationToken)
        {
            var directory = settings.RemoteCatalogBuildPath.GetValue(settings);

            var catalogFileName = string.IsNullOrEmpty(settings.OverridePlayerVersion) ? "catalog" : $"catalog_{settings.OverridePlayerVersion}";
            var originPath = Path.Combine(directory, $"{catalogFileName}.json");
            var json = await File.ReadAllTextAsync(originPath, cancellationToken);

            return JsonUtility.FromJson<ContentCatalogData>(json);
        }

        private static async UniTask CleanUnnecessaryBundle(AddressableAssetSettings settings, CancellationToken cancellationToken)
        {
            try
            {
                var fileNames = new HashSet<string>();

                var labels = settings.GetLabels();
                var locations =
                    await AddressablesUtility.LoadLocationsAsync(labels, Addressables.MergeMode.Union, cancellationToken);

                foreach (var location in locations)
                {
                    var id = location.InternalId;
                    if (id.CustomEndsWith(".bundle"))
                    {
                        fileNames.Add(Path.GetFileName(id));
                    }
                }

                var profileId = settings.activeProfileId;
                var buildPath = settings.profileSettings.GetValueByName(profileId, AddressableAssetSettings.kRemoteBuildPath);

                foreach (var path in Directory.EnumerateFiles(buildPath, "*", SearchOption.AllDirectories))
                {
                    if (path.IndexOf(".bundle", StringComparison.Ordinal) == -1)
                    {
                        continue;
                    }
                    var fileName = Path.GetFileName(path);
                    if (fileNames.Contains(fileName))
                    {
                        continue;
                    }

                    File.Delete(path);
                    Debug.Log($"Delete Unnecessary Bundle: {fileName}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            Debug.Log("Clean Unnecessary Bundle");
        }

        private static async UniTask EncryptBundle(AddressableAssetSettings settings, CancellationToken cancellationToken)
        {
            try
            {
                var fileNames = new HashSet<string>();

                var labels = settings.GetLabels();
                var locations =
                    await AddressablesUtility.LoadLocationsAsync(labels, Addressables.MergeMode.Union, cancellationToken);

                foreach (var location in locations)
                {
                    var id = location.InternalId;
                    if (id.CustomEndsWith(".bundle"))
                    {
                        fileNames.Add(Path.GetFileName(id));
                    }
                }

                var profileId = settings.activeProfileId;
                var buildPath = settings.profileSettings.GetValueByName(profileId, AddressableAssetSettings.kRemoteBuildPath);

                foreach (var path in Directory.EnumerateFiles(buildPath, "*.bundle", SearchOption.AllDirectories))
                {
                    var fileName = Path.GetFileName(path);
                    if (!fileNames.Contains(fileName))
                    {
                        continue;
                    }

                    await EncryptFile(path, path, "password", Encoding.UTF8.GetBytes(fileName), 128);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            Debug.Log("Encrypt Bundle");
        }

        private static async UniTask EncryptFile(string inputFile, string outputFile, string password, byte[] salt, int encriptionLength)
        {
            if (!File.Exists(inputFile))
            {
                Debug.LogError($"File not found: {inputFile}");

                return;
            }

            var equal = string.Equals(inputFile, outputFile, StringComparison.OrdinalIgnoreCase);
            if (equal)
            {
                var tempFile = outputFile + ".tmp";
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
                File.Move(inputFile, tempFile);
                inputFile = tempFile;
            }

            await using var inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, true);
            await using var outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
            await using var aesStream = new SeekableAesStream(outputStream, password, salt, encriptionLength);
            await inputStream.CopyToAsync(aesStream);

            if (equal)
            {
                File.Delete(inputFile);
            }
        }
    }
}
