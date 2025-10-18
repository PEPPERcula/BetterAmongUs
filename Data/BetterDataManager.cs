using BetterAmongUs.Helpers;

namespace BetterAmongUs.Data;

class BetterDataManager
{
    internal static BetterDataFile BetterDataFile = new();
    internal static BetterGameSettingsFile BetterGameSettingsFile = new();

    internal static string dataPathOLD = GetFilePath("BetterData");
    internal static string dataPath = GetFilePath("BetterDataV2");
    internal static string filePathFolder = Path.Combine(BAUPlugin.GetGamePathToAmongUs(), $"Better_Data");
    internal static string filePathFolderSaveInfo = Path.Combine(filePathFolder, $"SaveInfo");
    internal static string filePathFolderSettings = Path.Combine(filePathFolder, $"Settings");
    internal static string filePathFolderReplays = Path.Combine(filePathFolder, $"Replays");
    internal static string SettingsFileOld = Path.Combine(filePathFolderSettings, "Preset.json");
    internal static string SettingsFile = Path.Combine(filePathFolderSettings, "Settings.dat");
    internal static string banPlayerListFile = Path.Combine(filePathFolderSaveInfo, "BanPlayerList.txt");
    internal static string banNameListFile = Path.Combine(filePathFolderSaveInfo, "BanNameList.txt");
    internal static string banWordListFile = Path.Combine(filePathFolderSaveInfo, "BanWordList.txt");

    private static string[] Paths =>
    [
        banPlayerListFile,
        banNameListFile,
        banWordListFile
    ];

    internal static string GetFilePath(string name)
    {
        return Path.Combine(BAUPlugin.GetDataPathToAmongUs(), $"{name}.json");
    }

    internal static void Init()
    {
        BetterDataFile.Init();
        BetterGameSettingsFile.Init();

        foreach (var path in Paths)
        {
            if (!File.Exists(path))
            {
                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.CreateText(path).Close();
            }
        }
    }

    internal static void SaveSetting(int id, object? input)
    {
        BetterGameSettingsFile.Settings[id] = input;
        BetterGameSettingsFile.Save();
    }

    internal static bool CanLoadSetting<T>(int id)
    {
        if (BetterGameSettingsFile.Settings.TryGetValue(id, out var value))
        {
            if (value is T)
            {
                return true;
            }
        }

        return false;
    }

    internal static T? LoadSetting<T>(int id, T? Default = default)
    {
        if (BetterGameSettingsFile.Settings.TryGetValue(id, out var value))
        {
            if (value is T castValue)
            {
                return castValue;
            }
        }

        SaveSetting(id, Default);
        return Default;
    }

    internal static void AddToBanList(string friendCode = "", string hashPUID = "")
    {
        if (!string.IsNullOrEmpty(friendCode) || !string.IsNullOrEmpty(hashPUID))
        {
            // Create the new string with the separator if both are not empty
            string newText = string.Empty;

            if (!string.IsNullOrEmpty(friendCode))
            {
                newText = friendCode;
            }

            if (!string.IsNullOrEmpty(hashPUID))
            {
                if (!string.IsNullOrEmpty(newText))
                {
                    newText += ", ";
                }
                newText += hashPUID.GetHashStr();
            }

            // Check if the file already contains the new entry
            if (!File.Exists(banPlayerListFile) || !File.ReadLines(banPlayerListFile).Any(line => line.Equals(newText)))
            {
                // Append the new string to the file if it's not already present
                File.AppendAllText(banPlayerListFile, Environment.NewLine + newText);
            }
        }
    }

    internal static bool RemovePlayer(string identifier)
    {
        identifier = identifier.Replace(' ', '_');
        bool didFind = false;

        foreach (var info in BetterDataFile.CheatData.ToArray())
        {
            if (info.PlayerName.Replace(' ', '_') == identifier || info.HashPuid == identifier || info.FriendCode == identifier)
            {
                BetterDataFile.CheatData.Remove(info);
                didFind = true;
            }
        }
        foreach (var info in BetterDataFile.SickoData.ToArray())
        {
            if (info.PlayerName.Replace(' ', '_') == identifier || info.HashPuid == identifier || info.FriendCode == identifier)
            {
                BetterDataFile.SickoData.Remove(info);
                didFind = true;
            }
        }
        foreach (var info in BetterDataFile.AUMData.ToArray())
        {
            if (info.PlayerName.Replace(' ', '_') == identifier || info.HashPuid == identifier || info.FriendCode == identifier)
            {
                BetterDataFile.AUMData.Remove(info);
                didFind = true;
            }
        }
        foreach (var info in BetterDataFile.KNData.ToArray())
        {
            if (info.PlayerName.Replace(' ', '_') == identifier || info.HashPuid == identifier || info.FriendCode == identifier)
            {
                BetterDataFile.KNData.Remove(info);
                didFind = true;
            }
        }

        if (didFind)
        {
            BetterDataFile.Save();
        }

        return didFind;
    }

    internal static void ClearCheatData()
    {
        BetterDataFile.CheatData.Clear();
        BetterDataFile.SickoData.Clear();
        BetterDataFile.AUMData.Clear();
        BetterDataFile.KNData.Clear();
        BetterDataFile.Save();
    }
}
