using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using BetterAmongUs.Patches;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat;

public class AUMHandler : RPCHandler
{
    public override byte CallId => unchecked((byte)CustomRPC.AUM);

    public override void HandleAntiCheatCheck(PlayerControl? sender, MessageReader reader)
    {
        if (Main.AntiCheat.Value && BetterGameSettings.DetectCheatClients.GetBool())
        {
            var aumid = reader.ReadByte();

            if (aumid == sender.PlayerId)
            {
                var flag = BAUAntiCheat.AUMData.ContainsKey(Utils.GetHashPuid(sender));

                if (!flag)
                {
                    sender.ReportPlayer(ReportReasons.Cheating_Hacking);
                    BAUAntiCheat.AUMData[Utils.GetHashPuid(sender)] = sender.Data.FriendCode;
                    BetterDataManager.SaveCheatData(Utils.GetHashPuid(sender), sender.Data.FriendCode, sender.Data.PlayerName, "aumData", "AUM RPC");
                    BetterNotificationManager.NotifyCheat(sender, Translator.GetString("AntiCheat.Cheat.AUM"), Translator.GetString("AntiCheat.HasBeenDetectedWithCheat2"));
                }
            }
        }
    }
}