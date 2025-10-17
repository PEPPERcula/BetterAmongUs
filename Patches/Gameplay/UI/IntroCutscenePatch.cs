using BetterAmongUs.Helpers;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay.UI;

[HarmonyPatch]
internal class IntroCutscenePatch
{
    [HarmonyPatch(typeof(IntroCutscene._ShowRole_d__41), nameof(IntroCutscene._ShowRole_d__41.MoveNext))]
    [HarmonyPostfix]
    private static void ShowRole_MoveNext_Postfix(IntroCutscene._ShowRole_d__41 __instance)
    {
        try
        {
            var introCutscene = __instance.__4__this;
            Color RoleColor = Utils.HexToColor32(PlayerControl.LocalPlayer.Data.RoleType.GetRoleHex());
            introCutscene.ImpostorText.gameObject.SetActive(false);
            introCutscene.TeamTitle.gameObject.SetActive(false);
            introCutscene.BackgroundBar.material.color = RoleColor;
            introCutscene.BackgroundBar.transform.SetLocalZ(-15);
            introCutscene.transform.Find("BackgroundLayer").transform.SetLocalZ(-16);
            introCutscene.YouAreText.color = RoleColor;
            introCutscene.RoleText.color = RoleColor;
            introCutscene.RoleBlurbText.color = RoleColor;
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }
}