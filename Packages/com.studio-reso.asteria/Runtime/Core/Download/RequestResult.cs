using System;

namespace Asteria
{
    public class RequestResult<T>
    {
        public readonly string Key;
        public readonly T Result;
        public readonly uint StatusCode;
        public readonly Exception InnerException;

        public bool IsSuccess => StatusCode is >= 200 and <= 299 && InnerException == null;

        internal RequestResult(string key, T result, uint statusCode, Exception exception = null)
        {
            Key = key;
            Result = result;
            StatusCode = statusCode;
            InnerException = exception;
        }
    }
}
