using BetterAmongUs.Modules;

namespace BetterAmongUs.Commands;

public class EndGameCommand : BaseCommand
{
    public override string Name => "endgame";
    public override string Description => "Force end the game";
    public override bool ShowCommand() => GameState.IsHost && !GameState.IsLobby && !GameState.IsFreePlay;
    public override void Run()
    {
        GameManager.Instance.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
    }
}
