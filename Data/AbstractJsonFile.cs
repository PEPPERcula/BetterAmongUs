using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Data;

internal abstract class AbstractJsonFile<T> where T : AbstractJsonFile<T>
{
    internal abstract string FilePath { get; }
    private bool _hasInit;

    protected virtual JsonSerializerOptions SerializerOptions { get; } = new()
    {
        WriteIndented = true,
        IncludeFields = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        PropertyNameCaseInsensitive = true,
    };

    internal virtual void Init()
    {
        if (_hasInit) return;
        _hasInit = true;

        if (!CheckFile())
        {
            Save();
            return;
        }

        Load();
        Save();
    }

    protected virtual bool Load()
    {
        try
        {
            var content = File.ReadAllText(FilePath);
            if (string.IsNullOrWhiteSpace(content))
            {
                Logger.Error("File is empty");
                return false;
            }

            var data = JsonSerializer.Deserialize<T>(content, SerializerOptions);
            if (data == null)
            {
                Logger.Error("Deserialization returned null");
                return false;
            }

            foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;
                if (property.CanWrite)
                {
                    var value = property.GetValue(data);
                    property.SetValue(this, value);
                }
            }

            return true;
        }
        catch (JsonException ex)
        {
            Logger.Error($"JSON parsing error: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Logger.Error($"Unexpected error: {ex.Message}");
            return false;
        }
    }

    internal virtual bool Save()
    {
        try
        {
            var json = JsonSerializer.Serialize((T)this, SerializerOptions);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            return false;
        }

        return true;
    }

    private bool CheckFile()
    {
        var directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!File.Exists(FilePath))
        {
            return false;
        }

        var content = File.ReadAllText(FilePath);
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(content);
        var json = JsonSerializer.Deserialize<T>(content);

        var fileInfo = new FileInfo(FilePath);
        if (fileInfo.Length == 0)
        {
            return false;
        }

        try
        {
            if (jsonElement.ValueKind == JsonValueKind.Object && !jsonElement.EnumerateObject().Any() ||
                jsonElement.ValueKind == JsonValueKind.Array && !jsonElement.EnumerateArray().Any())
            {
                return false;
            }
        }
        catch (JsonException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }
}
