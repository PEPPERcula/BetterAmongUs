using System;
using System.Text.Json;
using System.IO;
using System.Reflection;
using AmongUs.Data;
using IEnumerator = System.Collections.IEnumerator;
using UnityEngine.Networking;

namespace BetterAmongUs;

public class DataBaseConnect
{
    private static bool initOnce = false;
    private static string? allUserInfo;
    private static Dictionary<string, string> userType = new();

    private const string ApiUrl = "https://api.weareten.ca";

    public static IEnumerator Init()
    {
        Logger.Log("Begin dbConnect Login flow", "DbConnect.Init");

        if (!initOnce)
        {
            yield return GetRoleTable();

            if (string.IsNullOrEmpty(GetToken()))
            {
                HandleFailure(FailedConnectReason.API_Token_Is_Empty);
                yield break;
            }

            if (userType.Count < 1)
            {
                HandleFailure(FailedConnectReason.Error_Getting_User_Role_Table);
                yield break;
            }
        }
        else
        {
            yield return GetRoleTable();
        }

        Logger.Log(initOnce ? "Finished Sync flow." : "Finished first init flow.", "DbConnect.Init");
        initOnce = true;
    }

    private static void HandleFailure(FailedConnectReason errorReason)
    {
        string errorMessage = errorReason switch
        {
            FailedConnectReason.Build_Not_Specified => "Build not specified",
            FailedConnectReason.API_Token_Is_Empty => "API token is empty",
            FailedConnectReason.Error_Getting_User_Role_Table => "Error fetching role table",
            FailedConnectReason.Error_Getting_EAC_List => "Error fetching EAC list",
            _ => "Reason not specified"
        };

        Logger.Error(errorMessage, "DbConnect.Init");

        bool shouldDisconnect = Main.ReleaseBuildType switch
        {
            ReleaseTypes.Dev => true,
            ReleaseTypes.Release or ReleaseTypes.Beta => HandlePublicWarning(),
            _ => true
        };

        if (shouldDisconnect)
        {
            DisconnectUser();
        }
    }

    private static bool HandlePublicWarning()
    {
        if (GameStates.IsLobby || GameStates.InGame)
        {
            DestroyableSingleton<HudManager>.Instance.ShowPopUp(Translator.GetString("dbConnect.InitFailurePublic"));
        }
        else
        {
            DestroyableSingleton<DisconnectPopup>.Instance.ShowCustom(Translator.GetString("dbConnect.InitFailurePublic"));
        }
        return false;
    }

    private static void DisconnectUser()
    {
        if (AmongUsClient.Instance.mode != InnerNet.MatchMakerModes.None)
        {
            AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
        }

        DataManager.Player.Account.LoginStatus = EOSManager.AccountLoginStatus.Offline;
        DataManager.Player.Save();
        DestroyableSingleton<DisconnectPopup>.Instance.ShowCustom(Translator.GetString("dbConnect.InitFailure"));
    }

    private static string GetToken()
    {
        string apiToken = "";
        Assembly assembly = Assembly.GetExecutingAssembly();
        string resourceName = "BetterAmongUs.token.env";

        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            using StreamReader reader = new(stream);
            apiToken = reader.ReadToEnd().Replace("API_TOKEN=", string.Empty).Trim();
        }

        if (string.IsNullOrEmpty(apiToken))
        {
            Logger.Error("Embedded resource not found or token is empty.", "apiToken.error");
        }
        return apiToken;
    }

    private static IEnumerator GetRoleTable()
    {
        string apiToken = GetToken();
        if (string.IsNullOrEmpty(apiToken))
        {
            Logger.Error("API token is empty.", "GetRoleTable.error");
            yield break;
        }

        string apiUrl = ApiUrl;
        string endpoint = $"{apiUrl}/userInfo/bau/?token={apiToken}";

        UnityWebRequest webRequest = UnityWebRequest.Get(endpoint);

        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Logger.Error($"Error fetching User List: {webRequest.error}", "GetRoleTable.error");
            yield break;
        }

        try
        {
            var userList = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(webRequest.downloadHandler.text);
            allUserInfo = JsonSerializer.Serialize(userList);
            userType = userList != null && userList.Count > 1
                ? userList.ToDictionary(u => u["id"].ToString(), u => u["role"].ToString())
                : new Dictionary<string, string>();

            if (!initOnce && userType.Count == 0)
            {
                Logger.Error("Incoming RoleTable is null, cannot init!", "GetRoleTable.error");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error processing response: {ex.Message}", "GetRoleTable.error");
        }
    }

    private enum FailedConnectReason
    {
        Build_Not_Specified,
        API_Token_Is_Empty,
        Error_Getting_User_Role_Table,
        Error_Getting_EAC_List,
    }
}
