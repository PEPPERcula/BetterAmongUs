using BetterAmongUs.Items.Attributes;

namespace BetterAmongUs.Commands;

[RegisterCommand]
internal class SetPrefixCommand : BaseCommand
{
    internal override string Name => "setprefix";
    internal override string Description => "Set command prefix";

    public SetPrefixCommand()
    {
        _arguments = new Lazy<BaseArgument[]>(() => new BaseArgument[]
        {
            new StringArgument(this, "{prefix}"),
        });
    }
    private readonly Lazy<BaseArgument[]> _arguments;
    internal override BaseArgument[]? Arguments => _arguments.Value;

    private StringArgument? prefixArgument => (StringArgument)Arguments[0];

    internal override void Run()
    {
        var oldPrefix = Main.CommandPrefix.Value;
        var prefix = prefixArgument.Arg.ToCharArray().First().ToString();
        if (!string.IsNullOrEmpty(prefix))
        {
            Main.CommandPrefix.Value = prefix;
            CommandResultText($"Command prefix set from <#c1c100>{oldPrefix}</color> to <#c1c100>{prefix}</color>");
        }
        else
        {
            CommandErrorText("Invalid Syntax!");
        }
    }
}
