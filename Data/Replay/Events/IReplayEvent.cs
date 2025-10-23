using System.Text.Json.Serialization;

namespace BetterAmongUs.Data.Replay.Events;

public interface IReplayEvent
{
    [JsonIgnore]
    string Id { get; }
    void Play();
}

public interface IReplayEvent<T> : IReplayEvent
{
    T? EventData { get; set; }
}
