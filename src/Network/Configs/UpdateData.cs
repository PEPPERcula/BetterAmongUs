using BetterAmongUs.Helpers;
using System.Collections;
using System.Reflection;
using System.Text.Json.Serialization;
using UnityEngine;

namespace BetterAmongUs.Network.Configs;

[Serializable]
internal sealed class UpdateData
{
    [JsonPropertyName("dllLink")]
    public string DllLink { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("releaseType")]
    public int ReleaseType { get; set; }

    [JsonPropertyName("isHotfix")]
    public bool IsHotfix { get; set; }

    [JsonPropertyName("hotfixNumber")]
    public int HotfixNumber { get; set; }

    [JsonPropertyName("betaNumber")]
    public int BetaNumber { get; set; }

    internal bool IsNewUpdate()
    {
        try
        {
            var updateVersion = new Version(Version);
            var modVersion = new Version(ModInfo.PLUGIN_VERSION);

            // 1. Compare main version (major.minor.build)
            if (updateVersion > modVersion)
            {
                return true;
            }
            else if (updateVersion < modVersion)
            {
                return false;
            }

            // 2. Versions are equal, compare release types
            var updateReleaseType = (ReleaseTypes)ReleaseType;
            var currentReleaseType = ModInfo.ReleaseBuildType;

            // Release is always preferred over Beta
            if (updateReleaseType == ReleaseTypes.Release && currentReleaseType == ReleaseTypes.Beta)
                return true;

            // Don't downgrade from Release to Beta
            if (updateReleaseType == ReleaseTypes.Beta && currentReleaseType == ReleaseTypes.Release)
                return false;

            // 3. Same version and release type, compare specific numbers
            if (updateReleaseType == ReleaseTypes.Beta)
            {
                return BetaNumber > int.Parse(ModInfo.BETA_NUM);
            }

            // Release version - check hotfixes
            if (IsHotfix && !ModInfo.IS_HOTFIX)
                return true;

            if (IsHotfix && ModInfo.IS_HOTFIX)
                return HotfixNumber > int.Parse(ModInfo.HOTFIX_NUM);

            // Same version, same type, no newer hotfix
            return false;
        }
        catch (Exception ex)
        {
            Logger_.Error($"Update check failed: {ex.Message}");
            return false;
        }
    }

    internal IEnumerator CoDownload()
    {
        int count = 0;
        float delay = 0;
        while (!GithubAPI.IsInternetAvailable())
        {
            count++;
            if (count >= 17)
            {
                yield break;
            }
            if (delay < 30f) delay += 2.5f;
            yield return new WaitForSeconds(delay);
        }

        object waiting = true;
        var dllPath = Assembly.GetExecutingAssembly().Location;
        yield return GitHubFile.CoDownloadFile(DllLink, dllPath + ".temp", path =>
        {
            File.Move(dllPath, dllPath + ".old");
            File.Move(path, dllPath);
            waiting = false;
        }, true);

        while (waiting is true)
        {
            yield return null;
        }
        yield break;
    }

    public override string ToString()
    {
        return $"{Version}:{ReleaseType}:{BetaNumber}:{HotfixNumber}";
    }
}