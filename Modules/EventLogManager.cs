using HarmonyLib;

namespace BetterAmongUs;

class EventLogManager
{
    [HarmonyPatch(typeof(PlayerControl))]
    class PlayerControlPatch
    {
        [HarmonyPatch(nameof(PlayerControl.MurderPlayer))]
        [HarmonyPostfix]
        public static void MurderPlayer__Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target) =>
            Logger.LogPrivate($"{__instance.Data.PlayerName} Has killed {target.Data.PlayerName} as {Utils.GetRoleName(__instance.Data.RoleType)}", "EventLog");
        [HarmonyPatch(nameof(PlayerControl.Shapeshift))]
        [HarmonyPostfix]
        public static void Shapeshift_Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target, [HarmonyArgument(1)] bool animate)
        {
            if (__instance != target)
                Logger.LogPrivate($"{__instance.Data.PlayerName} Has Shapeshifted into {target.Data.PlayerName}, did animate: {animate}", "EventLog");
            else
                Logger.LogPrivate($"{__instance.Data.PlayerName} Has Un-Shapeshifted, did animate: {animate}", "EventLog");
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics))]
    class PlayerPhysicsPatch
    {
        [HarmonyPatch(nameof(PlayerPhysics.BootFromVent))]
        [HarmonyPostfix]
        public static void BootFromVent_Postfix(PlayerPhysics __instance, [HarmonyArgument(0)] int ventId) =>
            Logger.LogPrivate($"{__instance.myPlayer.Data.PlayerName} Has been booted from vent: {ventId}, as {Utils.GetRoleName(__instance.myPlayer.Data.RoleType)}", "EventLog");
        [HarmonyPatch(nameof(PlayerPhysics.CoEnterVent))]
        [HarmonyPostfix]
        public static void CoEnterVent_Postfix(PlayerPhysics __instance, [HarmonyArgument(0)] int ventId) =>
            Logger.LogPrivate($"{__instance.myPlayer.Data.PlayerName} Has entered vent: {ventId}, as {Utils.GetRoleName(__instance.myPlayer.Data.RoleType)}", "EventLog");
        [HarmonyPatch(nameof(PlayerPhysics.CoExitVent))]
        [HarmonyPostfix]
        public static void CoExitVent_Postfix(PlayerPhysics __instance, [HarmonyArgument(0)] int ventId) =>
            Logger.LogPrivate($"{__instance.myPlayer.Data.PlayerName} Has exit vent: {ventId}, as {Utils.GetRoleName(__instance.myPlayer.Data.RoleType)}", "EventLog");
    }
}
