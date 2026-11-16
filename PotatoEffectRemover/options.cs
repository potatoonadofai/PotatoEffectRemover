using System;
using System.Collections.Generic;
using UnityEngine;
using UnityModManagerNet;
using HarmonyLib;
using ADOFAI;
using MonsterLove.StateMachine;
using System.Runtime.InteropServices;
using UnityEngine.LowLevel;
using UnityEngine.Events;
using System.Threading;
using static UnityEngine.Random;
using System.Collections;
using System.Xml.Linq;
using System.IO;
using SA.GoogleDoc;
using System.Linq;
using UnityEngine.UIElements;
using static UnityModManagerNet.UnityModManager;
using static System.Collections.Specialized.BitVector32;


namespace PER
{
    public static class Options
    {
        public class Config
        {
            public string name;
            public bool if_blacklist;
            public Dictionary<string, bool> events;
            public Config()
            {
                name = "New_Config";
                if_blacklist = true;
                events = new Dictionary<string, bool>();
            }
        }

        public static bool popup=false;
        public static bool langpopup = false;

        public static List<Config> configs = new List<Config>();
        public static int chosen_config = 0;
        public static Dictionary<string, bool> Other_Settings = new Dictionary<string, bool>();
        public static Dictionary<string, int> Other_Ints = new Dictionary<string, int>();
        public static Dictionary<string, string> Other_Strings = new Dictionary<string, string>();

        public static bool if_main = true;

        public static bool if_rename = false;
        public static Vector2 scrollPosition = Vector2.zero;
        public static bool if_fixs = false;
        public static bool if_UIsets = false;

        public static Dictionary<string, Dictionary<string, string>> eventtext = new Dictionary<string, Dictionary<string, string>>();

        public static string inputText = "";

        public static List<LevelEventType> levelEvents = new List<LevelEventType>();

        // ===== 新增：搜索与批量操作 =====
        public static string search_text = "";

        // ===== 新增：导入导出 =====
        public static bool if_export = false;
        public static bool if_import = false;
        public static string export_buffer = "";
        public static string import_buffer = "";

        // ===== 新增：统计信息 =====
        public static bool if_stats = false;
        public static int stat_events = 0;
        public static int stat_decorations = 0;
        public static int stat_events_filtered = 0;
        public static int stat_decorations_filtered = 0;

        public static void Reset()
        {
            popup = false;
            if_main = true;
            if_rename = false;
            scrollPosition = Vector2.zero;
            if_fixs = false;
            langpopup = false;
            if_UIsets=false;
            // 新增
            search_text = "";
            if_export = false;
            if_import = false;
            export_buffer = "";
            import_buffer = "";
            if_stats = false;
        }

