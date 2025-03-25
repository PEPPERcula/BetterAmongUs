using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Modules;

namespace BetterAmongUs.Commands;

[RegisterCommand]
internal class KickCommand : BaseCommand
{
    internal override string Name => "kick";
    internal override string Description => "Kick a player from the game";
    public KickCommand()
    {
        _arguments = new Lazy<BaseArgument[]>(() => new BaseArgument[]
        {
            new PlayerArgument(this),
            new BoolArgument(this, "{ban}"),
        });
    }
    private readonly Lazy<BaseArgument[]> _arguments;
    internal override BaseArgument[]? Arguments => _arguments.Value;

    private PlayerArgument? playerArgument => (PlayerArgument)Arguments[0];
    private BoolArgument? boolArgument => (BoolArgument)Arguments[1];
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
