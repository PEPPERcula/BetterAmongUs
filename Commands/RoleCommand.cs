#if DEBUG
using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Modules;
using BetterAmongUs.Patches.Managers;

namespace BetterAmongUs.Commands;

[RegisterCommand]
internal class RoleCommand : BaseCommand
{
    internal override CommandType Type => CommandType.Debug;
    internal override string Name => "role";
    internal override string Description => "Set your role for the next game";

    internal RoleCommand()
    {
        roleArgument = new StringArgument(this, "{role}")
        {
            GetArgSuggestions = () => { return RoleManager.Instance.AllRoles.ToArray().Select(role => role.NiceName.ToLower()).ToArray(); }
        };
        Arguments = [roleArgument];
    }
    private StringArgument roleArgument { get; }

    internal override bool ShowCommand() => GameState.IsHost && BAUPlugin.MyData.HasAll() && BAUPlugin.MyData.IsVerified();

    internal override void Run()
    {
        var role = RoleManager.Instance.AllRoles.ToArray().FirstOrDefault(r => r.NiceName.StartsWith(roleArgument.Arg));
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
