using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Managers;
using BetterAmongUs.Patches;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class AUMHandler : RPCHandler
{
    internal override byte CallId => unchecked((byte)CustomRPC.AUM);

    internal override void HandleAntiCheatCheck(PlayerControl? sender, MessageReader reader)
    {
        if (Main.AntiCheat.Value && BetterGameSettings.DetectCheatClients.GetBool())
        {
            var aumid = reader.ReadByte();

            if (aumid == sender.PlayerId)
            {
                var flag = BetterAntiCheat.AUMData.ContainsKey(Utils.GetHashPuid(sender));

                if (!flag)
                {
                    sender.ReportPlayer(ReportReasons.Cheating_Hacking);
                    BetterAntiCheat.AUMData[Utils.GetHashPuid(sender)] = sender.Data.FriendCode;
                    BetterDataManager.SaveCheatData(Utils.GetHashPuid(sender), sender.Data.FriendCode, sender.Data.PlayerName, "aumData", "AUM RPC");
                    BetterNotificationManager.NotifyCheat(sender, Translator.GetString("AntiCheat.Cheat.AUM"), Translator.GetString("AntiCheat.HasBeenDetectedWithCheat2"));
                }
            }
        }
    }
}