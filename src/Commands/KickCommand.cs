using BetterAmongUs.Commands.Arguments;
using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Modules;

namespace BetterAmongUs.Commands;

[RegisterCommand]
internal sealed class KickCommand : BaseCommand
{
    internal override string Name => "kick";
    internal override string Description => "Kick a player from the game";
    public KickCommand()
    {
        playerArgument = new PlayerArgument(this);
        boolArgument = new BoolArgument(this, "{ban}");
        Arguments = [playerArgument, boolArgument];
    }
    private PlayerArgument playerArgument { get; }
    private BoolArgument boolArgument { get; }

    internal override bool ShowCommand() => GameState.IsHost;
    internal override void Run()
    {
        var player = playerArgument.TryGetTarget();
        var isBan = boolArgument.GetBool();
        if (player != null && isBan != null && !player.IsHost())
        {
            player.Kick((bool)isBan);
        }
    }
}
