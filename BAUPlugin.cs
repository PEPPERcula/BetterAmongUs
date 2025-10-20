using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BetterAmongUs.Data;
using BetterAmongUs.Helpers;
using BetterAmongUs.Items;
using BetterAmongUs.Items.Attributes;
using BetterAmongUs.Modules;
using BetterAmongUs.Network;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
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

[BepInPlugin(ModInfo.PluginGuid, ModInfo.PluginName, ModInfo.PluginVersion)]
[BepInProcess("Among Us.exe")]
internal class BAUPlugin : BasePlugin
{
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

        switch (ModInfo.ReleaseBuildType)
        {
            case ReleaseTypes.Release:
                text = $"v{BetterAmongUsVersion}";
                break;
            case ReleaseTypes.Beta:
                text = $"v{BetterAmongUsVersion}{newLineText}Beta {ModInfo.BetaNum}";
                break;
            case ReleaseTypes.Dev:
                text = $"v{BetterAmongUsVersion}{newLineText}Dev {ModInfo.ReleaseDate}";
                break;
            default:
                break;
        }

        if (ModInfo.IsHotFix)
            text += $"{newLineText}Hotfix {ModInfo.HotfixNum}";

        return text;
    }

    internal static Harmony Harmony { get; } = new Harmony(ModInfo.PluginGuid);

    internal static string BetterAmongUsVersion => ModInfo.PluginVersion;
    internal static string AppVersion => Application.version;
    internal static string AmongUsVersion => ReferenceDataManager.Instance.Refdata.userFacingVersion;

    internal static PlatformSpecificData PlatformData => Constants.GetPlatformData();

    internal static List<string> SupportedAmongUsVersions =
    [
        "2025.10.14",
    ];

    internal static List<PlayerControl> AllPlayerControls = [];
    internal static List<PlayerControl> AllAlivePlayerControls => AllPlayerControls.Where(pc => pc.IsAlive()).ToList();
    internal static DeadBody[] AllDeadBodys => UnityEngine.Object.FindObjectsOfType<DeadBody>().ToArray();
    internal static Vent[] AllVents => UnityEngine.Object.FindObjectsOfType<Vent>();

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
            Logger = BepInEx.Logging.Logger.CreateLogSource(ModInfo.PluginGuid);
            var customLogListener = new CustomLogListener();
            BepInEx.Logging.Logger.Listeners.Add(customLogListener);
            ConsoleManager.SetConsoleColor(ConsoleColor.Green);
            ConsoleManager.ConsoleStream.WriteLine($".--------------------------------------------------------------------------------.\r\n|  ____       _   _                 _                                  _   _     |\r\n| | __ )  ___| |_| |_ ___ _ __     / \\   _ __ ___   ___  _ __   __ _  | | | |___ |\r\n| |  _ \\ / _ \\ __| __/ _ \\ '__|   / _ \\ | '_ ` _ \\ / _ \\| '_ \\ / _` | | | | / __||\r\n| | |_) |  __/ |_| ||  __/ |     / ___ \\| | | | | | (_) | | | | (_| | | |_| \\__ \\|\r\n| |____/ \\___|\\__|\\__\\___|_|    /_/   \\_\\_| |_| |_|\\___/|_| |_|\\__, |  \\___/|___/|\r\n|                                                              |___/             |\r\n'--------------------------------------------------------------------------------'");

            {
                RegisterAllMonoBehavioursInAssembly();
                // AddComponent<UserDataLoader>();
            }

            GithubAPI.Connect();
            BetterDataManager.Init();
            LoadOptions();
            Translator.Init();
            Harmony.PatchAll();
            GameSettingMenuPatch.SetupSettings(true);
            FileChecker.Initialize();
            InstanceAttribute.RegisterAll();

            if (File.Exists(Path.Combine(BetterDataManager.filePathFolder, "better-log.txt")))
                File.WriteAllText(Path.Combine(BetterDataManager.filePathFolder, "better-previous-log.txt"), File.ReadAllText(Path.Combine(BetterDataManager.filePathFolder, "better-log.txt")));

            File.WriteAllText(Path.Combine(BetterDataManager.filePathFolder, "better-log.txt"), "");
            BetterAmongUs.Logger.Log("Better Among Us successfully loaded!");

            string SupportedVersions = string.Empty;
            foreach (string text in SupportedAmongUsVersions.ToArray())
                SupportedVersions += $"{text} ";
            BetterAmongUs.Logger.Log($"BetterAmongUs {BetterAmongUsVersion}-{ModInfo.ReleaseDate} - [{AppVersion} --> {SupportedVersions.Substring(0, SupportedVersions.Length - 1)}] {Utils.GetPlatformName(PlatformData.Platform)}");
        }
        catch (Exception ex)
        {
            BetterAmongUs.Logger.Error(ex);
        }
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
