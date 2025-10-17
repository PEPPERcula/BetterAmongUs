using AmongUs.Data;
using BetterAmongUs.Modules;
using BetterAmongUs.Patches.Gameplay.UI.Chat;
using InnerNet;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace BetterAmongUs.Helpers;

internal static class Utils
{
    internal static bool IsInternetAvailable()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
            return false;

        UnityWebRequest? www = null;
        try
        {
            www = UnityWebRequest.Get("http://clients3.google.com/generate_204");
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
    // Get player by client id
    internal static ClientData? ClientFromClientId(int clientId) => AmongUsClient.Instance.allClients.ToArray().FirstOrDefault(cd => cd.Id == clientId) ?? null;
    // Get player data from player id
    internal static NetworkedPlayerInfo? PlayerDataFromPlayerId(int playerId) => GameData.Instance.AllPlayers.ToArray().FirstOrDefault(data => data.PlayerId == playerId);
    // Get player data from client id
    internal static NetworkedPlayerInfo? PlayerDataFromClientId(int clientId) => GameData.Instance.AllPlayers.ToArray().FirstOrDefault(data => data.ClientId == clientId);
    // Get player data from friend code
    internal static NetworkedPlayerInfo? PlayerDataFromFriendCode(string friendCode) => GameData.Instance.AllPlayers.ToArray().FirstOrDefault(data => data.FriendCode == friendCode);
    // Get player from player id
    internal static PlayerControl? PlayerFromPlayerId(int playerId) => Main.AllPlayerControls.FirstOrDefault(player => player.PlayerId == playerId) ?? null;
    // Get player from client id
    internal static PlayerControl? PlayerFromClientId(int clientId) => Main.AllPlayerControls.FirstOrDefault(player => player.GetClientId() == clientId) ?? null;
    // Get player from net id
    internal static PlayerControl? PlayerFromNetId(uint netId) => Main.AllPlayerControls.FirstOrDefault(player => player.NetId == netId) ?? null;
    // Add msg to chat
    internal static void AddChatPrivate(string text, string overrideName = "", bool setRight = false)
    {
        if (!GameState.IsInGame) return;

        var chat = HudManager.Instance?.Chat;
        if (chat == null) return;
        var data = PlayerControl.LocalPlayer?.Data;
        if (data == null) return;
        ChatBubble pooledBubble = chat.GetPooledBubble();
        string MsgName = $"<color=#ffffff><b>(<color=#00ff44>{Translator.GetString("SystemMessage")}</color>)</b>" + ChatPatch.CommandPostfixName;
        if (overrideName != "")
            MsgName = overrideName + ChatPatch.CommandPostfixName;
        try
        {
            pooledBubble.transform.SetParent(chat.scroller.Inner);
            pooledBubble.transform.localScale = Vector3.one;
            pooledBubble.SetCosmetics(data);
            pooledBubble.gameObject.transform.Find("PoolablePlayer").gameObject.SetActive(false);
            pooledBubble.ColorBlindName.gameObject.SetActive(false);
            if (!setRight)
            {
                pooledBubble.SetLeft();
                pooledBubble.gameObject.transform.Find("NameText (TMP)").transform.localPosition += new Vector3(-0.7f, 0f);
                pooledBubble.gameObject.transform.Find("ChatText (TMP)").transform.localPosition += new Vector3(-0.7f, 0f);
            }
            else
            {
                pooledBubble.SetRight();
            }
            chat.SetChatBubbleName(pooledBubble, data, data.IsDead, false, PlayerNameColor.Get(data), null);
            pooledBubble.SetText(text);
            pooledBubble.AlignChildren();
            chat.AlignAllBubbles();
            pooledBubble.NameText.text = MsgName;
            if (!chat.IsOpenOrOpening && chat.notificationRoutine == null)
            {
                chat.notificationRoutine = chat.StartCoroutine(chat.BounceDot());
            }
            SoundManager.Instance.PlaySound(chat.messageSound, false, 1f, null).pitch = 0.5f + data.PlayerId / 15f;
            ChatPatch.ChatControllerPatch.SetChatPoolTheme(pooledBubble);
        }
        catch
        {
        }
    }
    internal static bool SystemTypeIsSabotage(SystemTypes type) => type is SystemTypes.Reactor
                    or SystemTypes.Laboratory
                    or SystemTypes.Comms
                    or SystemTypes.LifeSupp
                    or SystemTypes.MushroomMixupSabotage
                    or SystemTypes.HeliSabotage
                    or SystemTypes.Electrical;
    internal static bool SystemTypeIsSabotage(int typeNum) => (SystemTypes)typeNum is SystemTypes.Reactor
                or SystemTypes.Laboratory
                or SystemTypes.Comms
                or SystemTypes.LifeSupp
                or SystemTypes.MushroomMixupSabotage
                or SystemTypes.HeliSabotage
                or SystemTypes.Electrical;
    // Get players HashPuid
    internal static string GetHashPuid(PlayerControl player)
    {
        if (player?.Data?.Puid == null) return "";
        return GetHashStr(player.Data.Puid);
    }
    // Get HashPuid from puid
    internal static string GetHashStr(this string str)
    {
        if (string.IsNullOrEmpty(str)) return "";

        using SHA256 sha256 = SHA256.Create();
        byte[] sha256Bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(str));
        string sha256Hash = BitConverter.ToString(sha256Bytes).Replace("-", "").ToLower();
        return sha256Hash.Substring(0, 5) + sha256Hash.Substring(sha256Hash.Length - 4);
    }
    internal static ushort GetHashUInt16(string input)
    {
        if (string.IsNullOrEmpty(input)) return 0;

        return (ushort)(BitConverter.ToUInt16(SHA256.HashData(Encoding.UTF8.GetBytes(input)), 0) % 65536);
    }
    // Remove Html Tags Template
    internal static string RemoveHtmlTagsTemplate(string str) => Regex.Replace(str, "", "");
    // Get raw text
    internal static string RemoveHtmlText(string text)
    {
        text = Regex.Replace(text, "<[^>]*>", "");
        text = Regex.Replace(text, "{[^}]*}", "");
        text = text.Replace("\n", " ").Replace("\r", " ");
        text = text.Trim();

        return text;
    }

