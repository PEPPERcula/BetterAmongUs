using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;

namespace BetterAmongUs.Patches;

[HarmonyPatch(typeof(RoleManager))]
public class RoleManagerPatch
{
    public static Dictionary<PlayerControl, RoleTypes> SetPlayerRole = []; // Player, Role
    public static Dictionary<string, int> ImpostorMultiplier = []; // HashPuid, Multiplier
    private static Random random = new Random();

    // Better role algorithm
    [HarmonyPatch(nameof(RoleManager.SelectRoles))]
    [HarmonyPrefix]
    public static bool RoleManager_Prefix(/*RoleManager __instance*/)
    {
        if (!Main.BetterRoleAlgorithma.Value) return true;

        Logger.LogHeader($"Better Role Assignment Has Started", "RoleManager");

        // Set roles up
        foreach (var addplayer in Main.AllPlayerControls.Where(pc => !ImpostorMultiplier.ContainsKey(Utils.GetHashPuid(pc))))
            ImpostorMultiplier[Utils.GetHashPuid(addplayer)] = 0;

        int NumImpostors = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors;

        int NumPlayers = Main.AllPlayerControls.Length;

        var impostorLimits = new Dictionary<int, int>
        {
            { 3, 1 },
            { 5, 2 },
            { 6, 3 }
        };

        foreach (var limit in impostorLimits)
        {
            if (NumPlayers <= limit.Key)
            {
                NumImpostors = Math.Min(NumImpostors, limit.Value);
                break;
            }
        }

        List<PlayerControl> Impostors = [];
        List<PlayerControl> Crewmates = [];

        Dictionary<RoleTypes, int> ImpostorRoles = new() // Role, Amount
        {
            { RoleTypes.Shapeshifter, 0 },
            { RoleTypes.Phantom, 0 }
        };

        Dictionary<RoleTypes, int> CrewmateRoles = new() // Role, Amount
        {
            { RoleTypes.Engineer, 0 },
            { RoleTypes.Scientist, 0 },
            { RoleTypes.Tracker, 0 },
            { RoleTypes.Noisemaker, 0 }
        };

        List<RoleTypes> Roles = [.. ImpostorRoles.Keys, .. CrewmateRoles.Keys];

        foreach (RoleTypes role in Roles)
        {
            if (IsImpostorRole(role))
                ImpostorRoles[role] = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(role);
            else
                CrewmateRoles[role] = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(role);
        }

        // Override player role assignment
        if (SetPlayerRole.Keys.Any())
        {
            foreach (var kvp in SetPlayerRole.Where(kvp => kvp.Key != null).OrderBy(kvp => kvp.Key == PlayerControl.LocalPlayer ? 0 : 1))
            {
                var player = kvp.Key;
                var role = kvp.Value;

                if (role is RoleTypes.Impostor)
                {
                    if (Impostors.Count < NumImpostors)
                    {
                        Impostors.Add(player);
                        player.RpcSetRole(RoleTypes.Impostor);
                        player.roleAssigned = true;
                        Logger.Log($"Override Assigned {Utils.GetRoleName(RoleTypes.Impostor)} role to {player.Data.PlayerName}", "RoleManager");
                    }
                    else continue;
                }
                else if (role is RoleTypes.Crewmate)
                {
                    Crewmates.Add(player);
                    player.RpcSetRole(RoleTypes.Crewmate);
                    player.roleAssigned = true;
                    Logger.Log($"Override Assigned {Utils.GetRoleName(RoleTypes.Crewmate)} role to {player.Data.PlayerName}", "RoleManager");
                }
                else
                {
                    if (IsImpostorRole(role))
                    {
                        if (Impostors.Count < NumImpostors && ImpostorRoles[role] > 0)
                        {
                            ImpostorRoles[role]--;
                            Impostors.Add(player);
                        }
                        else continue;
                    }
                    else
                    {
                        if (CrewmateRoles[role] > 0)
                        {
                            CrewmateRoles[role]--;
                            Crewmates.Add(player);
                        }
                        else continue;
                    }

                    player.RpcSetRole(role);
                    player.roleAssigned = true;
                    Logger.Log($"Override Assigned {Utils.GetRoleName(role)} role to {player.Data.PlayerName}", "RoleManager");
                }
            }
        }

        // Get players in random order
        List<PlayerControl> players = Main.AllPlayerControls
            .Where(player => !Impostors.Contains(player) && !Crewmates.Contains(player) && player.roleAssigned == false)
            .ToList();

        int n = players.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            PlayerControl value = players[k];
            players[k] = players[n];
            players[n] = value;
        }

