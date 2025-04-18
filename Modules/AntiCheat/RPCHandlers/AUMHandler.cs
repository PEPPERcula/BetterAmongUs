using BetterAmongUs.Data;
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

    internal override void HandleCheatRpcCheck(PlayerControl? sender, MessageReader reader)
    {
        if (Main.AntiCheat.Value && BetterGameSettings.DetectCheatClients.GetBool())
        {
            var aumid = reader.ReadByte();

            if (aumid == sender.PlayerId)
            {
                if (!BetterDataManager.BetterDataFile.AUMData.Any(info => info.CheckPlayerData(sender.Data)))
                {
                    sender.ReportPlayer(ReportReasons.Cheating_Hacking);
                    BetterDataManager.BetterDataFile.AUMData.Add(new(sender?.BetterData().RealName ?? sender.Data.PlayerName, sender.GetHashPuid(), sender.Data.FriendCode, "AUM RPC"));
                    BetterDataManager.BetterDataFile.Save();
                    BetterNotificationManager.NotifyCheat(sender, Translator.GetString("AntiCheat.Cheat.AUM"), Translator.GetString("AntiCheat.HasBeenDetectedWithCheat2"));
                }
            }
        }
    }
}