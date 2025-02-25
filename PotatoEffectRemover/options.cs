using System;
using System.Collections.Generic;
using UnityEngine;
using UnityModManagerNet;
using SkyHook;
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

        public static void Reset()
        {
            popup = false;
            if_main = true;
            if_rename = false;
            scrollPosition = Vector2.zero;
            if_fixs = false;
            langpopup = false;
            if_UIsets=false;
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
        }
        public static void OnGUI(UnityModManager.ModEntry modEntry)
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
                eventtext[Other_Strings["currlanguage"]]=new Dictionary<string, string>();
            }

            if (Other_Settings["follow_game_language"])
            {
                LocalizationManager.GetLanguages();
            }

            if (LocalizationManager.currentLanguage!=Other_Strings["currlanguage"])
            {
                LocalizationManager.SwitchLanguage(Other_Strings["currlanguage"]);
            }

            while (chosen_config >= configs.Count)
            {
                chosen_config--;
            }
            if (!if_main)
            {
                if (GUILayout.Button($"<size={Other_Ints["text_size"]}><"+LocalizationManager.GetLocalizedText("editor.save")+"</size>", GUILayout.Width((float)(Other_Ints["text_size"]*15)), GUILayout.Height((float)(Other_Ints["text_size"]*1.5))))
                {
                    if_main = true;
                }

                if (GUILayout.Button($"<size={Other_Ints["text_size"]}>"+LocalizationManager.GetLocalizedText("editor.filtering_mode") +":"+(configs[chosen_config].if_blacklist ? LocalizationManager.GetLocalizedText("editor.blacklist") : LocalizationManager.GetLocalizedText("editor.whitelist")) +"</size>", GUILayout.Width((float)(Other_Ints["text_size"]*25)), GUILayout.Height((float)(Other_Ints["text_size"]*1.5))))
                {
                    configs[chosen_config].if_blacklist = !configs[chosen_config].if_blacklist;
                }

                scrollPosition = GUILayout.BeginScrollView(scrollPosition);

                foreach (ADOFAI.LevelEventType eventType in levelEvents)
                {
                    if(eventType != LevelEventType.None && !EditorConstants.settingsTypes.Contains(eventType))
                    {
                        string key = "editor." + eventType.ToString();
                        string text = "";
                        if (RDString.Get("editor." + eventType.ToString(), null, LangSection.Translations) == "")
                        {
                            continue;
                        }
                        if (eventtext[Other_Strings["currlanguage"]].ContainsKey(key))
                        {
                            text= eventtext[Other_Strings["currlanguage"]][key];
                        }
                        else
                        {
                            text = Localization.GetLocalizedString(key, LangSection.Translations, LocalizationManager.CodeToGoogleDocCode(Other_Strings["currlanguage"]));
                            eventtext[Other_Strings["currlanguage"]][key] = text;
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
                    GUILayout.Label($"<size={Other_Ints["text_size"]}>"+ LocalizationManager.GetLocalizedText("main.renameto") + ":</size>");
                    inputText = GUILayout.TextField(inputText);
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
                GUI.enabled = true;

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
                    if (!Other_Settings.ContainsKey("tile_opti"))
                    {
                        Other_Settings["tile_opti"] = false;
                    }
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
