using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using System.Collections;
using System.Text.Json;
using BetterAmongUs.Helpers;
using UnityEngine;
using UnityEngine.Networking;
using System.Reflection;
using BetterAmongUs.Network.Configs;

namespace BetterAmongUs.Modules;

internal class UserDataLoader : MonoBehaviour
{
    private bool isRunning;

    private const string ApiUrl = "https://api.weareten.ca";
    private string apiToken = string.Empty;

    internal void Start()
    {
        FetchData();
    }

    internal void FetchData()
    {
        if (isRunning) return;

        apiToken = GetToken();
        if (!string.IsNullOrEmpty(apiToken))
        {
            this.StartCoroutine(CoFetchUserData());
        }
        else
        {
            Logger.Error("API token is null or empty. Unable to fetch user data.");
        }
    }

    [HideFromIl2Cpp]
    private IEnumerator CoFetchUserData()
    {
        isRunning = true;

        int count = 0;
        float hang = 0;
        while (!Utils.IsInternetAvailable())
        {
            count++;
            if (count >= 17)
            {
                Logger.Error("No internet connection after multiple retries.");
                yield break;
            }

            if (hang < 30f) hang += 2.5f;
            yield return new WaitForSeconds(hang);
        }

        string endpoint = $"{ApiUrl}/userInfo/bau/?token={apiToken}";
        var www = new UnityWebRequest(endpoint, UnityWebRequest.kHttpVerbGET)
        {
            downloadHandler = new DownloadHandlerBuffer()
        };

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Logger.Error($"HTTP request failed: {www.error}");
            isRunning = false;
            yield break;
        }

        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true
        };

        try
        {
            var users = JsonSerializer.Deserialize<List<UserData>>(www.downloadHandler.text, options);
            if (users != null)
            {
                foreach (var user in users)
                {
                    UserData.AllUsers.Add(user);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error parsing JSON in CoFetchUserData: {ex.Message}");
        }

        isRunning = false;
        www.Dispose();

        this.StartCoroutine(CoFetchEACData());
    }

    [HideFromIl2Cpp]
    private IEnumerator CoFetchEACData()
    {
        isRunning = true;

        int count = 0;
        float hang = 0;
        while (!Utils.IsInternetAvailable())
        {
            count++;
            if (count >= 17)
            {
                Logger.Error("No internet connection after multiple retries during BAC data fetch.");
                yield break;
            }

            if (hang < 30f) hang += 2.5f;
            yield return new WaitForSeconds(hang);
        }

        string endpoint = $"{ApiUrl}/eac/bau/?token={apiToken}";
        var www = new UnityWebRequest(endpoint, UnityWebRequest.kHttpVerbGET)
        {
            downloadHandler = new DownloadHandlerBuffer()
        };

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Logger.Error($"HTTP request failed during BAC data fetch: {www.error}");
            isRunning = false;
            yield break;
        }

        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true
        };

        try
        {
            var bannedUsers = JsonSerializer.Deserialize<List<BannedUserData>>(www.downloadHandler.text, options);
            if (bannedUsers != null)
            {
                foreach (var user in bannedUsers)
                {
                    BannedUserData.AllBannedUsers.Add(user);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error parsing JSON in CoFetchEACData: {ex.Message}");
        }

        isRunning = false;
        www.Dispose();
        Destroy(this);
    }

    private static string GetToken()
    {
        string apiToken = "";
        Assembly assembly = Assembly.GetExecutingAssembly();
        string resourceName = "BetterAmongUs.token.env";

        try
        {
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using StreamReader reader = new(stream);
                apiToken = reader.ReadToEnd().Replace("API_TOKEN=", string.Empty).Trim();
            }
            else
            {
                Logger.Error("Embedded resource not found: BetterAmongUs.token.env");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error reading token from embedded resource: {ex.Message}");
        }

        if (string.IsNullOrEmpty(apiToken))
        {
            Logger.Error("API token is empty after reading embedded resource.");
        }

        return apiToken;
    }
}