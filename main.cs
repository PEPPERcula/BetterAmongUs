using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Innersloth.IO;
using UnityEngine;

namespace BetterAmongUs;

public enum ReleaseTypes : int
{
    Release,
    Canary,
    Dev,
}

[BepInPlugin(PluginGuid, "BetterAmongUs", PluginVersion)]
[BepInProcess("Among Us.exe")]
public class Main : BasePlugin
{
    public const ReleaseTypes ReleaseBuildType = ReleaseTypes.Release;
    public const string CanaryNum = "0";
    public const string HotfixNum = "1";
    public const bool IsHotFix = true;
    public const string PluginGuid = "com.d1gq.betteramongus";
    public const string PluginVersion = "1.0.0";
    public const string ReleaseDate = "08.08.2024"; // mm/dd/yyyy
    public const string Github = "https://github.com/D1GQ/BetterAmongUs-Public";
    public const string Discord = "https://discord.gg/vjYrXpzNAn";

    public static string GetVersionText(bool newLine = false)
    {
        string text = string.Empty;

        string newLineText = newLine ? "\n" : " ";

        if (ReleaseBuildType == ReleaseTypes.Release)
            text = $"v{BetterAmongUsVersion}";
        else if (ReleaseBuildType == ReleaseTypes.Canary)
            text = $"v{BetterAmongUsVersion}{newLineText}Canary {Main.CanaryNum}";
        else if (Main.ReleaseBuildType == ReleaseTypes.Dev)
            text = $"v{BetterAmongUsVersion}{newLineText}Dev {Main.ReleaseDate}";

        if (IsHotFix)
            text += $" Hotfix {HotfixNum}";

        return text;
    }
    public Harmony Harmony { get; } = new Harmony(PluginGuid);

    public static string BetterAmongUsVersion => PluginVersion;
    public static string AmongUsVersion => Application.version;

    public static List<string> SupportedAmongUsVersions =
    [
        "2024.6.18",
    ];

    public static string[] DevUser =
    [
        "8f23c48e2+notegusty#8646",
    ];

    public static PlayerControl[] AllPlayerControls => PlayerControl.AllPlayerControls.ToArray().Where(pc => pc != null).ToArray();

    public static PlayerControl[] AllAlivePlayerControls => AllPlayerControls.ToArray().Where(pc => pc.IsAlive()).ToArray();

    public static Dictionary<int, string> GetRoleName = new Dictionary<int, string>
    {
        { 0, "Crewmate" },
        { 1, "Impostor" },
        { 2, "Scientist" },
        { 3, "Engineer" },
        { 4, "Guardian Angel" },
        { 5, "Shapeshifter" },
        { 6, "Crewmate" },
        { 7, "Impostor" },
        { 8, "Noisemaker" },
        { 9, "Phantom" },
        { 10, "Tracker" }
    };

    public static ManualLogSource Logger;
    public static DebugMenu debugmenu { get; set; } = null;

    public override void Load()
    {
        try
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(PluginGuid);

            Harmony.PatchAll();
            BetterDataManager.SetUp();
            BetterDataManager.LoadCheatData();
            LoadOptions();

            if (File.Exists(Path.Combine($"{Environment.CurrentDirectory}/Among Us_Data/Plugins/x86", "steam_api.dll")))
                File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "steam_appid.txt"), "945360");

            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "better-log.txt")))
                File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "better-previous-log.txt"), File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "better-log.txt")));

            File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "better-log.txt"), "");
            BetterAmongUs.Logger.Log("Better Among Us successfully loaded!");

            // Set up debug menu
            ClassInjector.RegisterTypeInIl2Cpp<DebugMenu>();
            ClassInjector.RegisterTypeInIl2Cpp<Resources.Coroutines.Component>();
            debugmenu = AddComponent<DebugMenu>();
            AddComponent<Resources.Coroutines.Component>();

            string SupportedVersions = string.Empty;
            foreach (string text in SupportedAmongUsVersions.ToArray())
                SupportedVersions += $"{text} ";
            BetterAmongUs.Logger.Log($"BetterAmongUs {BetterAmongUsVersion}-{ReleaseDate} - [{AmongUsVersion} --> {SupportedVersions.Substring(0, SupportedVersions.Length - 1)}]");
        }
        catch (Exception ex)
        {
            BetterAmongUs.Logger.Error(ex.ToString());
        }
    }

    public static ConfigEntry<bool> AntiCheat { get; private set; }
    public static ConfigEntry<bool> BetterHost { get; private set; }
    public static ConfigEntry<bool> BetterRoleAlgorithma { get; private set; }
    public static ConfigEntry<bool> LobbyPlayerInfo { get; private set; }
    public static ConfigEntry<bool> DisableLobbyTheme { get; private set; }
    public static ConfigEntry<bool> UnlockFPS { get; private set; }
    public static ConfigEntry<bool> ShowFPS { get; private set; }
    public static ConfigEntry<string> CommandPrefix { get; set; }
    private void LoadOptions()
    {
        AntiCheat = Config.Bind("Better Options", "AntiCheat", true);
        BetterHost = Config.Bind("Better Options", "BetterHost", false);
        BetterRoleAlgorithma = Config.Bind("Better Options", "BetterRoleAlgorithma", true);
        LobbyPlayerInfo = Config.Bind("Better Options", "LobbyPlayerInfo", true);
        DisableLobbyTheme = Config.Bind("Better Options", "DisableLobbyTheme", true);
        UnlockFPS = Config.Bind("Better Options", "UnlockFPS", false);
        ShowFPS = Config.Bind("Better Options", "ShowFPS", false);
        CommandPrefix = Config.Bind("Client Options", "CommandPrefix", "/");
    }

    public static string GetDataPathToAmongUs() => FileIO.GetRootDataPath();
    public static string GetGamePathToAmongUs() => Environment.CurrentDirectory;
}
