using BetterAmongUs.Helpers;
using System.Text;
using TMPro;
using UnityEngine;
using static BetterAmongUs.Patches.Managers.GamePlayManager;

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
            string pingColor = Utils.Color32ToHex(Utils.LerpColor([Color.green, Color.yellow, new Color(1f, 0.5f, 0f), Color.red], (25, 250), AmongUsClient.Instance.Ping));
            sb.AppendFormat("{0}: <b>{1}</b>\n", Translator.GetString("Ping").ToUpper(), $"<{pingColor}>{AmongUsClient.Instance.Ping}</color>");
        }

        if (GameState.IsLobby && GameState.IsHost && GameState.IsVanillaServer && !GameState.IsLocalGame)
        {
            string timeColor = Utils.Color32ToHex(Utils.LerpColor([Color.green, Color.yellow, new Color(1f, 0.5f, 0f), Color.red], (0, 300), GameStartManagerPatch.lobbyTimer, true));
            sb.AppendFormat("{0}: <b>{1}</b>\n", Translator.GetString("Timer").ToUpper(), $"<{timeColor}>{GameStartManagerPatch.lobbyTimerDisplay}</color>");
        }

        sb.Append($"<color=#00dbdb><size=75%>BetterAmongUs {BAUPlugin.GetVersionText(true)}</size></color>\n");
        sb.Append("<size=68%><color=#8040bf>By</color> <color=#bc4345>The Enhanced Network</color></size>\n");

        if (GameState.IsTOHEHostLobby) sb.Append($"<size=75%><color=#e197dc>TOHE Lobby</color></size>\n");

        if (BAUPlugin.ShowFPS.Value)
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
}