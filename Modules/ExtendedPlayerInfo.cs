using AmongUs.GameOptions;
using InnerNet;
using UnityEngine;

namespace BetterAmongUs;

public static class PlayerControlDataExtension
{
    // Base
    public class ExtendedPlayerInfo
    {
        public BetterAccountInfo? AccountInfo { get; set; }
        public NetworkedPlayerInfo? _Data { get; set; }
        public string? RealName { get; set; }
        public Dictionary<byte, string> LastNameSetFor { get; set; } = [];
        public bool IsBetterUser { get; set; } = false;
        public bool IsVerifiedBetterUser { get; set; } = false;
        public bool IsBetterHost { get; set; } = false;
        public bool IsTOHEHost { get; set; } = false;
        public bool HasShowDcMsg { get; set; } = false;

        // Track Game Info
        public DisconnectReasons DisconnectReason { get; set; } = DisconnectReasons.Unknown;
        public ExtendedRoleInfo? RoleInfo { get; set; }
        public ExtendedAntiCheatInfo? AntiCheatInfo { get; set; }
    }

    public class ExtendedAntiCheatInfo
    {
        public bool BannedByAntiCheat { get; set; } = false;
        public List<string> AUMChats { get; set; } = [];
        public int TimesAttemptedKilled { get; set; } = 0;
        public int OpenSabotageNum { get; set; } = 0;
        public bool IsFixingPanelSabotage => OpenSabotageNum != 0;
        public int TimesCalledMeeting { get; set; } = 0;
        public float TimeSinceLastTask { get; set; } = 5f;
        public uint LastTaskId { get; set; } = 999;
    }

    public class ExtendedRoleInfo
    {
        public int Kills { get; set; } = 0;
        public bool HasNoisemakerNotify { get; set; } = false;
        public RoleTypes DeadDisplayRole { get; set; }
    }

    public static readonly Dictionary<string, ExtendedPlayerInfo> playerInfo = [];

    public static void ClearData(this ExtendedPlayerInfo BetterData) => playerInfo[BetterData._Data.Puid] = new ExtendedPlayerInfo
    {
        _Data = BetterData._Data,
        RoleInfo = new ExtendedRoleInfo(),
        AntiCheatInfo = new ExtendedAntiCheatInfo(),
        AccountInfo = new BetterAccountInfo()
    };

    // Reset info when needed
    public static void ResetPlayerData(PlayerControl player)
    {
        if (player == null) return;

        if (GameStates.IsLobby)
        {
            player.BetterData().AntiCheatInfo.TimesCalledMeeting = 0;
            player.BetterData().RoleInfo.HasNoisemakerNotify = false;
            player.BetterData().AntiCheatInfo.TimeSinceLastTask = 5f;
            player.BetterData().AntiCheatInfo.LastTaskId = 999;
            player.BetterData().RoleInfo.Kills = 0;
            player.BetterData().AntiCheatInfo.OpenSabotageNum = 0;
            player.BetterData().AntiCheatInfo.TimesAttemptedKilled = 0;
        }
        else
        {
            player.BetterData().AntiCheatInfo.TimeSinceLastTask += Time.deltaTime;

            if (player.IsAlive() || player.Data.RoleType == RoleTypes.GuardianAngel)
                player.BetterData().RoleInfo.DeadDisplayRole = player.Data.RoleType;
        }

        if (string.IsNullOrEmpty(player.BetterData().RealName))
        {
            player.BetterData().RealName = player.Data.PlayerName;
        }
    }

    // Helper method to initialize and return BetterData
    private static ExtendedPlayerInfo GetOrCreateBetterData(string puid, NetworkedPlayerInfo data, string? realName = null)
    {
        if (!playerInfo.ContainsKey(puid))
        {
            playerInfo[puid] = new ExtendedPlayerInfo
            {
                _Data = data,
                RealName = realName,
                RoleInfo = new ExtendedRoleInfo(),
                AntiCheatInfo = new ExtendedAntiCheatInfo(),
                AccountInfo = new BetterAccountInfo()
            };
        }
        return playerInfo[puid];
    }

    // Get BetterData from PlayerControl
    public static ExtendedPlayerInfo? BetterData(this PlayerControl player)
    {
        return player == null ? null : GetOrCreateBetterData(player.Data.Puid, player.Data);
    }

    // Get BetterData from NetworkedPlayerInfo
    public static ExtendedPlayerInfo? BetterData(this NetworkedPlayerInfo info)
    {
        return GetOrCreateBetterData(info.Puid, info);
    }

    // Get BetterData from ClientData
    public static ExtendedPlayerInfo? BetterData(this ClientData data)
    {
        var player = Utils.PlayerFromClientId(data.Id);
        return player == null ? null : GetOrCreateBetterData(player.Data.Puid, player.Data, player.Data.PlayerName);
    }

}
