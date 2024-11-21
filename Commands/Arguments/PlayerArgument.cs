using BetterAmongUs.Helpers;

namespace BetterAmongUs.Commands;

public class PlayerArgument(BaseCommand? command, string argInfo = "{player}") : BaseArgument(command, argInfo)
{
    protected override string[] ArgSuggestions => Main.AllPlayerControls.OrderBy(pc => pc.PlayerId).Select(pc => pc.Data.PlayerName.Replace(' ', '_')).ToArray();
    public PlayerControl? TryGetTarget()
    {
        var player = Main.AllPlayerControls.FirstOrDefault(pc => pc.Data.PlayerName.ToLower().Replace(' ', '_') == Arg.ToLower());

        if (player == null)
        {
            BaseCommand.CommandErrorText($"Player not found!");
        }

        return player;
    }
}