        public static void GetEvent()
        {
            levelEvents.Clear();
            foreach (ADOFAI.LevelEventType eventType in Enum.GetValues(typeof(ADOFAI.LevelEventType)))
            {
                levelEvents.Add(eventType);
            }
        }
        public static void OnHideGUI(UnityModManager.ModEntry modEntry)
        {
            ConfigManager.SaveConfigs(configs, chosen_config, Other_Settings,Other_Ints,Other_Strings, Path.Combine(modEntry.Path, "Configs.json"));
            Reset();
            if (Options.Other_Settings["tile_opti"])
            {
                Main.harmony.PatchCategory("PER.tile_opti");
            }
            else
            {
                Main.harmony.UnpatchCategory("PER.tile_opti");
            }
            popup = false;
            if_main = true;
            if_rename = false;
            scrollPosition = Vector2.zero;
            if_fixs = false;
            langpopup = false;
            if_UIsets = false;
        }
        public static void Check()
        {
            if (Other_Settings == null)
            {
                Other_Settings = new Dictionary<string, bool>();
            }
            if (Other_Ints == null)
            {
                Other_Ints = new Dictionary<string, int>();
            }
            if (Other_Strings == null)
            {
                Other_Strings = new Dictionary<string, string>();
            }
            if (eventtext == null)
            {
                eventtext = new Dictionary<string, Dictionary<string, string>>();
            }
            if (!Other_Settings.ContainsKey("tile_opti"))
            {
                Other_Settings["tile_opti"] = false;
            }
            if (!Other_Ints.ContainsKey("text_size"))
            {
                Other_Ints["text_size"] = 20;
            }
            if (!Other_Settings.ContainsKey("follow_game_language"))
            {
                Other_Settings["follow_game_language"] = true;
            }
            if (!Other_Strings.ContainsKey("currlanguage"))
            {
                Other_Strings["currlanguage"] = LocalizationManager.GetLangCode(RDString.language);
            }
            if (!eventtext.ContainsKey(Other_Strings["currlanguage"]))
            {
                eventtext[Other_Strings["currlanguage"]] = new Dictionary<string, string>();
            }

            if (Other_Settings["follow_game_language"])
            {
                LocalizationManager.GetLanguages();
                Other_Strings["currlanguage"] = LocalizationManager.GetLangCode(RDString.language);
            }

            if (LocalizationManager.currentLanguage != Other_Strings["currlanguage"])
            {
                LocalizationManager.SwitchLanguage(Other_Strings["currlanguage"]);
            }

            while (chosen_config >= configs.Count)
            {
                chosen_config--;
            }

            while (chosen_config < 0)
            {
                chosen_config++;
            }
        }
        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            Check();
            if (!if_main)
            {
                // 保护：若配置被删空则返回主界面，避免越界
                if (configs.Count == 0)
                {
                    if_main = true;
                }
                else
                {
                    if (GUILayout.Button($"<size={Other_Ints["text_size"]}><"+LocalizationManager.GetLocalizedText("editor.save")+"</size>", GUILayout.Width((float)(Other_Ints["text_size"]*15)), GUILayout.Height((float)(Other_Ints["text_size"]*1.5))))
                    {
                        if_main = true;
                    }

                    if (GUILayout.Button($"<size={Other_Ints["text_size"]}>"+LocalizationManager.GetLocalizedText("editor.filtering_mode") +":"+(configs[chosen_config].if_blacklist ? LocalizationManager.GetLocalizedText("editor.blacklist") : LocalizationManager.GetLocalizedText("editor.whitelist")) +"</size>", GUILayout.Width((float)(Other_Ints["text_size"]*25)), GUILayout.Height((float)(Other_Ints["text_size"]*1.5))))
                    {
                        configs[chosen_config].if_blacklist = !configs[chosen_config].if_blacklist;
                    }

                    // ---------- 搜索框 ----------
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"<size={Other_Ints["text_size"]}>" + LocalizationManager.GetLocalizedText("editor.search") + ":</size>",
                        GUILayout.Width((float)(Other_Ints["text_size"] * 7)));
                    GUIStyle searchStyle = new GUIStyle(GUI.skin.textField);
                    searchStyle.fontSize = Other_Ints["text_size"];
                    search_text = GUILayout.TextField(search_text, searchStyle,
                        GUILayout.Width((float)(Other_Ints["text_size"] * 18)),
                        GUILayout.Height((float)(Other_Ints["text_size"] * 1.5)));
                    if (GUILayout.Button($"<size={Other_Ints["text_size"]}>" + LocalizationManager.GetLocalizedText("editor.search_clear") + "</size>",
                        GUILayout.Width((float)(Other_Ints["text_size"] * 7)),
                        GUILayout.Height((float)(Other_Ints["text_size"] * 1.5))))
                    {
                        search_text = "";
                    }
                    GUILayout.EndHorizontal();

                    // ---------- 批量操作 ----------
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button($"<size={Other_Ints["text_size"]}>" + LocalizationManager.GetLocalizedText("editor.select_all") + "</size>",
                        GUILayout.Width((float)(Other_Ints["text_size"] * 7)),
                        GUILayout.Height((float)(Other_Ints["text_size"] * 1.5))))
                    {
                        var keys = new List<string>(configs[chosen_config].events.Keys);
                        foreach (var k in keys) configs[chosen_config].events[k] = true;
                    }
                    if (GUILayout.Button($"<size={Other_Ints["text_size"]}>" + LocalizationManager.GetLocalizedText("editor.select_none") + "</size>",
                        GUILayout.Width((float)(Other_Ints["text_size"] * 7)),
                        GUILayout.Height((float)(Other_Ints["text_size"] * 1.5))))
                    {
                        var keys = new List<string>(configs[chosen_config].events.Keys);
                        foreach (var k in keys) configs[chosen_config].events[k] = false;
                    }
                    if (GUILayout.Button($"<size={Other_Ints["text_size"]}>" + LocalizationManager.GetLocalizedText("editor.invert_selection") + "</size>",
                        GUILayout.Width((float)(Other_Ints["text_size"] * 7)),
                        GUILayout.Height((float)(Other_Ints["text_size"] * 1.5))))
                    {
                        var keys = new List<string>(configs[chosen_config].events.Keys);
                        foreach (var k in keys) configs[chosen_config].events[k] = !configs[chosen_config].events[k];
                    }
                    GUILayout.EndHorizontal();

