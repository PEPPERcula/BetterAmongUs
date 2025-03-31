using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Data;

internal sealed class BetterGameSettingsFile : AbstractJsonFile
{
    internal override string FilePath => BetterDataManager.SettingsFile;

    protected override bool Load()
    {
        var success = base.Load();
        if (success)
        {
            foreach (var kvp in Settings.ToArray())
            {
                if (kvp.Value is JsonElement jsonElement)
                {
                    try
                    {
                        Settings[kvp.Key] = jsonElement.ValueKind switch
                        {
                            JsonValueKind.Number when jsonElement.TryGetInt32(out int intValue) => intValue,
                            JsonValueKind.Number when jsonElement.TryGetSingle(out float floatValue) => floatValue,
                            JsonValueKind.Number when jsonElement.TryGetByte(out byte byteValue) => byteValue,
                            JsonValueKind.True => true,
                            JsonValueKind.False => false,
                            JsonValueKind.String => jsonElement.GetString(),
                            _ => throw new NotSupportedException($"Unsupported JSON type: {jsonElement.ValueKind}")
                        };
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to convert JSON element for key {kvp.Key}: {ex.Message}");
                    }
                }
            }
        }
        return success;
    }

    protected override void WriteToFile(string json)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(json);
            var settingsDict = jsonDoc.RootElement.GetProperty(nameof(Settings));
            var sb = new StringBuilder();
            foreach (var kvp in settingsDict.EnumerateObject())
            {
                if (sb.Length > 0) sb.Append('|');
                sb.Append(kvp.Name).Append(',').Append(kvp.Value);
            }
            byte[] flattenedData = Encoding.UTF8.GetBytes(sb.ToString());
            using var ms = new MemoryStream();
            using (var gzip = new GZipStream(ms, CompressionMode.Compress))
            {
                gzip.Write(flattenedData, 0, flattenedData.Length);
            }
            File.WriteAllText(FilePath, Convert.ToBase64String(ms.ToArray()));
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }

    protected override string ReadFromFile()
    {
        byte[] compressedBytes = Convert.FromBase64String(File.ReadAllText(FilePath));
        using var ms = new MemoryStream(compressedBytes);
        using var gzip = new GZipStream(ms, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        gzip.CopyTo(resultStream);
        string flattenedData = Encoding.UTF8.GetString(resultStream.ToArray());
        var settingsDict = new Dictionary<string, string>();
        foreach (var pair in flattenedData.Split('|', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split(',', 2);
            if (parts.Length == 2)
                settingsDict[parts[0]] = parts[1];
        }
        return $"{{\"Settings\":{JsonSerializer.Serialize(settingsDict)}}}";
    }

    [JsonPropertyName(nameof(Settings))]
    public Dictionary<int, object?> Settings { get; set; } = [];
}