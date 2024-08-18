using AmongUs.GameOptions;
using InnerNet;
using TMPro;
using UnityEngine;

namespace BetterAmongUs;

static class ExtendedPlayerControl
{
    // Get players client
    public static ClientData? GetClient(this PlayerControl player)
    {
        try
        {
            var client = AmongUsClient.Instance.allClients.ToArray().FirstOrDefault(cd => cd.Character.PlayerId == player.PlayerId);
            return client;
        }
        catch
        {
            return null;
        }
    }
    // Get players client id
    public static int GetClientId(this PlayerControl player)
    {
        if (player == null) return -1;
        var client = player.GetClient();
        return client == null ? -1 : client.Id;
    }
    // Get player name with outfit color
    public static string GetPlayerNameAndColor(this PlayerControl player)
    {
        if (player?.Data == null) return string.Empty;

        try
        {
            return $"<color={Utils.Color32ToHex(Palette.PlayerColors[player.CurrentOutfit.ColorId])}>{player.Data.PlayerName}</color>";
        }
        catch
        {
            return player.Data.PlayerName;
        }
    }
    // Set players over head text
    public static void SetPlayerTextInfo(this PlayerControl player, string text, bool isBottom = false, bool isInfo = false)
    {
        if (player == null) return;

        string infoType = isBottom ? "InfoText_B_TMP" : "InfoText_T_TMP";

        if (isInfo)
        {
            infoType = "InfoText_Info_TMP";
            var topText = player.gameObject.transform.Find("Names/NameText_TMP/InfoText_T_TMP")?.GetComponent<TextMeshPro>();

            if (topText != null && string.IsNullOrEmpty(Utils.GetRawText(topText.text)))
            {
                text = "<voffset=-2.25em>" + text + "</voffset>";
            }
        }

        text = "<size=65%>" + text + "</size>";
        var textObj = player.gameObject.transform.Find($"Names/NameText_TMP/{infoType}")?.GetComponent<TextMeshPro>();

        if (textObj != null)
        {
            textObj.text = text;
        }
    }

    // Reset players over head text
    public static void ResetAllPlayerTextInfo(this PlayerControl player)
    {
        if (player == null) return;
        player.SetPlayerTextInfo("", isInfo: true);
        player.SetPlayerTextInfo("");
        player.SetPlayerTextInfo("", isBottom: true);
    }
    // Check if players character has been created and received from the Host
    public static bool DataIsCollected(this PlayerControl player)
    {
        if (player == null) return false;

        if (player.isDummy || GameStates.IsLocalGame || !GameStates.IsVanillaServer)
        {
            return true;
        }

        if (player.gameObject.transform.Find("Names/NameText_TMP").GetComponent<TextMeshPro>().text
        is "???" or "Player" or "<color=#b5b5b5>Loading</color>" or null
        || player.Data == null
        || string.IsNullOrEmpty(player.Data.Puid)
        || player.Data.PlayerLevel == uint.MaxValue
        || player.CurrentOutfit == null
        || player.CurrentOutfit.ColorId == -1)
        {
            return false;
        }
        return true;
    }

    // Kick player
    public static void Kick(this PlayerControl player, bool ban = false, string setReasonInfo = "")
    {
        if (!GameStates.IsHost || PlayerControl.LocalPlayer == player || !player.DataIsCollected() || player.IsHost() || player.BetterData().BannedByAntiCheat || player.isDummy)
        {
            return;
        }

        player.BetterData().BannedByAntiCheat = true;

        if (setReasonInfo != "")
        {
            player.RpcSetName(setReasonInfo + "<size=0%>");
        }

        NetworkedPlayerInfo playerData = player.Data;
        string saveName = player.Data.PlayerName;
        AmongUsClient.Instance.KickPlayer(player.GetClientId(), ban);
        playerData.PlayerName = saveName;
    }

    // RPCs
    public static void RpcSendHostChat(this PlayerControl player, string text, string title = "<color=#ffffff><b>(<color=#00ff44>System Message</color>)</b>", bool sendToBetterUser = true)
    {
        if (player == null) return;
        RPC.SendHostChatToPlayer(player, text, title, sendToBetterUser);
    }
    public static void RpcSetNamePrivate(this PlayerControl player, string name, PlayerControl target)
    {
        if (player == null || target == null) return;
        RPC.SetNamePrivate(player, name, target);
    }
    public static void RpcExile(this PlayerControl player)
    {
        if (player == null) return;
        RPC.ExileAsync(player);
    }