                    scrollPosition = GUILayout.BeginScrollView(scrollPosition);

                    foreach (ADOFAI.LevelEventType eventType in levelEvents)
                    {
                        if(eventType != LevelEventType.None && !EditorConstants.settingsTypes.Contains(eventType))
                        {
                            string key = "editor." + eventType.ToString();
                            string text = RDString.Get(key, null);
                            if (text == "")
                            {
                                continue;
                            }
                            if (eventtext[Other_Strings["currlanguage"]].ContainsKey(key))
                            {
                                text= eventtext[Other_Strings["currlanguage"]][key];
                            }
                            else
                            {
                                text = Localization.GetLocalizedString(key, LocalizationManager.CodeToLang(Other_Strings["currlanguage"]));
                                eventtext[Other_Strings["currlanguage"]][key] = text;
                            }

                            // ---------- 搜索过滤 ----------
                            if (!string.IsNullOrEmpty(search_text))
                            {
                                string ls = search_text.ToLower();
                                if (!text.ToLower().Contains(ls) && !eventType.ToString().ToLower().Contains(ls))
                                {
                                    continue;
                                }
                            }

                            if (!configs[chosen_config].events.ContainsKey(eventType.ToString()))
                            {
                                configs[chosen_config].events[eventType.ToString()] = false;
                            }
                            GUILayout.BeginHorizontal();
                            configs[chosen_config].events[eventType.ToString()] = GUILayout.Toggle(configs[chosen_config].events[eventType.ToString()], $"<size={Other_Ints["text_size"]}>{text}</size>");
                            GUILayout.EndHorizontal();
                        }
                    }

