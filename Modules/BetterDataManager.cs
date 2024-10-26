using System.Text.Json;

namespace BetterAmongUs;

class BetterDataManager
{
    private static string filePath = GetFilePath("BetterData");
    public static string filePathFolder = Path.Combine(Main.GetGamePathToAmongUs(), $"Better_Data");
    public static string filePathFolderSaveInfo = Path.Combine(filePathFolder, $"SaveInfo");
    public static string filePathFolderSettings = Path.Combine(filePathFolder, $"Settings");
    public static string SettingsFile = Path.Combine(filePathFolderSettings, "Preset.json");
    public static string banPlayerListFile = Path.Combine(filePathFolderSaveInfo, "BanPlayerList.txt");
    public static string banNameListFile = Path.Combine(filePathFolderSaveInfo, "BanNameList.txt");
    public static string banWordListFile = Path.Combine(filePathFolderSaveInfo, "BanWordList.txt");

    public static string GetFilePath(string name)
    {
        return Path.Combine(Main.GetDataPathToAmongUs(), $"{name}.json");
    }

    public static void SetUp()
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

    public static void Save(string name, string dataToSave, string category = "Data")
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

    public static string Load(string name, string category = "Data")
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

    public static void SaveSetting(int id, string input)
    {
        string filePath = SettingsFile;

        string json = File.ReadAllText(filePath);
        var jsonData = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        jsonData[id.ToString()] = input;

        json = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    public static bool CanLoadSetting(int id)
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

    public static bool LoadBoolSetting(int id, bool Default = false)
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

    public static float LoadFloatSetting(int id, float Default = 0f)
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

    public static int LoadIntSetting(int id, int Default = 0)
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

    public static void SaveBanList(string friendCode = "", string hashPUID = "")
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
                newText += Utils.GetHashPuid(hashPUID);
            }

            // Check if the file already contains the new entry
            if (!File.Exists(banPlayerListFile) || !File.ReadLines(banPlayerListFile).Any(line => line.Equals(newText)))
            {
                // Append the new string to the file if it's not already present
                File.AppendAllText(banPlayerListFile, Environment.NewLine + newText);
            }
        }
    }

    public static void SaveCheatData(string puid, string friendCode, string name, string category = "cheatData", string reason = "None")
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

    public static bool RemovePlayer(string identifier)
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
            foreach (var data in AntiCheat.PlayerData)
            {
                if (data.Value != identifier)
                {
                    AntiCheat.PlayerData.Remove(identifier);
                }
                else
                {
                    AntiCheat.PlayerData.Remove(data.Key);
                }
            }

            foreach (var data in AntiCheat.SickoData)
            {
                if (data.Value != identifier)
                {
                    AntiCheat.SickoData.Remove(identifier);
                }
                else
                {
                    AntiCheat.SickoData.Remove(data.Key);
                }
            }

            foreach (var data in AntiCheat.AUMData)
            {
                if (data.Value != identifier)
                {
                    AntiCheat.AUMData.Remove(identifier);
                }
                else
                {
                    AntiCheat.AUMData.Remove(data.Key);
                }
            }
        }
    }

    public static void ClearCheatData()
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

            json = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        // Clear in-memory data
        AntiCheat.PlayerData.Clear();
        AntiCheat.SickoData.Clear();
        AntiCheat.AUMData.Clear();

        Logger.LogCheat("Cleared cheat memory and data");
    }

    public static void LoadData()
    {
        LoadCheatData();
    }

    public static void LoadCheatData()
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
                        AntiCheat.PlayerData[item.Value["HashPUID"]] = item.Value["FriendCode"];
                    }
                }

                if (jsonData.ContainsKey("sickoData"))
                {
                    foreach (var item in jsonData["sickoData"])
                    {
                        AntiCheat.SickoData[item.Value["HashPUID"]] = item.Value["FriendCode"];
                    }
                }

                if (jsonData.ContainsKey("aumData"))
                {
                    foreach (var item in jsonData["aumData"])
                    {
                        AntiCheat.AUMData[item.Value["HashPUID"]] = item.Value["FriendCode"];
                    }
                }

                if (jsonData.ContainsKey("knData"))
                {
                    foreach (var item in jsonData["knData"])
                    {
                        AntiCheat.KNData[item.Value["HashPUID"]] = item.Value["FriendCode"];
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
