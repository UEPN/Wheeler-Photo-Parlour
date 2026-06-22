using System;
using System.IO;
using System.Text.Json;

namespace WheelerPhotoParlour.Services
{
    /// <summary>
    /// 应用设置的读写。使用 JSON 持久化到用户本地数据目录下的单一设置文件中。
    /// </summary>
    public static class ConfigService
    {
        private static readonly string SettingsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WheelerPhotoParlour");

        private static readonly string SettingsFilePath = Path.Combine(SettingsFolder, "settings.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public static AppConfig Config { get; private set; } = CreateDefault();

        private static AppConfig CreateDefault()
        {
            return new AppConfig
            {
                FirstRun = true,
                SourcePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "Rockstar Games",
                    "Red Dead Redemption 2",
                    "Profiles")
            };
        }

        public static void Load()
        {
            if (!File.Exists(SettingsFilePath)) return;

            try
            {
                var json = File.ReadAllText(SettingsFilePath);
                var loaded = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions);
                if (loaded != null)
                {
                    Config = loaded;
                }
            }
            catch
            {
                // 设置文件损坏或格式不符时，保留内存中的默认值，不影响程序继续运行
            }
        }

        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(SettingsFolder);
                var json = JsonSerializer.Serialize(Config, JsonOptions);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch
            {
                // 写入失败（例如磁盘只读）时静默忽略，不影响主流程
            }
        }
    }

    public class AppConfig
    {
        /// <summary>是否为本机首次启动本程序</summary>
        public bool FirstRun { get; set; } = true;

        /// <summary>照片存档所在文件夹</summary>
        public string SourcePath { get; set; } = "";

        /// <summary>界面语言："zh" 为中文，"en" 为英文。默认中文。</summary>
        public string Language { get; set; } = "zh";
    }
}
