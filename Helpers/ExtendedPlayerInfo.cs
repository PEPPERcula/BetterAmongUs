using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils;
using BetterAmongUs.Items;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules;
using InnerNet;
using UnityEngine;

namespace BetterAmongUs.Helpers;

internal class ExtendedPlayerInfo : MonoBehaviour, IMonoExtension<NetworkedPlayerInfo>
{
    internal ExtendedPlayerInfo()
    {
        HandshakeHandler = new(this);
    }

    public NetworkedPlayerInfo? BaseMono { get; set; }
    internal NetworkedPlayerInfo? _Data => BaseMono;

    private bool hasSet = false;
    internal void SetInfo(NetworkedPlayerInfo data)
    {
        if (hasSet) return;
        MyUserData = UserData.GetPlayerUserData(data);
        _PlayerId = data.PlayerId;
        hasSet = true;
    }

    private float timeAccumulator = 0f;

    private void Awake()
    {
        if (!MonoExtensionManager.RegisterExtension(this)) return;
        this.StartCoroutine(HandshakeHandler.CoSendSecretToPlayer());
    }

    private void OnDestroy()
    {
        MonoExtensionManager.UnregisterExtension(this);
    }

    internal void Update()
    {
        var time = Time.deltaTime;

        AntiCheatInfo.TimeSinceLastTask += time;

        if (AntiCheatInfo.RPCSentPS > 0)
        {
            bool flag = _Data.Object.IsCheater();

            if (AntiCheatInfo.RPCSentPS >= ExtendedAntiCheatInfo.MaxRPCSent && !flag)
            {
                BetterNotificationManager.NotifyCheat(_Data.Object,
                    Translator.GetString("AntiCheat.Reason.RPCSentPS"),
                    Translator.GetString("AntiCheat.UnauthorizedAction")
                );
                Logger.LogCheat($"{_Data.Object.BetterData().RealName} {AntiCheatInfo.RPCSentPS} Sent.");
            }

            timeAccumulator += time;
            if (timeAccumulator >= 0.25f - (0.005 * AntiCheatInfo.RPCSentPS))
            {
                AntiCheatInfo.RPCSentPS -= 1;
                timeAccumulator = 0f;
            }
        }
    }

    internal void LateUpdate()
    {
        if (gameObject == null) return;

        if (_Data != null && string.IsNullOrEmpty(RealName) && !string.IsNullOrEmpty(_Data.PlayerName))
        {
            RealName = _Data.PlayerName;
        }
    }

    internal HandshakeHandler HandshakeHandler { get; }
    internal UserData? MyUserData { get; private set; } = UserData.AllUsers.First();
    internal byte _PlayerId { get; private set; }
    internal string? RealName { get; private set; }
    internal bool IsDirtyInfo { get; set; } = true;
    internal Dictionary<byte, string> LastNameSetFor { get; set; } = [];
    internal bool IsBetterUser { get; set; } = false;
    internal bool IsVerifiedBetterUser { get; set; } = false;
    internal bool IsTOHEHost { get; set; } = false;
    internal bool HasShowDcMsg { get; set; } = false;
    internal DisconnectReasons DisconnectReason { get; set; } = DisconnectReasons.Unknown;
    internal ExtendedRoleInfo? RoleInfo { get; } = new();
    internal ExtendedAntiCheatInfo? AntiCheatInfo { get; } = new();
}

internal class ExtendedAntiCheatInfo
{
    internal bool BannedByAntiCheat { get; set; } = false;
    internal List<string> AUMChats { get; set; } = [];
    internal static int MaxRPCSent => 50;
    internal int RPCSentPS { get; set; } = 0;
    internal int TimesAttemptedKilled { get; set; } = 0;
    internal int OpenSabotageNum { get; set; } = 0;
    internal bool IsFixingPanelSabotage => OpenSabotageNum != 0;
    internal float TimeSinceLastTask { get; set; } = 5f;
    internal uint LastTaskId { get; set; } = 999;
    internal bool HasSetName { get; set; }
    internal bool HasSetLevel { get; set; }
}

internal class ExtendedRoleInfo
{
    internal int Kills { get; set; } = 0;
    internal bool HasNoisemakerNotify { get; set; } = false;
    internal RoleTypes DeadDisplayRole { get; set; }
}

internal static class PlayerControlDataExtension
{
    // Get BetterData from PlayerControl
    internal static ExtendedPlayerInfo? BetterData(this PlayerControl player)
    {
        return MonoExtensionManager.Get<ExtendedPlayerInfo>(player.Data);
    }

    // Get BetterData from NetworkedPlayerInfo
    internal static ExtendedPlayerInfo? BetterData(this NetworkedPlayerInfo data)
    {
        return MonoExtensionManager.Get<ExtendedPlayerInfo>(data);
    }

    // Get BetterData from ClientData
    internal static ExtendedPlayerInfo? BetterData(this ClientData data)
    {
        var player = Utils.PlayerFromClientId(data.Id);
        return MonoExtensionManager.Get<ExtendedPlayerInfo>(player.Data);
    }
}
