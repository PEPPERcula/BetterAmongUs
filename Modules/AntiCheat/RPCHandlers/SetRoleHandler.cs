using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class SetRoleHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetRole;

    internal override void Handle(PlayerControl? sender, MessageReader reader)
    {
        sender.DirtyName();
    }
}
