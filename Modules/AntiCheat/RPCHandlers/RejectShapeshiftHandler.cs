namespace BetterAmongUs.Modules.AntiCheat;

public class RejectShapeshiftHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.RejectShapeshift;
}
