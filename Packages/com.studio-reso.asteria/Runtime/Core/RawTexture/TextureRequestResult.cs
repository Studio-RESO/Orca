using System;
using UnityEngine;

namespace Asteria
{
    internal class TextureRequestResult
    {
        public readonly string Key;
        public readonly Texture2D Result;
        public readonly string Etag;
        public readonly uint StatusCode;
        public readonly Exception InnerException;

        public bool IsSuccess => (StatusCode is >= 200 and <= 299 || StatusCode is 304) && InnerException == null;

        internal TextureRequestResult(string key, Texture2D result, string etag, uint statusCode, Exception exception = null)
        {
            Key = key;
            Result = result;
            Etag = etag;
            StatusCode = statusCode;
            InnerException = exception;
        }
    }
}
