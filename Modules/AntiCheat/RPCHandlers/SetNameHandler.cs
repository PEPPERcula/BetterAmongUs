using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class SetNameHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetName;

    internal override void Handle(PlayerControl? sender, MessageReader reader)
    {
        Utils.DirtyAllNames();
    }

    internal override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        if (GameState.IsHost) return true;

        _ = reader.ReadUInt32();
        var name = reader.ReadString();

        if (sender.DataIsCollected() == true && sender.BetterData().AntiCheatInfo.HasSetName && !GameState.IsLocalGame && GameState.IsVanillaServer)
        {
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatSetText()))
            {
                Utils.AddChatPrivate($"{sender.GetPlayerNameAndColor()} Has tried to change their name to '{name}' but has been undone!");
                Logger.LogCheat($"{sender.BetterData().RealName} Has tried to change their name to '{name}' but has been undone!");
                LogRpcInfo($"{sender.DataIsCollected() == true} && {!GameState.IsLocalGame} && {GameState.IsVanillaServer}");
            }

            return false;
        }

        sender.BetterData().AntiCheatInfo.HasSetName = true;

        return true;
    }
}
