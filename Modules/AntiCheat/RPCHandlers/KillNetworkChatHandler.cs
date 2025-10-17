using BetterAmongUs.Data;
using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Managers;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class KillNetworkChatHandler : RPCHandler
{
    internal override byte CallId => unchecked((byte)CustomRPC.KillNetworkChat);

    internal override void HandleCheatRpcCheck(PlayerControl? sender, MessageReader reader)
    {
        if (!Main.AntiCheat.Value || !BetterGameSettings.DetectCheatClients.GetBool()) return;

        if (!BetterDataManager.BetterDataFile.KNData.Any(info => info.CheckPlayerData(sender.Data)))
        {
            sender.ReportPlayer(ReportReasons.Cheating_Hacking);
            BetterDataManager.BetterDataFile.KNData.Add(new(sender?.BetterData().RealName ?? sender.Data.PlayerName, sender.GetHashPuid(), sender.Data.FriendCode, "KillNetwork Chat RPC"));
            BetterDataManager.BetterDataFile.Save();
            BetterNotificationManager.NotifyCheat(sender, Translator.GetString("AntiCheat.Cheat.KNC"), Translator.GetString("AntiCheat.HasBeenDetectedWithCheat2"));
        }
    }
}