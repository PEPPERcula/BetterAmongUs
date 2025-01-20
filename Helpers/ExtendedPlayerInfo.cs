using AmongUs.GameOptions;
using BetterAmongUs.Items;
using HarmonyLib;
using InnerNet;
using UnityEngine;

namespace BetterAmongUs.Helpers;

public class ExtendedPlayerInfo : MonoBehaviour
{
    private bool hasSet = false;
    public void SetInfo(NetworkedPlayerInfo data)
    {
        if (hasSet) return;
        MyUserData = UserData.GetPlayerUserData(data);
        _Data = data;
        _PlayerId = data.PlayerId;
        hasSet = true;
    }

    public void Update()
    {
        AntiCheatInfo.TimeSinceLastTask += Time.deltaTime;
    }

    public void LateUpdate()
    {
        if (gameObject == null) return;

        if (_Data != null && string.IsNullOrEmpty(RealName) && !string.IsNullOrEmpty(_Data.PlayerName))
        {
            RealName = _Data.PlayerName;
        }
    }

    public UserData? MyUserData { get; private set; } = UserData.AllUsers.First();
    public byte _PlayerId { get; private set; }
    public NetworkedPlayerInfo? _Data { get; private set; }
    public string? RealName { get; private set; }
    public bool IsDirtyInfo { get; set; } = true;
    public Dictionary<byte, string> LastNameSetFor { get; set; } = [];
    public bool IsBetterUser { get; set; } = false;
    public bool IsVerifiedBetterUser { get; set; } = false;
    public bool IsTOHEHost { get; set; } = false;
    public bool HasShowDcMsg { get; set; } = false;
    public DisconnectReasons DisconnectReason { get; set; } = DisconnectReasons.Unknown;
    public ExtendedRoleInfo? RoleInfo { get; } = new();
    public ExtendedAntiCheatInfo? AntiCheatInfo { get; } = new();
}

public class ExtendedAntiCheatInfo
{
    public bool BannedByAntiCheat { get; set; } = false;
    public List<string> AUMChats { get; set; } = [];
    public int TimesAttemptedKilled { get; set; } = 0;
    public int OpenSabotageNum { get; set; } = 0;
    public bool IsFixingPanelSabotage => OpenSabotageNum != 0;
    public float TimeSinceLastTask { get; set; } = 5f;
    public uint LastTaskId { get; set; } = 999;
    public bool HasSetName { get; set; }
    public bool HasSetLevel { get; set; }
}

public class ExtendedRoleInfo
{
    public int Kills { get; set; } = 0;
    public bool HasNoisemakerNotify { get; set; } = false;
    public RoleTypes DeadDisplayRole { get; set; }
}

public static class PlayerControlDataExtension
{
    [HarmonyPatch(typeof(NetworkedPlayerInfo))]
    class NetworkedPlayerInfoPatch
    {
        [HarmonyPatch(nameof(NetworkedPlayerInfo.Init))]
        [HarmonyPostfix]
        public static void Init_Postfix(NetworkedPlayerInfo __instance)
        {
            TryCreateExtendedData(__instance);
        }

        [HarmonyPatch(nameof(NetworkedPlayerInfo.Serialize))]
        [HarmonyPostfix]
        public static void Serialize_Postfix(NetworkedPlayerInfo __instance)
        {
            __instance.DirtyNameDelay();
        }

        [HarmonyPatch(nameof(NetworkedPlayerInfo.Deserialize))]
        [HarmonyPostfix]
        public static void Deserialize_Postfix(NetworkedPlayerInfo __instance)
        {
            TryCreateExtendedData(__instance);
            __instance.DirtyNameDelay();
        }

        public static void TryCreateExtendedData(NetworkedPlayerInfo data)
        {
            if (data.BetterData() == null)
            {
                ExtendedPlayerInfo newBetterData = data.gameObject.AddComponent<ExtendedPlayerInfo>();
                newBetterData.SetInfo(data);
                data.DirtyNameDelay(3f);
            }
        }
    }

    // Get BetterData from PlayerControl
    public static ExtendedPlayerInfo? BetterData(this PlayerControl player)
    {
        return player?.Data?.GetComponent<ExtendedPlayerInfo>();
    }

    // Get BetterData from NetworkedPlayerInfo
    public static ExtendedPlayerInfo? BetterData(this NetworkedPlayerInfo data)
    {
        return data?.GetComponent<ExtendedPlayerInfo>();
    }

    // Get BetterData from ClientData
    public static ExtendedPlayerInfo? BetterData(this ClientData data)
    {
        var player = Utils.PlayerFromClientId(data.Id);
        return player?.Data?.GetComponent<ExtendedPlayerInfo>();
    }
}
