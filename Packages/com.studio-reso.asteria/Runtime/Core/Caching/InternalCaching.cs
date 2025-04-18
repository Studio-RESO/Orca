using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

namespace Asteria
{
    internal static class InternalCaching
    {
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Setup()
        {
            CachePath = null;
            CachedFiles.Clear();
        }
#endif

        private static readonly char[] PathSeparators = { Path.DirectorySeparatorChar };
        private static readonly Dictionary<string, bool> CachedFiles = new();

        private static string cachePath;
        public static string CachePath
        {
            get => cachePath;
            set
            {
                if (!string.IsNullOrEmpty(value) && !value.EndsWith(Path.DirectorySeparatorChar))
                {
                    value += Path.DirectorySeparatorChar;
                }
                cachePath = value;
            }
        }

        /// <summary>
        /// FileNameから保存先のキャッシュファイルパスを返す
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static string FileNameToCachePath(string fileName, string hash)
        {
            Assert.IsFalse(string.IsNullOrEmpty(fileName), "file name is null or empty.");
            Assert.IsFalse(string.IsNullOrEmpty(hash), "hash is null or empty.");

            // AssetBundle名設定がFileName or AppendHashToFileNameの場合
            // address名のフォルダに保存されパス内にセパレータが存在する
            if (fileName.IndexOfAny(PathSeparators) >= 0)
            {
                // AppendHashToFileNameの場合はそのままのファイル名で保存
                if (fileName.CustomEndsWith($"{hash}.bundle"))
                {
                    return fileName;
                }

                // FileNameの場合はファイル名にハッシュを付与して保存
                return Path.Combine(fileName, hash);
            }

            var directory = fileName.Replace(".bundle", "");

            // UseHashOfAssetBundleの設定でビルドされた場合はファイル名がAssetBundleHashと一致する
            if (directory.Equals(hash, StringComparison.Ordinal))
            {
                // 1つのフォルダに大量のファイルが保存されることになってしまうので頭文字でフォルダ切って保存
                return Path.Combine(fileName[0].ToString(), fileName);
            }

            // UseHashOfFileNameの場合はAssetBundleが更新されてもファイル名が変わらない
            // そのためHashのフォルダ名を作成しその中に保存
            return Path.Combine(fileName[0].ToString(), directory, hash);
        }
    }
}
