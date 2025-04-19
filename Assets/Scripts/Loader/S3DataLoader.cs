using System.IO;
using System.Net.Http;
using Cysharp.Net.Http;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public sealed class S3DataLoader
{
    private HttpClient httpClient;
    private const string hostname = "https://d3b4dxjfr5axs5.cloudfront.net/";

    public UniTask<byte[]> LoadAsync(string path)
    {
        using var handler = new YetAnotherHttpHandler();
        httpClient = new HttpClient(handler);
        Debug.Log($"{Path.Join(hostname, path)}");
        return httpClient.GetByteArrayAsync(Path.Join(hostname, path)).AsUniTask();
    }

    public async UniTask<byte[]> Download(string path)
    {
        Debug.Log($"{Path.Join(hostname, path)}");

        var request = UnityWebRequest.Get(Path.Join(hostname, path));
        request.downloadHandler = new DownloadHandlerBuffer();

        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            return request.downloadHandler.data;
        }
        else
        {
            Debug.LogError("Failed to download: " + request.error);
        }

        return null;
    }
}
