using System.Collections.Generic;
using System.Text.Json;

namespace BetterAmongUs;

class BetterDataManager
{
    public static string GetFilePath(string name)
    {
        return Path.Combine(Main.GetDataPathToAmongUs(), $"{name}.json");
    }

    public static void SetUp()
    {
        string filePath = GetFilePath("BetterData");

        if (!File.Exists(filePath))
        {
            // Initialize with predefined categories
            var initialData = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>
        {
            { "Data", new Dictionary<string, Dictionary<string, string>>() }, // Default category
            { "cheatData", new Dictionary<string, Dictionary<string, string>>() },
            { "sickoData", new Dictionary<string, Dictionary<string, string>>() },
            { "aumData", new Dictionary<string, Dictionary<string, string>>() }
        };

            string json = JsonSerializer.Serialize(initialData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }
        else
        {
            // Load the existing JSON data
            string json = File.ReadAllText(filePath);
            var jsonData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(json) ?? new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

            // Check and add missing categories
            if (!jsonData.ContainsKey("Data"))
            {
                jsonData["Data"] = new Dictionary<string, Dictionary<string, string>>();
            }
            if (!jsonData.ContainsKey("cheatData"))
            {
                jsonData["cheatData"] = new Dictionary<string, Dictionary<string, string>>();
            }
            if (!jsonData.ContainsKey("sickoData"))
            {
                jsonData["sickoData"] = new Dictionary<string, Dictionary<string, string>>();
            }
            if (!jsonData.ContainsKey("aumData"))
            {
                jsonData["aumData"] = new Dictionary<string, Dictionary<string, string>>();
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
            Logger.Error(ex.ToString());
        }

        return string.Empty;
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
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToString());
        }
    }
}
