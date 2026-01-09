using BepInEx.Unity.IL2CPP.Utils;
using BetterAmongUs.Network.Loaders;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;
using UnityEngine.Networking;

namespace BetterAmongUs.Network;

internal sealed class GithubAPI : MonoBehaviour
{
    internal static GithubAPI? Instance { get; private set; }
    internal static bool HasConnectedAPI { get; private set; } = false;
    internal static bool Finished { get; private set; } = false;

    private static bool hasTryConnect = false;
    internal static void Connect()
    {
        if (hasTryConnect) return;
        hasTryConnect = true;

        var obj = new GameObject("GithubAPI(BAU)") { hideFlags = HideFlags.HideAndDontSave };
        DontDestroyOnLoad(obj);
        Instance = obj.AddComponent<GithubAPI>();
    }

    internal void Start()
    {
        ConnectToAPI();
    }

    [HideFromIl2Cpp]
    private void ConnectToAPI()
    {
        ConnectToAPIUsers();

        var newsLoader = gameObject.AddComponent<NewsLoader>();
        this.StartCoroutine(newsLoader.CoFetchNewsData());

        var updateLoader = gameObject.AddComponent<UpdateLoader>();
        this.StartCoroutine(updateLoader.CoFetchUpdateData());
    }

    [HideFromIl2Cpp]
    private void ConnectToAPIUsers()
    {
    }

    internal static void SetConnectedAPI(UnityWebRequest www, bool hasErrored)
    {
        if (www.result == UnityWebRequest.Result.ConnectionError ||
            www.result == UnityWebRequest.Result.ProtocolError || hasErrored)
        {
            HasConnectedAPI = false;
        }
        else
        {
            HasConnectedAPI = true;
        }
    }

    internal static bool IsInternetAvailable()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
            return false;

        UnityWebRequest? www = null;
        try
        {
            www = UnityWebRequest.Get("https://clients3.google.com/generate_204");
            www.SendWebRequest();
            while (!www.isDone) { }
            return www.result == UnityWebRequest.Result.Success && www.responseCode == 204;
        }
        catch
        {
            return false;
        }
        finally
        {
            www?.Dispose();
        }
    }
}
