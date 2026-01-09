using System.Reflection;

namespace BetterAmongUs;

internal static class ModInfo
{
    internal static readonly ReleaseTypes ReleaseBuildType = ReleaseTypes.Release;
    internal const string BetaNum = "0";
    internal const string HotfixNum = "0";
    internal const bool IsHotFix = false;
    internal const string PluginName = "BetterAmongUs";
    internal const string PluginGuid = "com.d1gq.betteramongus";
    internal const string PluginVersion = "1.3.1";
    internal const string Github = "https://github.com/D1GQ/BetterAmongUs";
    internal const string Discord = "https://discord.gg/vjYrXpzNAn";
    public static string CommitHash => GetAssemblyMetadata("CommitHash");
    public static string BuildDate => GetAssemblyMetadata("BuildDate");

    private static string GetAssemblyMetadata(string key)
    {
        var attribute = Assembly.GetExecutingAssembly()
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == key);

        return attribute?.Value ?? "unknown";
    }
}