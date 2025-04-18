using UnityEngine.Networking;

namespace Asteria
{
    public static class UnityWebRequestExtension
    {
        public static bool IsError(this UnityWebRequest request)
        {
            return request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.DataProcessingError;
        }
    }
}