    internal static string ToColor(this string str, string hexColor) => $"<{hexColor}>{str}</color>";
    internal static string ToColor(this string str, Color color) => $"<{Color32ToHex(color)}>{str}</color>";

    internal static bool IsHtmlText(string text)
    {
        if (Regex.IsMatch(text, "<[^>]*>"))
        {
            return true;
        }
        if (Regex.IsMatch(text, "{[^}]*}"))
        {
            return true;
        }
        if (text.Contains("\n") || text.Contains("\r"))
        {
            return true;
        }

        return false;
    }

    // Get hex color for team
    internal static string GetTeamHexColor(RoleTeamTypes team)
    {
        if (team == RoleTeamTypes.Impostor)
        {
            return "#f00202";
        }
        else
        {
            return "#8cffff";
        }
    }
    internal static string Color32ToHex(Color32 color) => $"#{color.r:X2}{color.g:X2}{color.b:X2}{255:X2}";

    internal static Color HexToColor32(string hex)
    {
        if (hex.StartsWith("#"))
        {
            hex = hex.Substring(1);
        }

        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        return new Color32(r, g, b, 255);
    }

    internal static Color LerpColor(Color[] colors, (float min, float max) lerpRange, float t, bool reverse = false)
    {
        float normalizedT = Mathf.InverseLerp(lerpRange.min, lerpRange.max, t);

        if (colors.Length == 1)
            return colors[0];

        if (reverse)
        {
            colors.Reverse();
        }

        if (normalizedT <= 0f)
            return colors[0];
        if (normalizedT >= 1f)
            return colors[^1];

        float segmentSize = 1f / (colors.Length - 1);
        int segmentIndex = (int)(normalizedT / segmentSize);
        float segmentT = (normalizedT - segmentIndex * segmentSize) / segmentSize;

        return Color.Lerp(colors[segmentIndex], colors[segmentIndex + 1], segmentT);
    }

    internal static void DisconnectAccountFromOnline(bool apiError = false)
    {
        if (GameState.IsInGame)
        {
            AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
        }

        DataManager.Player.Account.LoginStatus = EOSManager.AccountLoginStatus.Offline;
        DataManager.Player.Save();
        if (apiError)
        {
            ShowPopUp(Translator.GetString("DataBaseConnect.InitFailure"), true);
        }
    }

    // Disconnect client
    internal static void DisconnectSelf(string reason, bool showReason = true)
    {
        AmongUsClient.Instance.ExitGame(0);
        _ = new LateTask(() =>
        {
            SceneChanger.ChangeScene("MainMenu");
            if (showReason)
            {
                _ = new LateTask(() =>
                {
                    var lines = "<color=#ebbd34>----------------------------------------------------------------------------------------------</color>";
                    ShowPopUp($"{lines}\n\n\n<size=150%>{reason}</size>\n\n\n{lines}");
                }, 0.1f, "DisconnectSelf 2");
            }
        }, 0.2f, "DisconnectSelf 1");
    }

