using System;
using System.Windows.Media;

namespace TransparentTwitchChatWPF.Models.Chat
{
    public class JChat : Chat
    {
        public JChat() : base(ChatTypes.jChat)
        {
        }

        public override string PushNewChatMessage(string message = "", string nickname = "", string color = "")
        {
            nickname = string.IsNullOrEmpty(nickname) ? "null" : $"\"{nickname}\"";

            string js = $"Chat.write({nickname}, null, \"{message}\");";

            if (!string.IsNullOrEmpty(color))
            {
                js = "var ttags = { color : \"" + color + "\", };\n";
                js += $"Chat.write({nickname}, ttags, \"\\x01ACTION {message}\\x01\");";
            }

            return js;
        }

        public override string PushNewMessage(string message = "")
        {
            return $"Chat.info.lines.push(\"<div class=\\\"chat_line\\\" data-time=\\\"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}\\\">{message}</div>\");";
        }

        public override string SetupJavascript()
        {
            PushNewMessage("jChat: Loaded...");

            string[] blockList = new string[SettingsSingleton.Instance.genSettings.BlockedUsersList.Count];
            SettingsSingleton.Instance.genSettings.BlockedUsersList.CopyTo(blockList, 0);

            string js = @"const jsCallback = chrome.webview.hostObjects.jsCallbackFunctions;";
            js += @"(function() {
                    var oldLog = console.log;
                    console.log = function(message) {
                        window.chrome.webview.postMessage(message);
                        oldLog.apply(console, arguments);
                    };
                })();
                ";

            if (SettingsSingleton.Instance.genSettings.HighlightUsersChat)
            {
                string[] vipList = new string[SettingsSingleton.Instance.genSettings.AllowedUsersList.Count];
                SettingsSingleton.Instance.genSettings.AllowedUsersList.CopyTo(vipList, 0);

                js += @"var oldChatWrite = Chat.write;
                            Chat.write = function(nick, info, message) {

                                var vips = ['";
                js += string.Join(",", vipList).Replace(",", "','").ToLower();
                js += @"'];
                                var blockUsers = ['";
                js += string.Join(",", blockList).Replace(",", "','").ToLower();
                js += @"'];
                                var allowOther = false;
                                var highlightSuffix = '';";

                if (SettingsSingleton.Instance.genSettings.FilterAllowAllVIPs)
                    js += CustomJS_Defaults.jChat_VIP_Check;

                if (SettingsSingleton.Instance.genSettings.FilterAllowAllMods)
                    js += CustomJS_Defaults.jChat_Mod_Check;

                js += @"if (blockUsers.includes(nick.toLowerCase())) {
                            return;
                        }";

                js += @"if (vips.includes(nick.toLowerCase()) || allowOther) {";

                if (SettingsSingleton.Instance.genSettings.ChatNotificationSound.ToLower() != "none")
                    js += CustomJS_Defaults.Callback_PlaySound;

                js += @"
                                Chat.info.lines.push('<div class=""highlight' + highlightSuffix + '"">');
                                oldChatWrite.apply(oldChatWrite, arguments);
                                Chat.info.lines.push('</div>');
                                return;
                            }
                            else
                            {
                                return oldChatWrite.apply(oldChatWrite, arguments);
                            }
                        }";

                return js;
            }
            else if (SettingsSingleton.Instance.genSettings.AllowedUsersOnlyChat)
            {
                string[] vipList = new string[SettingsSingleton.Instance.genSettings.AllowedUsersList.Count];
                SettingsSingleton.Instance.genSettings.AllowedUsersList.CopyTo(vipList, 0);

                js += @"var oldChatWrite = Chat.write;
                            Chat.write = function(nick, info, message) {

                                var vips = ['";
                js += string.Join(",", vipList).Replace(",", "','").ToLower();
                js += @"'];
                                var allowOther = false;";

                if (SettingsSingleton.Instance.genSettings.FilterAllowAllVIPs)
                    js += CustomJS_Defaults.jChat_VIP_Check;

                if (SettingsSingleton.Instance.genSettings.FilterAllowAllMods)
                    js += CustomJS_Defaults.jChat_Mod_Check;

                js += @"if (vips.includes(nick.toLowerCase()) || allowOther) {";

                if (SettingsSingleton.Instance.genSettings.ChatNotificationSound.ToLower() != "none")
                    js += CustomJS_Defaults.Callback_PlaySound;

                js += @"
                            return oldChatWrite.apply(oldChatWrite, arguments);
                        }
                    }";

                return js;
            }
            else if (SettingsSingleton.Instance.genSettings.ChatNotificationSound.ToLower() != "none")
            {
                // Insert JS to play a sound on each chat message, and check the block list

                js += @"var oldChatWrite = Chat.write;
                            Chat.write = function(nick, info, message) {
                                var blockUsers = ['";
                js += string.Join(",", blockList).Replace(",", "','").ToLower();
                js += @"'];
                                if (blockUsers.includes(nick.toLowerCase())) {
                                    return;
                                }
                                (async function() {
                                    await jsCallback.playSound();
                                })();
                                return oldChatWrite.apply(oldChatWrite, arguments);
                            }";

                return js;
            }
            else if ((SettingsSingleton.Instance.genSettings.BlockedUsersList != null) &&
                    (SettingsSingleton.Instance.genSettings.BlockedUsersList.Count > 0))
            {
                // No other options were selected, we're just gonna check the block list only here

                js += @"var oldChatWrite = Chat.write;
                            Chat.write = function(nick, info, message) {
                                var blockUsers = ['";
                js += string.Join(",", blockList).Replace(",", "','").ToLower();
                js += @"'];
                                if (blockUsers.includes(nick.toLowerCase())) {
                                    return;
                                }
                                return oldChatWrite.apply(oldChatWrite, arguments);
                            }";

                return js;
            }

            return String.Empty;
        }

        public override string SetupCustomCSS()
        {
            string css;

            if (!string.IsNullOrEmpty(SettingsSingleton.Instance.genSettings.CustomCSS))
                css = SettingsSingleton.Instance.genSettings.CustomCSS;
            else
            {
                // Highlight
                Color c = SettingsSingleton.Instance.genSettings.ChatHighlightColor;
                float a = (c.A / 255f);
                string rgba = $"rgba({c.R},{c.G},{c.B},{a:0.00})";
                css = ".highlight { background-color: " + rgba + " !important; }";

                // Mods Highlight
                c = SettingsSingleton.Instance.genSettings.ChatHighlightModsColor;
                a = (c.A / 255f);
                rgba = $"rgba({c.R},{c.G},{c.B},{a:0.00})";
                css += "\n .highlightMod { background-color: " + rgba + " !important; }";

                // VIPs Highlight
                c = SettingsSingleton.Instance.genSettings.ChatHighlightVIPsColor;
                a = (c.A / 255f);
                rgba = $"rgba({c.R},{c.G},{c.B},{a:0.00})";
                css += "\n .highlightVIP { background-color: " + rgba + " !important; }";
            }

            return css;
        }
    }
}
