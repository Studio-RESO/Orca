using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor.Asset
{
    public sealed class WwiseSoundBankBuilder : EditorWindow
    {
        private string bundleName = "wwise_soundbanks";
        private string outputPath = "AssetBundles";
        private string wwiseBankPath = "Assets/StreamingAssets/Audio/GeneratedSoundBanks";
        private bool includeInitBank = true;
        private BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
        private BuildAssetBundleOptions bundleOptions = BuildAssetBundleOptions.ChunkBasedCompression;
        
        [MenuItem("Tools/Wwise/Build SoundBank AssetBundle")]
        public static void ShowWindow()
        {
            GetWindow<WwiseSoundBankBuilder>("Wwise SoundBank Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Wwise SoundBank AssetBundle Builder", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            bundleName = EditorGUILayout.TextField("Bundle Name", bundleName);
        
            EditorGUILayout.Space();
            GUILayout.Label("Source Settings", EditorStyles.boldLabel);
            wwiseBankPath = EditorGUILayout.TextField("Wwise Bank Path", wwiseBankPath);
            includeInitBank = EditorGUILayout.Toggle("Include Init Bank", includeInitBank);

            EditorGUILayout.Space();
            GUILayout.Label("Build Settings", EditorStyles.boldLabel);
            outputPath = EditorGUILayout.TextField("Output Path", outputPath);
            buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", buildTarget);
        
            EditorGUILayout.Space();
            GUILayout.Label("Bundle Options", EditorStyles.boldLabel);
            var useChunkCompression = (bundleOptions & BuildAssetBundleOptions.ChunkBasedCompression) != 0;
            var useUncompressed = (bundleOptions & BuildAssetBundleOptions.UncompressedAssetBundle) != 0;
            var disableWriteTypeTree = (bundleOptions & BuildAssetBundleOptions.DisableWriteTypeTree) != 0;
        
            useChunkCompression = EditorGUILayout.Toggle("Chunk Based Compression", useChunkCompression);
            useUncompressed = EditorGUILayout.Toggle("Uncompressed", useUncompressed);
            disableWriteTypeTree = EditorGUILayout.Toggle("Disable Write Type Tree", disableWriteTypeTree);
        
            bundleOptions = BuildAssetBundleOptions.None;
            if (useChunkCompression) bundleOptions |= BuildAssetBundleOptions.ChunkBasedCompression;
            if (useUncompressed) bundleOptions |= BuildAssetBundleOptions.UncompressedAssetBundle;
            if (disableWriteTypeTree) bundleOptions |= BuildAssetBundleOptions.DisableWriteTypeTree;

            EditorGUILayout.Space();
            if (GUILayout.Button("Build AssetBundle"))
            {
                BuildAssetBundle();
            }
        }
        
        private void BuildAssetBundle()
    {
        if (string.IsNullOrEmpty(bundleName))
        {
            EditorUtility.DisplayDialog("Error", "Bundle name cannot be empty", "OK");
            return;
        }

        if (!Directory.Exists(wwiseBankPath))
        {
            EditorUtility.DisplayDialog("Error", $"Wwise bank path does not exist: {wwiseBankPath}", "OK");
            return;
        }

        // Create output directory if it doesn't exist
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        // Find all .bnk files in the Wwise bank path
        var bankPaths = new List<string>();
        
        // Select platform folder based on build target
        var platformFolder = "";
        switch (buildTarget)
        {
            case BuildTarget.iOS:
                platformFolder = "iOS";
                break;
            case BuildTarget.Android:
                platformFolder = "Android";
                break;
            case BuildTarget.StandaloneOSX:
                platformFolder = "Mac";
                break;
            case BuildTarget.StandaloneWindows:
                platformFolder = "Windows";

                break;
            case BuildTarget.StandaloneWindows64:
                platformFolder = "Windows";
                break;
            // Add other platforms as needed...
        }
        
        if (string.IsNullOrEmpty(platformFolder))
        {
            EditorUtility.DisplayDialog("Error", "Unsupported build target", "OK");
            return;
        }
        
        var fullBankPath = Path.Combine(wwiseBankPath, platformFolder);
        
        if (!Directory.Exists(fullBankPath))
        {
            EditorUtility.DisplayDialog("Error", $"Platform bank folder does not exist: {fullBankPath}", "OK");
            return;
        }
        
        foreach (var file in Directory.GetFiles(fullBankPath, "*.bnk"))
        {
            var filename = Path.GetFileName(file);
            
            // Skip Init.bnk if not included
            if (!includeInitBank && filename.Equals("Init.bnk"))
                continue;
            
            bankPaths.Add(file);
            Debug.Log($"Found bank file: {file}");
        }

        if (bankPaths.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No .bnk files found in the specified path", "OK");
            return;
        }

        // Temporarily copy bank files to Assets folder
        var tempFolder = "Assets/Editor/WwiseTempBanks";
        if (Directory.Exists(tempFolder))
        {
            Directory.Delete(tempFolder, true);
        }
        Directory.CreateDirectory(tempFolder);

        // Copy bank files and set appropriate asset bundle names
        AssetDatabase.Refresh();
        var assetPaths = new List<string>();
        
        foreach (var bankPath in bankPaths)
        {
            var destPath = Path.Combine(tempFolder, Path.GetFileName(bankPath));
            File.Copy(bankPath, destPath);
            assetPaths.Add(destPath);
        }
        
        AssetDatabase.Refresh();
        
        // Set asset bundle names
        foreach (var assetPath in assetPaths)
        {
            var importer = AssetImporter.GetAtPath(assetPath);
            if (importer != null)
            {
                importer.assetBundleName = bundleName;
                Debug.Log($"Set asset bundle name for {assetPath}");
            }
            else
            {
                Debug.LogError($"Failed to get asset importer for {assetPath}");
            }
        }

        // Build asset bundle
        var manifest = BuildPipeline.BuildAssetBundles(
            outputPath, 
            bundleOptions,
            buildTarget);

        if (!manifest)
        {
            EditorUtility.DisplayDialog("Error", "Failed to build asset bundle", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Success", "Asset bundle built successfully", "OK");
            EditorUtility.RevealInFinder(outputPath);
        }

        // Clean up temp folder
        if (Directory.Exists(tempFolder))
        {
            AssetDatabase.DeleteAsset(tempFolder);
        }
        
        AssetDatabase.Refresh();
    }
    }
}
