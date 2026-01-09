using Hazel;
using InnerNet;

namespace BetterAmongUs.Structs;

internal readonly struct RPCData(InnerNetObject sender, SendOption sendOption, int targetId, RpcCalls callId, MessageReader reader)
{
    public readonly InnerNetObject Sender = sender;
    public readonly SendOption SendOption = sendOption;
    public readonly int TargetId = targetId;
    public readonly RpcCalls CalledRpc = callId;
    public readonly MessageReader Reader = reader;
}
