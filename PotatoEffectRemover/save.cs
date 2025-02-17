using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using static PER.Options;

public static class ConfigManager
{
    public static void SaveConfigs(List<Config> configs, int chosenConfig, string filePath)
    {
        // 创建一个容器对象，用于存储 configs 和 chosenConfig
        var data = new
        {
            ChosenConfig = chosenConfig,
            Configs = configs
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

    public static void LoadConfigs(out List<Config> configs, out int chosenConfig, string filePath)
    {
        configs = new List<Config>();
        chosenConfig = 0;

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

            // 将数据赋值给 configs 和 chosenConfig
            chosenConfig = data.ChosenConfig;
            configs = data.Configs;
        }
        catch (JsonException e)
        {
            Debug.LogError("Error parsing JSON file: " + e.Message);
        }
    }

    // 辅助类，用于反序列化
    private class ConfigData
    {
        public int ChosenConfig { get; set; }
        public List<Config> Configs { get; set; }
    }
}