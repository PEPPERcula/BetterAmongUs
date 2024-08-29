using BetterAmongUs.Resources;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace BetterAmongUs;

public class DebugMenu : MonoBehaviour
{
    public static bool RevealRoles = false;
#if DEBUG

    [HideFromIl2Cpp]
    public DragWindow Window { get; }
    public bool WindowEnabled { get; set; } = true;

    private List<string> tabs = new List<string>();
    private List<Action> tabActions = new List<Action>();
    private Dictionary<string, List<(bool isText, string content, Action action, bool startNewRow, bool centerText, string colorHex)>>
        tabContents
        = new Dictionary<string, List<(bool isText, string content, Action action, bool startNewRow, bool centerText, string colorHex)>>();

    private string selectedTab = null;

    public void Start()
    {
        InitializeMenu();
    }

    private void InitializeMenu()
    {
        string Main = "Main";
        string HostOnly = "Host Only";
        string Misc = "Misc";
        AddTab(Main);
        AddTab(HostOnly);
        AddTab(Misc);

        AddButtonToTab(Main, "Reveal All Roles", () =>
        {
            RevealRoles = !RevealRoles;
        });

        AddButtonToTab(Main, "Trigger Anti-Cheat", () =>
        {
            if (GameStates.IsInGame || GameStates.IsLobby || GameStates.IsFreePlay)
            {
                BetterNotificationManager.NotifyCheat(PlayerControl.LocalPlayer, "Debug Test", kickPlayer: false);
            }
        }, true);

        AddButtonToTab(Main, "Clear Anti-Cheat Data", () =>
        {
            BetterDataManager.ClearCheatData();
        });

        AddButtonToTab(Main, "Send Better RPC", () =>
        {
            RPC.SendBetterCheck();
        }, true);

        AddButtonToTab(HostOnly, "Sync All Names", () =>
        {
            RPC.SyncAllNames();
        });

        // Automatically select the first tab
        if (tabs.Count > 0)
        {
            selectedTab = tabs[0];
        }

        WindowEnabled = false;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            Toggle();

        if (!GameStates.IsInGamePlay)
            RevealRoles = false;
    }

    public DebugMenu(IntPtr ptr) : base(ptr)
    {
        Window = new DragWindow(new Rect(20, 20, 400, 300), "Debug Menu", () =>
        {
            GUILayout.BeginVertical();
            try
            {
                GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 18,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };

                GUILayout.Label("Debug Menu - Press F1 To Hide", headerStyle);
                GUILayout.Space(10);

                if (tabs.Count > 0)
                {
                    // Display tabs horizontally
                    GUILayout.BeginHorizontal();
                    foreach (var tab in tabs)
                    {
                        // Highlight the selected tab with color change
                        GUI.backgroundColor = tab == selectedTab ? new Color(0.2f, 0.2f, 0.2f) : GUI.backgroundColor;
                        if (GUILayout.Button(tab))
                        {
                            selectedTab = tab;
                        }
                        // Reset the background color after drawing the button
                        GUI.backgroundColor = Color.white;
                    }
                    GUILayout.EndHorizontal();

                    // Display content of selected tab
                    if (!string.IsNullOrEmpty(selectedTab) && tabContents.ContainsKey(selectedTab))
                    {
                        var contents = tabContents[selectedTab];

                        // Display contents for selected tab
                        GUILayout.BeginVertical();
                        int buttonCount = 0;
                        bool isNewRowStarted = false;

                        foreach (var (isText, content, action, startNewRow, centerText, colorHex) in contents)
                        {
                            if (isText)
                            {
                                if (isNewRowStarted)
                                {
                                    GUILayout.EndHorizontal();
                                    isNewRowStarted = false;
                                }
                                GUIStyle style = new GUIStyle(GUI.skin.label);

                                // Set text color if specified
                                if (!string.IsNullOrEmpty(colorHex) && ColorUtility.TryParseHtmlString(colorHex, out Color color))
                                {
                                    style.normal.textColor = color;
                                }

                                if (centerText)
                                {
                                    GUILayout.BeginHorizontal();
                                    GUILayout.FlexibleSpace();
                                    GUILayout.Label(content, style);
                                    GUILayout.FlexibleSpace();
                                    GUILayout.EndHorizontal();
                                }
                                else
                                {
                                    GUILayout.Label(content, style);
                                }
                                buttonCount = 0; // Reset button count after text
                            }
                            else
                            {
                                if (startNewRow || buttonCount % 3 == 0)
                                {
                                    if (isNewRowStarted)
                                    {
                                        GUILayout.EndHorizontal(); // End previous row if any
                                    }
                                    GUILayout.BeginHorizontal(); // Start a new row
                                    isNewRowStarted = true;
                                }

                                if (GUILayout.Button(content))
                                {
                                    action?.Invoke();
                                }

                                buttonCount++;
                            }
                        }

                        // End the last row if not empty
                        if (isNewRowStarted)
                        {
                            GUILayout.EndHorizontal();
                        }

                        GUILayout.EndVertical();
                    }
                }
            }
            finally
            {
                GUILayout.EndVertical();
            }
        });
    }


    public void AddTab(string tabName)
    {
        tabs.Add(tabName);
        tabActions.Add(() => { /* Default action, can be replaced later */ });
        tabContents[tabName] = new List<(bool isText, string content, Action action, bool startNewRow, bool centerText, string colorHex)>();
        AddNewRowToTab(tabName);
    }

    public void AddButtonToTab(string tabName, string buttonLabel, Action action, bool startNewRow = false)
    {
        if (tabContents.ContainsKey(tabName))
        {
            tabContents[tabName].Add((false, buttonLabel, action, startNewRow, false, null));
        }
    }

    public void AddTextToTab(string tabName, string text, bool centerText = false, string colorHex = null)
    {
        if (tabContents.ContainsKey(tabName))
        {
            tabContents[tabName].Add((true, text, null, false, centerText, colorHex));
        }
    }

    public void AddNewRowToTab(string tabName)
    {
        AddTextToTab(tabName, ""); // Adding an empty text item to create a new row
    }

    public void OnGUI()
    {
        if (WindowEnabled)
            Window.OnGUI();
    }

    public void Toggle()
    {
        if (!GameStates.IsDev || Main.ReleaseBuildType != ReleaseTypes.Dev) return;

        WindowEnabled = !WindowEnabled;
    }
#endif
}
