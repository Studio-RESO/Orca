using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Asteria
{
    public static class HashCreator
    {
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Setup()
        {
            md5Instance?.Dispose();
            md5Instance = null;
        }
#endif

        private static MD5 md5Instance;

        public static string ComputeMD5Hash(string input)
        {
            md5Instance ??= MD5.Create();

            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = md5Instance.ComputeHash(inputBytes);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
