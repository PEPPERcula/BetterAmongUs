namespace BetterAmongUs.Managers;

internal class CustomLoadingBarManager
{
    internal static AmongUsLoadingBar? LoadingBar => DestroyableSingleton<LoadingBarManager>.Instance?.loadingBar;

    internal static void ToggleLoadingBar(bool on)
    {
        DestroyableSingleton<LoadingBarManager>.Instance.loadingBar.gameObject.SetActive(on);
    }

    internal static void SetLoadingPercent(float percent, string loadText)
    {
        var loadingBar = DestroyableSingleton<LoadingBarManager>.Instance.loadingBar;
        loadingBar.SetLoadingPercent(percent, StringNames.None);
        loadingBar.loadingText.SetText(loadText);
    }
}
