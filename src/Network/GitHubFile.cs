using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using Il2CppInterop.Runtime.Attributes;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace BetterAmongUs.Network;

internal static class GitHubFile
{
    /// <summary>
    /// Downloads an individual visor-related file from the remote repository and saves it locally.
    /// Handles errors and logs any failed downloads to avoid missing assets.
    /// </summary>
    [HideFromIl2Cpp]
    internal static IEnumerator CoDownloadFile(string fileUrl, string localFilePath, Action<string>? callback = null, bool showProgress = false)
    {
        var www = new UnityWebRequest(fileUrl, UnityWebRequest.kHttpVerbGET)
        {
            downloadHandler = new DownloadHandlerBuffer()
        };

        if (showProgress)
        {
            CustomLoadingBarManager.ToggleLoadingBar(true);
            CustomLoadingBarManager.SetLoadingPercent(0f, "Starting download...");
        }

        var operation = www.SendWebRequest();

        // Track progress while downloading
        while (!operation.isDone)
        {
            if (showProgress)
            {
                int dotCount = (int)(Time.time * 2f) % 4;
                string dots = new('.', dotCount);
                float progress = www.downloadProgress * 100f;
                if (progress < 1f)
                {
                    CustomLoadingBarManager.SetLoadingPercent(0f, $"Starting Download{dots}");
                }
                CustomLoadingBarManager.SetLoadingPercent(progress, $"Downloading{dots}");
            }
            yield return null;
        }

        if (www.result == UnityWebRequest.Result.Success)
        {
            CustomLoadingBarManager.SetLoadingPercent(100f, "Saving File!");
            yield return new WaitForSeconds(1f);
            CustomLoadingBarManager.ToggleLoadingBar(false);
        }
        else
        {
            Logger_.Error($"Error downloading file from URL '{fileUrl}': {www.error} (Response Code: {(int)www.responseCode})");
            if (showProgress)
            {
                CustomLoadingBarManager.SetLoadingPercent(100f, "Download Failed!");
                yield return new WaitForSeconds(2f);
                CustomLoadingBarManager.ToggleLoadingBar(false);
            }
            yield break;
        }

        byte[] bytes = www.downloadHandler.GetNativeData().ToArray();
        File.WriteAllBytes(localFilePath, bytes);

        Logger_.Log($"Saved file: {localFilePath}");
        callback?.Invoke(localFilePath);
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
            Logger_.Error($"Error downloading {fileUrl}/{fileName}: {www.error}");
            yield break;
        }

        var response = www.downloadHandler.text;
        www.Dispose();
        Callback.Invoke(response);
    }
}
