namespace BetterAmongUs.Modules.AntiCheat;

public class SetScannerHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SetScanner;
}
