using BetterAmongUs.Items.Attributes;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class SetColorHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SetColor;
    public override void Handle(PlayerControl? sender, MessageReader reader)
    {
        Logger.InGame("TEST");
    }
}
