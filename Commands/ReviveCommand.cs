#if DEBUG
using BetterAmongUs.Modules;

namespace BetterAmongUs.Commands;

public class ReviveCommand : BaseCommand
{
    public override CommandType Type => CommandType.Debug;
    public override string Name => "revive";
    public override string Description => "Set self as alive";
    public override bool ShowCommand() => GameState.IsFreePlay;
    public override void Run()
    {
        PlayerControl.LocalPlayer.Revive();
    }
}
#endif
