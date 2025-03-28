using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Managers;
using BetterAmongUs.Patches;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class KillNetworkHandler : RPCHandler
{
    internal override byte CallId => unchecked((byte)CustomRPC.KillNetwork);

    internal override void HandleAntiCheatCheck(PlayerControl? sender, MessageReader reader)
    {
        if (Main.AntiCheat.Value && BetterGameSettings.DetectCheatClients.GetBool())
        {
            var flag = BetterAntiCheat.KNData.ContainsKey(Utils.GetHashPuid(sender));

            if (!flag)
            {
                sender.ReportPlayer(ReportReasons.Cheating_Hacking);
                BetterAntiCheat.KNData[Utils.GetHashPuid(sender)] = sender.Data.FriendCode;
                BetterDataManager.SaveCheatData(Utils.GetHashPuid(sender), sender.Data.FriendCode, sender.Data.PlayerName, "knData", "KN RPC");
                BetterNotificationManager.NotifyCheat(sender, Translator.GetString("AntiCheat.Cheat.KN"), Translator.GetString("AntiCheat.HasBeenDetectedWithCheat2"));
            }
        }
    }
}