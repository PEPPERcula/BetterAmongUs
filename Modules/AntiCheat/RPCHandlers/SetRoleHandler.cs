using BetterAmongUs.Helpers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

public class SetRoleHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SetRole;

    public override void Handle(PlayerControl? sender, MessageReader reader)
    {
        sender.DirtyName();
    }
}
