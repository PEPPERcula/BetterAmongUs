using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using BetterAmongUs.Patches;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat;

public class SickoHandler : RPCHandler
{
    public override byte CallId => unchecked((byte)CustomRPC.Sicko);

    public override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        if (Main.AntiCheat.Value && BetterGameSettings.DetectCheatClients.GetBool())
        {
            var flag = BAUAntiCheat.SickoData.ContainsKey(Utils.GetHashPuid(sender));

            if (reader.BytesRemaining == 0 && !flag)
            {
                sender.ReportPlayer(ReportReasons.Cheating_Hacking);
                BAUAntiCheat.SickoData[Utils.GetHashPuid(sender)] = sender.Data.FriendCode;
                BetterDataManager.SaveCheatData(Utils.GetHashPuid(sender), sender.Data.FriendCode, sender.Data.PlayerName, "sickoData", "Sicko Menu RPC");
                BetterNotificationManager.NotifyCheat(sender, Translator.GetString("AntiCheat.Cheat.Sicko"), newText: Translator.GetString("AntiCheat.HasBeenDetectedWithCheat2"));
            }
        }
    }
}