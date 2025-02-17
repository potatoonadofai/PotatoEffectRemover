using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using Rewired.UI.ControlMapper;
using SA.GoogleDoc;

public static class LocalizationManager
{
    public static Dictionary<string, Dictionary<string, string>> languageData = new Dictionary<string, Dictionary<string, string>>(); // 语言数据存储
    public static string currentLanguage="en"; // 当前语言

    public static void GetLanguages()
    {
        switch (RDString.language)
        {
            case SystemLanguage.ChineseSimplified:
                currentLanguage = "zh";
                break;
            case SystemLanguage.ChineseTraditional:
                currentLanguage = "zh-t";
                break;
            case SystemLanguage.English:
                currentLanguage = "en";
                break;
            case SystemLanguage.Korean:
                currentLanguage = "ko";
                break;
            case SystemLanguage.Japanese:
                currentLanguage = "ja";
                break;
            case SystemLanguage.Spanish:
                currentLanguage = "es";
                break;
            case SystemLanguage.Portuguese:
                currentLanguage = "pt";
                break;
            case SystemLanguage.French:
                currentLanguage = "fr";
                break;
            case SystemLanguage.Polish:
                currentLanguage = "pl";
                break;
            case SystemLanguage.Romanian:
                currentLanguage = "ro";
                break;
            case SystemLanguage.Russian:
                currentLanguage = "ru";
                break;
            case SystemLanguage.Vietnamese:
                currentLanguage = "vi";
                break;
            case SystemLanguage.Czech:
                currentLanguage = "cs";
                break;
            case SystemLanguage.German:
                currentLanguage = "de";
                break;
        }
    }

    public static void LoadLanguages(string filePath)
    {
        try
        {
            string jsonText = File.ReadAllText(filePath);
            languageData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonText);
            Debug.Log("Languages loaded successfully!");
        }
        catch (JsonException e)
        {
            Debug.LogError($"Failed to load languages: {e.Message}");
        }
    }

    // 获取对应语言的翻译
    public static string GetLocalizedText(string key)
    {
        if (languageData == null)
        {
            Debug.LogError("LanguageData is null!");
            return key;
        }

        if (languageData.TryGetValue(currentLanguage, out Dictionary<string, string> langDict))
        {
            if (langDict.TryGetValue(key, out string value))
            {
                return value;
            }
            else
            {
                Debug.LogWarning($"Key '{key}' not found in current language '{currentLanguage}'.");
            }
        }
        else
        {
            Debug.LogWarning($"Language '{currentLanguage}' not found.");
        }

        // 如果找不到，返回默认值
        return key;
    }

    // 切换语言
    public static void SwitchLanguage(string langCode)
    {
        if (languageData.ContainsKey(langCode))
        {
            currentLanguage = langCode;
            Debug.Log($"Language switched to {langCode}.");
        }
        else
        {
            Debug.LogWarning($"Language '{langCode}' not found in LocalizationData.");
        }
    }
}