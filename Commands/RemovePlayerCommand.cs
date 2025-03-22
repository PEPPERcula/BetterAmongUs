using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules.AntiCheat;

namespace BetterAmongUs.Commands;

[RegisterCommand]
public class RemovePlayerCommand : BaseCommand
{
    public override string Name => "removeplayer";
    public override string Description => "Remove player from local <color=#4f92ff>Anti-Cheat</color> data";

    public RemovePlayerCommand()
    {
        _arguments = new Lazy<BaseArgument[]>(() => new BaseArgument[]
        {
            new StringArgument(this, "{identifier}"),
        });
        identifierArgument.GetArgSuggestions = BAUAntiCheat.GatherAllData;
    }
    private readonly Lazy<BaseArgument[]> _arguments;
    public override BaseArgument[]? Arguments => _arguments.Value;

    private StringArgument? identifierArgument => (StringArgument)Arguments[0];
    public override void Run()
    {
        if (BetterDataManager.RemovePlayer(identifierArgument.Arg) == true)
        {
            Utils.AddChatPrivate($"{identifierArgument.Arg} successfully removed from local <color=#4f92ff>Anti-Cheat</color> data!");
            Utils.DirtyAllNames();
        }
        else
        {
            Utils.AddChatPrivate($"{identifierArgument.Arg} Could not find player data from identifier");
        }
    }
}
