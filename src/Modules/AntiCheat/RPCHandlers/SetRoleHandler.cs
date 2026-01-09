using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class SetRoleHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetRole;
}
