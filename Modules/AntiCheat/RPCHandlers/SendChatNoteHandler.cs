namespace BetterAmongUs.Modules.AntiCheat;

public class SendChatNoteHandler : RPCHandler
{
    public override byte CallId => (byte)RpcCalls.SendChatNote;
}
