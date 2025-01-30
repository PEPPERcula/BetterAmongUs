using System.Collections.ObjectModel;
using System.Reflection;

namespace BetterAmongUs.Modules;

class FileChecker
{
    private static bool enabled = false;
    private static FileSystemWatcher? fileWatcher;
    private static bool hasUnauthorizedFileOrMod = false;
    public static string WarningMsg { get; private set; } = string.Empty;
    public static bool HasShownWarning { get; set; } = false;
    public static bool HasUnauthorizedFileOrMod => hasUnauthorizedFileOrMod && (!Main.MyData.IsDev() || !Main.MyData.IsVerified());

    private static readonly ReadOnlyCollection<string> TrustedNamespaces = new(new List<string>
    {
        "System", "Unity", "Harmony", "BepInEx", "Microsoft", "Il2Cpp", "Hazel",
        "MonoMod", "netstandard", "mscorlib", "AssetRipper", "Cpp2IL", "AsmResolver", "Iced",
        "SemanticVersioning", "Mono.Cecil", "Assembly-CSharp", "StableNameDotNet",
        "Disarm", "Gee.External.Capstone", "Rewired_Core", "AddressablesPlayAssetDelivery",
        "Assembly-CSharp-firstpass", "Sentry", "BetterAmongUs", "MCI", "CrowdedMod", "Mini.RegionInstall", "Unlock", "Skin"
    });

    private static readonly ReadOnlyCollection<string> UntrustedNamespaces = new(new List<string>
    {
        "Sicko", "Malum", "Menu", "Cheat", "Hack", "Exploit", "Bypass", "Crack", "Spoof"
    });

    private static readonly string[] TrustedPaths =
    {
        Environment.CurrentDirectory,
        Path.Combine(Environment.CurrentDirectory, "BepInEx"),
    };

    private static readonly string[] UnauthorizedFiles = [
        "version.dll",
        "sicko-settings.json",
        "sicko-log.txt",
        "sicko-prev-log.txt",
        "sicko-config",
        "settings.json",
        "aum-log.txt",
        "aum-prev-log.txt"
    ];

    public static void Initialize()
    {
        if (enabled) return;
        enabled = true;

        fileWatcher = new FileSystemWatcher(Environment.CurrentDirectory)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName,
            IncludeSubdirectories = true
        };

        fileWatcher.Created += (sender, args) => CheckUnauthorizedFiles();
        fileWatcher.Deleted += (sender, args) => CheckUnauthorizedFiles();

        fileWatcher.EnableRaisingEvents = true;

        CheckUnauthorizedFiles();
        CheckUnauthorizedMods();
    }

    private static void CheckUnauthorizedFiles()
    {
        if (hasUnauthorizedFileOrMod) return;

        foreach (var fileName in UnauthorizedFiles)
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, fileName)))
            {
                WarningMsg = "<#D20200>Unauthorized File Detected</color>\n<#9D9D9D><size=70%>Look in logs for further information!</size></color>";
                Logger.Warning($"Unauthorized File: {Path.Combine(Environment.CurrentDirectory, fileName)}");
                if (GameState.IsInGame)
                {
                    SceneChanger.ChangeScene("MainMenu");
                }
                hasUnauthorizedFileOrMod = true;
                return;
            }
        }
    }

    private static void CheckUnauthorizedMods()
    {
        if (hasUnauthorizedFileOrMod) return;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic)
            {
                continue;
            }

            if (!TrustedNamespaces.Any(ns => assembly.FullName.Contains(ns, StringComparison.OrdinalIgnoreCase)))
            {
                WarningMsg = "<#D20200>Unregistered Assembly Detected</color>\n<#9D9D9D><size=70%>Look in logs for further information!</size></color>";
                Logger.Warning($"Unauthorized Assembly: {assembly.FullName} (Unregistered Namespace)");
                hasUnauthorizedFileOrMod = true;
                return;
            }

            if (UntrustedNamespaces.Any(ns => assembly.FullName.Contains(ns, StringComparison.OrdinalIgnoreCase)))
            {
                WarningMsg = "<#D20200>Untrusted Assembly Detected</color>\n<#9D9D9D><size=70%>Look in logs for further information!</size></color>";
                Logger.Warning($"Unauthorized Assembly: {assembly.FullName} (Untrusted Namespace)");
                hasUnauthorizedFileOrMod = true;
                return;
            }

            if (!IsSafeLocation(assembly))
            {
                WarningMsg = "<#D20200>Unregistered Assembly Location Detected</color>\n<#9D9D9D><size=70%>Look in logs for further information!</size></color>";
                Logger.Warning($"Unauthorized Assembly: {assembly.FullName} (Unregistered Location: {assembly.Location})");
                hasUnauthorizedFileOrMod = true;
                return;
            }
        }
    }

    private static bool IsSafeLocation(Assembly assembly)
    {
        if (string.IsNullOrEmpty(assembly.Location))
        {
            return true;
        }

        return TrustedPaths.Any(path => assembly.Location.StartsWith(path, StringComparison.OrdinalIgnoreCase));
    }
}
