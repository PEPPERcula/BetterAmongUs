using BetterAmongUs.Data;
using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Managers;
using BetterAmongUs.Patches;
using HarmonyLib;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat;

class BetterAntiCheat
{
    internal static bool IsEnabled => PlayerControl.LocalPlayer?.Data?.IsIncomplete == false;

    internal static void Update()
    {
        if (GameState.IsHost && GameState.IsInGame)
        {
            foreach (var player in Main.AllPlayerControls)
            {
                if (BetterDataManager.BetterDataFile.SickoData.Any(info => info.CheckPlayerData(player.Data)))
                {
                    string reason = Translator.GetString("AntiCheat.Reason.SickoMenuUser");
                    string kickMessage = string.Format(Translator.GetString("AntiCheat.KickMessage"), Translator.GetString("AntiCheat.ByAntiCheat"), reason);
                    player.Kick(true, kickMessage, true);
                }
                else if (BetterDataManager.BetterDataFile.AUMData.Any(info => info.CheckPlayerData(player.Data)))
                {
                    string reason = Translator.GetString("AntiCheat.Reason.AUMUser");
                    string kickMessage = string.Format(Translator.GetString("AntiCheat.KickMessage"), Translator.GetString("AntiCheat.ByAntiCheat"), reason);
                    player.Kick(true, kickMessage, true);
                }
                else if (BetterDataManager.BetterDataFile.KNData.Any(info => info.CheckPlayerData(player.Data)))
                {
                    string reason = Translator.GetString("AntiCheat.Reason.KNUser");
                    string kickMessage = string.Format(Translator.GetString("AntiCheat.KickMessage"), Translator.GetString("AntiCheat.ByAntiCheat"), reason);
                    player.Kick(true, kickMessage, true);
                }
                else if (BetterDataManager.BetterDataFile.CheatData.Any(info => info.CheckPlayerData(player.Data)))
                {
                    string reason = Translator.GetString("AntiCheat.Reason.KnownCheater");
                    string kickMessage = string.Format(Translator.GetString("AntiCheat.KickMessage"), Translator.GetString("AntiCheat.ByAntiCheat"), reason);
                    player.Kick(true, kickMessage, true);
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlatformSpecificData))]
    class PlatformSpecificDataPatch
    {
        [HarmonyPatch(nameof(PlatformSpecificData.Deserialize))]
        [HarmonyPostfix]
        internal static void Deserialize_Postfix(PlatformSpecificData __instance)
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

                    }, 3.5f, shouldLog: false);
                }
                catch { }
            }
        }
    }

    // Handle RPC before anti cheat detection
    internal static void HandleCheatRPCBeforeCheck(PlayerControl player, byte callId, MessageReader oldReader)
    {
        MessageReader reader = MessageReader.Get(oldReader);

        if (!IsEnabled) return;

        RPCHandler.HandleRPC(callId, player, reader, HandlerFlag.AntiCheatCheck);
    }

    // Check and notify for invalid rpcs
    internal static void CheckRPC(PlayerControl player, byte callId, MessageReader oldReader)
    {
        MessageReader reader = MessageReader.Get(oldReader);

        if (player == null || player?.Data == null || reader == null) return;
        if (!IsEnabled || !Main.AntiCheat.Value || (GameState.IsBetterHostLobby && !GameState.IsHost) || !BetterGameSettings.DetectInvalidRPCs.GetBool()) return;
        if (player.IsLocalPlayer() && player.IsHost()) return;

        RPCHandler.HandleRPC(callId, player, reader, HandlerFlag.AntiCheat);
    }

    // Check notify and cancel out request for invalid rpcs
    internal static bool CheckCancelRPC(PlayerControl player, byte callId, MessageReader oldReader)
    {
        try
        {
            MessageReader reader = MessageReader.Get(oldReader);

            if (player == null || player?.Data == null || reader == null) return true;
            if (!IsEnabled || !Main.AntiCheat.Value || (GameState.IsBetterHostLobby && !GameState.IsHost) || !BetterGameSettings.DetectInvalidRPCs.GetBool()) return true;
            if (player.IsLocalPlayer() && player.IsHost()) return true;

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
                    or (byte)RpcCalls.SetHat_Deprecated
                    or (byte)RpcCalls.SetSkin_Deprecated
                    or (byte)RpcCalls.SetVisor_Deprecated
                    or (byte)RpcCalls.SetPet_Deprecated
                    or (byte)RpcCalls.SetNamePlate_Deprecated)
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

    // Handle RPC received from players
    internal static void HandleRPC(PlayerControl player, byte callId, MessageReader oldReader)
    {
        if (player == null || player?.Data == null || player.IsLocalPlayer()) return;

        MessageReader reader = MessageReader.Get(oldReader);

        RPCHandler.HandleRPC(callId, player, reader, HandlerFlag.Handle);
    }

    // Check game states when sabotaging
    internal static bool RpcUpdateSystemCheck(PlayerControl player, SystemTypes systemType, MessageReader oldReader)
    {
        if (Utils.SystemTypeIsSabotage(systemType) || systemType is SystemTypes.Doors)
        {
            if (GameState.IsPrivateOnlyLobby && BetterGameSettings.DisableSabotages.GetBool()) return false;
        }

        MessageReader reader = MessageReader.Get(oldReader);

        RegisterRPCHandlerAttribute.GetClassInstance<UpdateSystemHandler>().CatchedSystemType = systemType;
        bool notCanceled = RPCHandler.HandleRPC((byte)RpcCalls.UpdateSystem, player, reader, HandlerFlag.AntiCheatCancel);
        if (!notCanceled)
        {
            Logger.LogCheat($"RPC canceled by Anti-Cheat: {Enum.GetName(typeof(SystemTypes), (int)systemType)} - {MessageReader.Get(reader).ReadByte()}");
        }
        return notCanceled;
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
