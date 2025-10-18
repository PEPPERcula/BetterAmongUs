#if DEBUG
using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Modules;
using BetterAmongUs.Patches.Managers;

namespace BetterAmongUs.Commands;

[RegisterCommand]
internal class SetRoleCommand : BaseCommand
{
    internal override CommandType Type => CommandType.Debug;
    internal override string Name => "setrole";
    internal override string Description => "Set another players role for the next game";

    internal SetRoleCommand()
    {
        _arguments = new Lazy<BaseArgument[]>(() => new BaseArgument[]
        {
            new PlayerArgument(this),
            new StringArgument(this, "{role}"),
        }); ;
        roleArgument.GetArgSuggestions = () => { return RoleManager.Instance.AllRoles.ToArray().Select(role => role.NiceName.ToLower()).ToArray(); };
    }
    private readonly Lazy<BaseArgument[]> _arguments;
    internal override BaseArgument[]? Arguments => _arguments.Value;

    private PlayerArgument? playerArgument => (PlayerArgument)Arguments[0];
    private StringArgument? roleArgument => (StringArgument)Arguments[1];

    internal override bool ShowCommand() => GameState.IsHost && BAUPlugin.MyData.HasAll() && BAUPlugin.MyData.IsVerified();

    internal override void Run()
    {
        var player = playerArgument.TryGetTarget();
        if (player.IsLocalPlayer())
        {
            CommandErrorText("Unable to use /setrole for self, use /role!");
            return;
        }
        var role = RoleManager.Instance.AllRoles.ToArray().FirstOrDefault(r => r.NiceName.ToLower().StartsWith(roleArgument.Arg.ToLower()));
        if (role != null)
        {
            Utils.AddChatPrivate($"Set role to <color={Utils.GetTeamHexColor(role.TeamType)}>{role.NiceName}</color> for the next game!");
            RoleManagerPatch.SetPlayerRole[PlayerControl.LocalPlayer] = role.Role;
        }
        else
        {
            CommandErrorText("Unable to find role");
        }
    }
}
#endif
