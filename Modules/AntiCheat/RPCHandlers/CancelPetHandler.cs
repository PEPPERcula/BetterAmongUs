namespace BetterAmongUs.Modules.AntiCheat;

public class CancelPetHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.CancelPet;
}
