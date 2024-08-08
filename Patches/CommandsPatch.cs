using AmongUs.Data;
using AmongUs.GameOptions;
using BetterAmongUs.Role;
using HarmonyLib;
using Hazel;
using Il2CppSystem.Text;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Patches;

[HarmonyPatch(typeof(ChatController))]
class CommandsPatch
{
    private static PlayerControl? cmdTarget = null;
    private static bool HasPermission = Permission != null;
    public static PlayerControl? Permission = null;

    // List of helper text when a command is being typed out
    // First word is command, {} are arguments, first --- is command description, second --- is for help command.
    public static string[] CommandListHelper =
        {
        "help---Get help with commands",
        "commands---Get a list of all available commands",
        "dump---Dump at the entire log to the user's desktop",
        "player {id}---Get a Players information---{player id}",
        "players---Get all Player information",
        "whisper {id} {msg}---Privately send an message to a player---{player id}",
        "w {id} {msg}---Privately send an message to a player---{player id} {text}",
        "setprefix {prefix}---Set command prefix",
        "name {name}---Set player name - <color=red>Host Only/Host Permission</color>---{text}",
        "kick {id}---Kick a player from the game - <color=red>Host Only</color>---{player id}",
        "ban {id}---Ban a player from the game - <color=red>Host Only</color>---{player id}",
        "endgame {reason}---Force end the game - <color=red>Host Only</color>---{player id}",
        "removeplayer {identifier}---Remove player from local <color=#4f92ff>Anti-Cheat</color> data---{Name, FriendCode, HashPuid}",
        };
    public static string[] DebugCommandListHelper =
        {
        "getposition---Get current player position",
        "role {role}---Set your role for the next game - <color=red>Host Only</color> - <color=#ff00f7>DeBug</color>",
        "setrole {id} {role}---Set another players role for the next game - <color=red>Host Only</color> - <color=#ff00f7>DeBug</color>",
        "syncallnames---Sync all players names for better host - <color=red>Host Only</color> - <color=#ff00f7>DeBug</color>",
        };


    // Run code for specific commands
    private static void HandleCommand(ChatController __instance, string[] command)
    {
        bool checkDebugCommand = false;
        string subArgs = command.Length > 1 ? command[1].ToLower().Trim() : "";
        string subArgs2 = command.Length > 2 ? command[2].ToLower().Trim() : "";
        bool flag = subArgs.Length > 0;
        bool flag2 = subArgs2.Length > 0;
        string error = "<color=#f50000><size=150%><b>Error:</b></size></color>";
        StringBuilder sb = new StringBuilder();

        // Commands
        switch (command[0][1..].ToLower().Trim())
        {
            case "help":
                Utils.AddChatPrivate(
                    "Welcome to <color=#0dff00>♻BetterAmongUs♻</color> This mod enhances your gameplay experience with a variety of exciting features.\n" +
                    "Explore the pause menu to access more options and better settings tailored to your needs.\n" +
                    "For a full list of available commands, use the `/commands` command.\n\n" +
                    "Our features include: \n" +
                    "- Built-in Client-Sided Anti-Cheat: Enjoy a fair game with our anti-cheat system that detects and prevents unauthorized actions.\n" +
                    "- Host Enhancements: Gain additional control as a host with improved options and settings.\n" +
                    "- Better Options: Customize your game with a range of new and improved settings.\n" +
                    "- Commands: Utilize a variety of commands to manage and enhance your gameplay.\n" +
                    "- Client Improvements: Experience smoother and more efficient gameplay with our client-side enhancements.\n" +
                    "Stay tuned for more exciting features and improvements coming your way!"
                );
                break;
            case "commands":
                string[] allCommands = CommandListHelper;
                string[] allDebugCommands = DebugCommandListHelper;
                string list;
                var open = "<color=#858585>┌──────── </color>";
                var mid = "<color=#858585>├ </color>";
                var close = "<color=#858585>└──────── </color>";
                list = "<color=#00751f><b><size=150%>Command List</size></b></color>\n" + open;
                for (int i = 0; i < allCommands.Length; i++)
                {
                    if (i < allCommands.Length)
                    {
                        list += $"\n{mid}<color=#e0b700><b>{Main.CommandPrefix.Value}{allCommands[i].Split(' ')[0].Split("---")[0]}</b></color> <size=65%><color=#735e00>{allCommands[i].Split("---")[1]}.</color></size>";
                    }
                }
                if (GameStates.IsDev)
                {
                    list += "\n" + close + "\n";
                    list += "<color=#00751f><b><size=150%>Debug Command List</size></b></color>\n" + open;
                    for (int i = 0; i < allDebugCommands.Length; i++)
                    {
                        if (i < allDebugCommands.Length)
                        {
                            list += $"\n{mid}<color=#e0b700><b>{Main.CommandPrefix.Value}{allDebugCommands[i].Split(' ')[0].Split("---")[0]}</b></color> <size=65%><color=#735e00>{allDebugCommands[i].Split("---")[1]}.</color></size>";
                        }
                    }
                }
                list += "\n" + close;
                Utils.AddChatPrivate(list);
                break;
            case "dump":
                if (GameStates.IsInGamePlay && !GameStates.IsDev) return;

                string logFilePath = Path.Combine(Environment.CurrentDirectory, "better-log.txt");
                string log = File.ReadAllText(logFilePath);
                string newLog = string.Empty;
                string[] logArray = log.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (string text in logArray)
                {
                    if (text.Contains("[PrivateLog]"))
                    {
                        newLog += text.Split(':')[0] + ":" + text.Split(':')[1].Replace("[PrivateLog]", "") + ": " + Encryptor.Decrypt(text.Split(':')[2][1..]) + "\n";
                    }
                    else
                    {
                        newLog += text + "\n";
                    }
                }

                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string logFolderPath = Path.Combine(desktopPath, "BetterAmongUsLog");
                if (!Directory.Exists(logFolderPath))
                {
                    Directory.CreateDirectory(logFolderPath);
                }
                string logFileName = "log-" + Main.GetVersionText().Replace(' ', '-').ToLower() + "-" + DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss") + ".log";
                string newLogFilePath = Path.Combine(logFolderPath, logFileName);
                File.WriteAllText(newLogFilePath, newLog);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = logFolderPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = newLogFilePath,
                    UseShellExecute = true,
                    Verb = "open"
                });

