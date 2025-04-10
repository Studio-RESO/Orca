// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Reflection;
// using UnityEditor;
// using UnityEditor.AddressableAssets;
// using UnityEditor.AddressableAssets.Settings;
// using UnityEditor.AddressableAssets.Settings.GroupSchemas;
// using UnityEngine.ResourceManagement.Util;
// using Debug = UnityEngine.Debug;
// using Object = UnityEngine.Object;
//
// public sealed class AddressableCatalogCreator
//     {
//         private class Item
//         {
//             public readonly string AddressPrefix;
//             public readonly int FindLayer;
//             public readonly string FindDirectory;
//             public readonly string Extension;
//             public readonly string[] Labels;
//             public readonly bool IsVariantOnly;
//
//             public Item(string addressPrefix, int findLayer, string extension, params string[] labels)
//             {
//                 AddressPrefix = addressPrefix;
//                 FindLayer = findLayer;
//                 Extension = extension;
//                 Labels = labels;
//             }
//
//             public Item(string addressPrefix, string findDirectory, string extension, params string[] labels)
//             {
//                 AddressPrefix = addressPrefix;
//                 FindDirectory = findDirectory;
//                 Extension = extension;
//                 Labels = labels;
//             }
//
//             public Item(string addressPrefix, int findLayer, string extension, bool isVariantOnly, params string[] labels)
//             {
//                 AddressPrefix = addressPrefix;
//                 FindLayer = findLayer;
//                 Extension = extension;
//                 IsVariantOnly = isVariantOnly;
//                 Labels = labels;
//             }
//         }
//
//         private const string Root = "Assets/ExternalAssets/";
//
//         private readonly AddressableAssetSettings addressableSettings;
//
//         private readonly string[] keepGroups =
//         {
//             "Default Local Group",
//             "Built In Data",
//         };
//         // pack separately group
//         private readonly Dictionary<string, Item[]> _settings = new()
//         {
//             ["CommonAssets"] = new[]
//             {
//                 new Item("Common", "*", "*.png", "common"),
//                 new Item("Common", "*", "*.hlsl", "common"),
//                 new Item("Common", "*", "*.shader", "common"),
//                 new Item("Common", "*", "*.physicMaterial", "common"),
//             },
//             ["Effects"] = new[]
//             {
//                 new Item("effect", 1, "*.prefab", "effect"),
//             },
//             ["Area"] = new[]
//             {
//                 new Item("area", 2, "*.unity", "area", "scene/area"),
//                 new Item("area", 2, "*_LightProbe.asset", "area"),
//             },
//             ["Area/CommonAssets"] = new[]
//             {
//                 new Item("area/common", 1, "*.prefab", true, "common/area"),
//                 new Item("area/common", 2, "*.prefab", true, "common/area"),
//             },
//             ["Npc"] = new[]
//             {
//                 new Item("npc", 2, "*.prefab", "npc"),
//             },
//             ["Area/Event"] = new[]
//             {
//                 new Item("area/effect", 1, "*.prefab", "area"),
//                 new Item("area/avatar", 1, "*.asset", "area"),
//             },
//             ["Livly"] = new[]
//             {
//                 new Item("master/livly", "Master", "*.asset", "master/livly"),
//                 new Item("thumb/livly", "Thumbnail", "*.png", "thumb/livly"),
//                 new Item("thumb/meteor", "Meteor", "*.png", "thumb/meteor"),
//             },
//             ["Avatar"] = new[]
//             {
//                 new Item("fashion", 3, "*.prefab", "fashion"),
//                 new Item("thumb/fashion", "Thumbnail", "*.png", "thumb/fashion"),
//             },
//             ["House"] = new[]
//             {
//                 new Item("house", 3, "*.prefab", "house"),
//                 new Item("house", "Diff", "*.prefab", "house"),
//                 new Item("thumb/house", "Thumbnail", "*.png", "thumb/house"),
//                 new Item("house/wallpaper", "WallPaper", "*.mat", "house/wallpaper"),
//                 new Item("house/blocklight", "BlockLight", "*.asset", "house/blocklight"),
//             },
//             ["Thumbnail"] = new[]
//             {
//                 new Item("thumb/element", "Element", "*.png", "thumb/element"),
//                 new Item("thumb/feed", "Feed", "*.png", "thumb/feed"),
//                 new Item("thumb/system", "System", "*.png", "thumb/system"),
//             },
//             ["TmpMaster"] = new[]
//             {
//                 new Item("master", "House", "*.json", "master"),
//                 new Item("master", "Fashion", "*.txt", "master"),
//             },
//             ["Audio"] = new[]
//             {
//                 new Item("audio/bgm", "BGM", "*.asset", "audio/bgm"),
//                 new Item("audio/se", "SE", "*.asset", "audio/se"),
//                 new Item("audio/environment", "Environment", "*.asset", "audio/environment"),
//             },
//         };
//
//         // pack together group
//         private readonly Dictionary<string, Item[]> _packTogetherSettings = new()
//         {
//             ["Livly"] = new[]
//             {
//                 new Item("livly", 2, "*.prefab", "livly"),
//                 new Item("anim/livly", "Animation", "*.anim", "livly"),
//                 new Item("texture/livly", "Texture", "*.png", "livly"),
//             },
//         };
//
//         public AddressableCatalogCreator()
//         {
//             addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
//         }
//
//         public void RunDefault()
//         {
//             if (!Directory.Exists(Root))
//             {
//                 return;
//             }
//
//             FindOrCreateGroup("Default Local Group");
//
//             foreach (var (key, value) in _settings)
//             {
//                 var directory = Directory.GetDirectories(Root, key, SearchOption.TopDirectoryOnly).FirstOrDefault();
//                 if (string.IsNullOrEmpty(directory))
//                 {
//                     Debug.LogWarning($"Directory not found. {key}");
//
//                     continue;
//                 }
//
//                 foreach (var item in value)
//                 {
//                     UpdateGroup(directory, item);
//                 }
//             }
//
//             foreach (var (key, value) in _packTogetherSettings)
//             {
//                 var directory = Directory.GetDirectories(Root, key, SearchOption.TopDirectoryOnly).FirstOrDefault();
//                 if (string.IsNullOrEmpty(directory))
//                 {
//                     Debug.LogWarning($"Directory not found. {key}");
//
//                     continue;
//                 }
//
//                 UpdateLivlyGroup(directory, value);
//             }
//
//             for (var i = addressableSettings.groups.Count - 1; i >= 0; --i)
//             {
//                 var group = addressableSettings.groups[i];
//
//                 foreach (var entry in group.entries.ToArray())
//                 {
//                     if (!File.Exists(entry.AssetPath) && !Directory.Exists(entry.AssetPath))
//                     {
//                         group.RemoveAssetEntry(entry);
//                     }
//                 }
//
//                 if (group.entries.Count <= 0 && !keepGroups.Contains(group.name))
//                 {
//                     addressableSettings.RemoveGroup(group);
//                 }
//             }
//             RemoveMissingGroupReferences();
//
//             AssetDatabase.SaveAssets();
//
//             Debug.Log("done asset bundle name set.");
//         }
//
//         public void RemoveAll()
//         {
//             for (var i = addressableSettings.groups.Count - 1; i >= 0; i--)
//             {
//                 var group = addressableSettings.groups[i];
//                 if (keepGroups.Contains(group.name))
//                 {
//                     continue;
//                 }
//                 addressableSettings.RemoveGroup(group);
//             }
//             AssetDatabase.SaveAssets();
//         }
//
//         private void RemoveMissingGroupReferences()
//         {
//             var settings = AddressableAssetSettingsDefaultObject.Settings;
//             var type = typeof(AddressableAssetSettings);
//             var methodInfo = type.GetMethod("RemoveMissingGroupReferences", BindingFlags.Instance | BindingFlags.NonPublic);
//
//             methodInfo.Invoke(settings, new object[0]);
//         }
//
//         private void UpdateGroup(string directoryPath, Item item)
//         {
//             var group = FindOrCreateGroup(item.AddressPrefix.Replace("/", "-"));
//             if (group == null)
//             {
//                 Debug.LogError("Group not found.");
//
//                 return;
//             }
//
//             IEnumerable<string> paths;
//             if (!string.IsNullOrEmpty(item.FindDirectory))
//             {
//                 paths = Directory.EnumerateDirectories(directoryPath, item.FindDirectory, SearchOption.AllDirectories);
//             }
//             else
//             {
//                 var count = directoryPath.Count(x => x == Path.DirectorySeparatorChar);
//
//                 paths = Directory.EnumerateDirectories(directoryPath, "*", SearchOption.AllDirectories).Where(x =>
//                 {
//                     var c = x.Count(y => y == Path.DirectorySeparatorChar);
//
//                     return c == count + item.FindLayer;
//                 });
//             }
//
//             foreach (var path in paths)
//             {
//                 var files = Directory.EnumerateFiles(path, item.Extension, SearchOption.TopDirectoryOnly);
//                 foreach (var assetPath in files)
//                 {
//                     AddAsset(assetPath, item, group);
//                 }
//             }
//         }
//
//         private void UpdateLivlyGroup(string directoryPath, Item[] items)
//         {
//             var filePath = Path.GetFileName(directoryPath);
//
//             var count = directoryPath.Count(x => x == Path.DirectorySeparatorChar);
//             var paths = Directory.EnumerateDirectories(directoryPath, "*", SearchOption.AllDirectories).Where(x =>
//             {
//                 var c = x.Count(y => y == Path.DirectorySeparatorChar);
//
//                 return c == count + 2 && Path.GetFileName(x).StartsWith("LV");
//             });
//             foreach (var path in paths)
//             {
//                 var groupName = $"{filePath}-{Path.GetFileName(path)}".ToLower();
//                 var group = FindOrCreateGroup(groupName, BundledAssetGroupSchema.BundlePackingMode.PackTogether);
//                 if (group == null)
//                 {
//                     Debug.LogError("Group not found.");
//
//                     continue;
//                 }
//                 foreach (var item in items)
//                 {
//                     IEnumerable<string> files;
//                     if (!string.IsNullOrEmpty(item.FindDirectory))
//                     {
//                         files = Directory.EnumerateFiles($"{path}/{item.FindDirectory}", item.Extension, SearchOption.TopDirectoryOnly);
//                     }
//                     else
//                     {
//                         files = Directory.EnumerateFiles(path, item.Extension, SearchOption.TopDirectoryOnly);
//                     }
//                     foreach (var assetPath in files)
//                     {
//                         AddAsset(assetPath, item, group);
//                     }
//                 }
//             }
//         }
//
//         private void AddAsset(string assetPath, Item item, AddressableAssetGroup group)
//         {
//             if (item.IsVariantOnly)
//             {
//                 var go = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
//                 var assetType = PrefabUtility.GetPrefabAssetType(go);
//                 if (assetType != PrefabAssetType.Variant)
//                 {
//                     return;
//                 }
//             }
//
//             // アセットのGUIDを取得
//             var guid = AssetDatabase.AssetPathToGUID(assetPath);
//
//             // 既にグループにアセットが含まれているかチェックしなければ追加
//             var entry = addressableSettings.FindAssetEntry(guid) ?? addressableSettings.CreateOrMoveEntry(guid, group);
//             if (entry.parentGroup != group)
//             {
//                 addressableSettings.MoveEntry(entry, group);
//             }
//             foreach (var label in item.Labels)
//             {
//                 entry.SetLabel(label, true, true);
//             }
//             entry.address = $"{item.AddressPrefix}/{Path.GetFileNameWithoutExtension(assetPath)}".ToLower();
//         }
//
//         private AddressableAssetGroup FindOrCreateGroup(string groupName, BundledAssetGroupSchema.BundlePackingMode mode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately)
//         {
//             var group = addressableSettings.groups.FirstOrDefault(x => x?.name == groupName);
//             if (group == null)
//             {
//                 group = addressableSettings.CreateGroup(groupName, false, false, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
//             }
//
//             var schema = group.GetSchema<BundledAssetGroupSchema>();
//             if (schema == null)
//             {
//                 return group;
//             }
//             schema.UseAssetBundleCrc = true;
//             schema.BundleMode = mode;
//             schema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.OnlyHash;
//             schema.IncludeInBuild = true;
//             schema.IncludeAddressInCatalog = true;
//             schema.IncludeGUIDInCatalog = false;
//             schema.IncludeLabelsInCatalog = true;
//             schema.RetryCount = 3;
//             schema.Timeout = 10;
//
//             schema.BuildPath.SetVariableByName(addressableSettings, AddressableAssetSettings.kRemoteBuildPath);
//             schema.LoadPath.SetVariableByName(addressableSettings, AddressableAssetSettings.kRemoteLoadPath);
//
//             SetAssetBundleProviderType(schema, typeof(CustomAssetBundleProvider));
//
//             return group;
//         }
//
//         private static void SetAssetBundleProviderType(BundledAssetGroupSchema schema, Type assetBundleProviderType)
//         {
//             const string name = "m_AssetBundleProviderType";
//             const BindingFlags attr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
//
//             var serializedType = new SerializedType
//             {
//                 Value = assetBundleProviderType,
//                 ValueChanged = true,
//             };
//
//             var type = typeof(BundledAssetGroupSchema);
//             var value = type.GetField(name, attr);
//
//             if (value != null)
//             {
//                 value.SetValue(schema, serializedType);
//             }
//         }
//     }
