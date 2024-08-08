using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches;

[HarmonyPatch(typeof(ModManager))]
public class ModManagerPatch
{
    [HarmonyPatch(nameof(ModManager.LateUpdate))]
    [HarmonyPostfix]
    public static void LateUpdate_Postfix(ModManager __instance)
    {
        __instance.ShowModStamp();

        if (__instance.ModStamp.gameObject.active == true)
            __instance.ModStamp.GetComponent<SpriteRenderer>().sprite = Utils.LoadSprite("BetterAmongUs.Resources.Images.BetterAmongUs-Mod.png", 250f);

        AntiCheat.Update();
        FileChecker.UpdateUnauthorizedFiles();
        LateTask.Update(Time.deltaTime);
        BetterNotificationManager.Update();
    }
}
