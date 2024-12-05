using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using BetterAmongUs.Patches;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat;

public class KillNetworkChatHandler : RPCHandler
{
    public override byte CallId => unchecked((byte)CustomRPC.KillNetworkChat);

    public override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        if (!Main.AntiCheat.Value || !BetterGameSettings.DetectCheatClients.GetBool()) return;

        var flag = BAUAntiCheat.KNData.ContainsKey(Utils.GetHashPuid(sender));

        if (!flag)
        {
            sender.ReportPlayer(ReportReasons.Cheating_Hacking);
            BAUAntiCheat.KNData[Utils.GetHashPuid(sender)] = sender.Data.FriendCode;
            BetterDataManager.SaveCheatData(Utils.GetHashPuid(sender), sender.Data.FriendCode, sender.Data.PlayerName, "knData", "KN RPC");
            BetterNotificationManager.NotifyCheat(sender, Translator.GetString("AntiCheat.Cheat.KNC"), Translator.GetString("AntiCheat.HasBeenDetectedWithCheat2"));
        }
    }
}