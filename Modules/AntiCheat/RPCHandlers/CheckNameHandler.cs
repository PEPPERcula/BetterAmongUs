using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

public class CheckNameHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.CheckName;

    public override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        if (!GameState.IsHost) return false;

        var name = reader.ReadString();

        if (sender.DataIsCollected() == true && sender.BetterData().AntiCheatInfo.HasSetName && !GameState.IsLocalGame && GameState.IsVanillaServer)
        {
            BetterNotificationManager.NotifyCheat(sender, GetFormatSetText());
            Utils.AddChatPrivate($"{sender.GetPlayerNameAndColor()} Has tried to change their name to '{name}' but has been undone!");
            Logger.LogCheat($"{sender.BetterData().RealName} Has tried to change their name to '{name}' but has been undone!");
            LogRpcInfo($"{sender.DataIsCollected() == true} && {!GameState.IsLocalGame} && {GameState.IsVanillaServer}");

            return false;
        }

        sender.BetterData().AntiCheatInfo.HasSetName = true;

        return true;
    }
}
