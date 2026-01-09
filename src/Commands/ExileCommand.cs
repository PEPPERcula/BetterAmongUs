#if DEBUG
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Modules;

namespace BetterAmongUs.Commands;

[RegisterCommand]
internal class ExileCommand : BaseCommand
{
    internal override CommandType Type => CommandType.Debug;
    internal override string Name => "exile";
    internal override string Description => "Set self as dead";
    internal override bool ShowCommand() => GameState.IsFreePlay;
    internal override void Run()
    {
        PlayerControl.LocalPlayer.Exiled();
    }
}
#endif
