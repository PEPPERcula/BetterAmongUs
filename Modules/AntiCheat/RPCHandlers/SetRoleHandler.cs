using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class SetRoleHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SetRole;

    public override void Handle(PlayerControl? sender, MessageReader reader)
    {
        sender.DirtyName();
    }
}
