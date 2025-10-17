namespace BetterAmongUs.Data.Replay.Events;

public interface IReplayEvent
{
    string Id { get; }
    void Play();
}

public interface IReplayEvent<T> : IReplayEvent
{
    T? EventData { get; set; }
}
