using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class CancelPetHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.CancelPet;
}
