using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class SendChatNoteHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SendChatNote;
}
