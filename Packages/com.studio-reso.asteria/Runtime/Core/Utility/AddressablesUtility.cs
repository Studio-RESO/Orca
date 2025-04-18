using System;
using UnityEditor;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
#endif

namespace Asteria
{
    public static class AddressablesUtility
    {
#if UNITY_EDITOR
        private static AddressableAssetSettings settings;
        private static AddressableAssetSettings Settings => AddressableAssetSettingsDefaultObject.Settings;

        public static bool IsEditorFastMode()
        {
            return Settings?.ActivePlayModeDataBuilder is BuildScriptFastMode;
        }

        // public static bool IsPackedMode()
        // {
        //     return Settings?.ActivePlayModeDataBuilder is BuildScriptPackedMode;
        // }

        public static bool IsPackedPlayMode()
        {
            // NOTE: Editorに設定が無い = Remoteのデータを読もうとしてるはず
            if (Settings == null)
            {
                return true;
            }

            return Settings.ActivePlayModeDataBuilder is BuildScriptPackedPlayMode;
        }

        public static void SetEditorFastMode()
        {
            SetPlayMode(typeof(BuildScriptFastMode));
        }

        // public static void SetPackedMode()
        // {
        //     SetPlayMode(typeof(BuildScriptPackedMode));
        // }

        public static void SetPackedPlayMode()
        {
            SetPlayMode(typeof(BuildScriptPackedPlayMode));
        }

        private static void SetPlayMode(Type type)
        {
            if (Settings == null)
            {
                return;
            }
            var index = 0;
            while (true)
            {
                var builder = Settings.GetDataBuilder(index);
                if (builder == null)
                {
                    break;
                }
                if (builder.GetType() == type)
                {
                    Settings.ActivePlayModeDataBuilderIndex = index;

                    break;
                }
                index++;
            }
        }

        public static string GetPlatform()
        {
            var platform = EditorUserBuildSettings.activeBuildTarget switch
            {
                BuildTarget.StandaloneOSX => RuntimePlatform.OSXPlayer,
                BuildTarget.StandaloneWindows => RuntimePlatform.WindowsPlayer,
                BuildTarget.StandaloneWindows64 => RuntimePlatform.WindowsPlayer,
                BuildTarget.iOS => RuntimePlatform.IPhonePlayer,
                BuildTarget.Android => RuntimePlatform.Android,
                _ => throw new ArgumentOutOfRangeException(),
            };

            return GetPlatformName(platform);
        }

        public static BuildTarget GetBuildTarget(RuntimePlatform platform)
        {
            return platform switch
            {
                RuntimePlatform.WindowsPlayer => BuildTarget.StandaloneWindows64,
                RuntimePlatform.OSXPlayer => BuildTarget.StandaloneOSX,
                RuntimePlatform.Android => BuildTarget.Android,
                RuntimePlatform.IPhonePlayer => BuildTarget.iOS,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        public static BuildTargetGroup GetBuildTargetGroup(RuntimePlatform platform)
        {
            return platform switch
            {
                RuntimePlatform.WindowsPlayer => BuildTargetGroup.Standalone,
                RuntimePlatform.OSXPlayer => BuildTargetGroup.Standalone,
                RuntimePlatform.Android => BuildTargetGroup.Android,
                RuntimePlatform.IPhonePlayer => BuildTargetGroup.iOS,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
#endif

        public static string GetPlatformName(RuntimePlatform platform)
        {
            return platform switch
            {
                RuntimePlatform.WindowsPlayer => "win",
                RuntimePlatform.WindowsEditor => "win",
                RuntimePlatform.OSXPlayer => "osx",
                RuntimePlatform.OSXEditor => "osx",
                RuntimePlatform.Android => "android",
                RuntimePlatform.IPhonePlayer => "ios",
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        public static RuntimePlatform GetRuntimePlatform(string platform)
        {
            var key = platform.ToLower();

            if (key.Contains("win", StringComparison.Ordinal))
            {
                return RuntimePlatform.WindowsPlayer;
            }
            if (key.Contains("osx", StringComparison.Ordinal))
            {
                return RuntimePlatform.OSXPlayer;
            }
            if (key.Contains("android", StringComparison.Ordinal))
            {
                return RuntimePlatform.Android;
            }
            if (key.Contains("ios", StringComparison.Ordinal))
            {
                return RuntimePlatform.IPhonePlayer;
            }

            throw new ArgumentOutOfRangeException();
        }

        public static async UniTask<IList<IResourceLocation>> LoadLocationsAsync(IEnumerable<string> label, Addressables.MergeMode mergeMode, CancellationToken token)
        {
            AsyncOperationHandle<IList<IResourceLocation>> handle = default;
            try
            {
                handle = Addressables.LoadResourceLocationsAsync(label, mergeMode);
                await handle.ToUniTask(cancellationToken: token);

                return handle.Status is AsyncOperationStatus.Succeeded ? handle.Result : Array.Empty<IResourceLocation>();
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            finally
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
        }
    }
}
