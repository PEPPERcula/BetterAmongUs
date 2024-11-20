namespace BetterAmongUs.Commands;

public class StringArgument(BaseCommand? command) : BaseArgument(command)
{
    public override string ArgInfo => suggestion;
    public string suggestion = "{String}";
}