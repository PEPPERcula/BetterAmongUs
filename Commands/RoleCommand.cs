#if DEBUG
using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Modules;
using BetterAmongUs.Patches;

namespace BetterAmongUs.Commands;

[RegisterCommand]
internal class RoleCommand : BaseCommand
{
    internal override CommandType Type => CommandType.Debug;
    internal override string Name => "role";
    internal override string Description => "Set your role for the next game";

    public RoleCommand()
    {
        _arguments = new Lazy<BaseArgument[]>(() => new BaseArgument[]
        {
            new StringArgument(this, "{role}"),
        });
        roleArgument.GetArgSuggestions = () => { return RoleManager.Instance.AllRoles.Select(role => role.NiceName.ToLower()).ToArray(); };
    }

    private readonly Lazy<BaseArgument[]> _arguments;
    internal override BaseArgument[]? Arguments => _arguments.Value;

    private StringArgument? roleArgument => (StringArgument)Arguments[0];

    internal override bool ShowCommand() => GameState.IsHost && Main.MyData.HasAll() && Main.MyData.IsVerified();

    internal override void Run()
    {
        var role = RoleManager.Instance.AllRoles.FirstOrDefault(r => r.NiceName.StartsWith(roleArgument.Arg));
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
