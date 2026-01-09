namespace BetterAmongUs.Managers;

internal static class CustomLoadingBarManager
{
    internal static AmongUsLoadingBar? LoadingBar => LoadingBarManager.Instance?.loadingBar;

    internal static void ToggleLoadingBar(bool on)
    {
        LoadingBarManager.Instance.loadingBar.gameObject.SetActive(on);
    }

    internal static void SetLoadingPercent(float percent, string loadText)
    {
        var loadingBar = LoadingBarManager.Instance.loadingBar;
        loadingBar.SetLoadingPercent(percent, StringNames.None);
        loadingBar.loadingText.SetText(loadText);
    }
}
