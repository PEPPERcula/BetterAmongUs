using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Managers;
using BetterAmongUs.Patches;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class SickoHandler : RPCHandler
{
    internal override byte CallId => unchecked((byte)CustomRPC.Sicko);

    internal override void HandleAntiCheatCheck(PlayerControl? sender, MessageReader reader)
    {
        if (Main.AntiCheat.Value && BetterGameSettings.DetectCheatClients.GetBool())
        {
            var flag = BetterAntiCheat.SickoData.ContainsKey(Utils.GetHashPuid(sender));

            if (reader.BytesRemaining == 0 && !flag)
            {
                sender.ReportPlayer(ReportReasons.Cheating_Hacking);
                BetterAntiCheat.SickoData[Utils.GetHashPuid(sender)] = sender.Data.FriendCode;
                BetterDataManager.SaveCheatData(Utils.GetHashPuid(sender), sender.Data.FriendCode, sender.Data.PlayerName, "sickoData", "Sicko Menu RPC");
                BetterNotificationManager.NotifyCheat(sender, Translator.GetString("AntiCheat.Cheat.Sicko"), newText: Translator.GetString("AntiCheat.HasBeenDetectedWithCheat2"));
            }
        }
    }
}