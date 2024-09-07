using BetterAmongUs.Patches;
using HarmonyLib;
using Hazel;

namespace BetterAmongUs;

enum CustomRPC : int
{
    // Cheat RPC's
    Sicko = 420, // Results in 164
    AUM = 42069, // Results in 85
    AUMChat = 101,

    // TOHE
    VersionCheck = 80,
    RequestRetryVersionCheck = 81,

    //Better Among Us
    BetterCheck = 150,
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
internal class PlayerControlRPCHandlerPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        AntiCheat.HandleCheatRPCBeforeCheck(__instance, callId, reader);

        if (AntiCheat.CheckCancelRPC(__instance, callId, reader) != true)
        {
            Logger.LogCheat($"RPC canceled by Anti-Cheat: {Enum.GetName((RpcCalls)callId)}{Enum.GetName((CustomRPC)callId)} - {callId}");
            return false;
        }

        var canceled = false;
        if (GameStates.IsHost)
        {
            if (BetterHostManager.CheckRPCAsHost(__instance, callId, reader, ref canceled) != true)
            {
                if (canceled)
                {
                    Logger.LogCheat($"RPC canceled by Host: {Enum.GetName((RpcCalls)callId)}{Enum.GetName((CustomRPC)callId)} - {callId}");
                }
                return false;
            }
        }

        AntiCheat.CheckRPC(__instance, callId, reader);
        RPC.HandleRPC(__instance, callId, reader);

        return true;
    }
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        RPC.HandleCustomRPC(__instance, callId, reader);
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
internal class PlayerPhysicsRPCHandlerPatch
{
    public static void Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        if (AntiCheat.CheckCancelRPC(__instance.myPlayer, callId, reader) != true)
        {
            Logger.LogCheat($"RPC canceled by Anti-Cheat: {Enum.GetName((RpcCalls)callId)}{Enum.GetName((CustomRPC)callId)} - {callId}");
        }

        AntiCheat.CheckRPC(__instance.myPlayer, callId, reader);
        RPC.HandleRPC(__instance.myPlayer, callId, reader);
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), typeof(SystemTypes), typeof(PlayerControl), typeof(MessageReader))]
public static class MessageReaderUpdateSystemPatch
{
    public static bool Prefix(/*ShipStatus __instance,*/ [HarmonyArgument(0)] SystemTypes systemType, [HarmonyArgument(1)] PlayerControl player, [HarmonyArgument(2)] MessageReader reader)
    {
        if (GameStates.IsHideNSeek) return false;

        var amount = MessageReader.Get(reader).ReadByte();
        if (AntiCheat.RpcUpdateSystemCheck(player, systemType, amount) != true)
        {
            Logger.LogCheat($"RPC canceled by Anti-Cheat: {Enum.GetName(typeof(SystemTypes), (int)systemType)} - {amount}");
            return false;
        }

        return true;
    }
}