    // Show dc pop up with text
    internal static void ShowPopUp(string text, bool enableWordWrapping = false)
    {
        DisconnectPopup.Instance.gameObject.SetActive(true);
        DisconnectPopup.Instance._textArea.enableWordWrapping = enableWordWrapping;
        DisconnectPopup.Instance._textArea.text = text;
    }

    internal static Dictionary<string, Sprite> CachedSprites = [];

    internal static Sprite? LoadSprite(string path, float pixelsPerUnit = 1f)
    {
        try
        {
            if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite))
                return sprite;

            var texture = LoadTextureFromResources(path);
            if (texture == null)
                return null;

            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;

            // Logger.Log($"Successfully loaded sprite from {path}");
            return CachedSprites[path + pixelsPerUnit] = sprite;
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            return null;
        }
    }

    internal static Texture2D? LoadTextureFromResources(string path)
    {
        try
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            if (stream == null)
                return null;

            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                if (!texture.LoadImage(ms.ToArray(), false))
                    return null;
            }

            // Logger.Log($"Successfully loaded texture from {path}");
            return texture;
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            return null;
        }
    }

    // Get platform name
    internal static string GetPlatformName(PlayerControl player, bool useTag = false)
    {
        if (player == null) return string.Empty;
        if (player.GetClient() == null) return string.Empty;

        string PlatformName = string.Empty;
        string Tag = string.Empty;

        Platforms platform = player.GetClient().PlatformData.Platform;

        switch (platform)
        {
            case Platforms.StandaloneSteamPC:
                PlatformName = "Steam";
                Tag = "PC";
                break;
            case Platforms.StandaloneEpicPC:
                PlatformName = "Epic Games";
                Tag = "PC";
                break;
            case Platforms.StandaloneWin10:
                PlatformName = "Microsoft Store";
                Tag = "PC";
                break;
            case Platforms.StandaloneMac:
                PlatformName = "Mac OS";
                Tag = "PC";
                break;
            case Platforms.StandaloneItch:
                PlatformName = "Itch.io";
                Tag = "PC";
                break;
            case Platforms.Xbox:
                PlatformName = "Xbox";
                Tag = "Console";
                break;
            case Platforms.Playstation:
                PlatformName = "Playstation";
                Tag = "Console";
                break;
            case Platforms.Switch:
                PlatformName = "Switch";
                Tag = "Console";
                break;
            case Platforms.Android:
                PlatformName = "Android";
                Tag = "Mobile";
                break;
            case Platforms.IPhone:
                PlatformName = "IPhone";
                Tag = "Mobile";
                break;
            case Platforms p when !Enum.IsDefined(p):
            case Platforms.Unknown:
                PlatformName = "Unknown";
                useTag = false;
                break;
            default:
                PlatformName = "None";
                useTag = false;
                break;
        }

        if (useTag == false)
            return PlatformName;

        return $"{Tag}: {PlatformName}";
    }

    internal static string GetPlatformName(Platforms platform, bool useTag = false)
    {
        string Tag = string.Empty;

        string PlatformName;
        switch (platform)
        {
            case Platforms.StandaloneSteamPC:
                PlatformName = "Steam";
                Tag = "PC";
                break;
            case Platforms.StandaloneEpicPC:
                PlatformName = "Epic Games";
                Tag = "PC";
                break;
            case Platforms.StandaloneWin10:
                PlatformName = "Microsoft Store";
                Tag = "PC";
                break;
            case Platforms.StandaloneMac:
                PlatformName = "Mac OS";
                Tag = "PC";
                break;
            case Platforms.StandaloneItch:
                PlatformName = "Itch.io";
                Tag = "PC";
                break;
            case Platforms.Xbox:
                PlatformName = "Xbox";
                Tag = "Console";
                break;
            case Platforms.Playstation:
                PlatformName = "Playstation";
                Tag = "Console";
                break;
            case Platforms.Switch:
                PlatformName = "Switch";
                Tag = "Console";
                break;
            case Platforms.Android:
                PlatformName = "Android";
                Tag = "Mobile";
                break;
            case Platforms.IPhone:
                PlatformName = "IPhone";
                Tag = "Mobile";
                break;
            case Platforms.Unknown:
                PlatformName = "None";
                break;
            default:
                return string.Empty;
        }

        if (useTag == false)
            return PlatformName;

        return $"{Tag}: {PlatformName}";
    }
}