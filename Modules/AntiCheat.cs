using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using InnerNet;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Rendering;

namespace BetterAmongUs;

class AntiCheat
{
    public static Dictionary<string, string> PlayerData = []; // HashPuid, FriendCode
    public static Dictionary<string, string> SickoData = []; // HashPuid, FriendCode
    public static Dictionary<string, string> AUMData = []; // HashPuid, FriendCode
    private static bool IsEnabled = true;

    public static void Update()
    {
        if (GameStates.IsHost && GameStates.IsInGame)
        {
            RPC.SyncAllNames(isBetterHost: false);

            foreach (var player in Main.AllPlayerControls)
            {
                var hashPuid = Utils.GetHashPuid(player);

                if (SickoData.ContainsKey(hashPuid))
                {
                    player.Kick(true, $"banned by <color=#4f92ff>Anti-Cheat</color>!\n Reason: <color=#00f583>Known Sicko Menu User</color>", true);
                }
                else if (AUMData.ContainsKey(hashPuid))
                {
                    player.Kick(true, $"banned by <color=#4f92ff>Anti-Cheat</color>!\n Reason: <color=#4f0000>Known AUM User</color>", true);
                }
                else if (PlayerData.ContainsKey(hashPuid))
                {
                    player.Kick(true, $"banned by <color=#4f92ff>Anti-Cheat</color>!\n Reason: <color=#fc0000>Known Cheater</color>", true);
                }
            }
        }
    }

    public static void PauseAntiCheat()
    {
        float time = 4f;
        if (IsEnabled)
        {
            IsEnabled = false;

            _ = new LateTask(() =>
            {
                IsEnabled = true;
            }, time, "PauseAntiCheat");
        }
    }

    [HarmonyPatch(typeof(PlatformSpecificData))]
    class PlatformSpecificDataPatch
    {
        [HarmonyPatch(nameof(PlatformSpecificData.Deserialize))]
        [HarmonyPostfix]
        public static void Deserialize_Postfix(PlatformSpecificData __instance)
        {
            if (!Main.AntiCheat.Value) return;

            if (GameStates.IsLobby)
            {
                _ = new LateTask(() =>
                    {
                        var player = Main.AllPlayerControls.FirstOrDefault(pc => pc.GetClient().PlatformData == __instance);

                        if (player != null)
                        {
                            if (__instance.Platform is Platforms.StandaloneWin10 or Platforms.Xbox)
                            {
                                if (__instance.XboxPlatformId.ToString().Length is < 10 or > 16)
                                {
                                    player.ReportPlayer(ReportReasons.Cheating_Hacking);
                                    BetterNotificationManager.NotifyCheat(player, $"Platform Spoofer", newText: "Has been detected with a cheat");
                                    Logger.LogCheat($"{player.Data.PlayerName} Platform Spoofer: {__instance.XboxPlatformId}");
                                }
                            }

                            if (__instance.Platform is Platforms.Playstation)
                            {
                                if (__instance.PsnPlatformId.ToString().Length is < 14 or > 20)
                                {
                                    player.ReportPlayer(ReportReasons.Cheating_Hacking);
                                    BetterNotificationManager.NotifyCheat(player, $"Platform Spoofer", newText: "Has been detected with a cheat");
                                    Logger.LogCheat($"{player.Data.PlayerName} Platform Spoofer: {__instance.PsnPlatformId}");
                                }
                            }
                        }
                    }, 3.5f, shoudLog: false);
            }
        }
    }

