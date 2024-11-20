using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;

namespace BetterAmongUs.Commands;

public class RemovePlayerCommand : BaseCommand
{
    public override string Name => "removeplayer";
    public override string Description => "Remove player from local <color=#4f92ff>Anti-Cheat</color> data";

    public RemovePlayerCommand()
    {
        _arguments = new Lazy<BaseArgument[]>(() => new BaseArgument[]
        {
            new StringArgument(this),
        });
        identifierArgument.suggestion = "{Identifier}";
        identifierArgument.GetArgSuggestions = () =>
        {
            return Main.AllPlayerControls
                .Select(pc => pc.Data.FriendCode)
                .Concat(Main.AllPlayerControls.Select(pc => pc.GetHashPuid()))
                .ToArray();
        };
    }
    private readonly Lazy<BaseArgument[]> _arguments;
    public override BaseArgument[]? Arguments => _arguments.Value;

    private StringArgument? identifierArgument => (StringArgument)Arguments[0];
    public override void Run()
    {
        if (BetterDataManager.RemovePlayer(identifierArgument.Arg) == true)
        {
            Utils.AddChatPrivate($"{identifierArgument.Arg} successfully removed from local <color=#4f92ff>Anti-Cheat</color> data!");
        }
        else
        {
            Utils.AddChatPrivate($"{identifierArgument.Arg}\nCould not find player data from identifier");
        }
    }
}