    public static bool IsInRoomSelect(this PlayerControl player)
    {
        if (player == null) return false;
        return GameStates.AirshipIsActive && Vector2.Distance(player.GetTruePosition(), new(-25, 40)) < 5f;
    }

    // Get vent Id that the player is in.
    public static int GetPlayerVentId(this PlayerControl player)
    {
        if (player == null) return -1;

        if (!(ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Ventilation, out var systemType) &&
              systemType.TryCast<VentilationSystem>() is VentilationSystem ventilationSystem))
            return 0;

        return ventilationSystem.PlayersInsideVents.TryGetValue(player.PlayerId, out var playerIdVentId) ? playerIdVentId : -1;
    }

    public static Vector2 GetCustomPosition(this PlayerControl player) => new(player.transform.position.x, player.transform.position.y);

    // Check if player is a dev
    public static bool IsDev(this PlayerControl player) => player != null && Main.DevUser.Contains($"{Utils.GetHashPuid(player)}+{player.Data.FriendCode}");
    // Check if player is alive
    public static bool IsAlive(this PlayerControl player) => player?.Data != null && !player.Data.IsDead;
    // Check if player is in a vent
    public static bool IsInVent(this PlayerControl player) => player != null && (player.inVent || player.walkingToVent || player.MyPhysics?.Animations?.IsPlayingEnterVentAnimation() == true);
    // Check if player role name
    public static string GetRoleName(this PlayerControl player)
    {
        if (!player.IsAlive() && !player.IsGhostRole() && Main.GetRoleName.TryGetValue((int)player.BetterData().RoleInfo.DeadDisplayRole, out var roleName))
        {
            return roleName;
        }

        if (player?.Data != null && Main.GetRoleName.TryGetValue((int)player.Data.RoleType, out var roleName2))
        {
            return roleName2;
        }

        return string.Empty;
    }
    // Check if player is Shapeshifting
    public static bool IsInShapeshift(this PlayerControl player) => player != null && (player.shapeshiftTargetPlayerId > -1 || player.shapeshifting);
    // Check if player is in vanish as Phantom
    public static bool IsInVanish(this PlayerControl player)
    {
        if (player != null && player.Data.Role is PhantomRole phantomRole)
        {
            return phantomRole.fading;
        }
        return false;
    }
    // Get hex color for team
    public static string GetTeamHexColor(this PlayerControl player)
    {
        if (player == null) return "#ffffff";

        if (player.IsImpostorTeam())
        {
            return "#f00202";
        }
        else
        {
            return "#8cffff";
        }
    }
    // Check if player is role type
    public static bool Is(this PlayerControl player, RoleTypes role) => player?.Data?.RoleType == role;
    // Check if player is Ghost role type
    public static bool IsGhostRole(this PlayerControl player) => player?.Data?.RoleType is RoleTypes.GuardianAngel;
    // Check if player is on imposter team
    public static bool IsImpostorTeam(this PlayerControl player) => player?.Data != null && (player.Data.RoleType is RoleTypes.Impostor or RoleTypes.ImpostorGhost or RoleTypes.Shapeshifter or RoleTypes.Phantom);
    // Check if player is a imposter teammate
    public static bool IsImpostorTeammate(this PlayerControl player) =>
        player != null && PlayerControl.LocalPlayer != null &&
        ((player == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsImpostorTeam()) ||
        (PlayerControl.LocalPlayer.IsImpostorTeam() && player.IsImpostorTeam()));
    // Check if player is in the Anti-Cheat list
    public static bool IsCheater(this PlayerControl player) =>
        player != null && (AntiCheat.PlayerData.ContainsKey(Utils.GetHashPuid(player)) ||
                           AntiCheat.SickoData.ContainsKey(Utils.GetHashPuid(player)) ||
                           AntiCheat.AUMData.ContainsKey(Utils.GetHashPuid(player)));
    // Check if player is the host
    public static bool IsHost(this PlayerControl player) => player?.Data != null && GameData.Instance?.GetHost()?.Puid == player.Data.Puid;

    // Report player
    public static void ReportPlayer(this PlayerControl player, ReportReasons reason = ReportReasons.None)
    {
        if (player != null)
        {
            if (!player.GetClient().HasBeenReported)
            {
                AmongUsClient.Instance.ReportPlayer(player.GetClientId(), reason);
            }
        }
    }
}
