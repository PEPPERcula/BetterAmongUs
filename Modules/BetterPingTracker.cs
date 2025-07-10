using BetterAmongUs.Helpers;
using System.Text;
using TMPro;
using UnityEngine;
using static BetterAmongUs.Patches.GamePlayManager;

namespace BetterAmongUs.Modules;

internal class BetterPingTracker : MonoBehaviour
{
    internal static BetterPingTracker? Instance { get; private set; }
    private AspectPosition? aspectPosition;
    private TextMeshPro? text;
    internal void SetUp(TextMeshPro pingText, AspectPosition pingAspectPosition)
    {
        if (Instance != null) return;
        if (pingText == null || pingAspectPosition == null)
        {
            Logger.Error("BetterPingTracker.SetUp() called with null parameters!");
            return;
        }

        Instance = this;
        text = pingText;
        aspectPosition = pingAspectPosition;
    }

    private void Update()
    {
        if (aspectPosition == null || text == null) return;

        // Update position and appearance
        aspectPosition.DistanceFromEdge = new Vector3(4f, 0.1f, -5);
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.RightTop;
        text.outlineWidth = 0.3f;

        StringBuilder sb = new();

        // Check AmongUsClient.Instance
        if (AmongUsClient.Instance != null && !GameState.IsFreePlay)
        {
            sb.AppendFormat("{0}: <b>{1}</b>\n", Translator.GetString("Ping").ToUpper(), GetPingColor(AmongUsClient.Instance.Ping));
        }

        if (GameState.IsLobby && GameState.IsHost && GameState.IsVanillaServer && !GameState.IsLocalGame)
        {
            sb.AppendFormat("{0}: <b>{1}</b>\n", Translator.GetString("Timer").ToUpper(), GetTimeColor(GameStartManagerPatch.lobbyTimer));
        }

        sb.Append($"<color=#00dbdb><size=75%>BetterAmongUs {Main.GetVersionText(true)}</size></color>\n");
        sb.Append("<size=68%><color=#8040bf>By</color> <color=#bc4345>The Enhanced Network</color></size>\n");

        if (GameState.IsTOHEHostLobby) sb.Append($"<size=75%><color=#e197dc>TOHE Lobby</color></size>\n");

        if (Main.ShowFPS.Value)
        {
            float FPSNum = 1.0f / Time.deltaTime;
            sb.AppendFormat("<color=#0dff00><size=75%>FPS: <b>{0}</b></size></color>\n", (int)FPSNum);
        }

        // Add Host Info if not in lobby
        if (GameState.IsInGamePlay && !GameState.IsFreePlay && AmongUsClient.Instance != null)
        {
            var hostInfo = AmongUsClient.Instance.GetHost();
            if (hostInfo?.Character != null)
            {
                sb.AppendFormat("<size=75%>{0}: {1}</size>\n", Translator.GetString("Host"), hostInfo.Character.GetPlayerNameAndColor());
            }
        }

        text?.SetText(sb.ToString());
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
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

    private static string GetTimeColor(float time)
    {
        float minutes = (int)time / 60 + 1f;

        string color;
        switch (minutes)
        {
            case float n when n <= 1:
                color = "#ff0000";
                break;
            case float n when n <= 3.5f:
                color = "#ffa200";
                break;
            case float n when n <= 5f:
                color = "#ffff00";
                break;
            default:
                color = "#00f04c";
                break;
        }

        string newTime = $"<color={color}>{GameStartManagerPatch.lobbyTimerDisplay}</color>";

        return newTime;
    }
}