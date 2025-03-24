namespace BetterAmongUs.Commands;

internal class StringArgument(BaseCommand? command, string argInfo = "{String}") : BaseArgument(command, argInfo)
{
}