using System.Text.Json;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Data;

internal sealed class BetterGameSettingsFile : AbstractJsonFile<BetterGameSettingsFile>
{
    [JsonIgnore]
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

    [JsonPropertyName("Settings")]
    public Dictionary<int, object?> Settings { get; set; } = [];
}