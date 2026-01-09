using BetterAmongUs.Helpers;

namespace BetterAmongUs.Commands.Arguments;

internal sealed class PlayerArgument(BaseCommand? command, string argInfo = "{player}") : BaseArgument(command, argInfo)
{
    protected override string[] ArgSuggestions => BAUPlugin.AllPlayerControls.OrderBy(pc => pc.IsLocalPlayer() ? 0 : 1).Select(pc => pc.Data.PlayerName.Replace(' ', '_')).ToArray();
    internal PlayerControl? TryGetTarget()
    {
        var player = BAUPlugin.AllPlayerControls.FirstOrDefault(pc => pc.Data.PlayerName.ToLower().Replace(' ', '_') == Arg.ToLower());

        if (player == null)
        {
            BaseCommand.CommandErrorText($"Player not found!");
        }

        return player;
    }
}