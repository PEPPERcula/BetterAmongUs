using BetterAmongUs.Helpers;

namespace BetterAmongUs.Commands;

public class PlayerArgument(BaseCommand? command) : BaseArgument(command)
{
    protected override string[] ArgSuggestions => Main.AllPlayerControls.OrderBy(pc => pc.PlayerId).Select(pc => pc.PlayerId.ToString()).ToArray();
    public override string ArgInfo => "{Id}";
    public PlayerControl? TryGetTarget()
    {
        var digits = Arg.Where(char.IsDigit).ToArray();
        bool isDigitFlag = digits.Any();
        bool playerFound = false;

        if (isDigitFlag)
        {
            if (int.TryParse(new string(digits), out var playerId))
            {
                playerFound = Main.AllPlayerControls.Any(player => !player.isDummy && player.Data.PlayerId == playerId);
            }
        }

        if (playerFound && byte.TryParse(Arg, out var num))
        {
            return Utils.PlayerFromPlayerId(num);
        }
        else
        {
            if (!isDigitFlag)
            {
                BaseCommand.CommandErrorText($"Invalid Syntax!");
            }
            else if (!playerFound)
            {
                BaseCommand.CommandErrorText($"Player not found!");
            }
        }

        return null;
    }
}