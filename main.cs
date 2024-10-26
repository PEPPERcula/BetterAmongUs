using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BetterAmongUs.Patches;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Innersloth.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using static BetterAmongUs.PlayerControlDataExtension;

namespace BetterAmongUs;

public enum ReleaseTypes : int
{
    Release,
    Beta,
    Dev,
}

[BepInPlugin(PluginGuid, "BetterAmongUs", PluginVersion)]
[BepInProcess("Among Us.exe")]
public class Main : BasePlugin
{
    public static readonly ReleaseTypes ReleaseBuildType = ReleaseTypes.Release;
    public const string BetaNum = "0";
    public const string HotfixNum = "0";
    public const bool IsHotFix = false;
    public const string PluginGuid = "com.ten.betteramongus";
    public const string PluginVersion = "1.1.5";
    public const string ReleaseDate = "10.7.2024"; // mm/dd/yyyy
    public const string Github = "https://github.com/EnhancedNetwork/BetterAmongUs-Public";
    public const string Discord = "https://discord.gg/ten";
    public static BetterAccountInfo myAccountInfo = new();

    public static string modSignature
    {
        get
        {
            string GetHash(string puid)
            {
                using SHA256 sha256 = SHA256.Create();
                byte[] sha256Bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(puid));
                string sha256Hash = BitConverter.ToString(sha256Bytes).Replace("-", "").ToLower();
                return sha256Hash.Substring(0, 16) + sha256Hash.Substring(sha256Hash.Length - 8);
            }

            var versionData = new StringBuilder()
                .Append(Enum.GetName(typeof(ReleaseTypes), ReleaseBuildType))
                .Append(BetaNum)
                .Append(HotfixNum)
                .Append(PluginGuid)
                .Append(GetVersionText().Replace(" ", "."))
                .Append(ReleaseDate)
                .Append(Github)
                .Append(Discord)
                .Append(string.Join(".", Enum.GetNames(typeof(CustomRPC))))
                .Append(string.Join(".", GetRoleColor.Values))
                .ToString();

            return GetHash(versionData);
        }
    }

    public static string GetVersionText(bool newLine = false)
    {
        string text = string.Empty;

        string newLineText = newLine ? "\n" : " ";

        switch (ReleaseBuildType)
        {
            case ReleaseTypes.Release:
                text = $"v{BetterAmongUsVersion}";
                break;
            case ReleaseTypes.Beta:
                text = $"v{BetterAmongUsVersion}{newLineText}Beta {Main.BetaNum}";
                break;
            case ReleaseTypes.Dev:
                text = $"v{BetterAmongUsVersion}{newLineText}Dev {Main.ReleaseDate}";
                break;
            default:
                break;
        }

        if (IsHotFix)
            text += $" Hotfix {HotfixNum}";

        return text;
    }
    public Harmony Harmony { get; } = new Harmony(PluginGuid);

    public static string BetterAmongUsVersion => PluginVersion;
    public static string AmongUsVersion => Application.version;

    public static PlatformSpecificData PlatformData => Constants.GetPlatformData();

    public static List<string> SupportedAmongUsVersions =
    [
        "2024.10.29",
        "2024.9.4",
        "2024.8.13",
        "2024.6.18",
    ];

    public static string[] DevUser =
    [
        "J4sxGDREO5bjLvzvMkv059g+7wpNg7PbyWa9vLVWQkw=",
    ];

    public static PlayerControl[] AllPlayerControls => PlayerControl.AllPlayerControls.ToArray().Where(pc => pc != null).ToArray();

    public static PlayerControl[] AllAlivePlayerControls => AllPlayerControls.ToArray().Where(pc => pc.IsAlive()).ToArray();

    public static Dictionary<int, string> GetRoleName()
    {
        return new Dictionary<int, string>
        {
            { 0, Translator.GetString(StringNames.Crewmate) },
            { 1, Translator.GetString(StringNames.Impostor) },
            { 2, Translator.GetString(StringNames.ScientistRole) },
            { 3, Translator.GetString(StringNames.EngineerRole) },
            { 4, Translator.GetString(StringNames.GuardianAngelRole) },
            { 5, Translator.GetString(StringNames.ShapeshifterRole) },
            { 6, Translator.GetString(StringNames.Crewmate) },
            { 7, Translator.GetString(StringNames.Impostor) },
            { 8, Translator.GetString(StringNames.NoisemakerRole) },
            { 9, Translator.GetString(StringNames.PhantomRole) },
           { 10, Translator.GetString(StringNames.TrackerRole) }
        };
    }


    public static Dictionary<int, string> GetRoleColor => new Dictionary<int, string>
    {
        { 0, "#8cffff" },
        { 1, "#f00202" },
        { 2, "#00d9d9" },
        { 3, "#8f8f8f" },
        { 4, "#8cffff" },
        { 5, "#f06102" },
        { 6, "#8cffff" },
        { 7, "#f00202" },
        { 8, "#fc7c7c" },
        { 9, "#d100b9" },
       { 10, "#59f002" }
    };

    public static ManualLogSource Logger;
    public static DebugMenu debugmenu { get; set; } = null;

    public override void Load()
    {
        AddComponent<ExtendedPlayerInfo>();

        try
        {
            ConsoleManager.CreateConsole();
            ConsoleManager.SetConsoleTitle("Among Us - BAU Console");
            ConsoleManager.ConfigPreventClose.Value = true;
            ConsoleManager.ConsoleStream.WriteLine($".--------------------------------------------------------------------------------.\r\n|  ____       _   _                 _                                  _   _     |\r\n| | __ )  ___| |_| |_ ___ _ __     / \\   _ __ ___   ___  _ __   __ _  | | | |___ |\r\n| |  _ \\ / _ \\ __| __/ _ \\ '__|   / _ \\ | '_ ` _ \\ / _ \\| '_ \\ / _` | | | | / __||\r\n| | |_) |  __/ |_| ||  __/ |     / ___ \\| | | | | | (_) | | | | (_| | | |_| \\__ \\|\r\n| |____/ \\___|\\__|\\__\\___|_|    /_/   \\_\\_| |_| |_|\\___/|_| |_|\\__, |  \\___/|___/|\r\n|                                                              |___/             |\r\n'--------------------------------------------------------------------------------'");

            Logger = BepInEx.Logging.Logger.CreateLogSource(PluginGuid);

            BetterDataManager.SetUp();
            BetterDataManager.LoadData();
            LoadOptions();
            Translator.Init();
            Harmony.PatchAll();
            GameSettingMenuPatch.SetupSettings(true);

            if (PlatformData.Platform == Platforms.StandaloneSteamPC)
                File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "steam_appid.txt"), "945360");

            if (File.Exists(Path.Combine(BetterDataManager.filePathFolder, "better-log.txt")))
                File.WriteAllText(Path.Combine(BetterDataManager.filePathFolder, "better-previous-log.txt"), File.ReadAllText(Path.Combine(BetterDataManager.filePathFolder, "better-log.txt")));

            File.WriteAllText(Path.Combine(BetterDataManager.filePathFolder, "better-log.txt"), "");
            BetterAmongUs.Logger.Log("Better Among Us successfully loaded!");

            // Set up debug menu
            for (int i = 0; i < DevUser.Length; i++)
                DevUser[i] = Encryptor.Decrypt(DevUser[i]);

