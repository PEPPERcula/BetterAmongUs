using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay.UI;

[HarmonyPatch]
internal static class ServerDropdownPatch
{
    [HarmonyPatch(typeof(FindAGameManager))]
    [HarmonyPatch(nameof(FindAGameManager.Start))]
    [HarmonyPrefix]
    private static void Start_Prefix(FindAGameManager __instance)
    {
        var aspectPosition = __instance.serverDropdown.transform.parent.GetComponent<AspectPosition>();
        if (aspectPosition != null)
        {
            aspectPosition.Alignment = AspectPosition.EdgeAlignments.Top;
            aspectPosition.anchorPoint = Vector3.zero;
            aspectPosition.DistanceFromEdge = new Vector3(-1.2f, 0.3f, 0f);
            aspectPosition.AdjustPosition();
        }

        __instance.modeText.transform.localPosition -= new Vector3(0.4f, 0f, 0f);
    }

    [HarmonyPatch(typeof(ServerDropdown))]
    [HarmonyPatch(nameof(ServerDropdown.FillServerOptions))]
    [HarmonyPrefix]
    private static bool FillServerOptions_Prefix(ServerDropdown __instance)
    {
        SpriteRenderer background = __instance.background;
        background.size = new Vector2(4, 1);
        ServerManager serverManager = ServerManager.Instance;
        TranslationController translationController = TranslationController.Instance;

        // Get all available regions except current one
        var regions = serverManager.AvailableRegions.ToList();
        IRegionInfo currentRegion = serverManager.CurrentRegion;
        var displayRegions = regions.Where(r => r.Name != currentRegion.Name).ToList();

        // Calculate total columns needed
        int totalColumns = Mathf.Max(1, Mathf.CeilToInt(displayRegions.Count / 5f));
        int rowLimit = Mathf.Min(displayRegions.Count, 5);

        __instance.defaultButtonSelected = __instance.firstOption;
        __instance.firstOption.ChangeButtonText(translationController.GetStringWithDefault(currentRegion.TranslateName, currentRegion.Name, new Il2CppReferenceArray<Il2CppSystem.Object>(0)));

        for (var index = 0; index < displayRegions.Count; index++)
        {
            IRegionInfo ri = displayRegions[index];
            var buttonPool = __instance.ButtonPool.Get<ServerListButton>();

            // Calculate position based on column and row
            buttonPool.transform.localPosition = new Vector3(((index / 5) - ((totalColumns - 1) / 2f)) * 3.15f, __instance.y_posButton - (0.5f * (index % 5)), -1f);
            buttonPool.Text.text = translationController.GetStringWithDefault(ri.TranslateName, ri.Name, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
            buttonPool.Text.ForceMeshUpdate();
            buttonPool.Button.OnClick.RemoveAllListeners();
            buttonPool.Button.OnClick.AddListener((Action)(() => __instance.ChooseOption(ri)));
            __instance.controllerSelectable.Add(buttonPool.Button);
        }

        // Calculate background dimensions
        float height = 1.2f + (0.5f * (rowLimit - 1));
        float width = totalColumns > 1 ? (3.15f * (totalColumns - 1)) + background.size.x : background.size.x;
        background.transform.localPosition = new Vector3(0f, __instance.initialYPos - ((height - 1.2f) / 2f), 0f);
        background.size = new Vector2(width, height);

        return false;
    }
}
