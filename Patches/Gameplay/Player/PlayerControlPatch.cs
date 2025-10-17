using BetterAmongUs.Helpers;
using HarmonyLib;

namespace BetterAmongUs.Patches.Gameplay.Player;

[HarmonyPatch(typeof(PlayerControl))]
class PlayerControlPatch
{
    [HarmonyPatch(nameof(PlayerControl.Start))]
    [HarmonyPostfix]
    internal static void Start_Postfix(PlayerControl __instance)
    {
        Main.AllPlayerControls.Add(__instance);
    }

    [HarmonyPatch(nameof(PlayerControl.OnDestroy))]
    [HarmonyPostfix]
    internal static void OnDestroy_Postfix(PlayerControl __instance)
    {
        Main.AllPlayerControls.Remove(__instance);
    }

    [HarmonyPatch(nameof(PlayerControl.MurderPlayer))]
    [HarmonyPostfix]
    internal static void MurderPlayer_Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        if (target == null) return;

        Logger.LogPrivate($"{__instance.Data.PlayerName} Has killed {target.Data.PlayerName} as {__instance.Data.RoleType.GetRoleName()}", "EventLog");

        __instance.BetterData().RoleInfo.Kills += 1;
    }
    [HarmonyPatch(nameof(PlayerControl.Shapeshift))]
    [HarmonyPostfix]
    internal static void Shapeshift_Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target, [HarmonyArgument(1)] bool animate)
    {
        if (target == null) return;

        if (__instance != target)
            Logger.LogPrivate($"{__instance.Data.PlayerName} Has Shapeshifted into {target.Data.PlayerName}, did animate: {animate}", "EventLog");
        else
            Logger.LogPrivate($"{__instance.Data.PlayerName} Has Un-Shapeshifted, did animate: {animate}", "EventLog");
    }
    [HarmonyPatch(nameof(PlayerControl.SetRoleInvisibility))]
    [HarmonyPostfix]
    internal static void SetRoleInvisibility_Postfix(PlayerControl __instance, [HarmonyArgument(0)] bool isActive, [HarmonyArgument(1)] bool animate)
    {

        if (isActive)
            Logger.LogPrivate($"{__instance.Data.PlayerName} Has Vanished as Phantom, did animate: {animate}", "EventLog");
        else
            Logger.LogPrivate($"{__instance.Data.PlayerName} Has Appeared as Phantom, did animate: {animate}", "EventLog");
    }
}

[HarmonyPatch(typeof(PlayerPhysics))]
internal class PlayerPhysicsPatch
{
    [HarmonyPatch(nameof(PlayerPhysics.BootFromVent))]
    [HarmonyPostfix]
    private static void BootFromVent_Postfix(PlayerPhysics __instance, [HarmonyArgument(0)] int ventId)
    {

        Logger.LogPrivate($"{__instance.myPlayer.Data.PlayerName} Has been booted from vent: {ventId}, as {__instance.myPlayer.Data.RoleType.GetRoleName()}", "EventLog");
    }
    [HarmonyPatch(nameof(PlayerPhysics.CoEnterVent))]
    [HarmonyPostfix]
    private static void CoEnterVent_Postfix(PlayerPhysics __instance, [HarmonyArgument(0)] int ventId)
    {

        Logger.LogPrivate($"{__instance.myPlayer.Data.PlayerName} Has entered vent: {ventId}, as {__instance.myPlayer.Data.RoleType.GetRoleName()}", "EventLog");
    }
    [HarmonyPatch(nameof(PlayerPhysics.CoExitVent))]
    [HarmonyPostfix]
    private static void CoExitVent_Postfix(PlayerPhysics __instance, [HarmonyArgument(0)] int ventId)
    {

        Logger.LogPrivate($"{__instance.myPlayer.Data.PlayerName} Has exit vent: {ventId}, as {__instance.myPlayer.Data.RoleType.GetRoleName()}", "EventLog");
    }
}
