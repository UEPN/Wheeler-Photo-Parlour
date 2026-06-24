using System;
using System.IO;
using System.Text.Json;

namespace WheelerPhotoParlour.Services
{
    /// <summary>应用设置读写，JSON持久化。</summary>
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
                // 设置文件损坏时保留默认值
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
                // 写入失败静默忽略
            }
        }
    }

    public class AppConfig
    {
        /// <summary>是否首次启动。</summary>
        public bool FirstRun { get; set; } = true;

        /// <summary>照片存档路径。</summary>
        public string SourcePath { get; set; } = "";

        /// <summary>界面语言："zh" / "en"。</summary>
        public string Language { get; set; } = "zh";
    }
}
