using AmongUs.Data;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BetterAmongUs.Data;
using BetterAmongUs.Helpers;
using BetterAmongUs.Items;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Modules;
using BetterAmongUs.Patches;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;

namespace BetterAmongUs;

internal enum ReleaseTypes : int
{
    Release,
    Beta,
    Dev,
}

[BepInPlugin(PluginGuid, "BetterAmongUs", PluginVersion)]
[BepInProcess("Among Us.exe")]
internal class Main : BasePlugin
{
    internal static readonly ReleaseTypes ReleaseBuildType = ReleaseTypes.Release;
    internal const string BetaNum = "0";
    internal const string HotfixNum = "0";
    internal const bool IsHotFix = false;
    internal const string PluginGuid = "com.ten.betteramongus";
    internal const string PluginVersion = "1.2.0";
    internal const string ReleaseDate = "07.09.2025"; // mm/dd/yyyy
    internal const string Github = "https://github.com/EnhancedNetwork/BetterAmongUs-Public";
    internal const string Discord = "https://discord.gg/ten";
    internal static UserData MyData = UserData.AllUsers.First();

    internal static uint ModSignature => modSignature.Value;
    private static readonly Lazy<uint> modSignature = new(() =>
    {
        string dllPath = Assembly.GetExecutingAssembly().Location;
        if (!File.Exists(dllPath))
            return 0;

        using FileStream stream = File.OpenRead(dllPath);
        using SHA256 sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(stream);
        string hashSubstring = BitConverter.ToString(hashBytes).Replace("-", "").ToLower()[..8];
        return Convert.ToUInt32(hashSubstring, 16);
    });

    internal static string GetVersionText(bool newLine = false)
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
            text += $"{newLineText}Hotfix {HotfixNum}";

