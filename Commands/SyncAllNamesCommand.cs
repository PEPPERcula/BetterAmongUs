#if DEBUG
using BetterAmongUs.Modules;

namespace BetterAmongUs.Commands;

public class SyncAllNamesCommand : BaseCommand
{
    public override CommandType Type => CommandType.Debug;
    public override string Name => "syncallnames";
    public override string Description => "Sync all players names for better host";
    public override bool ShowCommand() => GameState.IsHost;

    public override void Run()
    {
        RPC.SyncAllNames(force: true);
        CommandResultText("<color=#0dff00>All player names have been updated and synced!</color>");
    }
}
#endif