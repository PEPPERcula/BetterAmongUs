using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;

namespace BetterAmongUs.Commands;

public class KickCommand : BaseCommand
{
    public override string Name => "kick";
    public override string Description => "Kick a player from the game";
    public KickCommand()
    {
        _arguments = new Lazy<BaseArgument[]>(() => new BaseArgument[]
        {
            new PlayerArgument(this),
            new BoolArgument(this),
        });

        boolArgument.suggestion = "{Ban}";
    }
    private readonly Lazy<BaseArgument[]> _arguments;
    public override BaseArgument[]? Arguments => _arguments.Value;

    private PlayerArgument? playerArgument => (PlayerArgument)Arguments[0];
    private BoolArgument? boolArgument => (BoolArgument)Arguments[1];
    public override bool ShowCommand() => GameState.IsHost;
    public override void Run()
    {
        var player = playerArgument.TryGetTarget();
        var isBan = boolArgument.GetBool();
        if (player != null && isBan != null && !player.IsHost())
        {
            player.Kick((bool)isBan);
        }
    }
}