        // Assign roles
        foreach (PlayerControl pc in players)
        {
            if (pc == null || pc.roleAssigned == true) continue;

            if (Impostors.Count < NumImpostors && RNG() > ImpostorMultiplier[Utils.GetHashPuid(pc)])
            {
                var impRoles = Shuffle(ImpostorRoles);
                foreach (var kvp in impRoles)
                {
                    if (RNG() <= GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(kvp.Key) && kvp.Value > 0)
                    {
                        ImpostorMultiplier[Utils.GetHashPuid(pc)] += 15;
                        ImpostorRoles[kvp.Key]--;
                        Impostors.Add(pc);
                        pc.RpcSetRole(kvp.Key);
                        pc.roleAssigned = true;
                        Logger.Log($"Assigned {Utils.GetRoleName(kvp.Key)} role to {pc.Data.PlayerName}", "RoleManager");
                        break;
                    }
                }

                if (!Impostors.Contains(pc))
                {
                    ImpostorMultiplier[Utils.GetHashPuid(pc)] += 15;
                    Impostors.Add(pc);
                    pc.RpcSetRole(RoleTypes.Impostor);
                    pc.roleAssigned = true;
                    Logger.Log($"Assigned {Utils.GetRoleName(RoleTypes.Impostor)} role to {pc.Data.PlayerName}", "RoleManager");
                }
            }
            else
            {
                var crewRoles = Shuffle(CrewmateRoles);
                foreach (var kvp in crewRoles)
                {
                    if (RNG() <= GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(kvp.Key) && kvp.Value > 0)
                    {
                        ImpostorMultiplier[Utils.GetHashPuid(pc)] = 0;
                        CrewmateRoles[kvp.Key]--;
                        Crewmates.Add(pc);
                        pc.RpcSetRole(kvp.Key);
                        pc.roleAssigned = true;
                        Logger.Log($"Assigned {Utils.GetRoleName(kvp.Key)} role to {pc.Data.PlayerName}", "RoleManager");
                        break;
                    }
                }

                if (!Crewmates.Contains(pc))
                {
                    ImpostorMultiplier[Utils.GetHashPuid(pc)] = 0;
                    Crewmates.Add(pc);
                    pc.RpcSetRole(RoleTypes.Crewmate);
                    pc.roleAssigned = true;
                    Logger.Log($"Assigned {Utils.GetRoleName(RoleTypes.Crewmate)} role to {pc.Data.PlayerName}", "RoleManager");
                }
            }
        }

        SetPlayerRole.Clear();

        _ = new LateTask(() =>
        {
            RPC.SyncAllNames(false, true, Main.BetterHost.Value);
        }, 1f, "RoleManager SyncAllNames");

        Logger.LogHeader($"Better Role Assignment Has Finished", "RoleManager");

        return false;
    }

    [HarmonyPatch(nameof(RoleManager.AssignRoleOnDeath))]
    [HarmonyPrefix]
    public static bool AssignRoleOnDeath_Prefix(/*RoleManager __instance*/ [HarmonyArgument(0)] PlayerControl player)
    {
        if (!Main.BetterRoleAlgorithma.Value) return true;

        Dictionary<RoleTypes, int> GhostRoles = new() // Role, Amount
        {
            { RoleTypes.GuardianAngel, 0 },
        };

        List<RoleTypes> Roles = [.. GhostRoles.Keys];

        foreach (RoleTypes role in Roles)
        {
            GhostRoles[role] = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(role);
        }

        foreach (var allDeadPlayers in Main.AllPlayerControls.Where(pc => !pc.IsAlive()))
        {
            for (int i = 0; i < Roles.Count; i++)
            {
                if (allDeadPlayers.Is(Roles[i]))
                {
                    GhostRoles[Roles[i]]--;
                }
            }
        }

        var ghostRoles = Shuffle(GhostRoles);

        foreach (var kvp in ghostRoles)
        {
            if (kvp.Value > 0 && RNG() <= GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(kvp.Key))
            {
                player.RpcSetRole(kvp.Key);
                break;
            }
            else
            {
                player.RpcSetRole(player.Data.Role.DefaultGhostRole);
            }
        }

        return false;
    }

    private static bool IsImpostorRole(RoleTypes role) => role is RoleTypes.Impostor or RoleTypes.Shapeshifter or RoleTypes.Phantom;

    private static Dictionary<TKey, TValue> Shuffle<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
    {
        List<TKey> keys = dictionary.Keys.ToList();

        // Fisher-Yates shuffle algorithm
        for (int i = keys.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            TKey temp = keys[i];
            keys[i] = keys[j];
            keys[j] = temp;
        }

        // Rebuild dictionary with shuffled keys
        Dictionary<TKey, TValue> shuffledDictionary = new Dictionary<TKey, TValue>();
        foreach (var key in keys)
        {
            shuffledDictionary[key] = dictionary[key];
        }

        return shuffledDictionary;
    }

    private static int RNG() => random.Next(0, 100);
}