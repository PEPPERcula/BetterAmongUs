using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class SetPetHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SetPet;
}