internal static class RPC
{
    public static void RpcSyncSettings()
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        foreach (var player in Main.AllPlayerControls.Where(pc => pc != PlayerControl.LocalPlayer))
        {
            var optionsByteArray = GameOptionsManager.Instance.gameOptionsFactory.ToBytes(GameOptionsManager.Instance.CurrentGameOptions, false);
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SyncSettings, SendOption.None, player.GetClientId());
            messageWriter.WriteBytesAndSize(optionsByteArray);
            AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
        }
    } 
    
    public static void SendBetterCheck()
    {
        var flag = GameStates.IsHost && Main.BetterHost.Value;
        MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.BetterCheck, SendOption.None, -1);
        messageWriter.Write(true);
        messageWriter.Write(flag);
        messageWriter.Write(Main.GetVersionText().Replace(" ", ""));
        AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
    }

    public static void SetNamePrivate(PlayerControl player, string name, PlayerControl target)
    {
        if (!GameStates.IsHost || !GameStates.IsVanillaServer)
        {
            return;
        }

        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetName, SendOption.Reliable, target.GetClientId());
        writer.Write(player.Data.NetId);
        writer.Write(name);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    public static void SyncAllNames(bool isForMeeting = false, bool force = false, bool isBetterHost = true)
    {
        if (!GameStates.IsHost || !GameStates.IsVanillaServer)
        {
            return;
        }

        foreach (PlayerControl player in Main.AllPlayerControls)
            BetterHostManager.SetPlayersInfoAsHost(player, isForMeeting, force, isBetterHost);
    }

    public static void ExileAsync(PlayerControl player)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.Exiled, SendOption.Reliable, -1);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        player.Exiled();
    }

    public static void HandleCustomRPC(PlayerControl player, byte callId, MessageReader oldReader)
    {
        if (PlayerControl.LocalPlayer == null || player == null || player == PlayerControl.LocalPlayer || player.Data == null) return;

        if (!Enum.IsDefined(typeof(CustomRPC), (int)unchecked(callId))) return;

        MessageReader reader = MessageReader.Get(oldReader);

        switch (callId)
        {
            case (byte)CustomRPC.BetterCheck:
                {
                    var SetBetterUser = reader.ReadBoolean();
                    var IsBetterHost = reader.ReadBoolean();

                    if (!player.IsHost() && IsBetterHost)
                    {
                        BetterNotificationManager.NotifyCheat(player, $"Invalid Action RPC: {Enum.GetName((CustomRPC)callId)} called as BetterHost");
                        break;
                    }

                    player.BetterData().IsBetterUser = SetBetterUser;
                    player.BetterData().IsBetterHost = IsBetterHost;

                    SyncAllNames(force: true);
                }
                break;
            case (byte)CustomRPC.VersionCheck or (byte)CustomRPC.RequestRetryVersionCheck:
                if (player.IsHost())
                {
                    player.BetterData().IsTOHEHost = true;
                    var BAU = "<color=#278720>♻</color><color=#0ed400><b>BetterAmongUs</b></color><color=#278720>♻</color>";
                    Utils.DisconnectSelf($"{BAU} does not support <color=#ff9cdc><b>TOHE</b></color>");
                }
                break;
        }
    }

    public static void HandleRPC(PlayerControl player, byte callId, MessageReader oldReader)
    {
        if (PlayerControl.LocalPlayer == null || player == null || player == PlayerControl.LocalPlayer || player.Data == null) return;

        MessageReader reader = MessageReader.Get(oldReader);

        switch (callId)
        {
            case (byte)RpcCalls.SetName:
                reader.ReadUInt32();
                var name = reader.ReadString();
                player.BetterData().RealName = name;
                break;
            case (byte)RpcCalls.SendChat:
                var text = reader.ReadString();

                if (player.IsHost() && player != PlayerControl.LocalPlayer)
                {
                    if (text.ToLower() == "/allow")
                    {
                        CommandsPatch.Permission = player;
                        BetterNotificationManager.Notify($"{player.GetPlayerNameAndColor()} has granted permission!");
                    }
                }

                // Check banned words
                if (BetterGameSettings.UseBanWordList.GetBool())
                {
                    try
                    {
                        Func<string, string> normalizeText = text => new string(text.Where(c => char.IsLetterOrDigit(c)).ToArray()).ToLower();

                        HashSet<string> bannedWords = new HashSet<string>(
                            File.ReadLines(BetterDataManager.banWordListFile)
                                .Select(normalizeText)
                                .Where(text => !string.IsNullOrWhiteSpace(text))
                        );

                        string normalizedMessage = normalizeText(text);

                        bool isWordBanned = bannedWords.Any(bannedWord =>
                            normalizedMessage.Contains(bannedWord)
                        );

                        if (!string.IsNullOrEmpty(normalizedMessage) && isWordBanned)
                        {
                            _ = new LateTask(() =>
                            {
                                player.Kick(false, $"has been kicked due to\nchat message containing a banned word!");
                            }, 1f, shoudLog: false);
                        }
                    }
                    catch { }
                }


                break;
        }
    }
}

