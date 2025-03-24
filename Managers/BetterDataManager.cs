using BetterAmongUs.Helpers;
using BetterAmongUs.Modules.AntiCheat;
using System.Text.Json;

namespace BetterAmongUs.Managers;

class BetterDataManager
{
    private static string filePath = GetFilePath("BetterData");
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

    internal static void SetUp()
    {
        if (!Directory.Exists(filePathFolder))
        {
            Directory.CreateDirectory(filePathFolder);
        }

        if (!Directory.Exists(filePathFolderSettings))
        {
            Directory.CreateDirectory(filePathFolderSettings);
        }

        if (!Directory.Exists(filePathFolderSaveInfo))
        {
            Directory.CreateDirectory(filePathFolderSaveInfo);
        }

        if (!File.Exists(banPlayerListFile))
        {
            File.WriteAllText(banPlayerListFile, "// Example\nFriendCode#0000\nHashPUID\n// Or\nFriendCode#0000, HashPUID");
        }

        if (!File.Exists(banNameListFile))
        {
            File.WriteAllText(banNameListFile, "// Example\nBanName1\nBanName2");
        }

        if (!File.Exists(banWordListFile))
        {
            File.WriteAllText(banWordListFile, "// Example\nStart");
        }

        if (!File.Exists(SettingsFile))
        {
            var initialData = new Dictionary<string, string>();
            string json = JsonSerializer.Serialize(initialData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFile, json);
        }

        if (!File.Exists(filePath))
        {
            // Initialize with predefined categories
            var initialData = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>
            {
                { "Data", new Dictionary<string, Dictionary<string, string>>() }, // Default category
                { "cheatData", new Dictionary<string, Dictionary<string, string>>() },
                { "sickoData", new Dictionary<string, Dictionary<string, string>>() },
                { "aumData", new Dictionary<string, Dictionary<string, string>>() },
                { "knData", new Dictionary<string, Dictionary<string, string>>() }
            };

            string json = JsonSerializer.Serialize(initialData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }
        else
        {
            // Load the existing JSON data
            string json = File.ReadAllText(filePath);

            try
            {
                using (JsonDocument.Parse(json))
                {
                }
            }
            catch (JsonException)
            {
                // JSON is invalid, reformat by writing an empty JSON object.
                json = "{}";

            }

            var jsonData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(json) ?? new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

            // Check and add missing categories
            if (!jsonData.ContainsKey("Data"))
            {
                jsonData["Data"] = [];
            }
            if (!jsonData.ContainsKey("cheatData"))
            {
                jsonData["cheatData"] = [];
            }
            if (!jsonData.ContainsKey("sickoData"))
            {
                jsonData["sickoData"] = [];
            }
            if (!jsonData.ContainsKey("aumData"))
            {
                jsonData["aumData"] = [];
            }
            if (!jsonData.ContainsKey("knData"))
            {
                jsonData["knData"] = [];
            }

            // Write the updated JSON data back to the file
            json = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }
    }

    internal static void Save(string name, string dataToSave, string category = "Data")
    {
        string filePath = GetFilePath("BetterData");

        string json = File.ReadAllText(filePath);
        var jsonData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json) ?? new Dictionary<string, Dictionary<string, string>>();

        if (!jsonData.ContainsKey(category))
        {
            jsonData[category] = new Dictionary<string, string>();
        }

        jsonData[category][name] = dataToSave;

        json = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    internal static string Load(string name, string category = "Data")
    {
        try
        {
            string filePath = GetFilePath("BetterData");

            string json = File.ReadAllText(filePath);
            var jsonData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);

            if (jsonData != null && jsonData.ContainsKey(category) && jsonData[category].ContainsKey(name))
            {
                return jsonData[category][name];
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }

        return string.Empty;
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

    internal static void SaveBanList(string friendCode = "", string hashPUID = "")
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
                newText += Utils.GetHashStr(hashPUID);
            }

            // Check if the file already contains the new entry
            if (!File.Exists(banPlayerListFile) || !File.ReadLines(banPlayerListFile).Any(line => line.Equals(newText)))
            {
                // Append the new string to the file if it's not already present
                File.AppendAllText(banPlayerListFile, Environment.NewLine + newText);
            }
        }
    }

    internal static void SaveCheatData(string puid, string friendCode, string name, string category = "cheatData", string reason = "None")
    {
        string filePath = GetFilePath("BetterData");

        string json = File.ReadAllText(filePath);
        var jsonData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(json) ?? new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

        if (!jsonData.ContainsKey(category))
        {
            jsonData[category] = new Dictionary<string, Dictionary<string, string>>();
        }

        if (!jsonData[category].ContainsKey(name))
        {
            jsonData[category][name] = new Dictionary<string, string>();
        }

        jsonData[category][name]["FriendCode"] = friendCode;
        jsonData[category][name]["HashPUID"] = puid;
        jsonData[category][name]["Reason"] = reason;

        json = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    internal static bool RemovePlayer(string identifier)
    {
        bool successful = false;
        string filePath = GetFilePath("BetterData");
        string json = File.ReadAllText(filePath);
        var jsonData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(json)
                       ?? new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

        if (jsonData != null)
        {
            if (jsonData.ContainsKey("cheatData"))
            {
                RemoveIdentifierFromSection(jsonData["cheatData"], identifier, ref successful);
            }

            if (jsonData.ContainsKey("sickoData"))
            {
                RemoveIdentifierFromSection(jsonData["sickoData"], identifier, ref successful);
            }

            if (jsonData.ContainsKey("aumData"))
            {
                RemoveIdentifierFromSection(jsonData["aumData"], identifier, ref successful);
            }

            if (jsonData.ContainsKey("knData"))
            {
                RemoveIdentifierFromSection(jsonData["knData"], identifier, ref successful);
            }

            json = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        return successful;
    }

    private static void RemoveIdentifierFromSection(Dictionary<string, Dictionary<string, string>> sectionData, string identifier, ref bool successful)
    {
        string keyToRemove = null;

        foreach (var entry in sectionData)
        {
            if (entry.Key == identifier || entry.Value.ContainsValue(identifier))
            {
                keyToRemove = entry.Key;
                break;
            }
        }

        if (keyToRemove != null)
        {
            sectionData.Remove(keyToRemove);
            successful = true;
        }

        if (successful)
        {
            foreach (var data in BAUAntiCheat.PlayerData)
            {
                if (data.Value != identifier)
                {
                    BAUAntiCheat.PlayerData.Remove(identifier);
                }
                else
                {
                    BAUAntiCheat.PlayerData.Remove(data.Key);
                }
            }

            foreach (var data in BAUAntiCheat.SickoData)
            {
                if (data.Value != identifier)
                {
                    BAUAntiCheat.SickoData.Remove(identifier);
                }
                else
                {
                    BAUAntiCheat.SickoData.Remove(data.Key);
                }
            }

            foreach (var data in BAUAntiCheat.AUMData)
            {
                if (data.Value != identifier)
                {
                    BAUAntiCheat.AUMData.Remove(identifier);
                }
                else
                {
                    BAUAntiCheat.AUMData.Remove(data.Key);
                }
            }

            foreach (var data in BAUAntiCheat.KNData)
            {
                if (data.Value != identifier)
                {
                    BAUAntiCheat.KNData.Remove(identifier);
                }
                else
                {
                    BAUAntiCheat.KNData.Remove(data.Key);
                }
            }
        }
    }

    internal static void ClearCheatData()
    {
        string filePath = GetFilePath("BetterData");
        string json = File.ReadAllText(filePath);
        var jsonData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(json)
                       ?? new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

        // Clear in-save data
        if (jsonData != null)
        {
            if (jsonData.ContainsKey("cheatData"))
            {
                jsonData["cheatData"].Clear();
            }

            if (jsonData.ContainsKey("sickoData"))
            {
                jsonData["sickoData"].Clear();
            }

            if (jsonData.ContainsKey("aumData"))
            {
                jsonData["aumData"].Clear();
            }

            if (jsonData.ContainsKey("knData"))
            {
                jsonData["knData"].Clear();
            }

            json = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        // Clear in-memory data
        BAUAntiCheat.PlayerData.Clear();
        BAUAntiCheat.SickoData.Clear();
        BAUAntiCheat.AUMData.Clear();
        BAUAntiCheat.KNData.Clear();

        Logger.LogCheat("Cleared cheat memory and data");
    }

    internal static void LoadData()
    {
        LoadCheatData();
    }

    internal static void LoadCheatData()
    {
        try
        {
            string filePath = GetFilePath("BetterData");
            string json = File.ReadAllText(filePath);
            var jsonData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(json);

            if (jsonData != null)
            {
                if (jsonData.ContainsKey("cheatData"))
                {
                    foreach (var item in jsonData["cheatData"])
                    {
                        BAUAntiCheat.PlayerData[item.Value["HashPUID"]] = item.Value["FriendCode"];
                    }
                }

                if (jsonData.ContainsKey("sickoData"))
                {
                    foreach (var item in jsonData["sickoData"])
                    {
                        BAUAntiCheat.SickoData[item.Value["HashPUID"]] = item.Value["FriendCode"];
                    }
                }

                if (jsonData.ContainsKey("aumData"))
                {
                    foreach (var item in jsonData["aumData"])
                    {
                        BAUAntiCheat.AUMData[item.Value["HashPUID"]] = item.Value["FriendCode"];
                    }
                }

                if (jsonData.ContainsKey("knData"))
                {
                    foreach (var item in jsonData["knData"])
                    {
                        BAUAntiCheat.KNData[item.Value["HashPUID"]] = item.Value["FriendCode"];
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }
}