        return text;
    }

    internal static Harmony Harmony { get; } = new Harmony(PluginGuid);

    internal static string BetterAmongUsVersion => PluginVersion;
    internal static string AppVersion => Application.version;
    internal static string AmongUsVersion => ReferenceDataManager.Instance.Refdata.userFacingVersion;

    internal static PlatformSpecificData PlatformData => Constants.GetPlatformData();

    internal static List<string> SupportedAmongUsVersions =
    [
        "2025.6.10",
    ];

    internal static List<PlayerControl> AllPlayerControls = [];
    internal static List<PlayerControl> AllAlivePlayerControls => AllPlayerControls.Where(pc => pc.IsAlive()).ToList();
    internal static DeadBody[] AllDeadBodys => UnityEngine.Object.FindObjectsOfType<DeadBody>().ToArray();
    internal static Vent[] AllVents => UnityEngine.Object.FindObjectsOfType<Vent>();

    internal static Dictionary<int, string> GetRoleName()
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


    internal static Dictionary<int, string> GetRoleColor => new Dictionary<int, string>
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

    internal static ManualLogSource? Logger;

    public override void Load()
    {
        try
        {
            foreach (var listener in BepInEx.Logging.Logger.Listeners)
            {
                if (listener.GetType().Name.ToLower().Contains("Unity"))
                {
                    BepInEx.Logging.Logger.Listeners.Remove(listener);
                    break;
                }
            }

            ConsoleManager.CreateConsole();
            ConsoleManager.ConfigPreventClose.Value = true;
            if (ConsoleManager.ConfigConsoleEnabled.Value) ConsoleManager.DetachConsole();
            ConsoleManager.ConfigConsoleEnabled.Value = false;
            ConsoleManager.SetConsoleTitle("Among Us - BAU Console");
            Logger = BepInEx.Logging.Logger.CreateLogSource(PluginGuid);
            var customLogListener = new CustomLogListener();
            BepInEx.Logging.Logger.Listeners.Add(customLogListener);
            ConsoleManager.SetConsoleColor(ConsoleColor.Green);
            ConsoleManager.ConsoleStream.WriteLine($".--------------------------------------------------------------------------------.\r\n|  ____       _   _                 _                                  _   _     |\r\n| | __ )  ___| |_| |_ ___ _ __     / \\   _ __ ___   ___  _ __   __ _  | | | |___ |\r\n| |  _ \\ / _ \\ __| __/ _ \\ '__|   / _ \\ | '_ ` _ \\ / _ \\| '_ \\ / _` | | | | / __||\r\n| | |_) |  __/ |_| ||  __/ |     / ___ \\| | | | | | (_) | | | | (_| | | |_| \\__ \\|\r\n| |____/ \\___|\\__|\\__\\___|_|    /_/   \\_\\_| |_| |_|\\___/|_| |_|\\__, |  \\___/|___/|\r\n|                                                              |___/             |\r\n'--------------------------------------------------------------------------------'");

            {
                RegisterAllMonoBehavioursInAssembly();
                // AddComponent<UserDataLoader>();
            }

            BetterDataManager.Init();
            LoadOptions();
            Translator.Init();
            Harmony.PatchAll();
            GameSettingMenuPatch.SetupSettings(true);
            FileChecker.Initialize();
            InstanceAttribute.RegisterAll();
            AddActions();

            if (PlatformData.Platform == Platforms.StandaloneSteamPC)
                File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "steam_appid.txt"), "945360");

            if (File.Exists(Path.Combine(BetterDataManager.filePathFolder, "better-log.txt")))
                File.WriteAllText(Path.Combine(BetterDataManager.filePathFolder, "better-previous-log.txt"), File.ReadAllText(Path.Combine(BetterDataManager.filePathFolder, "better-log.txt")));

            File.WriteAllText(Path.Combine(BetterDataManager.filePathFolder, "better-log.txt"), "");
            BetterAmongUs.Logger.Log("Better Among Us successfully loaded!");

            string SupportedVersions = string.Empty;
            foreach (string text in SupportedAmongUsVersions.ToArray())
                SupportedVersions += $"{text} ";
            BetterAmongUs.Logger.Log($"BetterAmongUs {BetterAmongUsVersion}-{ReleaseDate} - [{AppVersion} --> {SupportedVersions.Substring(0, SupportedVersions.Length - 1)}] {Utils.GetPlatformName(PlatformData.Platform)}");
        }
        catch (Exception ex)
        {
            BetterAmongUs.Logger.Error(ex);
        }
    }

    private static void AddActions()
    {
        DataManager.Settings.Gameplay.OnStreamerModeChanged += (Action)(() =>
        {
            if (GameState.IsInGame)
            {
                Utils.DirtyAllNames();
            }
        });
    }

    private static void RegisterAllMonoBehavioursInAssembly()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var monoBehaviourTypes = assembly.GetTypes()
            .Where(type => type.IsSubclassOf(typeof(MonoBehaviour)) && !type.IsAbstract)
            .OrderBy(type => type.Name);

        foreach (var type in monoBehaviourTypes)
        {
            try
            {
                ClassInjector.RegisterTypeInIl2Cpp(type);
            }
            catch (Exception ex)
            {
                BetterAmongUs.Logger.Error($"Failed to register MonoBehaviour: {type.FullName}\n{ex}");
            }
        }
    }

    internal static ConfigEntry<bool>? PrivateOnlyLobby { get; private set; }
    internal static ConfigEntry<bool>? AntiCheat { get; private set; }
    internal static ConfigEntry<bool>? SendBetterRpc { get; private set; }
    internal static ConfigEntry<bool>? BetterNotifications { get; private set; }
    internal static ConfigEntry<bool>? ForceOwnLanguage { get; private set; }
    internal static ConfigEntry<bool>? ChatDarkMode { get; private set; }
    internal static ConfigEntry<bool>? ChatInGameplay { get; private set; }
    internal static ConfigEntry<bool>? LobbyPlayerInfo { get; private set; }
    internal static ConfigEntry<bool>? DisableLobbyTheme { get; private set; }
    internal static ConfigEntry<bool>? UnlockFPS { get; private set; }
    internal static ConfigEntry<bool>? ShowFPS { get; private set; }
    internal static ConfigEntry<string>? CommandPrefix { get; set; }
    private void LoadOptions()
    {
        PrivateOnlyLobby = Config.Bind("Mod", "PrivateOnlyLobby", false);
        AntiCheat = Config.Bind("Better Options", "AntiCheat", true);
        SendBetterRpc = Config.Bind("Better Options", "SendBetterRpc", true);
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

    internal static string GetDataPathToAmongUs() => Application.persistentDataPath;
    internal static string GetGamePathToAmongUs() => Path.GetDirectoryName(Application.dataPath) ?? Application.dataPath;
}
