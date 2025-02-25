using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using Rewired.UI.ControlMapper;
using SA.GoogleDoc;
using System.Data;

public static class LocalizationManager
{
    public static Dictionary<string, Dictionary<string, string>> languageData = new Dictionary<string, Dictionary<string, string>>(); // 语言数据存储
    public static string currentLanguage = "en"; // 当前语言

    public static Dictionary<SystemLanguage, string> langlist= new Dictionary<SystemLanguage,string>
    {
        {
            SystemLanguage.English,
            "en"
        },
        {
            SystemLanguage.Korean,
            "ko"
        },
        {
            SystemLanguage.ChineseSimplified,
            "zh"
        },
        {
            SystemLanguage.ChineseTraditional,
            "zh-t"
        },
        {
            SystemLanguage.Spanish,
            "es"
        },
        {
            SystemLanguage.Portuguese,
            "pt"
        },
        {
            SystemLanguage.Japanese,
            "ja"
        },
        {
            SystemLanguage.Polish,
            "pl"
        },
        {
            SystemLanguage.Russian,
            "ru"
        },
        {
            SystemLanguage.Romanian,
            "ro"
        },
        {
            SystemLanguage.Vietnamese,
            "vi"
        },
        {
            SystemLanguage.French,
            "fr"
        },
        {
            SystemLanguage.Czech,
            "cs"
        },
        {
            SystemLanguage.German,
            "de"
        }
    };
    public static Dictionary<string, SystemLanguage> langCodeToLanguage = new Dictionary<string, SystemLanguage>
    {
        {
            "en",
            SystemLanguage.English
        },
        {
            "ko",
            SystemLanguage.Korean
        },
        {
            "zh",
            SystemLanguage.ChineseSimplified
        },
        {
            "zh-t",
            SystemLanguage.ChineseTraditional
        },
        {
            "es",
            SystemLanguage.Spanish
        },
        {
            "pt",
            SystemLanguage.Portuguese
        },
        {
            "ja",
            SystemLanguage.Japanese
        },
        {
            "pl",
            SystemLanguage.Polish
        },
        {
            "ru",
            SystemLanguage.Russian
        },
        {
            "ro",
            SystemLanguage.Romanian
        },
        {
            "vi",
            SystemLanguage.Vietnamese
        },
        {
            "fr",
            SystemLanguage.French
        },
        {
            "cs",
            SystemLanguage.Czech
        },
        {
            "de",
            SystemLanguage.German
        }
    };

    public static Dictionary<string, LangCode> langCodeToGoogleDocCode = new Dictionary<string, LangCode>
    {
        {
            "en",
            LangCode.English
        },
        {
            "ko",
            LangCode.Korean
        },
        {
            "zh",
            LangCode.ChineseSimplified
        },
        {
            "zh-t",
            LangCode.ChineseTraditional
        },
        {
            "es",
            LangCode.Spanish
        },
        {
            "pt",
            LangCode.Portuguese
        },
        {
            "ja",
            LangCode.Japanese
        },
        {
            "pl",
            LangCode.Polish
        },
        {
            "ru",
            LangCode.Russian
        },
        {
            "ro",
            LangCode.Romanian
        },
        {
            "vi",
            LangCode.Vietnamese
        },
        {
            "fr",
            LangCode.French
        },
        {
            "cs",
            LangCode.Czech
        },
        {
            "de",
            LangCode.German
        }
    };
    public static string GetLangCode(SystemLanguage lang)
    {
        if(langlist.ContainsKey(lang)) return langlist[lang];
        return "en";
    }
    public static SystemLanguage CodeToLang(string code)
    {
        if (langCodeToLanguage.ContainsKey(code)) return langCodeToLanguage[code];
        return SystemLanguage.English;
    }

    public static LangCode CodeToGoogleDocCode(string code)
    {
        if (langCodeToGoogleDocCode.ContainsKey(code)) return langCodeToGoogleDocCode[code];
        return LangCode.English;
    }
    public static void GetLanguages()
    {
        if (langlist.ContainsKey(RDString.language))
        {
            currentLanguage=langlist[RDString.language];
        }
    }

    public static void LoadLanguages(string filePath)
    {
        try
        {
            string jsonText = File.ReadAllText(filePath);
            languageData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonText);
            //Debug.Log("Languages loaded successfully!");
        }
        catch (JsonException e)
        {
            //Debug.LogError($"Failed to load languages: {e.Message}");
        }
    }

    // 获取对应语言的翻译
    public static string GetLocalizedText(string key , string lang = null)
    {
        if (lang == null)
        {
            lang = currentLanguage;
        }

        if (languageData == null)
        {
            //Debug.LogError("LanguageData is null!");
            return key;
        }

        if (languageData.TryGetValue(lang, out Dictionary<string, string> langDict))
        {
            if (langDict.TryGetValue(key, out string value))
            {
                return value;
            }
            else
            {
                //Debug.LogWarning($"Key '{key}' not found in current language '{lang}'.");
            }
        }
        else
        {
            //Debug.LogWarning($"Language '{lang}' not found.");
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
            //Debug.LogWarning($"Language '{langCode}' not found in LocalizationData.");
        }
    }
}