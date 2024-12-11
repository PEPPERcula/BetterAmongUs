#if DEBUG
using BetterAmongUs.Modules;

namespace BetterAmongUs.Commands;

public class ExileCommand : BaseCommand
{
    public override CommandType Type => CommandType.Debug;
    public override string Name => "exile";
    public override string Description => "Set self as dead";
    public override bool ShowCommand() => GameState.IsFreePlay;
    public override void Run()
    {
        PlayerControl.LocalPlayer.Exiled();
    }
}
#endif
