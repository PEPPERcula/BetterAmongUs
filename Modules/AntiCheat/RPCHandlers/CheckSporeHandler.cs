using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

public class CheckSporeHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.CheckSpore;

    public override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        if (!GameState.IsHost)
        {
            return false;
        }

        return true;
    }
}
