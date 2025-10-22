using AmongUs.GameOptions;
using BetterAmongUs.Data;
using BetterAmongUs.Modules;
using BetterAmongUs.Network;
using BetterAmongUs.Patches.Gameplay.Player;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using InnerNet;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Helpers;

static class PlayerControlHelper
{
    // Get players client
    internal static ClientData? GetClient(this PlayerControl player)
    {
        if (AmongUsClient.Instance?.allClients == null || player == null)
            return null;

        foreach (var client in AmongUsClient.Instance.allClients)
        {
            if (client?.Character?.PlayerId == player.PlayerId)
                return client;
        }
        return null;
    }
    // Get players client id
    internal static int GetClientId(this PlayerControl player) => player?.GetClient()?.Id ?? -1;
    // Get player name with outfit color
    internal static string GetPlayerNameAndColor(this PlayerControl player)
    {
        if (player?.Data == null) return string.Empty;

        try
        {
            return $"<color={Utils.Color32ToHex(Palette.PlayerColors[player.Data.DefaultOutfit.ColorId])}>{player.Data.PlayerName}</color>";
        }
        catch
        {
            return player.Data.PlayerName;
        }
    }

    // Check if players character has been created and received from the Host
    internal static bool DataIsCollected(this PlayerControl player)
    {
        if (player == null) return false;

        if (player.isDummy || GameState.IsLocalGame/* || !GameState.IsVanillaServer*/)
        {
            return true;
        }

        if (player.gameObject.transform.Find("Names/NameText_TMP").GetComponent<TextMeshPro>().text
        is "???" or "Player" or "" or null
        || player.Data == null
        || player.CurrentOutfit == null
        || player.CurrentOutfit.ColorId == -1)
        {
            return false;
        }
        return true;
    }
    // Kick player
    internal static void Kick(this PlayerControl player, bool ban = false, string setReasonInfo = "", bool AntiCheatBan = false, bool bypassDataCheck = false, bool forceBan = false)
    {
        var Ban = ban || forceBan;

        if (!GameState.IsHost || player.IsLocalPlayer() || !player.DataIsCollected() && !bypassDataCheck || player.IsHost() || player.isDummy)
        {
            return;
        }

        if (AntiCheatBan)
        {
            if (BetterGameSettings.WhenCheating.GetStringValue() == 0 && !forceBan)
            {
                return;
            }

            Ban = (Ban && BetterGameSettings.WhenCheating.GetStringValue() == 2) || forceBan;
        }

        if (setReasonInfo != "")
        {
            GameDataShowNotificationPatch.BetterShowNotification(player.Data, forceReasonText: string.Format(setReasonInfo, Ban ? Translator.GetString("AntiCheat.Ban").ToLower() : Translator.GetString("AntiCheat.Kick").ToLower()));
        }

        AmongUsClient.Instance.KickPlayer(player.GetClientId(), Ban);

        player.BetterData().AntiCheatInfo.BannedByAntiCheat = AntiCheatBan;
    }
    // Set color outline on player
    internal static void SetOutline(this PlayerControl player, bool active, Color? color = null)
    {
        player.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", active ? 1 : 0);
        SpriteRenderer[] longModeParts = player.cosmetics.currentBodySprite.LongModeParts;
        for (int i = 0; i < longModeParts.Length; i++)
        {
            longModeParts[i].material.SetFloat("_Outline", active ? 1 : 0);
        }
        if (color != null)
        {
            player.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", color.Value);
            longModeParts = player.cosmetics.currentBodySprite.LongModeParts;
            for (int i = 0; i < longModeParts.Length; i++)
            {
                longModeParts[i].material.SetColor("_OutlineColor", color.Value);
            }
        }
    }
    // Set color outline on player
    internal static void SetOutlineByHex(this PlayerControl player, bool active, string hexColor = "")
    {
        Color color = Utils.HexToColor32(hexColor);
        player.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", active ? 1 : 0);
        SpriteRenderer[] longModeParts = player.cosmetics.currentBodySprite.LongModeParts;
        for (int i = 0; i < longModeParts.Length; i++)
        {
            longModeParts[i].material.SetFloat("_Outline", active ? 1 : 0);
        }
        if (color != null)
        {
            player.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", color);
            longModeParts = player.cosmetics.currentBodySprite.LongModeParts;
            for (int i = 0; i < longModeParts.Length; i++)
            {
                longModeParts[i].material.SetColor("_OutlineColor", color);
            }
        }
    }

    // Exile player
    internal static void RpcExile(this PlayerControl player)
    {
        if (player == null) return;
        RPC.RpcExile(player);
    }
    // Check if player is selecting room to spawn in, for Airship
    internal static bool IsInRoomSelect(this PlayerControl player)
    {
        if (player == null) return false;
        return GameState.AirshipIsActive && Vector2.Distance(player.GetTruePosition(), new(-25, 40)) < 5f;
    }
    // Check if player controller is self client
    internal static bool IsLocalPlayer(this PlayerControl player) => player != null && PlayerControl.LocalPlayer != null && player == PlayerControl.LocalPlayer;
    // Get vent Id that the player is in.
    internal static int GetPlayerVentId(this PlayerControl player)
    {
        if (player == null) return -1;

        if (!(ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Ventilation, out var systemType) &&
              systemType.TryCast<VentilationSystem>() is VentilationSystem ventilationSystem))
            return 0;