                Utils.AddChatPrivate($"Dump logs at <color=#b1b1b1>'{newLogFilePath}'</color>");
                break;
            case "player":
                if (HandlePlayerArgument(command, subArgs) == true)
                {
                    var player = cmdTarget;
                    var hexColor = Utils.Color32ToHex(Palette.PlayerColors[player.CurrentOutfit.ColorId]);
                    var format1 = "┌ •";
                    var format2 = "├ •";
                    var format3 = "└ •";
                    sb.Append($"<size=150%><color={hexColor}><b>{player?.Data?.PlayerName}</color> Info:</b></size>\n");
                    sb.Append($"{format1} <color=#c1c1c1>ID: {player?.Data?.PlayerId}</color>\n");
                    sb.Append($"{format2} <color=#c1c1c1>HashPUID: {Utils.GetHashPuid($"{player?.Data?.Puid}")}</color>\n");
                    sb.Append($"{format2} <color=#c1c1c1>Platform: {Utils.GetPlatformName(player)}</color>\n");
                    sb.Append($"{format3} <color=#c1c1c1>FriendCode: {player?.Data?.FriendCode}</color>");
                    Utils.AddChatPrivate(sb.ToString());
                }
                break;
            case "players":
                foreach (PlayerControl player in Main.AllPlayerControls.Where(player => !player.isDummy))
                {
                    var hexColor = Utils.Color32ToHex(Palette.PlayerColors[player.CurrentOutfit.ColorId]);
                    sb.Append($"<color={hexColor}><b>{player?.Data?.PlayerName}</color> Info:</b>\n");
                    sb.Append($"<color=#c1c1c1>{player?.Data?.PlayerId}</color> - ");
                    sb.Append($"<color=#c1c1c1>{Utils.GetHashPuid($"{player?.Data?.Puid}")}</color> - ");
                    sb.Append($"<color=#c1c1c1>{Utils.GetPlatformName(player)}</color> - ");
                    sb.Append($"<color=#c1c1c1>{player?.Data?.FriendCode}</color>");
                    sb.Append("\n\n");
                }
                Utils.AddChatPrivate(sb.ToString());
                break;      
            case "whisper" or "w":
                if (HandlePlayerArgument(command, subArgs) == true)
                {
                    if (UseCommandInGame() == true)
                    {
                        var player = cmdTarget;

                        MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SendChat, SendOption.None, player.GetClientId());
                        messageWriter.Write(string.Join(" ", command[2..].ToArray()));
                        AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
                        Utils.AddChatPrivate($"{string.Join(" ", command[2..].ToArray())}", overrideName: $"<color=#696969>Sent Private Message to</color> <b>{player.GetPlayerNameAndColor()}</b>");
                    }
                }
                break;
            case "setprefix":
                Main.CommandPrefix.Value = subArgs;
                break;
            case "name":
                if (GameStates.IsHost)
                {
                    PlayerControl.LocalPlayer.RpcSetName(command[1]);
                    _ = new LateTask(() =>
                    {
                        RPC.SyncAllNames();
                    }, 1f, $"Command {Main.CommandPrefix.Value}name");
                }
                else if (command.Length > 1 && !string.IsNullOrWhiteSpace(command[1]) && System.Text.RegularExpressions.Regex.IsMatch(command[1], @"^[a-zA-Z0-9]+$"))
                {
                    if (HandleHasPermission(command) == true)
                    {
                        PlayerControl.LocalPlayer.CmdCheckName(command[1]);
                    }
                }
                else
                {
                    Utils.AddChatPrivate($"{error}\nInvalid Name!");
                    return;
                }

                PlayerControl.LocalPlayer.Data.PlayerName = command[1];
                DataManager.Player.customization.Name = command[1];
                Utils.AddChatPrivate($"Player name has been sent to: {command[1]}");
                break;
            case "kick" or "ban":
                if (HandlePlayerArgument(command, subArgs) == true)
                {
                    if (HandleIsHost(command) == true)
                    {
                        var player = cmdTarget;
                        var cmdFlag = command[0][1..].ToLower().Trim() != "kick";
                        player.Kick(cmdFlag);
                    }
                }
                break;
            case "endgame":
                if (HandleIsHost(command) == true)
                {
                    if (!GameStates.IsLobby && !GameStates.IsFreePlay)
                    {
                        if (subArgs == "")
                        {
                            foreach (PlayerControl player in Main.AllPlayerControls)
                            {
                                player.roleAssigned = false;
                                player.RpcSetRole(RoleTypes.Crewmate, true);
                            }
                            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorByKill, false);
                            return;
                        }
                        else if (subArgs is "impostor" or "1")
                        {
                            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorByKill, false);
                        }
                        else if (subArgs is "crewmate" or "2")
                        {
                            GameManager.Instance.RpcEndGame(GameOverReason.HumansByTask, false);
                        }
                    }
                }
                break;
            case "removeplayer":
                if (!string.IsNullOrEmpty(subArgs))
                {
                    if (BetterDataManager.RemovePlayer(subArgs) == true)
                    {
                        Utils.AddChatPrivate($"<color=#0dff00>{subArgs} successfully removed from local <color=#4f92ff>Anti-Cheat</color> data!</color>");
                    }
                    else
                    {
                        Utils.AddChatPrivate($"{error}\nCould not find player data from identifier");
                    }
                }
                break;
            default:
                if (GameStates.IsDev)
                {
                    checkDebugCommand = true;
                    break;
                }

                Utils.AddChatPrivate("<color=#f50000><size=150%><b>Invalid Command!</b></size></color>");
                break;
        }

        // DeBug Commands
        if (checkDebugCommand)
        {
            switch (command[0][1..].ToLower().Trim())
            {
                case "getposition":
                    Utils.AddChatPrivate($"Current position: {PlayerControl.LocalPlayer.GetTruePosition()}");
                    break;
                case "role" or "setrole":
                    if (HandleIsHost(command) == true)
                    {
                        if (!Main.BetterRoleAlgorithma.Value)
                        {
                            Utils.AddChatPrivate($"<color=#730000>{error}\nBetter Role Algorithma in better options must be turned on to use this command");
                            return;
                        }
                        if (command[0][1..].ToLower().Trim() == "role")
                        {
                            var player = PlayerControl.LocalPlayer;
                            if (subArgs == "")
                            {
                                Utils.AddChatPrivate($"Set role to <color=#878787>Random</color> for the next game!");
                                RoleManagerPatch.SetPlayerRole.Remove(player);
                                return;
                            }

                            RoleTypes role;
                            switch (subArgs)
                            {
                                case "impostor" or "1":
                                    role = RoleTypes.Impostor;
                                    break;
                                case "shapeshifter" or "2":
                                    role = RoleTypes.Shapeshifter;
                                    break;
                                case "phantom" or "3":
                                    role = RoleTypes.Phantom;
                                    break;
                                case "crewmate" or "4":
                                    role = RoleTypes.Crewmate;
                                    break;
                                case "engineer" or "5":
                                    role = RoleTypes.Engineer;
                                    break;
                                case "scientist" or "6":
                                    role = RoleTypes.Scientist;
                                    break;
                                case "tracker" or "7":
                                    role = RoleTypes.Tracker;
                                    break;
                                case "noisemaker" or "8":
                                    role = RoleTypes.Noisemaker;
                                    break;
                                default:
                                    Utils.AddChatPrivate($"<color=#730000>{error}\nInvalid RoleType!");
                                    return;
                            }
                            string RoleHexColor = role is RoleTypes.Impostor or RoleTypes.ImpostorGhost or RoleTypes.Shapeshifter or RoleTypes.Phantom ? "#a60d0d" : "#63bfbf";
                            Utils.AddChatPrivate($"Set role to <color={RoleHexColor}>{Main.GetRoleName[(int)role]}</color> for the next game!");
                            RoleManagerPatch.SetPlayerRole[player] = role;
                        }
                        else if (HandlePlayerArgument(command, subArgs) == true)
                        {
                            var player = cmdTarget;
                            if (player == PlayerControl.LocalPlayer)
                            {
                                Utils.AddChatPrivate($"<color=#730000>{error}\nUnable to use /SetRole for self, use /Role!");
                                return;
                            }
                            var hexColor = Utils.Color32ToHex(Palette.PlayerColors[player.CurrentOutfit.ColorId]);
                            if (subArgs2 == "")
                            {
                                Utils.AddChatPrivate($"Set <color={hexColor}><b>{player?.Data?.PlayerName}</b></color> role to <color=#878787>Random</color> for the next game!");
                                RoleManagerPatch.SetPlayerRole.Remove(player);
                                return;
                            }
                            RoleTypes role;
                            switch (subArgs2)
                            {
                                case "impostor" or "1":
                                    role = RoleTypes.Impostor;
                                    break;
                                case "shapeshifter" or "2":
                                    role = RoleTypes.Shapeshifter;
                                    break;
                                case "phantom" or "3":
                                    role = RoleTypes.Phantom;
                                    break;
                                case "crewmate" or "4":
                                    role = RoleTypes.Crewmate;
                                    break;
                                case "engineer" or "5":
                                    role = RoleTypes.Engineer;
                                    break;
                                case "scientist" or "6":
                                    role = RoleTypes.Scientist;
                                    break;
                                case "tracker" or "7":
                                    role = RoleTypes.Tracker;
                                    break;
                                case "noisemaker" or "8":
                                    role = RoleTypes.Noisemaker;
                                    break;
                                default:
                                    Utils.AddChatPrivate($"<color=#730000>{error}\nInvalid RoleType!");
                                    return;
                            }
                            string RoleHexColor = role is RoleTypes.Impostor or RoleTypes.ImpostorGhost or RoleTypes.Shapeshifter or RoleTypes.Phantom ? "#a60d0d" : "#63bfbf";
                            Utils.AddChatPrivate($"Set <color={hexColor}><b>{player?.Data?.PlayerName}</b></color> role to <color={RoleHexColor}>{Main.GetRoleName[(int)role]}</color> for the next game!");
                            RoleManagerPatch.SetPlayerRole[player] = role;
                        }
                    }
                    break;
                case "syncallnames":
                    if (HandleIsHost(command) == true)
                    {
                        RPC.SyncAllNames(force: true);
                        Utils.AddChatPrivate("<color=#0dff00>All player names have been updated and synced!</color>");
                    }
                    break;
                default:
                    Utils.AddChatPrivate("<color=#f50000><size=150%><b>Invalid Command!</b></size></color>");
                    break;
            }
        }

        cmdTarget = null;
    }

    // Condition when a player is allowed to run a specific command
    private static bool UseCommandInGame()
    {
        bool flag = GameStates.IsInGame && GameStates.IsMeeting || GameStates.IsExilling || GameStates.IsLobby || GameStates.IsFreePlay || !PlayerControl.LocalPlayer.IsAlive();
        if (!flag)
        {
            Utils.AddChatPrivate("<color=#f50000><size=125%><b>Unable To Use Command While In Game!</b></size></color>");
        }
        return flag;
    }

    // Handle player ID arguments in commands
    private static bool HandlePlayerArgument(string[] command, string subArg)
    {
        string error = "<color=#f50000><size=150%><b>Error:</b></size></color>";

        bool flag = subArg.Where(char.IsDigit).ToArray().Any();
        bool flag2 = command.Length > 2;
        bool flag3 = false;

        if (flag)
            flag3 = Main.AllPlayerControls.ToArray().Any(player => !player.isDummy && player.Data.PlayerId == int.Parse(subArg.Where(char.IsDigit).ToArray()));

        if (flag3 == true)
        {
            cmdTarget = Utils.PlayerFromId(int.Parse(subArg));
            return true;
        }
        else
        {
            if (!flag)
            {
                if (!flag2)
                {
                    Utils.AddChatPrivate($"<color=#e0b700>{error}\n{command[0][1..]} " + "<b>{Player ID}</b></color>");
                }
                else
                {
                    Utils.AddChatPrivate($"<color=#e0b700>{error}\n{command[0][1..]} {command[1]} " + "<b>{Player ID}</b></color>");
                }
            }
            else
            {
                Utils.AddChatPrivate($"<color=#730000>{error}\nPlayer not found");
            }

            return false;
        }
    }

    // Handle host only commands
    private static bool HandleIsHost(string[] command)
    {
        string error = "<color=#f50000><size=150%><b>Error:</b></size></color>";

        bool flag = PlayerControl.LocalPlayer.IsHost();

        if (!flag)
        {
            Utils.AddChatPrivate($"{error}\n<color=#e0b700><b>{command[0]}</b></color> Is only available as host!");
        }

        return flag;
    }

    // Handle if player has permission from the host to run command
    private static bool HandleHasPermission(string[] command)
    {
        string error = "<color=#f50000><size=150%><b>Error:</b></size></color>";

        if (HasPermission != true && !GameStates.IsHost && !GameStates.IsDev)
        {
            Utils.AddChatPrivate($"{error}\n<color=#e0b700><b>{command[0]}</b></color> Is only available with the host permission!\nask the host to type /allow in chat to get permissions");
            return false;
        }
        return true;
    }

    // Check if command is typed when sending chat message
    [HarmonyPatch(nameof(ChatController.SendChat))]
    [HarmonyPrefix]
    public static bool SendChat_Prefix(ChatController __instance)
    {
        string text = __instance.freeChatField.textArea.text;

        if (string.IsNullOrEmpty(text) || text.Length <= 1 || text[0].ToString() != Main.CommandPrefix.Value || 3f - __instance.timeSinceLastMessage > 0f)
        {
            if (GameStates.InGame && !GameStates.IsLobby && !GameStates.IsFreePlay && !GameStates.IsMeeting && !GameStates.IsExilling && PlayerControl.LocalPlayer.IsAlive())
                return false;

            if (ChatPatch.ChatHistory.Count == 0 || ChatPatch.ChatHistory[^1] != text) ChatPatch.ChatHistory.Add(text);
            ChatPatch.CurrentHistorySelection = ChatPatch.ChatHistory.Count;
            return true;
        }

        string[] command = text.Split(' ');

        HandleCommand(__instance, command);

        if (ChatPatch.ChatHistory.Count == 0 || ChatPatch.ChatHistory[^1] != text) ChatPatch.ChatHistory.Add(text);
        ChatPatch.CurrentHistorySelection = ChatPatch.ChatHistory.Count;

        __instance.timeSinceLastMessage = 0f;
        __instance.freeChatField.Clear();
        __instance.quickChatMenu.Clear();
        __instance.quickChatField.Clear();
        return false;
    }

    // Set up command helper
    private static GameObject commandText;
    private static GameObject commandInfo;
    private static RandomNameGenerator NameRNG;
    [HarmonyPatch(nameof(ChatController.Toggle))]
    [HarmonyPostfix]
    public static void Awake_Postfix(ChatController __instance)
    {
        if (commandText == null)
        {
            var TextArea = __instance.freeChatField.textArea.gameObject;
            GameObject CommandDisplay = UnityEngine.Object.Instantiate(TextArea, TextArea.transform.parent.transform);
            CommandDisplay.transform.SetSiblingIndex(TextArea.transform.GetSiblingIndex() + 1);
            CommandDisplay.transform.DestroyChildren();
            CommandDisplay.name = "CommandArea";
            CommandDisplay.GetComponent<TextMeshPro>().color = new Color(1f, 1f, 1f, 0.5f);
            commandText = CommandDisplay;
        }

        if (commandInfo == null)
        {
            var TextArea = __instance.freeChatField.textArea.gameObject;
            GameObject CommandInformation = UnityEngine.Object.Instantiate(TextArea, TextArea.transform.parent.transform);
            CommandInformation.transform.SetSiblingIndex(TextArea.transform.GetSiblingIndex() + 1);
            CommandInformation.transform.DestroyChildren();
            CommandInformation.transform.localPosition = new Vector3(CommandInformation.transform.localPosition.x, 0.45f);
            CommandInformation.name = "CommandInfoText";
            CommandInformation.GetComponent<TextMeshPro>().color = Color.yellow;
            CommandInformation.GetComponent<TextMeshPro>().outlineColor = new Color(0f, 0f, 0f, 1f);
            CommandInformation.GetComponent<TextMeshPro>().outlineWidth = 0.2f;
            CommandInformation.GetComponent<TextMeshPro>().characterWidthAdjustment = 1.5f;
            CommandInformation.GetComponent<TextMeshPro>().enableWordWrapping = false;
            commandInfo = CommandInformation;
        }

        if (NameRNG == null)
        {
            RandomNameGenerator rng = __instance.gameObject.AddComponent<RandomNameGenerator>();
            NameRNG = rng;
        }
    }

    // Command helper
    [HarmonyPatch(nameof(ChatController.Update))]
    [HarmonyPostfix]
    public static void Update_Postfix(ChatController __instance)
    {
        string text = __instance.freeChatField.textArea.text;

        if (commandText != null && commandInfo != null)
        {
            // Check if the first character is the command prefix
            if (text.Length > 0 && text[0].ToString() == Main.CommandPrefix.Value)
            {
                // Get the typed command without the prefix
                string typedCommand = text.Substring(1);

                // Find the closest matching command
                string closestCommand = GetClosestCommand(typedCommand.Split(' ')[0]);
                string CommandInfo = GetClosestCommand(typedCommand.Split(' ')[0]);

                // Check for character mismatches
                if (!string.IsNullOrEmpty(closestCommand) && !string.IsNullOrEmpty(CommandInfo) && IsMatch(typedCommand.Split(' ')[0], closestCommand))
                {
                    closestCommand = closestCommand.Split("---")[0];
                    CommandInfo = CommandInfo.Split("---")[1];

                    // Handle arguments correctly
                    string[] typedParts = typedCommand.Split(' ');
                    string[] commandParts = closestCommand.Split(' ');

                    // If there are arguments, ensure to display the appropriate part
                    string suggestion = commandParts[0].Substring(typedParts[0].Length); // Initial suggestion

                    suggestion = typedParts[0].Substring(0, typedParts[0].Length) + suggestion;

                    // Handle additional arguments
                    for (int i = 1; i < typedParts.Length; i++)
                    {
                        if (i < commandParts.Length && typedParts[i] != "")
                        {
                            // Add spaces to match the already typed part of the current argument
                            suggestion = typedCommand + " " + new string(' ', typedParts[i].Length);
                        }
                        else if (i < commandParts.Length)
                        {
                            // Add the next argument if the current one is empty
                            suggestion = typedCommand + " " + commandParts[i];
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.Tab) && typedParts.Length < 2)
                    {
                        __instance.freeChatField.textArea.SetText(Main.CommandPrefix.Value + commandParts[0]);
                    }

                    // Display the command suggestion
                    commandText.GetComponent<TextMeshPro>().text = Main.CommandPrefix.Value + suggestion;
                    commandInfo.GetComponent<TextMeshPro>().text = CommandInfo;
                }
                else
                {
                    // Clear the suggestion if there is a mismatch
                    commandText.GetComponent<TextMeshPro>().text = string.Empty;
                    commandInfo.GetComponent<TextMeshPro>().text = string.Empty;
                }
            }
            else
            {
                commandText.GetComponent<TextMeshPro>().text = string.Empty;
                commandInfo.GetComponent<TextMeshPro>().text = string.Empty;
            }
        }
    }

    public static string GetClosestCommand(string typedCommand)
    {
        var closestCommand = CommandListHelper.FirstOrDefault(c => c.StartsWith(typedCommand, StringComparison.OrdinalIgnoreCase));

        if (closestCommand == null && PlayerControl.LocalPlayer.IsDev())
            closestCommand = DebugCommandListHelper.FirstOrDefault(c => c.StartsWith(typedCommand, StringComparison.OrdinalIgnoreCase));

        return closestCommand ?? string.Empty;
    }

    public static bool IsMatch(string typedCommand, string closestCommand) => closestCommand.Length >= typedCommand.Length &&
        closestCommand.Substring(0, typedCommand.Length).Equals(typedCommand, System.StringComparison.OrdinalIgnoreCase);
}
