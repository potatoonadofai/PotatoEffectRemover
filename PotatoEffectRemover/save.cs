using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using static PER.Options;

public static class ConfigManager
{
    public static void SaveConfigs(List<Config> configs, int chosenConfig, Dictionary<string, bool> sets, Dictionary<string, int> ints, Dictionary<string, string> strings, string filePath)
    {
        // 创建一个容器对象，用于存储 configs 和 chosenConfig
        var data = new
        {
            ChosenConfig = chosenConfig,
            Configs = configs,
            Sets = sets,
            Ints=ints,
            Strings=strings
        };

        // 将数据序列化为 JSON 字符串
        string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);

        // 确保文件夹存在
        string directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 将 JSON 字符串写入文件
        File.WriteAllText(filePath, jsonString);
    }

    public static void LoadConfigs(out List<Config> configs, out int chosenConfig, out Dictionary<string, bool> sets, out Dictionary<string, int> ints, out Dictionary<string, string> strings, string filePath)
    {
        configs = new List<Config>();
        chosenConfig = 0;
        sets = new Dictionary<string, bool>();
        ints = new Dictionary<string, int>();
        strings = new Dictionary<string, string>();

        if (!File.Exists(filePath))
        {
            Debug.LogError("File not found: " + filePath);
            return;
        }

        // 读取文件内容
        string jsonString = File.ReadAllText(filePath);

        try
        {
            // 反序列化 JSON 数据
            var data = JsonConvert.DeserializeObject<ConfigData>(jsonString);
            try
            {
                chosenConfig = data.ChosenConfig;
            }
            catch (JsonException e)
            {
                Debug.LogError("Error loading data: " + e.Message);
                chosenConfig = 0;
            }

            try
            {
                configs = data.Configs;
            }
            catch (JsonException e)
            {
                Debug.LogError("Error loading data: " + e.Message);
                configs = new List<Config>();
            }

            try
            {
                sets = data.Sets;
            }
            catch (JsonException e)
            {
                Debug.LogError("Error loading data: " + e.Message);
                sets = new Dictionary<string, bool>();
            }

            try
            {
                ints = data.Ints;
            }
            catch (JsonException e)
            {
                Debug.LogError("Error loading data: " + e.Message);
                ints = new Dictionary<string, int>();
            }

            try
            {
                strings = data.Strings;
            }
            catch (JsonException e)
            {
                Debug.LogError("Error loading data: " + e.Message);
                strings = new Dictionary<string, string>();
            }

        }
        catch (JsonException e)
        {
            Debug.LogError("Error parsing JSON file: " + e.Message);
            configs = new List<Config>();
            chosenConfig = 0;
            sets = new Dictionary<string, bool>();
            ints = new Dictionary<string, int>();
            strings = new Dictionary<string, string>();
        }
    }

    // 辅助类，用于反序列化
    private class ConfigData
    {
        public int ChosenConfig { get; set; }
        public List<Config> Configs { get; set; }
        public Dictionary<string, bool> Sets { get; set; }
        public Dictionary<string, int> Ints { get; set; }
        public Dictionary<string, string> Strings { get; set; }
    }
}