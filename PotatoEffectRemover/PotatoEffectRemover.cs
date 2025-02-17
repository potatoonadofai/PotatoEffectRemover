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
using System.IO;
using ADOFAI.Editor.Actions;
using System.Reflection;


namespace PER
{
    public static class Main
    {
        public static UnityModManager.ModEntry mod;
        public static void Load(UnityModManager.ModEntry modEntry)
        {
            mod = modEntry;
            mod.OnToggle = new Func<UnityModManager.ModEntry, bool, bool>(OnToggle);

            mod.OnGUI = Options.OnGUI;
            Options.GetEvent();

            ConfigManager.LoadConfigs(out Options.configs, out Options.chosen_config, Path.Combine(modEntry.Path, "Configs.json"));
            LocalizationManager.LoadLanguages(modEntry.Path + "text.json");
        }
        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool isToggled)
        {
            var harmony = new Harmony(modEntry.Info.Id);// 创建唯一标识的 Harmony 实例
            if (isToggled)
            {
                harmony.PatchAll();
            }
            else
            {
                harmony.UnpatchAll(modEntry.Info.Id);
            }
            return true;
        }

        [HarmonyPatch(typeof(LevelData), "Decode")]

        public class DecodePatch
        {
            public static void Postfix(LevelData __instance, Dictionary<string, object> dict)
            {
                if (mod.Enabled)
                {
                    if(Options.chosen_config < 0 || Options.chosen_config >= Options.configs.Count)
                    {
                        //status = LoadResult.Successful;
                        return;
                    }
                    int i = 0;
                    while (i < __instance.levelEvents.Count)
                    {
                        string eventname = __instance.levelEvents[i].eventType.ToString();
                        //Debug.Log(eventname);
                        if (Options.configs[Options.chosen_config].events.ContainsKey(eventname))
                        {
                            if (Options.configs[Options.chosen_config].events[eventname] == Options.configs[Options.chosen_config].if_blacklist)
                            {
                                __instance.levelEvents.RemoveAt(i);
                                continue;
                            }
                        }
                        else
                        {
                            if (Options.configs[Options.chosen_config].if_blacklist == false)
                            {
                                __instance.levelEvents.RemoveAt(i);
                                continue;
                            }
                        }
                        i++;
                    }
                    i = 0;
                    while (i < __instance.decorations.Count)
                    {
                        string eventname = __instance.decorations[i].eventType.ToString();
                        //Debug.Log(eventname);
                        if (Options.configs[Options.chosen_config].events.ContainsKey(eventname))
                        {
                            if(Options.configs[Options.chosen_config].events[eventname] == Options.configs[Options.chosen_config].if_blacklist)
                            {
                                __instance.decorations.RemoveAt(i);
                                continue;
                            }
                        }
                        else
                        {
                            if (Options.configs[Options.chosen_config].if_blacklist==false)
                            {
                                __instance.decorations.RemoveAt(i);
                                continue;
                            }
                        }
                        i++;
                    }
                }
                //status = LoadResult.Successful;
            }
        }

        [HarmonyPatch(typeof(SaveLevelEditorAction), "Execute")]
        private class DisableSave
        {
            // Token: 0x06000012 RID: 18 RVA: 0x00002A6C File Offset: 0x00000C6C
            private static bool Prefix()
            {
                if(mod.Enabled)
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(scnEditor), "OpenLevelCo")]
        private class DisableSaveButton
        {
            // Token: 0x06000010 RID: 16 RVA: 0x000029FC File Offset: 0x00000BFC
            private static void Postfix(scnEditor __instance)
            {
                __instance.buttonSave.interactable = false;
            }
        }
    }
}
