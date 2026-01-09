using BetterAmongUs.Helpers;
using BetterAmongUs.Attributes;
using BetterAmongUs.Managers;
using BetterAmongUs.Mono;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

[RegisterRPCHandler]
internal sealed class CompleteTaskHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.CompleteTask;

    internal override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        var taskId = reader.ReadPackedUInt32();

        if (sender.IsImpostorTeam() || !sender.Data.Tasks.AnyIl2Cpp(task => task.Id == taskId)
            || sender.BetterData().AntiCheatInfo.LastTaskId == taskId || (sender.BetterData().AntiCheatInfo.LastTaskId != taskId
            && sender.BetterData().AntiCheatInfo.TimeSinceLastTask < 1.25f))
        {
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText()))
            {
                LogRpcInfo($"{sender.IsImpostorTeam()} || {!sender.Data.Tasks.AnyIl2Cpp(task => task.Id == taskId)} ||" +
                    $" {sender.BetterData().AntiCheatInfo.LastTaskId == taskId} || ({sender.BetterData().AntiCheatInfo.LastTaskId != taskId} && {sender.BetterData().AntiCheatInfo.TimeSinceLastTask < 1.25f})");
            }
        }

        sender.BetterData().AntiCheatInfo.TimeSinceLastTask = 0f;
        sender.BetterData().AntiCheatInfo.LastTaskId = taskId;
    }
}
