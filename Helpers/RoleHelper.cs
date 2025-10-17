using AmongUs.GameOptions;

namespace BetterAmongUs.Helpers;

internal static class RoleHelper
{
    internal static RoleBehaviour? GetBehaviour(this RoleTypes role) => RoleManager.Instance?.AllRoles.ToArray().FirstOrDefault(r => r.Role == role);

    internal static bool IsImpostorRole(RoleTypes role) =>
        role.GetBehaviour().TeamType is RoleTeamTypes.Impostor;

    internal static string GetRoleName(this RoleTypes role)
    {
        return role.GetBehaviour()?.NiceName ?? "???";
    }

    internal static string GetRoleHex(this RoleTypes role)
    {
        if (RoleColor.TryGetValue(role, out var color))
        {
            return color;
        }

        return string.Empty;
    }

    internal static Dictionary<RoleTypes, string> RoleColor => new()
    {
        { RoleTypes.CrewmateGhost, "#8cffff" },
        { RoleTypes.GuardianAngel, "#8cffff" },
        { RoleTypes.Crewmate, "#8cffff" },
        { RoleTypes.Scientist, "#00d9d9" },
        { RoleTypes.Engineer, "#8f8f8f" },
        { RoleTypes.Noisemaker, "#fc7c7c" },
        { RoleTypes.Tracker, "#59f002" },
        { RoleTypes.Detective, "#0027FF" },
        { RoleTypes.ImpostorGhost, "#f00202" },
        { RoleTypes.Impostor, "#f00202" },
        { RoleTypes.Shapeshifter, "#f06102" },
        { RoleTypes.Phantom, "#d100b9" },
        { RoleTypes.Viper, "#367400" }
    };
}
