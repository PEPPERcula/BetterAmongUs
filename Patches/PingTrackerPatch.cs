using HarmonyLib;
using System.Text;
using UnityEngine;

namespace BetterAmongUs.Patches;

[HarmonyPatch(typeof(PingTracker))]
public class PingTrackerPatch
{
    [HarmonyPatch(nameof(PingTracker.Update))]
    [HarmonyPrefix]
    public static bool Prefix(PingTracker __instance)
    {
        try
        {
            PlayerControl Host = AmongUsClient.Instance.GetHost().Character;

            if (GameStates.IsFreePlay)
            {
                __instance.gameObject.SetActive(false);
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("PING: <b>{0}</b>\n", GetPingColor(AmongUsClient.Instance.Ping));
            sb.Append($"<color=#00dbdb><size=75%>BetterAmongUs {Main.GetVersionText(true)}</size></color>\n");
            sb.AppendFormat("<color=#b0b0b0><size=50%>{0}</size></color>\n", Main.Github);

            if (Main.BetterHost.Value && GameStates.IsHost)
            {
                sb.Append("<size=75%><color=#4f92ff>Better Host</color>: <color=#00f04c>Enabled</color></size>\n");
            }

            if (GameStates.IsModdedProtocol)
            {
                sb.Append("<size=75%><color=#4f92ff>Modded Protocol</color>: <color=#00f04c>Enabled</color></size>\n");
            }

            if (Main.ShowFPS.Value)
            {
                float FPSNum = 1.0f / Time.deltaTime;
                sb.AppendFormat("<color=#0dff00><size=75%>FPS: <b>{0}</b></size></color>\n", (int)FPSNum);
            }

            // Add Host Info if not in lobby
            if (GameStates.IsInGamePlay && Host != null)
            {
                string hostColor = Utils.Color32ToHex(Palette.PlayerColors[Host.CurrentOutfit.ColorId]);
                sb.AppendFormat("<size=75%>Host: <color={0}>{1}</color></size>\n", hostColor, Host.CurrentOutfit.PlayerName);
            }

            __instance.aspectPosition.DistanceFromEdge = new Vector3(4f, 0.1f, -5);
            __instance.aspectPosition.Alignment = AspectPosition.EdgeAlignments.RightTop;
            __instance.text.outlineWidth = 0.3f;
            __instance.text.text = sb.ToString();
        }
        catch { }

        return false;
    }

    private static string GetPingColor(int ping)
    {
        string color;
        switch (ping)
        {
            case int n when n > 250:
                color = "#ff0000";
                break;
            case int n when n > 100:
                color = "#ffa200";
                break;
            case int n when n > 50:
                color = "#ffff00";
                break;
            default:
                color = "#00f04c";
                break;
        }

        string newPing = $"<color={color}>{ping} ms</color>";

        return newPing;
    }
}
