using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace Asteria.Editor
{
    public static class AddressableTool
    {
        [MenuItem("Tools/Asteria/Build Assets")]
        public static void BuildAddressable()
        {
            AddressableBuilder.BuildCrypto();
        }

        [MenuItem("Tools/Asteria/Build Catalog Only")]
        public static void BuildOnlyCatalog()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var temp = new Dictionary<string, (bool, bool)>();

            foreach (var group in settings.groups)
            {
                var schema = group.GetSchema<BundledAssetGroupSchema>();
                if (schema == null)
                {
                    continue;
                }

                var buildPath = schema.BuildPath.GetName(settings);
                temp[group.name] = (schema.IncludeInBuild, buildPath.Contains("Local"));

                schema.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kLocalBuildPath);
                schema.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kLocalLoadPath);
                schema.IncludeInBuild = false;

                EditorUtility.SetDirty(group);
            }
            AssetDatabase.SaveAssets();

            AddressableAssetSettings.BuildPlayerContent();

            foreach (var group in settings.groups)
            {
                var schema = group.GetSchema<BundledAssetGroupSchema>();
                if (schema == null)
                {
                    continue;
                }

                if (temp.TryGetValue(group.name, out (bool includeInBuild, bool local) value))
                {
                    schema.IncludeInBuild = value.includeInBuild;

                    schema.BuildPath.SetVariableByName(settings, value.local ? AddressableAssetSettings.kLocalBuildPath : AddressableAssetSettings.kRemoteBuildPath);
                    schema.LoadPath.SetVariableByName(settings, value.local ? AddressableAssetSettings.kLocalLoadPath : AddressableAssetSettings.kRemoteLoadPath);
                }
                EditorUtility.SetDirty(group);
            }
            AssetDatabase.SaveAssets();

            Debug.Log("Catalog only build complete.");
        }
    }
}