        return ventilationSystem.PlayersInsideVents.TryGetValue(player.PlayerId, out var playerIdVentId) ? playerIdVentId : -1;
    }
    // Get true position
    internal static Vector2 GetCustomPosition(this PlayerControl player) => new(player.transform.position.x, player.transform.position.y);
    // Check if player is a dev
    internal static bool IsDev(this PlayerControl player) => player.BetterData().MyUserData.IsDev();
    // Check if player is alive
    internal static bool IsAlive(this PlayerControl player) => player?.Data != null && !player.Data.IsDead;
    // Check if player is in a vent
    internal static bool IsInVent(this PlayerControl player) => player != null && (player.inVent || player.walkingToVent || player.MyPhysics?.Animations?.IsPlayingEnterVentAnimation() == true);
    // Check if player role name

    internal static string GetRoleName(this PlayerControl player)
    {
        if (!player.IsAlive() && !player.IsGhostRole())
        {
            return player.BetterData().RoleInfo.DeadDisplayRole.GetRoleName();
        }

        if (player?.Data != null)
        {
            return player.Data.RoleType.GetRoleName();
        }

        return string.Empty;
    }
    // Check if player is Shapeshifting
    internal static bool IsInShapeshift(this PlayerControl player) => player != null && (player.shapeshiftTargetPlayerId > -1 || player.shapeshifting) && !player.waitingForShapeshiftResponse;
    // Check if player is in vanish as Phantom
    internal static bool IsInVanish(this PlayerControl player)
    {
        if (player != null && player.Data.Role is PhantomRole phantomRole)
        {
            return phantomRole.fading;
        }
        return false;
    }
    // Get hex color for team
    internal static string GetTeamHexColor(this PlayerControl player) => player.Data.GetTeamHexColor();
    internal static string GetTeamHexColor(this NetworkedPlayerInfo data)
    {
        if (data == null) return "#ffffff";

        if (data.IsImpostorTeam())
        {
            return "#f00202";
        }
        else
        {
            return "#8cffff";
        }
    }

    // Check if player is role type
    internal static bool Is(this PlayerControl player, RoleTypes role) => player?.Data?.RoleType == role;
    // Check if player is Ghost role type
    internal static bool IsGhostRole(this PlayerControl player) => player?.Data?.RoleType is RoleTypes.GuardianAngel;
    // Check if player is on imposter team
    internal static bool IsImpostorTeam(this PlayerControl player) => player?.Data?.IsImpostorTeam() == true;
    internal static bool IsImpostorTeam(this NetworkedPlayerInfo data) => data?.RoleType is RoleTypes.Impostor or RoleTypes.ImpostorGhost or RoleTypes.Shapeshifter or RoleTypes.Phantom or RoleTypes.Viper;
    // Check if player is a imposter teammate
    internal static bool IsImpostorTeammate(this PlayerControl player) =>
        player != null && PlayerControl.LocalPlayer != null &&
        (player.IsLocalPlayer() && PlayerControl.LocalPlayer.IsImpostorTeam() ||
        PlayerControl.LocalPlayer.IsImpostorTeam() && player.IsImpostorTeam());
    // Check if player is in the Anti-Cheat list
    internal static bool IsCheater(this PlayerControl player) => BetterDataManager.BetterDataFile?.CheckPlayerData(player.Data) == true;
    // Check if player is in the Anti-Cheat list
    internal static bool IsCheater(this NetworkedPlayerInfo data) => BetterDataManager.BetterDataFile?.CheckPlayerData(data) == true;
    // Check if player is the host
    internal static bool IsHost(this PlayerControl player) => player?.Data != null && GameData.Instance?.GetHost() == player.Data;

    // Get players HashPuid
    internal static string GetHashPuid(this PlayerControl player)
    {
        return player.Data.GetHashPuid() ?? "";
    }
    internal static string GetHashPuid(this NetworkedPlayerInfo data)
    {
        if (data?.Puid == null) return "";
        return Utils.GetHashStr(data.Puid);
    }
    // Get players Friendcode
    internal static string GetHashFriendcode(this PlayerControl player)
    {
        return player.Data.GetHashFriendcode() ?? "";
    }
    internal static string GetHashFriendcode(this NetworkedPlayerInfo data)
    {
        if (data?.FriendCode == null) return "";
        return Utils.GetHashStr(data.FriendCode);
    }

    // Report player
    internal static void ReportPlayer(this PlayerControl player, ReportReasons reason = ReportReasons.None)
    {
        if (player?.GetClient() != null)
        {
            if (!player.GetClient().HasBeenReported)
            {
                AmongUsClient.Instance.ReportPlayer(player.GetClientId(), reason);
            }
        }
    }
}
