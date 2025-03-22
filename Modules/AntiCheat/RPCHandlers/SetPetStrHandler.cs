using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class SetPetStrHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SetPetStr;
}
