using AmongUs.GameOptions;
using InnerNet;
using UnityEngine;

namespace BetterAmongUs;

public static class PlayerControlDataExtension
{
    public class ExtendedPlayerInfo
    {
        public NetworkedPlayerInfo? _Data { get; set; }
        public string RealName { get; set; } = string.Empty;
        public Dictionary<byte, string> LastNameSetFor { get; set; } = [];
        public bool IsBetterUser { get; set; } = false;
        public bool IsBetterHost { get; set; } = false;
        public bool IsTOHEHost { get; set; } = false;
        public bool BannedByAntiCheat { get; set; } = false;
        public int TimesAttemptedKilled { get; set; } = 0;

        // Track Game Info
        public int OpenSabotageNum { get; set; } = 0;
        public bool IsFixingPanelSabotage => OpenSabotageNum != 0;
        public float TimeSinceLastTask { get; set; } = 5f;
        public uint LastTaskId { get; set; } = 999;
        public int TimesCalledMeeting { get; set; } = 0;
        public DisconnectReasons DisconnectReason { get; set; } = DisconnectReasons.Unknown;
        public ExtendedRoleInfo? RoleInfo { get; set; }
    }

    public class ExtendedRoleInfo
    {
        public int Kills { get; set; } = 0;
        public bool HasNoisemakerNotify { get; set; } = false;
        public RoleTypes DeadDisplayRole { get; set; }
    }

    public static readonly Dictionary<string, ExtendedPlayerInfo> playerInfo = [];

    // Reset info when needed
    public static void ResetPlayerData(PlayerControl player)
    {
        if (GameStates.IsLobby)
        {
            player.BetterData().TimesCalledMeeting = 0;
            player.BetterData().RoleInfo.HasNoisemakerNotify = false;
            player.BetterData().TimeSinceLastTask = 5f;
            player.BetterData().LastTaskId = 999;
            player.BetterData().RoleInfo.Kills = 0;
            player.BetterData().OpenSabotageNum = 0;
            player.BetterData().TimesAttemptedKilled = 0;
        }
        else
        {
            player.BetterData().TimeSinceLastTask += Time.deltaTime;

            if (player.IsAlive() || player.Data.RoleType == RoleTypes.GuardianAngel)
                player.BetterData().RoleInfo.DeadDisplayRole = player.Data.RoleType;
        }
    }

    // Get BetterData from PlayerControl
    public static ExtendedPlayerInfo? BetterData(this PlayerControl player)
    {
        if (player == null) return null;

        if (!playerInfo.ContainsKey(player.Data.Puid))
        {
            playerInfo[player.Data.Puid] = new ExtendedPlayerInfo
            {
                _Data = player.Data,
                RoleInfo = new ExtendedRoleInfo()
            };
        }

        return playerInfo[player.Data.Puid];
    }

    // Get BetterData from NetworkedPlayerInfo
    public static ExtendedPlayerInfo? BetterData(this NetworkedPlayerInfo info)
    {
        if (!playerInfo.ContainsKey(info.Puid))
        {
            playerInfo[info.Puid] = new ExtendedPlayerInfo
            {
                _Data = info,
                RoleInfo = new ExtendedRoleInfo()
            };
        }

        return playerInfo[info.Puid];
    }

    // Get BetterData from ClientData
    public static ExtendedPlayerInfo? BetterData(this ClientData data)
    {
        var player = Utils.PlayerFromClientId(data.Id);

        if (player != null)
        {
            if (!playerInfo.ContainsKey(player.Data.Puid))
            {
                playerInfo[player.Data.Puid] = new ExtendedPlayerInfo
                {
                    _Data = player.Data,
                    RoleInfo = new ExtendedRoleInfo()
                };
            }

            return playerInfo[player.Data.Puid];
        }

        return null;
    }
}
