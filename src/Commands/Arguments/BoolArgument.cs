namespace BetterAmongUs.Commands;

internal class BoolArgument(BaseCommand? command, string argInfo = "{bool}") : BaseArgument(command, argInfo)
{
    protected override string[] ArgSuggestions => ["true", "false"];
    internal bool? GetBool()
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