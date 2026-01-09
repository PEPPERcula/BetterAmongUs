using BetterAmongUs.Data.Replay.Events;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Data.Replay;

public class ReplayEventConverter : JsonConverter<IReplayEvent>
{
    private static readonly Dictionary<string, Type> _eventTypes;

    static ReplayEventConverter()
    {
        _eventTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReplayEvent<>)))
            .ToDictionary(t =>
            {
                var instance = (IReplayEvent)Activator.CreateInstance(t)!;
                return instance.Id;
            });
    }

    public override IReplayEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        string id = root.GetProperty("Id").GetString()!;

        if (_eventTypes.TryGetValue(id, out Type? eventType))
        {
            return (IReplayEvent)JsonSerializer.Deserialize(
                root.GetRawText(),
                eventType,
                options
            )!;
        }

        throw new JsonException($"Unknown event type: {id}");
    }

    public override void Write(Utf8JsonWriter writer, IReplayEvent value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (object)value, options);
    }
}
