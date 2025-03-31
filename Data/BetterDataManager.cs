using BetterAmongUs.Helpers;
using System.Text.Json;

namespace BetterAmongUs.Data;

class BetterDataManager
{
    public static BetterDataFile BetterDataFile = new();

    private static string filePathOLD = GetFilePath("BetterData");
    internal static string filePathFolder = Path.Combine(Main.GetGamePathToAmongUs(), $"Better_Data");
    internal static string filePathFolderSaveInfo = Path.Combine(filePathFolder, $"SaveInfo");
    internal static string filePathFolderSettings = Path.Combine(filePathFolder, $"Settings");
    internal static string SettingsFile = Path.Combine(filePathFolderSettings, "Preset.json");
    internal static string banPlayerListFile = Path.Combine(filePathFolderSaveInfo, "BanPlayerList.txt");
    internal static string banNameListFile = Path.Combine(filePathFolderSaveInfo, "BanNameList.txt");
    internal static string banWordListFile = Path.Combine(filePathFolderSaveInfo, "BanWordList.txt");

    internal static string GetFilePath(string name)
    {
        return Path.Combine(Main.GetDataPathToAmongUs(), $"{name}.json");
    }

    internal static void Init()
    {
        BetterDataFile.Init();
    }

    internal static void SaveSetting(int id, string input)
    {
        string filePath = SettingsFile;

        string json = File.ReadAllText(filePath);
        var jsonData = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        jsonData[id.ToString()] = input;

        json = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    internal static bool CanLoadSetting(int id)
    {
        string filePath = SettingsFile;
        string json = File.ReadAllText(filePath);
        var jsonData = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();

        if (jsonData.ContainsKey(id.ToString()))
        {
            if (!string.IsNullOrEmpty(jsonData[id.ToString()]))
            {
                return true;
            }
        }

        return false;
    }

    internal static bool LoadBoolSetting(int id, bool Default = false)
    {
        string filePath = SettingsFile;
        string json = File.ReadAllText(filePath);
        var jsonData = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();

        try
        {
            if (jsonData.ContainsKey(id.ToString()))
            {
                return bool.Parse(jsonData[id.ToString()]);
            }
        }
        catch
        {
            SaveSetting(id, Default.ToString());
        }

        return Default;
    }

    internal static float LoadFloatSetting(int id, float Default = 0f)
    {
        string filePath = SettingsFile;
        string json = File.ReadAllText(filePath);
        var jsonData = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();

        try
        {
            if (jsonData.ContainsKey(id.ToString()))
            {
                return float.Parse(jsonData[id.ToString()]);
            }
        }
        catch
        {
            SaveSetting(id, Default.ToString());
        }

        return Default;
    }

    internal static int LoadIntSetting(int id, int Default = 0)
    {
        string filePath = SettingsFile;
        string json = File.ReadAllText(filePath);
        var jsonData = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();

        try
        {
            if (jsonData.ContainsKey(id.ToString()))
            {
                return int.Parse(jsonData[id.ToString()]);
            }
        }
        catch
        {
            SaveSetting(id, Default.ToString());
        }

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
