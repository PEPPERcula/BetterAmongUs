#if DEBUG
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Modules;

namespace BetterAmongUs.Commands;

[RegisterCommand]
internal class SuicideCommand : BaseCommand
{
    internal override CommandType Type => CommandType.Debug;
    internal override string Name => "suicide";
    internal override string Description => "Kill self";
    internal override bool ShowCommand() => GameState.IsFreePlay;
    internal override void Run()
    {
        PlayerControl.LocalPlayer.MurderPlayer(PlayerControl.LocalPlayer, MurderResultFlags.Succeeded);
    }
}
#endif