using Il2CppInterop.Runtime.Attributes;
using System.Collections;
using UnityEngine.Networking;

namespace BetterAmongUs.Network;

internal class GitHubFile
{
    /// <summary>
    /// Downloads an individual visor-related file from the remote repository and saves it locally.
    /// Handles errors and logs any failed downloads to avoid missing assets.
    /// </summary>
    [HideFromIl2Cpp]
    internal static IEnumerator CoDownloadFile(string fileUrl, string localFilePath, string fileName)
    {
        var www = new UnityWebRequest(fileUrl, UnityWebRequest.kHttpVerbGET)
        {
            downloadHandler = new DownloadHandlerBuffer()
        };

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Logger.Error($"Error downloading file '{fileName}' from URL '{fileUrl}': {www.error} (Response Code: {(int)www.responseCode})");
            yield break;
        }

        File.WriteAllBytes(localFilePath, www.downloadHandler.GetNativeData().ToArray());

        Logger.Log($"Saved file: {localFilePath}");
        www.Dispose();
    }

    /// <summary>
    /// Downloads an individual visor-related file from the remote repository and saves it locally.
    /// Handles errors and logs any failed downloads to avoid missing assets.
    /// </summary>
    [HideFromIl2Cpp]
    internal static IEnumerator CoDownloadManifest(string fileUrl, string fileName, Action<string> Callback)
    {
        var www = new UnityWebRequest($"{fileUrl}/{fileName}", UnityWebRequest.kHttpVerbGET)
        {
            downloadHandler = new DownloadHandlerBuffer()
        };
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Logger.Error($"Error downloading {fileUrl}/{fileName}: {www.error}");
            yield break;
        }

        var response = www.downloadHandler.text;
        www.Dispose();
        Callback.Invoke(response);
    }
}
