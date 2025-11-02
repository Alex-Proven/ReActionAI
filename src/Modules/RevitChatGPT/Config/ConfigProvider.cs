using System.IO;
using System.Text.Json;

namespace ReActionAI.Modules.RevitChatGPT.Config
{
    public class AppConfig
    {
        public string? ApiKey { get; set; }
        public string? Model { get; set; }
    }

    public static class ConfigProvider
    {
        public static AppConfig Load()
        {
            var path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config", "appsettings.json");
            if (!File.Exists(path))
                return new AppConfig();

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }
    }
}
