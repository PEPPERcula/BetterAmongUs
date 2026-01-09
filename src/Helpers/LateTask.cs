namespace BetterAmongUs.Helpers;

class LateTask
{
    internal string name;
    internal float timer;
    internal bool shouldLog;
    internal Action action;
    internal static List<LateTask> Tasks = [];
    internal bool Run(float deltaTime)
    {
        timer -= deltaTime;
        if (timer <= 0)
        {
            action();

            if (shouldLog == true)
                Logger.Log($"{name} has finished", "LateTask");
            return true;
        }
        return false;
    }
    public LateTask(Action action, float time, string name = "No Name Task", bool shouldLog = true)
    {
        this.action = action;
        timer = time;
        this.name = name;
        this.shouldLog = shouldLog;
        Tasks.Add(this);
    }

    internal static void Update(float deltaTime)
    {
        var TasksToRemove = new List<LateTask>();
        foreach (var task in Tasks.ToArray())
        {
            try
            {
                if (task.Run(deltaTime))
                {
                    TasksToRemove.Add(task);
                }
            }
            catch (Exception ex)
            {
                TasksToRemove.Add(task);
                Logger.Error(ex);
            }
        }
        TasksToRemove.ForEach(task => Tasks.Remove(task));
    }
}