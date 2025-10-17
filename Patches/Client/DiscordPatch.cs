using AmongUs.Data;
using BetterAmongUs.Modules;
using Discord;
using HarmonyLib;

namespace BetterAmongUs.Patches.Client;


[HarmonyPatch(typeof(ActivityManager), nameof(ActivityManager.UpdateActivity))]
internal class DiscordRPC
{
    private static string lobbycode = "";
    private static string region = "";

    internal static void Prefix([HarmonyArgument(0)] Activity activity)
    {
        if (activity == null) return;

        string details = $"BAU {Main.GetVersionText()}";
        activity.Details = details;

        if (activity.State == "In Menus") return;

        try
        {
            if (!DataManager.Settings.Gameplay.StreamerMode)
            {
                UpdateRegionAndLobbyCode();
                if (!string.IsNullOrEmpty(lobbycode) && !string.IsNullOrEmpty(region))
                {
                    if (GameState.IsNormalGame)
                        details = $"BAU - {lobbycode} ({region})";
                    else if (GameState.IsHideNSeek)
                        details = $"BAU Hide & Seek - {lobbycode} ({region})";
                }
            }
            else
            {
                if (GameState.IsHideNSeek)
                    details = $"BAU v{ModInfo.PluginVersion} - Hide & Seek";
            }
        }
        catch
        {
        }

        activity.Details = details;
    }

    private static void UpdateRegionAndLobbyCode()
    {
        if (GameState.IsLobby)
        {
            lobbycode = GameStartManager.Instance.GameRoomNameCode.text;
            region = ServerManager.Instance.CurrentRegion.Name;
            region = region switch
            {
                "North America" => "NA",
                "Europe" => "EU",
                "Asia" => "AS",
                _ when region.Contains("MNA") => "MNA",
                _ when region.Contains("MEU") => "MEU",
                _ when region.Contains("MAS") => "MAS",
                _ => region
            };
        }
    }
}
