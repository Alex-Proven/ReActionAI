using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ReActionAI.Abstractions;

namespace ReActionAI.Modules.RevitChatGPT.Services
{
    public class ChatClient : IChatClient, IChatService
    {
        private readonly HttpClient _http = new HttpClient();
        private readonly string _apiKey;
        private readonly string _model;

        public ChatClient()
        {
            var cfg = Config.ConfigProvider.Load();
            _apiKey = cfg.ApiKey ?? "PUT_YOUR_OPENAI_KEY_HERE";
            _model = cfg.Model ?? "gpt-4o-mini";
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _http.Timeout = TimeSpan.FromSeconds(60);
        }

        public async Task<string> SendAsync(string prompt) => await AskAsync(prompt);

        public async Task<string> AskAsync(string input)
        {
            var payload = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant inside Autodesk Revit." },
                    new { role = "user", content = input }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var res = await _http.SendAsync(req);
                var body = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(body);
                var content = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                return content ?? "(empty)";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
