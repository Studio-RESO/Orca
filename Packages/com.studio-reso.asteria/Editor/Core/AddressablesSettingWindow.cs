using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace Asteria.Editor
{
    public class AddressablesSettingWindow : EditorWindow
    {
        // JSONシリアライズのためのラッパークラス
        [System.Serializable]
        internal class StringListWrapper
        {
            public List<string> items = new();
        }

        // デフォルトの検索ディレクトリ
        private string targetDirectory = "Assets/App/ExternalAssets";

        // グループ名のリスト
        private List<string> groupNames = new List<string> { };
        private string newGroupName = "";

        // 拡張子リスト
        private List<string> fileExtensions = new List<string> { };
        private string newExtension = "";

        // スクロール位置
        private Vector2 groupScrollPosition;
        private Vector2 extensionScrollPosition;

        // EditorPrefsのキー
        private const string PrefsKeyPrefix = "AddressablesSettingWindow_";
        private const string TargetDirectoryKey = PrefsKeyPrefix + "TargetDirectory";
        private const string GroupNamesKey = PrefsKeyPrefix + "GroupNames";
        private const string FileExtensionsKey = PrefsKeyPrefix + "FileExtensions";

        [MenuItem("Tools/Asteria/Addressables Group Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<AddressablesSettingWindow>("Addressables Setting");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private void OnEnable()
        {
            // エディタ起動時に設定を読み込む
            LoadPrefs();
        }

        private void OnDisable()
        {
            // エディタ終了時に設定を保存
            SavePrefs();
        }

        private void SavePrefs()
        {
            // ディレクトリを保存
            EditorPrefs.SetString(TargetDirectoryKey, targetDirectory);

            // グループ名をJSON形式で保存
            string groupNamesJson = JsonUtility.ToJson(new StringListWrapper { items = groupNames });
            EditorPrefs.SetString(GroupNamesKey, groupNamesJson);

            // 拡張子をJSON形式で保存
            string fileExtensionsJson = JsonUtility.ToJson(new StringListWrapper { items = fileExtensions });
            EditorPrefs.SetString(FileExtensionsKey, fileExtensionsJson);
        }

        private void LoadPrefs()
        {
            // ディレクトリを読み込み
            if (EditorPrefs.HasKey(TargetDirectoryKey))
            {
                targetDirectory = EditorPrefs.GetString(TargetDirectoryKey);
            }

            // グループ名をJSONから読み込み
            if (EditorPrefs.HasKey(GroupNamesKey))
            {
                var groupNamesJson = EditorPrefs.GetString(GroupNamesKey);
                var wrapper = JsonUtility.FromJson<StringListWrapper>(groupNamesJson);
                if (wrapper is { items: not null })
                {
                    groupNames = wrapper.items;
                }
            }

            // 拡張子をJSONから読み込み
            if (EditorPrefs.HasKey(FileExtensionsKey))
            {
                var fileExtensionsJson = EditorPrefs.GetString(FileExtensionsKey);
                var wrapper = JsonUtility.FromJson<StringListWrapper>(fileExtensionsJson);
                if (wrapper is { items: not null })
                {
                    fileExtensions = wrapper.items;
                }
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("Addressables Group Settings", EditorStyles.boldLabel);

            EditorGUILayout.Space(10);

            // ターゲットディレクトリの設定
            EditorGUILayout.LabelField("Target Root Directory", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            targetDirectory = EditorGUILayout.TextField(targetDirectory);
            if (GUILayout.Button("Browse", GUILayout.Width(80)))
            {
                var selectedPath = EditorUtility.OpenFolderPanel("Select Directory", targetDirectory, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // プロジェクトのパスを相対パスに変換
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        targetDirectory = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        targetDirectory = selectedPath;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            if (!Directory.Exists(targetDirectory))
            {
                EditorGUILayout.HelpBox("Directory does not exist!", MessageType.Warning);
            }

            EditorGUILayout.Space(10);

            // グループ名の設定
            EditorGUILayout.LabelField("Group Names (Directory Names)", EditorStyles.boldLabel);

            groupScrollPosition = EditorGUILayout.BeginScrollView(groupScrollPosition, GUILayout.Height(150));
            for (var i = 0; i < groupNames.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                groupNames[i] = EditorGUILayout.TextField(groupNames[i]);
                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    groupNames.RemoveAt(i);
                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            newGroupName = EditorGUILayout.TextField(newGroupName);
            if (GUILayout.Button("Add Group", GUILayout.Width(80)))
            {
                if (!string.IsNullOrEmpty(newGroupName))
                {
                    groupNames.Add(newGroupName.ToLower());
                    newGroupName = "";

                    // 設定を保存 (状態変更時に都度保存)
                    SavePrefs();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // 拡張子リストの設定
            EditorGUILayout.LabelField("File Extensions", EditorStyles.boldLabel);

            extensionScrollPosition = EditorGUILayout.BeginScrollView(extensionScrollPosition, GUILayout.Height(150));
            for (var i = 0; i < fileExtensions.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                fileExtensions[i] = EditorGUILayout.TextField(fileExtensions[i]).TrimStart('*', '.');
                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    fileExtensions.RemoveAt(i);

                    // 設定を保存 (状態変更時に都度保存)
                    SavePrefs();

                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            newExtension = EditorGUILayout.TextField(newExtension);
            if (GUILayout.Button("Add Extension", GUILayout.Width(100)))
            {
                if (!string.IsNullOrEmpty(newExtension))
                {
                    // 拡張子から先頭の「*.」を削除
                    newExtension = newExtension.TrimStart('*', '.');
                    if (!fileExtensions.Contains(newExtension))
                    {
                        fileExtensions.Add(newExtension);
                        newExtension = "";

                        // 設定を保存 (状態変更時に都度保存)
                        SavePrefs();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(20);

            // 処理実行ボタン
            EditorGUI.BeginDisabledGroup(groupNames.Count == 0 || fileExtensions.Count == 0 || !Directory.Exists(targetDirectory));
            if (GUILayout.Button("Process Assets to Addressables", GUILayout.Height(30)))
            {
                ProcessAssets();
            }
            EditorGUI.EndDisabledGroup();

            // 結果レポート表示スペース
            if (groupNames.Count == 0)
            {
                EditorGUILayout.HelpBox("Please add at least one group name.", MessageType.Info);
            }

            if (fileExtensions.Count == 0)
            {
                EditorGUILayout.HelpBox("Please add at least one file extension.", MessageType.Info);
            }
        }

        private void ProcessAssets()
        {
            if (!Directory.Exists(targetDirectory))
            {
                EditorUtility.DisplayDialog("Error", $"Directory does not exist: {targetDirectory}", "OK");
                return;
            }

            if (groupNames.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "Please add at least one group name.", "OK");
                return;
            }

            if (fileExtensions.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "Please add at least one file extension.", "OK");
                return;
            }

            var groupKeys = groupNames.ToArray();
            var extensions = fileExtensions.ToArray();

            // AddressableAssetSettingsのデフォルトインスタンスを取得
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null)
            {
                Debug.LogError("Failed to load AddressableAssetSettings.");
                return;
            }

            foreach (var groupKey in groupKeys)
            {
                var group = settings.FindGroup(groupKey);
                if (group == null)
                {
                    group = settings.CreateGroup(groupKey, false, false, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
                }

                foreach (var extension in extensions)
                {
                    var searchPattern = $"*.{extension.TrimStart('*', '.')}";
                    var assets = Directory.EnumerateFiles(targetDirectory, searchPattern, SearchOption.AllDirectories);

                    foreach (var assetPath in assets)
                    {
                        // アセットのGUIDを取得
                        var guid = AssetDatabase.AssetPathToGUID(assetPath);

                        // 既にグループにアセットが含まれているかチェックしなければ追加
                        var entry = settings.FindAssetEntry(guid) ?? settings.CreateOrMoveEntry(guid, group);
                        entry.address = $"{groupKey}/{Path.GetFileNameWithoutExtension(assetPath)}".ToLower();
                    }
                }
            }

            // 設定を保存
            AssetDatabase.SaveAssets();
            Debug.Log($"Added assets to Addressables groups: {string.Join(", ", groupKeys)}");

            // 成功メッセージ
            EditorUtility.DisplayDialog("Success",
                $"Assets processed successfully!\n\nDirectory: {targetDirectory}\nGroups: {string.Join(", ", groupNames)}\nExtensions: {string.Join(", ", extensions)}",
                "OK");
        }
    }

}
