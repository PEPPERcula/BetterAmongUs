#if DEBUG
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Modules;

namespace BetterAmongUs.Commands;

[RegisterCommand]
internal class ReviveCommand : BaseCommand
{
    internal override CommandType Type => CommandType.Debug;
    internal override string Name => "revive";
    internal override string Description => "Set self as alive";
    internal override bool ShowCommand() => GameState.IsFreePlay;
    internal override void Run()
    {
        PlayerControl.LocalPlayer.Revive();
    }
}
#endif
