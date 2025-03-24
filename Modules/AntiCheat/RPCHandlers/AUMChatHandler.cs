using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Managers;
using BetterAmongUs.Patches;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class AUMChatHandler : RPCHandler
{
    internal override byte CallId => unchecked((byte)CustomRPC.AUMChat);

    internal override void HandleAntiCheatCheck(PlayerControl? sender, MessageReader reader)
    {
        var nameString = reader.ReadString();
        var msgString = reader.ReadString();
        var colorId = reader.ReadInt32();

        var flag3 = sender.BetterData().AntiCheatInfo.AUMChats.Count > 0 && sender.BetterData().AntiCheatInfo.AUMChats.Last() == msgString;
        if (!flag3)
        {
            Utils.AddChatPrivate($"{msgString}", overrideName: $"<b><color=#870000>AUM Chat</color> - {sender.GetPlayerNameAndColor()}</b>");
            sender.BetterData().AntiCheatInfo.AUMChats.Add(msgString);
        }

        Logger.Log($"{sender.Data.PlayerName} -> {msgString}", "AUMChatLog");

        if (!Main.AntiCheat.Value || !BetterGameSettings.DetectCheatClients.GetBool()) return;

        var flag = string.IsNullOrEmpty(nameString) && string.IsNullOrEmpty(msgString);
        var flag2 = BAUAntiCheat.AUMData.ContainsKey(Utils.GetHashPuid(sender));

        if (!flag && !flag2)
        {
            sender.ReportPlayer(ReportReasons.Cheating_Hacking);
            BAUAntiCheat.AUMData[Utils.GetHashPuid(sender)] = sender.Data.FriendCode;
            BetterDataManager.SaveCheatData(Utils.GetHashPuid(sender), sender.Data.FriendCode, sender.Data.PlayerName, "aumData", "AUM Chat RPC");
            BetterNotificationManager.NotifyCheat(sender, Translator.GetString("AntiCheat.Cheat.AUM"), Translator.GetString("AntiCheat.HasBeenDetectedWithCheat2"));
        }
    }
}