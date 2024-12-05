using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using BetterAmongUs.Patches;
using HarmonyLib;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat;

class BAUAntiCheat
{
    public static Dictionary<string, string> PlayerData = []; // HashPuid, FriendCode
    public static Dictionary<string, string> SickoData = []; // HashPuid, FriendCode
    public static Dictionary<string, string> AUMData = []; // HashPuid, FriendCode
    public static Dictionary<string, string> KNData = []; // HashPuid, FriendCode
    public static bool IsEnabled { get; private set; } = true;

    public static string[] GatherAllData()
    {
        return PlayerData.Keys
            .Concat(PlayerData.Values)
            .Concat(SickoData.Values)
            .Concat(AUMData.Values)
            .Concat(KNData.Values)
            .Concat(SickoData.Keys)
            .Concat(AUMData.Keys)
            .Concat(KNData.Keys)
            .ToHashSet()
            .ToArray();
    }

    public static void Update()
    {
        if (GameState.IsHost && GameState.IsInGame)
        {
            RPC.SyncAllNames(isBetterHost: false);

            foreach (var player in Main.AllPlayerControls)
            {
                var hashPuid = Utils.GetHashPuid(player);

                if (SickoData.ContainsKey(hashPuid))
                {
                    string reason = Translator.GetString("AntiCheat.Reason.SickoMenuUser");
                    string kickMessage = string.Format(Translator.GetString("AntiCheat.KickMessage"), Translator.GetString("AntiCheat.ByAntiCheat"), reason);
                    player.Kick(true, kickMessage, true);
                }
                else if (AUMData.ContainsKey(hashPuid))
                {
                    string reason = Translator.GetString("AntiCheat.Reason.AUMUser");
                    string kickMessage = string.Format(Translator.GetString("AntiCheat.KickMessage"), Translator.GetString("AntiCheat.ByAntiCheat"), reason);
                    player.Kick(true, kickMessage, true);
                }
                else if (KNData.ContainsKey(hashPuid))
                {
                    string reason = Translator.GetString("AntiCheat.Reason.KNUser");
                    string kickMessage = string.Format(Translator.GetString("AntiCheat.KickMessage"), Translator.GetString("AntiCheat.ByAntiCheat"), reason);
                    player.Kick(true, kickMessage, true);
                }
                else if (PlayerData.ContainsKey(hashPuid))
                {
                    string reason = Translator.GetString("AntiCheat.Reason.KnownCheater");
                    string kickMessage = string.Format(Translator.GetString("AntiCheat.KickMessage"), Translator.GetString("AntiCheat.ByAntiCheat"), reason);
                    player.Kick(true, kickMessage, true);
                }
            }
        }
    }