                    GUILayout.EndScrollView();
                }
            }
            else
            {
                GUILayout.Label($"<size={Other_Ints["text_size"]}>"+LocalizationManager.GetLocalizedText("main.title")+"</size>");
                GUILayout.Label($"<color=#FF2222><size={Other_Ints["text_size"]}>"+LocalizationManager.GetLocalizedText("main.warning")+"</size></color>", Array.Empty<GUILayoutOption>());

                if (GUILayout.Button($"<size={Other_Ints["text_size"]}>"+LocalizationManager.GetLocalizedText("main.now_selection") +":"+(chosen_config>=0 && chosen_config < configs.Count ? configs[chosen_config].name : LocalizationManager.GetLocalizedText("main.none")) +"</size>", GUILayout.Width((float)(Other_Ints["text_size"]*25)), GUILayout.Height((float)(Other_Ints["text_size"]*1.5))))
                {
                    popup = !popup;
                }

                if (popup)
                {
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width((float)(Other_Ints["text_size"]*30)), GUILayout.Height(100));
                    for (int i = 0; i < configs.Count; i++)
                    {
                        if (GUILayout.Button($"<size={Other_Ints["text_size"]}>{configs[i].name}</size>", GUILayout.Width((float)(Other_Ints["text_size"]*25)), GUILayout.Height((float)(Other_Ints["text_size"]*1.5))))
                        {
                            chosen_config = i;
                            popup = false;
                        }
                    }
                    GUILayout.EndScrollView();
                }

                if (GUILayout.Button($"<size={Other_Ints["text_size"]}>"+ LocalizationManager.GetLocalizedText("main.new")+ "</size>", GUILayout.Width((float)(Other_Ints["text_size"]*10)), GUILayout.Height((float)(Other_Ints["text_size"]*1.5))))
                {
                    configs.Add(new Config());
                    chosen_config = configs.Count - 1;
                    if_main = false;
                }

                if(configs.Count == 0)
                {
                    GUI.enabled = false;
                }
                GUILayout.BeginHorizontal();
                if (!if_rename)
                {
                    if (GUILayout.Button($"<size={Other_Ints["text_size"]}>"+ LocalizationManager.GetLocalizedText("main.rename_selection") + "</size>", GUILayout.Width((float)(Other_Ints["text_size"]*15)), GUILayout.Height((float)(Other_Ints["text_size"]*1.5))))
                    {
                        if_rename = true;
                    }
                }
                else
                {
                    GUIStyle customTextFieldStyle = new GUIStyle(GUI.skin.textField);
                    customTextFieldStyle.fontSize = Other_Ints["text_size"];
                    GUILayout.Label($"<size={Other_Ints["text_size"]}>"+ LocalizationManager.GetLocalizedText("main.renameto") + ":</size>");
                    inputText = GUILayout.TextField(inputText, customTextFieldStyle, GUILayout.Width((float)(Other_Ints["text_size"] * 15)), GUILayout.Height((float)(Other_Ints["text_size"] * 1.5)));
                    if (GUILayout.Button($"<size={Other_Ints["text_size"]}>"+ LocalizationManager.GetLocalizedText("main.confirm") + "</size>", GUILayout.Width((float)(Other_Ints["text_size"]*15)), GUILayout.Height((float)(Other_Ints["text_size"]*1.5))))
                    {
                        if (inputText != "")
                        {
                            configs[chosen_config].name = inputText;
                        }
                        if_rename = false;
                    }
                }
                GUILayout.EndHorizontal();
                if (GUILayout.Button($"<size={Other_Ints["text_size"]}>"+ LocalizationManager.GetLocalizedText("main.edit_selection") + "</size>", GUILayout.Width((float)(Other_Ints["text_size"]*15)), GUILayout.Height((float)(Other_Ints["text_size"]*1.5))))
                {
                    if_main = false;
                }
                if (GUILayout.Button($"<size={Other_Ints["text_size"]}>"+ LocalizationManager.GetLocalizedText("main.delete_selection") + "</size>", GUILayout.Width((float)(Other_Ints["text_size"]*15)), GUILayout.Height((float)(Other_Ints["text_size"]*1.5))))
                {
                    configs.RemoveAt(chosen_config);
                    while(chosen_config>=configs.Count)
                    {
                        chosen_config --;
                    }
                }

                // ---------- 复制选中项 ----------
                if (GUILayout.Button($"<size={Other_Ints["text_size"]}>" + LocalizationManager.GetLocalizedText("main.copy_selection") + "</size>",
                    GUILayout.Width((float)(Other_Ints["text_size"] * 15)),
                    GUILayout.Height((float)(Other_Ints["text_size"] * 1.5))))
                {
                    if (chosen_config >= 0 && chosen_config < configs.Count)
                    {
                        var newConfig = new Config();
                        newConfig.name = configs[chosen_config].name + "_copy";
                        newConfig.if_blacklist = configs[chosen_config].if_blacklist;
                        newConfig.events = new Dictionary<string, bool>(configs[chosen_config].events);
                        configs.Add(newConfig);
                        chosen_config = configs.Count - 1;
                    }
                }

                // ---------- 导出配置（配置为空时禁用）----------
                if (GUILayout.Button($"<size={Other_Ints["text_size"]}>" + (if_export ? LocalizationManager.GetLocalizedText("main.export_close") : LocalizationManager.GetLocalizedText("main.export_config")) + "</size>",
                    GUILayout.Width((float)(Other_Ints["text_size"] * 15)),
                    GUILayout.Height((float)(Other_Ints["text_size"] * 1.5))))
                {
                    if_export = !if_export;
                    if_import = false;
                    if (if_export && chosen_config >= 0 && chosen_config < configs.Count)
                    {
                        try
                        {
                            export_buffer = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
                                Newtonsoft.Json.JsonConvert.SerializeObject(configs[chosen_config], Newtonsoft.Json.Formatting.None)));
                        }
                        catch { export_buffer = ""; }
                    }
                }

                GUI.enabled = true;

                // ---------- 导入配置（始终可用）----------
                if (GUILayout.Button($"<size={Other_Ints["text_size"]}>" + (if_import ? LocalizationManager.GetLocalizedText("main.import_close") : LocalizationManager.GetLocalizedText("main.import_config")) + "</size>",
                    GUILayout.Width((float)(Other_Ints["text_size"] * 15)),
                    GUILayout.Height((float)(Other_Ints["text_size"] * 1.5))))
                {
                    if_import = !if_import;
                    if_export = false;
                }

                if (if_export)
                {
                    GUIStyle exportStyle = new GUIStyle(GUI.skin.textArea) { fontSize = 12 };
                    GUILayout.TextArea(export_buffer, exportStyle, GUILayout.Height(60));
                }

                if (if_import)
                {
                    GUIStyle importStyle = new GUIStyle(GUI.skin.textArea) { fontSize = 12 };
                    import_buffer = GUILayout.TextArea(import_buffer, importStyle, GUILayout.Height(60));
                    if (GUILayout.Button($"<size={Other_Ints["text_size"]}>" + LocalizationManager.GetLocalizedText("main.confirm_import") + "</size>",
                        GUILayout.Width((float)(Other_Ints["text_size"] * 15)),
                        GUILayout.Height((float)(Other_Ints["text_size"] * 1.5))))
                    {
                        if (!string.IsNullOrEmpty(import_buffer))
                        {
                            try
                            {
                                string json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(import_buffer.Trim()));
                                var newConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(json);
                                if (newConfig != null)
                                {
                                    if (string.IsNullOrEmpty(newConfig.name)) newConfig.name = "Imported";
                                    if (newConfig.events == null) newConfig.events = new Dictionary<string, bool>();
                                    configs.Add(newConfig);
                                    chosen_config = configs.Count - 1;
                                    import_buffer = "";
                                    if_import = false;
                                }
                            }
                            catch { }
                        }
                    }
                }

                // ---------- 统计信息 ----------
                bool flagStats = GUILayout.Button($"<size={Other_Ints["text_size"]}>" + (if_stats ? "◢" : "▶") + LocalizationManager.GetLocalizedText("main.stats") + "</size>",
                    GUILayout.Width((float)(Other_Ints["text_size"] * 20)),
                    GUILayout.Height((float)(Other_Ints["text_size"] * 1.5)));
                if (flagStats) if_stats = !if_stats;
                if (if_stats)
                {
                    GUILayout.BeginVertical();
                    GUILayout.Label($"<size={Other_Ints["text_size"]}>" + LocalizationManager.GetLocalizedText("main.stat_events") + ": " + stat_events +
                        " (" + LocalizationManager.GetLocalizedText("main.stat_filtered") + ": " + stat_events_filtered + ")</size>");
                    GUILayout.Label($"<size={Other_Ints["text_size"]}>" + LocalizationManager.GetLocalizedText("main.stat_decorations") + ": " + stat_decorations +
                        " (" + LocalizationManager.GetLocalizedText("main.stat_filtered") + ": " + stat_decorations_filtered + ")</size>");
                    GUILayout.Label($"<size={Other_Ints["text_size"]}>" + LocalizationManager.GetLocalizedText("main.stat_configs") + ": " + configs.Count + "</size>");
                    GUILayout.EndVertical();
                }

                GUILayout.Label("");

                bool flag1 = GUILayout.Button($"<size={Other_Ints["text_size"]}>" + (if_fixs ? "◢" : "▶") + LocalizationManager.GetLocalizedText("fixs.title") + "</size>", GUILayout.Width((float)(Other_Ints["text_size"]*20)), GUILayout.Height((float)(Other_Ints["text_size"]*1.5)));
                if (flag1)
                {
                    if_fixs = !if_fixs;
                }
                if (if_fixs)
                {
                    if (Other_Settings == null)
                    {
                        Other_Settings = new Dictionary<string, bool>();
                    }
                    GUILayout.BeginVertical();
                    Other_Settings["tile_opti"] = GUILayout.Toggle(Other_Settings["tile_opti"], $"<size={Other_Ints["text_size"]}>" + LocalizationManager.GetLocalizedText("fixs.text1") + "</size>");
                    GUILayout.EndVertical();
                }



                bool flag2 = GUILayout.Button($"<size={Other_Ints["text_size"]}>" + (if_UIsets ? "◢" : "▶") + LocalizationManager.GetLocalizedText("modUI.title") + "</size>", GUILayout.Width((float)(Other_Ints["text_size"]*20)), GUILayout.Height((float)(Other_Ints["text_size"]*1.5)));
                if (flag2)
                {
                    if_UIsets = !if_UIsets;
                }
                if (if_UIsets)
                {
                    GUILayout.BeginVertical();
                    if (!Other_Settings.ContainsKey("follow_game_language"))
                    {
                        Other_Settings["follow_game_language"] = true;
                    }
                    Other_Settings["follow_game_language"] = GUILayout.Toggle(Other_Settings["follow_game_language"], $"<size={Other_Ints["text_size"]}>" + LocalizationManager.GetLocalizedText("modUI.gamelanguage") + "</size>");
                    

                    if (Other_Settings["follow_game_language"] == true)
                    {
                        Other_Strings["currlanguage"] = LocalizationManager.GetLangCode( RDString.language);
                        GUI.enabled = false;
                        langpopup = false;
                    }

                    if (GUILayout.Button($"<size={Other_Ints["text_size"]}>" + LocalizationManager.GetLocalizedText("modUI.language") + ":" + LocalizationManager.GetLocalizedText("modUI.currlanguage") + "</size>", GUILayout.Width((float)(Other_Ints["text_size"]*25)), GUILayout.Height((float)(Other_Ints["text_size"]*1.5))))
                    {
                        langpopup = !langpopup;
                    }

                    if (langpopup)
                    {
                        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width((float)(Other_Ints["text_size"]*30)), GUILayout.Height(100));
                        foreach (string lang in LocalizationManager.langCodeToLanguage.Keys)
                        {
                            if (GUILayout.Button($"<size={Other_Ints["text_size"]}>{LocalizationManager.GetLocalizedText("modUI.currlanguage", lang)}</size>", GUILayout.Width((float)(Other_Ints["text_size"]*25)), GUILayout.Height((float)(Other_Ints["text_size"]*1.5))))
                            {
                                Other_Strings["currlanguage"] = lang;
                                langpopup = false;
                            }
                        }
                        GUILayout.EndScrollView();
                    }

                    GUI.enabled = true;

                    GUILayout.BeginHorizontal();
                    if (!Other_Ints.ContainsKey("text_size"))
                    {
                        Other_Ints["text_size"] = 20;
                    }
                    GUILayout.Label($"<size={Other_Ints["text_size"]}>" + LocalizationManager.GetLocalizedText("modUI.textsize") + ":" + Other_Ints["text_size"].ToString() + "</size>", GUILayout.Width((float)(Other_Ints["text_size"]*10)), GUILayout.Height((float)(Other_Ints["text_size"]*1.5)));
                    if (GUILayout.Button($"<size={Other_Ints["text_size"]}>-</size>", GUILayout.Width((float)(Other_Ints["text_size"]*2.5)), GUILayout.Height((float)(Other_Ints["text_size"]*1.5))))
                    {
                        if (Other_Ints["text_size"] > 10)
                        {
                            Other_Ints["text_size"]--;
                        }
                        
                    }
                    if (GUILayout.Button($"<size={Other_Ints["text_size"]}>+</size>", GUILayout.Width((float)(Other_Ints["text_size"]*2.5)), GUILayout.Height((float)(Other_Ints["text_size"]*1.5))))
                    {
                        if (Other_Ints["text_size"] < 40)
                        {
                            Other_Ints["text_size"]++;
                        }
                    }

                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                    
                }
            }
        }
    }
}