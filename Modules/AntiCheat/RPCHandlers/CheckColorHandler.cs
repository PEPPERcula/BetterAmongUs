using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

public class CheckColorHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.CheckColor;

    public override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        if (!GameState.IsHost)
        {
            return false;
        }

        return true;
    }
}