    public static void PauseAntiCheat()
    {
        float time = 2.5f;
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
            if (!Main.AntiCheat.Value || !GameState.IsVanillaServer) return;

            if (GameState.IsLobby)
            {
                try
                {
                    _ = new LateTask(() =>
                    {
                        var player = Main.AllPlayerControls.FirstOrDefault(pc => pc.GetClient().PlatformData == __instance);

                        if (player != null && __instance?.Platform != null)
                        {
                            if (__instance.Platform is Platforms.StandaloneWin10 or Platforms.Xbox)
                            {
                                if (__instance.XboxPlatformId.ToString().Length is < 10 or > 16)
                                {
                                    player.ReportPlayer(ReportReasons.Cheating_Hacking);
                                    BetterNotificationManager.NotifyCheat(player,
                                        Translator.GetString("AntiCheat.Reason.PlatformSpoofer"),
                                        Translator.GetString("AntiCheat.HasBeenDetectedWithCheat")
                                    );
                                    Logger.LogCheat($"{player.BetterData().RealName} {Translator.GetString("AntiCheat.PlatformSpoofer")}: {__instance.XboxPlatformId}");
                                }
                            }

                            if (__instance.Platform is Platforms.Playstation)
                            {
                                if (__instance.PsnPlatformId.ToString().Length is < 14 or > 20)
                                {
                                    player.ReportPlayer(ReportReasons.Cheating_Hacking);
                                    BetterNotificationManager.NotifyCheat(player,
                                        Translator.GetString("AntiCheat.Reason.PlatformSpoofer"),
                                        Translator.GetString("AntiCheat.HasBeenDetectedWithCheat")
                                    );
                                    Logger.LogCheat($"{player.BetterData().RealName} {Translator.GetString("AntiCheat.PlatformSpoofer")}: {__instance.PsnPlatformId}");
                                }
                            }

                            if (__instance.Platform is Platforms.Unknown || !Enum.IsDefined(__instance.Platform))
                            {
                                BetterNotificationManager.NotifyCheat(player,
                                    Translator.GetString("AntiCheat.Reason.PlatformSpoofer"),
                                    Translator.GetString("AntiCheat.HasBeenDetectedWithCheat")
                                );
                            }
                        }

                    }, 3.5f, shoudLog: false);
                }
                catch { }
            }
        }
    }

    // Handle RPC before anti cheat detection
    public static void HandleCheatRPCBeforeCheck(PlayerControl player, byte callId, MessageReader Oldreader)
    {
        MessageReader reader = MessageReader.Get(Oldreader);

        if (player.IsLocalPlayer() || player == null || !IsEnabled) return;

        RPCHandler.HandleRPC(callId, player, reader, HandlerFlag.AntiCheatCheck);
    }

    // Check and notify for invalid rpcs
    public static void CheckRPC(PlayerControl player, byte callId, MessageReader Oldreader)
    {
        MessageReader reader = MessageReader.Get(Oldreader);

        if (player == null || player.IsLocalPlayer() || player.BetterData().IsBetterHost || reader == null || !IsEnabled || !Main.AntiCheat.Value
            || GameState.IsBetterHostLobby && !GameState.IsHost || !BetterGameSettings.DetectInvalidRPCs.GetBool()) return;

        RPCHandler.HandleRPC(callId, player, reader, HandlerFlag.AntiCheat);
    }

    // Check notify and cancel out request for invalid rpcs
    public static bool CheckCancelRPC(PlayerControl player, byte callId, MessageReader Oldreader)
    {
        try
        {
            MessageReader reader = MessageReader.Get(Oldreader);

            if (PlayerControl.LocalPlayer == null || player == null || player.IsLocalPlayer() || player.BetterData().IsBetterHost || reader == null) return true;

            if (!IsEnabled || !Main.AntiCheat.Value || GameState.IsBetterHostLobby && !GameState.IsHost || !BetterGameSettings.DetectInvalidRPCs.GetBool()) return true;

            if (TrustedRPCs(callId) != true && !player.IsHost())
            {
                BetterNotificationManager.NotifyCheat(player, $"Unregistered RPC received: {callId}");
                return false;
            }

            if (RPCHandler.HandleRPC(callId, player, reader, HandlerFlag.AntiCheatCancel) == false)
            {
                return false;
            }

            if (!player.IsHost())
            {
                if (callId is (byte)RpcCalls.SetTasks
                or (byte)RpcCalls.ExtendLobbyTimer
                or (byte)RpcCalls.CloseMeeting)
                {
                    BetterNotificationManager.NotifyCheat(player, string.Format(Translator.GetString("AntiCheat.InvalidHostRPC"), Enum.GetName((RpcCalls)callId)));
                    Logger.LogCheat($"{player.BetterData().RealName} {Enum.GetName((RpcCalls)callId)}: {!player.IsHost()}");
                    return false;
                }
            }

            if (GameState.IsInGamePlay)
            {
                if (callId is (byte)RpcCalls.SetColor
                    or (byte)RpcCalls.SetHat
                    or (byte)RpcCalls.SetSkin
                    or (byte)RpcCalls.SetVisor
                    or (byte)RpcCalls.SetPet
                    or (byte)RpcCalls.SetNamePlate)
                {
                    BetterNotificationManager.NotifyCheat(player, string.Format(Translator.GetString("AntiCheat.InvalidSetRPC"), Enum.GetName((RpcCalls)callId)));
                    Logger.LogCheat($"{player.BetterData().RealName} {Enum.GetName((RpcCalls)callId)}: {GameState.IsInGamePlay}");
                    return false;
                }
            }

            if (GameState.IsInGame && GameState.IsLobby)
            {
                if (callId is (byte)RpcCalls.StartMeeting
                    or (byte)RpcCalls.ReportDeadBody
                    or (byte)RpcCalls.SendChatNote
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
                    BetterNotificationManager.NotifyCheat(player, string.Format(Translator.GetString("AntiCheat.InvalidLobbyRPC"), Enum.GetName((RpcCalls)callId)));
                    Logger.LogCheat($"{player.BetterData().RealName} {Enum.GetName((RpcCalls)callId)}: {GameState.IsInGame} && {GameState.IsLobby}");
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            return true;
        }
    }

    // Check game states when sabotaging
    public static bool RpcUpdateSystemCheck(PlayerControl player, SystemTypes systemType, MessageReader reader)
    {
        RPCHandler.GetHandlerInstance<UpdateSystemHandler>().CatchedSystemType = systemType;
        return RPCHandler.HandleRPC((byte)RpcCalls.UpdateSystem, player, reader, HandlerFlag.AntiCheatCancel);
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
