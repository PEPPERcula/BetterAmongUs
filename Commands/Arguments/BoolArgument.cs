namespace BetterAmongUs.Commands;

public class BoolArgument(BaseCommand? command) : BaseArgument(command)
{
    public override string ArgInfo => suggestion;
    public string suggestion = "{bool}";
    protected override string[] ArgSuggestions => ["true", "false"];
    public bool? GetBool()
    {
        if (Arg.ToLower() is "true")
        {
            return true;
        }
        else if (Arg.ToLower() is "false" or "")
        {
            return false;
        }
        else
        {
            BaseCommand.CommandErrorText($"Invalid Syntax!");
        }

        return null;
    }
}