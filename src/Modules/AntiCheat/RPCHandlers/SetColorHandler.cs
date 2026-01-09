using BetterAmongUs.Items.Attributes;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class SetColorHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetColor;
    internal override void Handle(PlayerControl? sender, MessageReader reader)
    {
    }
}
