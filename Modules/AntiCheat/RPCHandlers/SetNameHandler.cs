using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

public class SetNameHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SetName;

    public override void Handle(PlayerControl? sender, MessageReader reader)
    {
        Utils.DirtyAllNames();
    }

    public override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        var name = reader.ReadString();

        if (sender.DataIsCollected() == true && !GameState.IsLocalGame && GameState.IsVanillaServer)
        {
            BetterNotificationManager.NotifyCheat(sender, GetFormatSetText());
            Utils.AddChatPrivate($"{sender.GetPlayerNameAndColor()} Has tried to change their name to '{name}' but has been undone!");
            Logger.LogCheat($"{sender.BetterData().RealName} Has tried to change their name to '{name}' but has been undone!");
            LogRpcInfo($"{sender.DataIsCollected() == true} && {!GameState.IsLocalGame} && {GameState.IsVanillaServer}");

            return false;
        }

        return true;
    }
}
