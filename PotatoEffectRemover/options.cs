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

        public static List<Config> configs = new List<Config>();
        public static int chosen_config = 0;

        public static bool if_main = true;
        public static bool if_rename = false;
        public static Vector2 scrollPosition = Vector2.zero;

        public static string inputText = "";

        public static List<LevelEventType> levelEvents = new List<LevelEventType>();

        public static void GetEvent()
        {
            levelEvents.Clear();
            foreach (ADOFAI.LevelEventType eventType in Enum.GetValues(typeof(ADOFAI.LevelEventType)))
            {
                levelEvents.Add(eventType);
            }
        }
        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            while (chosen_config >= configs.Count)
            {
                chosen_config--;
            }
            if (!if_main)
            {
                if (GUILayout.Button("<size=20><保存并返回</size>", GUILayout.Width(150), GUILayout.Height(30)))
                {
                    if_main = true;
                    ConfigManager.SaveConfigs(configs, chosen_config, Path.Combine(modEntry.Path, "Configs.json"));
                }

                if (GUILayout.Button($"<size=20>筛选模式:{(configs[chosen_config].if_blacklist ? "黑名单" : "白名单")}</size>", GUILayout.Width(150), GUILayout.Height(30)))
                {
                    configs[chosen_config].if_blacklist = !configs[chosen_config].if_blacklist;
                }

                scrollPosition = GUILayout.BeginScrollView(scrollPosition);

                foreach (ADOFAI.LevelEventType eventType in levelEvents)
                {
                    if (!configs[chosen_config].events.ContainsKey(eventType.ToString()))
                    {
                        configs[chosen_config].events[eventType.ToString()] = false;
                    }
                    GUILayout.BeginHorizontal();
                    configs[chosen_config].events[eventType.ToString()]=GUILayout.Toggle(configs[chosen_config].events[eventType.ToString()],$"<size=20>{eventType}</size>");
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("<size=20>Potato Effect Remover</size>");
                GUILayout.Label("<color=#FF2222><size=20> 注意！编辑器内保存功能将会被禁用！</size></color>", Array.Empty<GUILayoutOption>());

                if (GUILayout.Button($"<size=20>当前配置:{(chosen_config>=0 && chosen_config < configs.Count ? configs[chosen_config].name : "无")}</size>", GUILayout.Width(500), GUILayout.Height(30)))
                {
                    popup = !popup;
                }

                if (popup)
                {
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                    for (int i = 0; i < configs.Count; i++)
                    {
                        if (GUILayout.Button($"<size=20>{configs[i].name}</size>", GUILayout.Width(500), GUILayout.Height(30)))
                        {
                            chosen_config = i;
                            popup = false;
                            ConfigManager.SaveConfigs(configs, chosen_config, Path.Combine(modEntry.Path, "Configs.json"));
                        }
                    }
                    GUILayout.EndScrollView();
                }
                
                if (GUILayout.Button("<size=20>新建</size>", GUILayout.Width(100), GUILayout.Height(30)))
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
                    if (GUILayout.Button("<size=20>重命名选中项</size>", GUILayout.Width(150), GUILayout.Height(30)))
                    {
                        if_rename = true;
                    }
                }
                else
                {
                    GUILayout.Label("<size=20>重命名为:</size>");
                    inputText = GUILayout.TextField(inputText);
                    if (GUILayout.Button("<size=20>确定</size>", GUILayout.Width(150), GUILayout.Height(30)))
                    {
                        if (inputText != "")
                        {
                            configs[chosen_config].name = inputText;
                        }
                        if_rename = false;
                        ConfigManager.SaveConfigs(configs, chosen_config, Path.Combine(modEntry.Path, "Configs.json"));
                    }
                }
                GUILayout.EndHorizontal();
                if (GUILayout.Button("<size=20>编辑选中项</size>", GUILayout.Width(150), GUILayout.Height(30)))
                {
                    if_main = false;
                }
                if (GUILayout.Button("<size=20>删除选中项</size>", GUILayout.Width(150), GUILayout.Height(30)))
                {
                    configs.RemoveAt(chosen_config);
                    while(chosen_config>=configs.Count)
                    {
                        chosen_config --;
                    }
                    ConfigManager.SaveConfigs(configs, chosen_config, Path.Combine(modEntry.Path, "Configs.json"));
                }
                GUI.enabled = true;
            }         
        }
    }
}
