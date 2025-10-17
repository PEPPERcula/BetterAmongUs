using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules.AntiCheat;
using BetterAmongUs.Patches.Client;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches.Managers;

[HarmonyPatch(typeof(ModManager))]
internal class ModManagerPatch
{
    [HarmonyPatch(nameof(ModManager.LateUpdate))]
    [HarmonyPostfix]
    internal static void LateUpdate_Postfix(ModManager __instance)
    {
        if (SplashIntroPatch.IsReallyDoneLoading)
        {
            __instance.ShowModStamp();
        }

        if (__instance.ModStamp.gameObject.active == true)
            __instance.ModStamp.GetComponent<SpriteRenderer>().sprite = Utils.LoadSprite("BetterAmongUs.Resources.Images.BetterAmongUs-Mod.png", 250f);

        BetterAntiCheat.Update();
        LateTask.Update(Time.deltaTime);
        BetterNotificationManager.Update();
    }
}
