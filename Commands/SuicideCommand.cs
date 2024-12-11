#if DEBUG
using BetterAmongUs.Modules;

namespace BetterAmongUs.Commands;

public class SuicideCommand : BaseCommand
{
    public override CommandType Type => CommandType.Debug;
    public override string Name => "suicide";
    public override string Description => "Kill self";
    public override bool ShowCommand() => GameState.IsFreePlay;
    public override void Run()
    {
        PlayerControl.LocalPlayer.MurderPlayer(PlayerControl.LocalPlayer, MurderResultFlags.Succeeded);
    }
}
#endif