    // Handle RPC before anti cheat detection
    public static void HandleRPCBeforeCheck(PlayerControl player, byte callId, MessageReader Oldreader)
    {
        MessageReader reader = MessageReader.Get(Oldreader);

        if (player == PlayerControl.LocalPlayer || PlayerControl.LocalPlayer == null || player == null || !IsEnabled) return;

        if (callId is unchecked((byte)CustomRPC.Sicko) && Main.AntiCheat.Value)
        {
            if (reader.BytesRemaining == 0)
            {
                var flag = SickoData.ContainsKey(Utils.GetHashPuid(player));

                if (reader.BytesRemaining == 0 && !flag)
                {
                    player.ReportPlayer(ReportReasons.Cheating_Hacking);
                    SickoData[Utils.GetHashPuid(player)] = player.FriendCode;
                    BetterDataManager.SaveCheatData(Utils.GetHashPuid(player), player.Data.FriendCode, player.Data.PlayerName, "sickoData", "Sicko Menu RPC");
                    BetterNotificationManager.NotifyCheat(player, $"Sicko Menu", newText: "Has been detected with a cheat client");
                }
            }

            return;
        }

        if (callId is unchecked((byte)CustomRPC.AUM) && Main.AntiCheat.Value)
        {
            try
            {
                var aumid = reader.ReadByte();

                if (aumid == player.PlayerId)
                {
                    var flag = AUMData.ContainsKey(Utils.GetHashPuid(player));

                    if (!flag)
                    {
                        player.ReportPlayer(ReportReasons.Cheating_Hacking);
                        AUMData[Utils.GetHashPuid(player)] = player.FriendCode;
                        BetterDataManager.SaveCheatData(Utils.GetHashPuid(player), player.Data.FriendCode, player.Data.PlayerName, "aumData", "AUM RPC");
                        BetterNotificationManager.NotifyCheat(player, $"AUM", newText: "Has been detected with a cheat client");
                    }
                }
            }
            catch { }

            return;
        }

        if (callId is unchecked((byte)CustomRPC.AUMChat))
        {
            try
            {
                var nameString = reader.ReadString();
                var msgString = reader.ReadString();
                var colorId = reader.ReadInt32();

                Utils.AddChatPrivate($"{msgString}", overrideName: $"<b><color=#870000>AUM Chat</color> - <color={Utils.Color32ToHex(Palette.PlayerColors[colorId])}>{nameString}</color></b>");

                PlayerControl AUMPlayer = Main.AllPlayerControls.First(pc => pc.Data.PlayerName == nameString && pc.CurrentOutfit.ColorId == colorId);

                if (AUMPlayer == null || !Main.AntiCheat.Value) return;

                var flag = string.IsNullOrEmpty(nameString) && string.IsNullOrEmpty(msgString);
                var flag2 = AUMData.ContainsKey(Utils.GetHashPuid(AUMPlayer));

                if (!flag && !flag2)
                {
                    player.ReportPlayer(ReportReasons.Cheating_Hacking);
                    AUMData[Utils.GetHashPuid(AUMPlayer)] = AUMPlayer.FriendCode;
                    BetterDataManager.SaveCheatData(Utils.GetHashPuid(player), player.Data.FriendCode, player.Data.PlayerName, "aumData", "AUM Chat RPC");
                    BetterNotificationManager.NotifyCheat(AUMPlayer, $"AUM", newText: "Has been detected with a cheat client");
                }
            }
            catch { }

            return;
        }
    }

    // Check and notify for invalid rpcs
    public static void CheckRPC(PlayerControl player, byte callId, MessageReader Oldreader)
    {
        try
        {
            MessageReader reader = MessageReader.Get(Oldreader);

            if (PlayerControl.LocalPlayer == null || player == null || player == PlayerControl.LocalPlayer || player.BetterData().IsBetterHost || reader == null || !IsEnabled || !Main.AntiCheat.Value
                || GameStates.IsBetterHostLobby && !GameStates.IsHost) return;

            RoleTypes? Role = player?.Data?.RoleType;
            Role ??= RoleTypes.Crewmate;
            string hashPuid = Utils.GetHashPuid(player);

            if (callId is (byte)RpcCalls.SendChat or (byte)RpcCalls.SendQuickChat)
            {
                if (player.IsAlive() && GameStates.IsInGamePlay && !GameStates.IsMeeting && !GameStates.IsExilling)
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                    Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName((RpcCalls)callId)}: {player.IsAlive()} && {GameStates.IsInGamePlay} && {!GameStates.IsMeeting} && {!GameStates.IsExilling}");
                }

                return;
            }

