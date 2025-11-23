using System;
using System.IO;
using System.Text.Json;

namespace ReActionAI.Integration.Revit.Config
{
    public sealed class AppConfig
    {
        public string? ApiKey { get; set; }
        public string? Model { get; set; }

        public void ApplyDefaults()
        {
            if (string.IsNullOrWhiteSpace(Model))
                Model = "gpt-4o-mini";
        }
    }

    public static class ConfigProvider
    {
        private const string ConfigFileName = "reactionai.config.json";

        public static AppConfig Load()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appData, "ReActionAI");
            var path = Path.Combine(folder, ConfigFileName);

            if (!File.Exists(path))
            {
                Directory.CreateDirectory(folder);
                var cfg = new AppConfig();
                cfg.ApplyDefaults();

                var json = JsonSerializer.Serialize(cfg, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(path, json);
                return cfg;
            }

            try
            {
                var json = File.ReadAllText(path);
                var config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();

                var envKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
                if (!string.IsNullOrWhiteSpace(envKey))
                {
                    config.ApiKey = envKey;
                }

                config.ApplyDefaults();
                return config;
            }
            catch
            {
                var cfg = new AppConfig();
                cfg.ApplyDefaults();
                return cfg;
            }
        }
    }
}
