using BetterAmongUs.Network.Configs;
using Il2CppInterop.Runtime.Attributes;
using System.Collections;
using System.Text.Json;
using UnityEngine;

namespace BetterAmongUs.Network.Loaders;

internal class UpdateLoader : MonoBehaviour
{
    internal static UpdateData? UpdateInfo { get; private set; }

    [HideFromIl2Cpp]
    internal IEnumerator CoFetchUpdateData()
    {
        int count = 0;
        float delay = 0;
        while (!GithubAPI.IsInternetAvailable())
        {
            count++;
            if (count >= 17)
            {
                Destroy(this);
                yield break;
            }
            if (delay < 30f) delay += 2.5f;
            yield return new WaitForSeconds(delay);
        }

        string callBack = "";
        yield return GitHubFile.CoDownloadManifest(GitPath.Repository.ToString(), "update.json", (string text) =>
        {
            callBack = text;
        });

        if (string.IsNullOrEmpty(callBack)) yield break;

        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
        };

        var response = JsonSerializer.Deserialize<UpdateData>(callBack, options);

        if (response != null)
        {
            UpdateInfo = response;
            Logger.Log($"Loaded update info");
        }

        UpdateManager.Init();

        Destroy(this);
    }
}