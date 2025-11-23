using System;
using System.IO;
using System.Text.Json;

namespace ReActionAI.Integration.Revit.Config
{
    public sealed class AppConfig
    {
        public string? ApiKey { get; set; }
        public string? Model { get; set; }

        /// <summary>
        /// Заполняет значения по умолчанию, если они не указаны в конфиге.
        /// </summary>
        public void ApplyDefaults()
        {
            if (string.IsNullOrWhiteSpace(Model))
            {
                // Модель по умолчанию для Revit-ассистента
                Model = "gpt-4o-mini";
            }
        }
    }

    /// <summary>
    /// Отвечает за загрузку конфигурации RevitChatGPT.
    /// Ищет Config/appsettings.json рядом с dll плагина и
    /// при наличии переопределяет ApiKey из переменной окружения OPENAI_API_KEY.
    /// </summary>
    public static class ConfigProvider
    {
        private const string ConfigFileName = "appsettings.json";

        public static AppConfig Load()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var configDir = Path.Combine(baseDir, "Config");
            var path = Path.Combine(configDir, ConfigFileName);

            AppConfig config;

            if (!File.Exists(path))
            {
                // Если конфиг не найден – возвращаем пустой объект и дальше работаем с дефолтами.
                config = new AppConfig();
            }
            else
            {
                try
                {
                    var json = File.ReadAllText(path);
                    config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
                }
                catch
                {
                    // Любая ошибка чтения/разбора конфига не должна ломать плагин.
                    config = new AppConfig();
                }
            }

            // Если ключ задан в переменной окружения – он имеет приоритет.
            var envKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (!string.IsNullOrWhiteSpace(envKey))
            {
                config.ApiKey = envKey;
            }

            config.ApplyDefaults();
            return config;
        }
    }
}