#if DEBUG
            ClassInjector.RegisterTypeInIl2Cpp<DebugMenu>();
            ClassInjector.RegisterTypeInIl2Cpp<Resources.Coroutines.Component>();
            debugmenu = AddComponent<DebugMenu>();
            AddComponent<Resources.Coroutines.Component>();
#endif

            string SupportedVersions = string.Empty;
            foreach (string text in SupportedAmongUsVersions.ToArray())
                SupportedVersions += $"{text} ";
            BetterAmongUs.Logger.Log($"BetterAmongUs {BetterAmongUsVersion}-{ReleaseDate} - [{AmongUsVersion} --> {SupportedVersions.Substring(0, SupportedVersions.Length - 1)}] {Utils.GetPlatformName(PlatformData.Platform)}");
        }
        catch (Exception ex)
        {
            BetterAmongUs.Logger.Error(ex);
        }
    }



    public static ConfigEntry<bool> AntiCheat { get; private set; }
    public static ConfigEntry<bool> BetterHost { get; private set; }
    public static ConfigEntry<bool> BetterNotifications { get; private set; }
    public static ConfigEntry<bool> ForceOwnLanguage { get; private set; }
    public static ConfigEntry<bool> ChatDarkMode { get; private set; }
    public static ConfigEntry<bool> ChatInGameplay { get; private set; }
    public static ConfigEntry<bool> LobbyPlayerInfo { get; private set; }
    public static ConfigEntry<bool> DisableLobbyTheme { get; private set; }
    public static ConfigEntry<bool> UnlockFPS { get; private set; }
    public static ConfigEntry<bool> ShowFPS { get; private set; }
    public static ConfigEntry<string> CommandPrefix { get; set; }
    private void LoadOptions()
    {
        AntiCheat = Config.Bind("Better Options", "AntiCheat", true);
        BetterHost = Config.Bind("Better Options", "BetterHost", false);
        BetterNotifications = Config.Bind("Better Options", "BetterNotifications", true);
        ForceOwnLanguage = Config.Bind("Better Options", "ForceOwnLanguage", false);
        ChatDarkMode = Config.Bind("Better Options", "ChatDarkMode", true);
        ChatInGameplay = Config.Bind("Better Options", "ChatInGameplay", true);
        LobbyPlayerInfo = Config.Bind("Better Options", "LobbyPlayerInfo", true);
        DisableLobbyTheme = Config.Bind("Better Options", "DisableLobbyTheme", true);
        UnlockFPS = Config.Bind("Better Options", "UnlockFPS", false);
        ShowFPS = Config.Bind("Better Options", "ShowFPS", false);
        CommandPrefix = Config.Bind("Client Options", "CommandPrefix", "/");
    }

    public static string GetDataPathToAmongUs() => FileIO.GetRootDataPath();
    public static string GetGamePathToAmongUs() => Environment.CurrentDirectory;
}
