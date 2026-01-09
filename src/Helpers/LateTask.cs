namespace BetterAmongUs.Helpers;

internal sealed class LateTask
{
    private readonly Action _action;
    private float _remainingTime;
    private readonly string _name;
    private readonly bool _shouldLog;

    private static readonly List<LateTask> Tasks = new();

    private LateTask(Action action, float delay, string name = "Unnamed Task", bool shouldLog = true)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
        _remainingTime = delay;
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _shouldLog = shouldLog;

        Tasks.Add(this);
    }

    private bool Update(float deltaTime)
    {
        _remainingTime -= deltaTime;

        if (_remainingTime > 0)
            return false;

        try
        {
            _action.Invoke();

            if (_shouldLog)
            {
                Logger_.Log($"{_name} has finished", nameof(LateTask));
            }

            return true;
        }
        catch (Exception ex)
        {
            Logger_.Error($"Error executing task '{_name}': {ex}");
            return true;
        }
    }

    internal static void Schedule(Action action, float delay, string name = "Unnamed Task", bool shouldLog = true)
    {
        _ = new LateTask(action, delay, name, shouldLog);
    }

    internal static void UpdateAll(float deltaTime)
    {
        if (Tasks.Count == 0)
            return;

        var completedTasks = new List<LateTask>(Tasks.Count);

        foreach (var task in Tasks.ToArray())
        {
            if (task.Update(deltaTime))
            {
                completedTasks.Add(task);
            }
        }

        foreach (var task in completedTasks)
        {
            Tasks.Remove(task);
        }
    }

    public static void CancelAll()
    {
        Tasks.Clear();
    }

    public static int ActiveTaskCount => Tasks.Count;
}