using AmongUs.GameOptions;

namespace BetterAmongUs.Helpers;

internal static class RoleHelper
{
    private static readonly Lazy<Dictionary<RoleTypes, RoleBehaviour>> roleLookup =
        new(() =>
        {
            var dict = new Dictionary<RoleTypes, RoleBehaviour>();
            foreach (var r in RoleManager.Instance.AllRoles)
            {
                dict[r.Role] = r;
            }
            return dict;
        });

    internal static RoleBehaviour? GetBehaviour(this RoleTypes role)
    {
        var lookup = roleLookup.Value;
        return lookup.TryGetValue(role, out var behaviour) ? behaviour : null;
    }

    internal static bool IsImpostorRole(RoleTypes role) =>
        role.GetBehaviour().TeamType is RoleTeamTypes.Impostor;

    internal static string GetRoleName(this RoleTypes role)
    {
        if (role is RoleTypes.ImpostorGhost)
        {
            return RoleTypes.Impostor.GetBehaviour()?.NiceName ?? "???";
        }
        else if (role is RoleTypes.CrewmateGhost)
        {
            return RoleTypes.Crewmate.GetBehaviour()?.NiceName ?? "???";
        }

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
