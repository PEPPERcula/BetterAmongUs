using AmongUs.GameOptions;
using BetterAmongUs.Modules;
using BetterAmongUs.Modules.AntiCheat;
using BetterAmongUs.Patches;
using InnerNet;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Helpers;

static class PlayerControlHelper
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
    public static int GetClientId(this PlayerControl player) => player?.GetClient()?.Id != null ? player.GetClient().Id : -1;
    // Get player name with outfit color
    public static string GetPlayerNameAndColor(this PlayerControl player)
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

    public static void DirtyName(this PlayerControl player) => player.BetterData().IsDirtyInfo = true;
    public static void DirtyName(this NetworkedPlayerInfo data) => data.BetterData().IsDirtyInfo = true;
    public static void DirtyNameDelay(this PlayerControl player, float delay = 1f) => player.Data.DirtyNameDelay(delay);
    public static void DirtyNameDelay(this NetworkedPlayerInfo data, float delay = 1f)
    {
        _ = new LateTask(() =>
        {
            if (data != null)
            {
                data.DirtyName();
            }
        }, delay, shouldLog: false);
    }


    // Set players over head text
    public static void SetPlayerTextInfo(this PlayerControl player, string text, bool isBottom = false, bool isInfo = false)
    {
        if (player == null) return;

        var textTop = player.BetterPlayerControl().InfoTextTop;
        var textBottom = player.BetterPlayerControl().InfoTextBottom;
        var textInfo = player.BetterPlayerControl().InfoTextInfo;

        var targetText = isBottom ? textBottom : textTop;
        if (isInfo)
        {
            targetText = textInfo;

            if (string.IsNullOrEmpty(Utils.RemoveHtmlText(textTop.text)))
            {
                text = "<voffset=-2.25em>" + text + "</voffset>";
            }
        }

        text = "<size=65%>" + text + "</size>";
        if (targetText != null)
        {
            targetText.text = text;
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

        if (player.isDummy || GameState.IsLocalGame/* || !GameState.IsVanillaServer*/)
        {
            return true;
        }

        if (player.gameObject.transform.Find("Names/NameText_TMP").GetComponent<TextMeshPro>().text
        is "???" or "Player" or "<color=#b5b5b5>Loading</color>" or "" or null
        || player.Data == null
        || player.CurrentOutfit == null
        || player.CurrentOutfit.ColorId == -1)
        {
            return false;
        }
        return true;
    }
    // Kick player
    public static void Kick(this PlayerControl player, bool ban = false, string setReasonInfo = "", bool AntiCheatBan = false, bool bypassDataCheck = false)
    {
        var Ban = ban;

        if (!GameState.IsHost || player.IsLocalPlayer() || !player.DataIsCollected() && !bypassDataCheck || player.IsHost() || player.isDummy)
        {
            return;
        }

        if (AntiCheatBan)
        {
            if (BetterGameSettings.WhenCheating.GetValue() == 0)
            {
                return;
            }

            Ban = Ban && BetterGameSettings.WhenCheating.GetValue() == 2;
        }

        if (setReasonInfo != "")
        {
            GameDataShowNotificationPatch.BetterShowNotification(player.Data, forceReasonText: string.Format(setReasonInfo, Ban ? Translator.GetString("AntiCheat.Ban").ToLower() : Translator.GetString("AntiCheat.Kick").ToLower()));
        }

        AmongUsClient.Instance.KickPlayer(player.GetClientId(), Ban);

        player.BetterData().AntiCheatInfo.BannedByAntiCheat = AntiCheatBan;
    }
    // Set color outline on player
    public static void SetOutline(this PlayerControl player, bool active, Color? color = null)
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
    public static void SetOutlineByHex(this PlayerControl player, bool active, string hexColor = "")
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
    // RPCs
    // Set name for Target
    public static void RpcSetNamePrivate(this PlayerControl player, string name, PlayerControl target)
    {
        if (player == null || target == null) return;
        RPC.SetNamePrivate(player, name, target);
    }
    // Exile player
    public static void RpcExile(this PlayerControl player)
    {
        if (player == null) return;
        RPC.ExileAsync(player);
    }
    // Check if player is selecting room to spawn in, for Airship
    public static bool IsInRoomSelect(this PlayerControl player)
    {
        if (player == null) return false;
        return GameState.AirshipIsActive && Vector2.Distance(player.GetTruePosition(), new(-25, 40)) < 5f;
    }
    // Check if player controller is self client
    public static bool IsLocalPlayer(this PlayerControl player) => player != null && PlayerControl.LocalPlayer != null && player == PlayerControl.LocalPlayer;
    // Get vent Id that the player is in.
    public static int GetPlayerVentId(this PlayerControl player)
    {
        if (player == null) return -1;

        if (!(ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Ventilation, out var systemType) &&
              systemType.TryCast<VentilationSystem>() is VentilationSystem ventilationSystem))
            return 0;

        return ventilationSystem.PlayersInsideVents.TryGetValue(player.PlayerId, out var playerIdVentId) ? playerIdVentId : -1;
    }
    // Get true position
    public static Vector2 GetCustomPosition(this PlayerControl player) => new(player.transform.position.x, player.transform.position.y);
    // Check if player is a dev
    public static bool IsDev(this PlayerControl player) => player.BetterData().MyUserData.IsDev();
    // Check if player is alive
    public static bool IsAlive(this PlayerControl player) => player?.Data != null && !player.Data.IsDead;
    // Check if player is in a vent
    public static bool IsInVent(this PlayerControl player) => player != null && (player.inVent || player.walkingToVent || player.MyPhysics?.Animations?.IsPlayingEnterVentAnimation() == true);
    // Check if player role name

    public static void UpdateColorBlindTextPosition(this PlayerControl player)
    {
        var text = player.cosmetics.colorBlindText;
        if (!text.enabled) return;
        if (!player.onLadder && !player.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
        {
            text.transform.localPosition = new Vector3(0f, -1.5f, 0.4999f);
        }
        else
        {
            text.transform.localPosition = new Vector3(0f, -1.75f, 0.4999f);
        }
    }

    public static string GetRoleName(this PlayerControl player)
    {
        if (!player.IsAlive() && !player.IsGhostRole() && Main.GetRoleName().TryGetValue((int)player.BetterData().RoleInfo.DeadDisplayRole, out var roleName))
        {
            return roleName;
        }

        if (player?.Data != null && Main.GetRoleName().TryGetValue((int)player.Data.RoleType, out var roleName2))
        {
            return roleName2;
        }

        return string.Empty;
    }
    // Check if player is Shapeshifting
    public static bool IsInShapeshift(this PlayerControl player) => player != null && (player.shapeshiftTargetPlayerId > -1 || player.shapeshifting) && !player.waitingForShapeshiftResponse;
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
    public static bool IsImpostorTeam(this PlayerControl player) => player?.Data != null && player.Data.RoleType is RoleTypes.Impostor or RoleTypes.ImpostorGhost or RoleTypes.Shapeshifter or RoleTypes.Phantom;
    // Check if player is a imposter teammate
    public static bool IsImpostorTeammate(this PlayerControl player) =>
        player != null && PlayerControl.LocalPlayer != null &&
        (player.IsLocalPlayer() && PlayerControl.LocalPlayer.IsImpostorTeam() ||
        PlayerControl.LocalPlayer.IsImpostorTeam() && player.IsImpostorTeam());
    // Check if player is in the Anti-Cheat list
    public static bool IsCheater(this PlayerControl player) =>
        player != null && (BAUAntiCheat.PlayerData.ContainsKey(player.GetHashPuid()) ||
                           BAUAntiCheat.SickoData.ContainsKey(player.GetHashPuid()) ||
                           BAUAntiCheat.AUMData.ContainsKey(player.GetHashPuid()));
    // Check if player is the host
    public static bool IsHost(this PlayerControl player) => player?.Data != null && GameData.Instance?.GetHost()?.Puid == player.Data.Puid;

    // Get players HashPuid
    public static string GetHashPuid(this PlayerControl player)
    {
        return player.Data.GetHashPuid() ?? "";
    }
    public static string GetHashPuid(this NetworkedPlayerInfo data)
    {
        if (data?.Puid == null) return "";
        return Utils.GetHashStr(data.Puid);
    }
    // Get players Friendcode
    public static string GetHashFriendcode(this PlayerControl player)
    {
        return player.Data.GetHashFriendcode() ?? "";
    }
    public static string GetHashFriendcode(this NetworkedPlayerInfo data)
    {
        if (data?.FriendCode == null) return "";
        return Utils.GetHashStr(data.FriendCode);
    }

    // Report player
    public static void ReportPlayer(this PlayerControl player, ReportReasons reason = ReportReasons.None)
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
