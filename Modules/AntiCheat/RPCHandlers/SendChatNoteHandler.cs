using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
public sealed class SendChatNoteHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SendChatNote;
}
