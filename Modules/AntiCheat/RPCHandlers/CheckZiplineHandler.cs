using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

public class CheckZiplineHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.CheckZipline;

    public override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        if (!GameState.IsHost)
        {
            return false;
        }

        return true;
    }
}
