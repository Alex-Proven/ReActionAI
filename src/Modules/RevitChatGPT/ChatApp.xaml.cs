using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace ReActionAI.Modules.RevitChatGPT
{
    /// <summary>
    /// WPF-приложение для окна клиента ChatGPT. НЕ путать с Revit App.cs.
    /// </summary>
    public partial class ChatApp : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Пример: считаем при запуске API-ключ (если есть).
            // Можно убрать, если не нужен.
            string? apiKey = TryReadSetting("OpenAI:ApiKey");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                // тихо игнорируем; окно всё равно откроется
            }

            // Окно задаётся через StartupUri в XAML (UI/ChatWindow.xaml).
            // Если хочешь создавать вручную:
            // var win = new UI.ChatWindow();
            // win.Show();
        }

        private static string? TryReadSetting(string key)
        {
            try
            {
                var configPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Config", "appsettings.json");

                if (!File.Exists(configPath))
                    return null;

                using var doc = JsonDocument.Parse(File.ReadAllText(configPath));
                var cursor = doc.RootElement;

                foreach (var part in key.Split(':'))
                {
                    if (cursor.ValueKind == JsonValueKind.Object &&
                        cursor.TryGetProperty(part, out var next))
                    {
                        cursor = next;
                    }
                    else return null;
                }

                return cursor.ValueKind == JsonValueKind.String ? cursor.GetString() : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
