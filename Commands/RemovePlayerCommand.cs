using BetterAmongUs.Data;
using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Commands;

[RegisterCommand]
internal class RemovePlayerCommand : BaseCommand
{
    internal override string Name => "removeplayer";
    internal override string Description => "Remove player from local <color=#4f92ff>Anti-Cheat</color> data";

    public RemovePlayerCommand()
    {
        _arguments = new Lazy<BaseArgument[]>(() => new BaseArgument[]
        {
            new StringArgument(this, "{identifier}"),
        });
        identifierArgument.GetArgSuggestions = () =>
            BetterDataManager.BetterDataFile.AllCheatData
                .SelectMany(info => new[] { info.HashPuid.Replace(' ', '_'), info.FriendCode.Replace(' ', '_'), info.PlayerName.Replace(' ', '_') })
                .ToArray();
    }
    private readonly Lazy<BaseArgument[]> _arguments;
    internal override BaseArgument[]? Arguments => _arguments.Value;

    private StringArgument? identifierArgument => (StringArgument)Arguments[0];
    internal override void Run()
    {
        if (BetterDataManager.RemovePlayer(identifierArgument.Arg) == true)
        {
            Utils.AddChatPrivate($"{identifierArgument.Arg} successfully removed from local <color=#4f92ff>Anti-Cheat</color> data!");
        }
        else
        {
            Utils.AddChatPrivate($"{identifierArgument.Arg} Could not find player data from identifier");
        }
    }
}
