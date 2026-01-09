using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class SetPetHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetPet_Deprecated;
}
