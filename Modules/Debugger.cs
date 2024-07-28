
namespace BetterAmongUs;

class Logger
{
    public static void Log(string info, string tag = "Log")
    {
        string mark = $"{DateTime.Now:HH:mm} [BetterLog][{tag}]";
        string logFilePath = Path.Combine(Environment.CurrentDirectory, "better-log.txt");
        string newLine = $"{mark}: {info}";
        File.AppendAllText(logFilePath, newLine + Environment.NewLine);
        Main.Logger.LogInfo($"[{tag}] {info}");
    }
    public static void LogHeader(string info, string tag = "Log") => Log($"   <-------------- {info} -------------->", tag);
    public static void LogCheat(string info, string tag = "AntiCheat") => Log(info, tag);
    public static void Error(string info, string tag = "Error") => Log(info, tag);
    public static void Test() => Log("------------------> TEST <------------------", "TEST");
    // Log in game join msg
    public static void InGame(string info)
    {
        if (DestroyableSingleton<HudManager>._instance) DestroyableSingleton<HudManager>.Instance.Notifier.AddDisconnectMessage(info);
        Log(info, "InGame");
    }

    // ------------- Private Log -------------
    // Logs that can only be accessed when dumped

    public static void LogPrivate(string info, string tag = "Log")
    {
        if (GameStates.IsDev)
        {
            Log(info, tag);
            return;
        }

        string mark = $"{DateTime.Now:HH:mm} [BetterLog][PrivateLog][{tag}]";
        string logFilePath = Path.Combine(Environment.CurrentDirectory, "better-log.txt");
        string newLine = $"{mark}: " + Encryptor.Encrypt($"{info}");
        File.AppendAllText(logFilePath, newLine + Environment.NewLine);
    }
}

