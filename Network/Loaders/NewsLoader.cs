using BetterAmongUs.Items;
using BetterAmongUs.Network.Configs;
using Il2CppInterop.Runtime.Attributes;
using System.Collections;
using System.Text.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace BetterAmongUs.Network.Loaders;

/// <summary>
/// Handles downloading and processing of news data from a remote repository.
/// </summary>
internal class NewsLoader : MonoBehaviour
{
    /// <summary>
    /// Coroutine to fetch the news data from the remote repository.
    /// If no internet connection is detected, it retries several times before giving up.
    /// </summary>
    [HideFromIl2Cpp]
    internal IEnumerator CoFetchNewsData()
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
        yield return GitHubFile.CoDownloadManifest(GitPath.Repository.ToString(), "manifest.json", (string text) =>
        {
            callBack = text;
        });

        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
        };

        var response = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(callBack, options);

        if (response == null || !response.ContainsKey("News"))
        {
            Logger.Error("manifest.json deserialization failed or no 'News' key found.");
            yield break;
        }

        foreach (var file in response["News"])
        {
            yield return CoDownloadNewsFile(file);
        }

        yield return CoLoadNewsTest();

        Logger.Log($"Loaded {ModNews.NewsDataToProcess.Count} news files");

        Destroy(this);
    }

    /// <summary>
    /// Coroutine to download an individual news file from the remote repository.
    /// If the download fails or the file cannot be deserialized, the process is skipped.
    /// </summary>
    /// <param name="fileName">The name of the news file to download.</param>
    [HideFromIl2Cpp]
    private IEnumerator CoDownloadNewsFile(string fileName)
    {
        string configUrl = GitPath.News.Combine(fileName);

        var wwwConfig = new UnityWebRequest(configUrl, UnityWebRequest.kHttpVerbGET)
        {
            downloadHandler = new DownloadHandlerBuffer()
        };
        yield return wwwConfig.SendWebRequest();

        if (wwwConfig.result != UnityWebRequest.Result.Success)
        {
            Logger.Error($"Error fetching config file for '{fileName}': {wwwConfig.error}");
            yield break;
        }

        try
        {
            var config = NewsData.Sanitize(wwwConfig.downloadHandler.text);
            if (config == null || !config.Show) yield break;
            ModNews.NewsDataToProcess.Add(config);
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to deserialize yaml for '{fileName}': {ex.Message}");
            yield break;
        }
    }

    /// <summary>
    /// Loads a test news configuration from an embedded resource for local testing purposes.
    /// </summary>
    [HideFromIl2Cpp]
    private IEnumerator CoLoadNewsTest()
    {
        string yamlDirectory = "BetterAmongUs.Resources.NewsTest.yaml";
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        using Stream? resourceStream = assembly.GetManifestResourceStream(yamlDirectory);
        if (resourceStream != null)
        {
            using StreamReader reader = new(resourceStream);

            try
            {
                var config = NewsData.Sanitize(reader.ReadToEnd());
                if (config == null || !config.Show) yield break;
                ModNews.NewsDataToProcess.Add(config);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to deserialize yaml for '{yamlDirectory}': {ex.Message}");
                yield break;
            }
        }
    }
}