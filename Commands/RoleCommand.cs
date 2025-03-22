#if DEBUG
using BetterAmongUs.Helpers;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Modules;
using BetterAmongUs.Patches;

namespace BetterAmongUs.Commands;

[RegisterCommand]
public class RoleCommand : BaseCommand
{
    public override CommandType Type => CommandType.Debug;
    public override string Name => "role";
    public override string Description => "Set your role for the next game";

    public RoleCommand()
    {
        _arguments = new Lazy<BaseArgument[]>(() => new BaseArgument[]
        {
            new StringArgument(this, "{role}"),
        });
        roleArgument.GetArgSuggestions = () => { return RoleManager.Instance.AllRoles.Select(role => role.NiceName.ToLower()).ToArray(); };
    }
    private readonly Lazy<BaseArgument[]> _arguments;
    public override BaseArgument[]? Arguments => _arguments.Value;

    private StringArgument? roleArgument => (StringArgument)Arguments[0];

    public override bool ShowCommand() => GameState.IsHost && Main.MyData.HasAll() && Main.MyData.IsVerified();

    public override void Run()
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