            if (callId is (byte)RpcCalls.EnterVent or (byte)RpcCalls.ExitVent)
            {
                if ((!player.IsImpostorTeam() && Role != RoleTypes.Engineer) || player.Data.IsDead)
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                    Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName((RpcCalls)callId)}: {player.IsImpostorTeam()} && {Role != RoleTypes.Engineer} || {player.Data.IsDead}");
                }

                return;
            }

            if (callId is (byte)RpcCalls.ProtectPlayer)
            {
                if (Role is not RoleTypes.GuardianAngel)
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                    Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName((RpcCalls)callId)}: {Role is not RoleTypes.GuardianAngel}");
                }

                return;
            }

            if (callId is (byte)RpcCalls.MurderPlayer)
            {
                if (reader.BytesRemaining > 0)
                {
                    PlayerControl target = reader.ReadNetObject<PlayerControl>();

                    if (target != null)
                    {
                        if (!player.IsImpostorTeam() || !player.IsAlive() || player.IsInVanish() || target.IsImpostorTeam())
                        {
                            BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                            Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName((RpcCalls)callId)}: {!player.IsImpostorTeam()} || {player.IsInVanish()}" +
                                $" || {!target.IsAlive()} || {target.IsImpostorTeam()}");
                        }
                    }
                }

                return;
            }

            if (callId is (byte)RpcCalls.SetLevel)
            {
                if (reader.BytesRemaining > 0)
                {
                    uint level = reader.ReadPackedUInt32();
                    if (level >= 200)
                    {
                        BetterNotificationManager.NotifyCheat(player, $"Invalid Level: {level}");
                        Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName((RpcCalls)callId)}: {level >= 200}");
                    }
                }
                return;
            }

            if (!player.IsImpostorTeam())
            {
                if (GameStates.IsInGamePlay)
                {
                    if (callId is (byte)RpcCalls.CloseDoorsOfType)
                    {
                        byte Type = reader.ReadByte();
                        BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)} - {Enum.GetName((SystemTypes)callId)}");
                        Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName((RpcCalls)callId)}: {!player.IsImpostorTeam()}");

                        return;
                    }
                }
            }

            if (callId is (byte)RpcCalls.CompleteTask)
            {
                var taskId = reader.ReadPackedUInt32();

                if (player.IsImpostorTeam() || !player.Data.Tasks.ToArray().Any(task => task.Id == taskId)
                    || player.BetterData().LastTaskId == taskId || player.BetterData().LastTaskId != taskId
                    && player.BetterData().TimeSinceLastTask < 1.25f)
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                    Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName((RpcCalls)callId)}: {player.IsImpostorTeam()} || {!player.Data.Tasks.ToArray().Any(task => task.Id == taskId)} ||" +
                        $" {player.BetterData().LastTaskId == taskId} || {player.BetterData().LastTaskId != taskId} && {player.BetterData().TimeSinceLastTask < 1.25f}");

                    player.BetterData().TimeSinceLastTask = 0f;
                    player.BetterData().LastTaskId = taskId;

                    return;
                }

                player.BetterData().TimeSinceLastTask = 0f;
                player.BetterData().LastTaskId = taskId;
            }

            if (callId is (byte)RpcCalls.CloseDoorsOfType)
            {
                if (!player.IsImpostorTeam())
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                    Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName((RpcCalls)callId)}: {!player.IsImpostorTeam()}");
                }
            }

            if (callId is (byte)RpcCalls.Pet or (byte)RpcCalls.CancelPet)
            {
                if (player?.CurrentOutfit?.PetId == "pet_EmptyPet")
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                    Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName((RpcCalls)callId)}: {player?.CurrentOutfit?.PetId == "pet_EmptyPet"}");
                }

                return;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToString());
        }
    }

    // Check game states when sabotaging
    public static bool RpcUpdateSystemCheck(PlayerControl player, SystemTypes systemType, byte amount)
    {
        if (PlayerControl.LocalPlayer == null || player == null || player == PlayerControl.LocalPlayer || player.BetterData().IsBetterHost || !IsEnabled || !Main.AntiCheat.Value
            || GameStates.IsBetterHostLobby && !GameStates.IsHost) return true;

        // Needs fixing!
        return true;

        byte hostNum = 128; // Only host should ever send this number
        byte singleFixNum = 0;
        byte minLightNum = 0;
        byte MaxLightNum = 4;
        byte fixNum = 16; // 16 and 17
        byte openPanelNum = 64; // 64 and 65
        byte closePanelNum = 32; // 32 and 33

        // Set fixing status
        if (Utils.SystemTypeIsSabotage(systemType))
        {
            if (amount == openPanelNum && !player.BetterData().IsFixingPanelSabotage)
            {
                player.BetterData().OpenSabotageNum = 1;
            }
            else if (amount == openPanelNum + 1 && !player.BetterData().IsFixingPanelSabotage)
            {
                player.BetterData().OpenSabotageNum = 2;
            }

            if (amount == closePanelNum && player.BetterData().OpenSabotageNum == 1)
            {
                player.BetterData().OpenSabotageNum = 0;
            }
            else if (amount == closePanelNum + 1 && player.BetterData().OpenSabotageNum == 2)
            {
                player.BetterData().OpenSabotageNum = 0;
            }
        }

        if (!GameStates.IsHost || PlayerControl.LocalPlayer == null || player == null || player == PlayerControl.LocalPlayer
            || player.BetterData().IsBetterHost || !IsEnabled || !Main.AntiCheat.Value
            || GameStates.IsBetterHostLobby && !GameStates.IsHost) return true;

        // Single Fix: 0
        // Lights: 0-1-2-3-4
        // Fix 1: 16
        // Fix 2: 17
        // Panel 1: Open/Hold 64 - Close/Release 32
        // Panel 2: Open/Hold 65 - Close/Release 33
        // Cheat Sabo: 128 

        // Activate sabotage
        if (systemType == SystemTypes.Sabotage)
        {
            SystemTypes SaboType = (SystemTypes)amount;

            if (!player.IsImpostorTeam() || GameStates.IsAnySabotageActive() || !Utils.SystemTypeIsSabotage(SaboType))
            {
                BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName(SaboType)}");
                Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName(SaboType)}: {!player.IsImpostorTeam()} || {GameStates.IsAnySabotageActive()} " +
                    $"|| {!Utils.SystemTypeIsSabotage(SaboType)}");
                return false;
            }
        }

        // Fix sabotage
        else if (Utils.SystemTypeIsSabotage(systemType))
        {
            if (amount == hostNum && !player.IsHost())
            {
                BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName(systemType)}");
                Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName(systemType)}: {amount == hostNum} && {!player.IsHost()}");
                return false;
            }

            if (!GameStates.IsSystemActive(systemType))
            {
                if (systemType is SystemTypes.Electrical)
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName(systemType)}");
                    Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName(systemType)}: {systemType is SystemTypes.Electrical}");
                }

                return false;
            }

            if (player.BetterData().IsFixingPanelSabotage)
            {
                if (player.BetterData().OpenSabotageNum == 0)
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName(systemType)}");
                    Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName(systemType)}: {player.BetterData().OpenSabotageNum == 0}");
                    return false;
                }

                if (amount == openPanelNum && amount == openPanelNum + 1)
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName(systemType)}");
                    Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName(systemType)}: {amount == openPanelNum} && {amount == openPanelNum + 1}");
                    return false;
                }

                if (player.BetterData().OpenSabotageNum == 1 && amount == openPanelNum + 1
                    || player.BetterData().OpenSabotageNum == 2 && amount == openPanelNum)
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName(systemType)}");
                    Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName(systemType)}: {player.BetterData().OpenSabotageNum == 1} && {amount == openPanelNum + 1} " +
                        $"|| {player.BetterData().OpenSabotageNum == 2} && {amount == openPanelNum}");
                    return false;
                }
            }
            else
            {
                if (amount != fixNum && amount != fixNum + 1
                    && amount != closePanelNum && amount != closePanelNum + 1
                    && amount > MaxLightNum)
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName(systemType)}");
                    Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName(systemType)}: {amount != fixNum} && {amount != fixNum + 1}" +
                        $" && {amount != closePanelNum} && {amount != closePanelNum + 1}" +
                        $" && {amount > MaxLightNum}");
                    return false;
                }

                if (systemType == SystemTypes.Electrical)
                {
                    if (amount > MaxLightNum)
                    {
                        BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName(systemType)}");
                        Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName(systemType)}: {amount > MaxLightNum}");
                        return false;
                    }
                }
            }
        }

        return true;
    }

    // Check notify and cancel out request for invalid rpcs
    public static bool CheckCancelRPC(PlayerControl player, byte callId, MessageReader Oldreader)
    {
        try
        {
            MessageReader reader = MessageReader.Get(Oldreader);

            if (PlayerControl.LocalPlayer == null || player == null || player == PlayerControl.LocalPlayer || player.BetterData().IsBetterHost || reader == null) return true;

            // Prevent ban exploit
            if (callId is (byte)RpcCalls.MurderPlayer)
            {
                if (reader.BytesRemaining > 0)
                {
                    PlayerControl target = reader.ReadNetObject<PlayerControl>();

                    if (target != null)
                    {
                        if (target == PlayerControl.LocalPlayer)
                        {
                            if (target.BetterData().CheckBanExploit && !target.IsAlive())
                            {
                                BetterNotificationManager.NotifyCheat(player, $"Invalid Action: Attempted To Ban Exploit");
                                Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName((RpcCalls)callId)} 2: {target.BetterData().CheckBanExploit} && {!target.IsAlive()}");
                                return false;
                            }
                            else
                            {
                                target.BetterData().CheckBanExploit = true;
                            }
                        }
                    }
                }
            }

            if (!IsEnabled || !Main.AntiCheat.Value || GameStates.IsBetterHostLobby && !GameStates.IsHost) return true;

            RoleTypes Role = player.Data.RoleType;
            bool IsImpostor = player.IsImpostorTeam();
            bool IsCrewmate = !player.IsImpostorTeam();

            if (TrustedRPCs(callId) != true)
            {
                BetterNotificationManager.NotifyCheat(player, $"Untrusted RPC received: {callId}");
                return false;
            }

            if (callId is (byte)RpcCalls.StartMeeting)
            {
                if (GameStates.IsMeeting && MeetingHudUpdatePatch.timeOpen > 0.5f || GameStates.IsHideNSeek || !player.IsAlive() || player.IsInVent() || player.shapeshifting
                    || player.inMovingPlat || player.onLadder || player.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                    Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName((RpcCalls)callId)}: {GameStates.IsMeeting} && {GameStates.IsHideNSeek} || {!player.IsAlive()} || {player.IsInVent()} || {player.shapeshifting}" +
                        $" || {player.inMovingPlat} || {player.onLadder} || {player.MyPhysics.Animations.IsPlayingAnyLadderAnimation()}");

                    if (GameStates.IsHost)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }

                var deadPlayerInfo = GameData.Instance.GetPlayerById(reader.ReadByte());
                bool isBodyReport = deadPlayerInfo != null;

                if (isBodyReport)
                {
                    if (!UnityEngine.Object.FindAnyObjectByType<DeadBody>() || !deadPlayerInfo.IsDead || deadPlayerInfo == player.Data)
                    {
                        BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                        Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName((RpcCalls)callId)}: {!UnityEngine.Object.FindAnyObjectByType<DeadBody>()} || {!deadPlayerInfo.IsDead} || {deadPlayerInfo == player.Data}");
                        if (GameStates.IsHost)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (player.BetterData().TimesCalledMeeting >= GameOptionsManager.Instance.currentNormalGameOptions.NumEmergencyMeetings)
                    {
                        BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                        Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName((RpcCalls)callId)}: {player.BetterData().TimesCalledMeeting} -> {GameOptionsManager.Instance.currentNormalGameOptions.NumEmergencyMeetings}" +
                            $" - {player.BetterData().TimesCalledMeeting >= GameOptionsManager.Instance.currentNormalGameOptions.NumEmergencyMeetings}");
                        if (GameStates.IsHost)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }

            if (callId is (byte)RpcCalls.Shapeshift)
            {
                var target = reader.ReadNetObject<PlayerControl>();
                var flag = reader.ReadBoolean();
                bool ShapeshiftAsTarget = target != player;

                if (Role is not RoleTypes.Shapeshifter || !player.IsAlive())
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                    Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName((RpcCalls)callId)} 1: {Role is not RoleTypes.Shapeshifter} || {!player.IsAlive()}");
                    return false;
                }

                else if (!flag && !GameStates.IsMeeting && !GameStates.IsExilling && !player.IsInVent())
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                    Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName((RpcCalls)callId)} 2: {!flag} && {!GameStates.IsMeeting} && {!GameStates.IsExilling} && {!player.IsInVent()}");
                    return false;
                }
            }

            if (callId is (byte)RpcCalls.StartVanish or (byte)RpcCalls.StartAppear)
            {
                if (Role is not RoleTypes.Phantom || player.Data.IsDead)
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                    Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName((RpcCalls)callId)} 1: {Role is not RoleTypes.Phantom} || {player.Data.IsDead}");
                    return false;
                }

                if (callId is (byte)RpcCalls.StartVanish)
                {
                    if (player.IsInVent())
                    {
                        BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((RpcCalls)callId)}");
                        Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName((RpcCalls)callId)} 2: {player.IsInVent()}");
                    }
                }
            }

            if (!player.IsHost())
            {
                if (callId is (byte)RpcCalls.SetTasks
                or (byte)RpcCalls.ExtendLobbyTimer
                or (byte)RpcCalls.CloseMeeting)
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Host RPC: {Enum.GetName((RpcCalls)callId)}");
                    Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName((RpcCalls)callId)}: {!player.IsHost()}");
                    return false;
                }
            }

            if (player.DataIsCollected() == true && !GameStates.IsLocalGame && GameStates.IsVanillaServer)
            {
                if (callId is (byte)RpcCalls.CheckName or (byte)RpcCalls.SetLevel)
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Set RPC: {Enum.GetName((RpcCalls)callId)}");
                    Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName((RpcCalls)callId)}: {player.DataIsCollected() == true}");

                    if (callId is (byte)RpcCalls.CheckName)
                    {
                        var name = reader.ReadString();
                        Utils.AddChatPrivate($"{player.GetPlayerNameAndColor()} Has tried to change their name to '{name}' but has been undone!");
                        Logger.LogCheat($"{player.Data.PlayerName} Has tried to change their name to '{name}' but has been undone!");
                    }
                    return false;
                }
            }

            if (GameStates.IsInGamePlay)
            {
                if (callId is (byte)RpcCalls.SetColor
                    or (byte)RpcCalls.SetHat
                    or (byte)RpcCalls.SetSkin
                    or (byte)RpcCalls.SetVisor
                    or (byte)RpcCalls.SetPet
                    or (byte)RpcCalls.SetNamePlate)
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Set RPC: {Enum.GetName((RpcCalls)callId)}");
                    Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName((RpcCalls)callId)}: {GameStates.IsInGamePlay}");
                    return false;
                }
            }

            if (GameStates.IsInGame && GameStates.IsLobby)
            {
                if (callId is (byte)RpcCalls.StartMeeting
                    or (byte)RpcCalls.ReportDeadBody
                    or (byte)RpcCalls.CloseMeeting
                    or (byte)RpcCalls.Exiled
                    or (byte)RpcCalls.CastVote
                    or (byte)RpcCalls.ClearVote
                    or (byte)RpcCalls.SetRole
                    or (byte)RpcCalls.ClimbLadder
                    or (byte)RpcCalls.UsePlatform
                    or (byte)RpcCalls.UseZipline
                    or (byte)RpcCalls.CompleteTask
                    or (byte)RpcCalls.BootFromVent
                    or (byte)RpcCalls.EnterVent
                    or (byte)RpcCalls.ExitVent
                    or (byte)RpcCalls.CloseDoorsOfType
                    or (byte)RpcCalls.MurderPlayer
                    or (byte)RpcCalls.CheckMurder
                    or (byte)RpcCalls.Shapeshift
                    or (byte)RpcCalls.RejectShapeshift
                    or (byte)RpcCalls.CheckShapeshift
                    or (byte)RpcCalls.CheckProtect
                    or (byte)RpcCalls.ProtectPlayer
                    or (byte)RpcCalls.StartAppear
                    or (byte)RpcCalls.StartVanish
                    or (byte)RpcCalls.CheckAppear
                    or (byte)RpcCalls.CheckVanish)
                {
                    BetterNotificationManager.NotifyCheat(player, $"Invalid Lobby RPC: {Enum.GetName((RpcCalls)callId)}");
                    Logger.LogCheat($"{player.Data.PlayerName} {Enum.GetName((RpcCalls)callId)}: {GameStates.IsInGame} && {GameStates.IsLobby}");
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToString());
            return true;
        }
    }

    // Check if RPC is known
    private static bool TrustedRPCs(int RPCId)
    {
        foreach (RpcCalls rpc in Enum.GetValues(typeof(RpcCalls)))
            if ((byte)rpc == RPCId || unchecked((byte)rpc) == RPCId || unchecked((byte)(short)rpc) == RPCId)
                return true;
        foreach (CustomRPC rpc in Enum.GetValues(typeof(CustomRPC)))
            if ((byte)rpc == RPCId || unchecked((byte)rpc) == RPCId || unchecked((byte)(short)rpc) == RPCId)
                return true;

        return false;
    }
}
