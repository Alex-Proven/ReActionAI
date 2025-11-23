using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ReActionAI.Abstractions;
using ReActionAI.Integration.Revit.Config;

namespace ReActionAI.Integration.Revit.Services
{
    public class ChatClient : IChatClient, IChatService
    {
        private const string DefaultModel = "gpt-4o-mini";
        private const string PlaceholderApiKey = "PUT_YOUR_OPENAI_KEY_HERE";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly HttpClient _http = default!;
        private readonly string _apiKey = string.Empty;
        private readonly string _model = DefaultModel;

        public ChatClient()
            : this(ConfigProvider.Load())
        {
        }

        public ChatClient(AppConfig config)
        {
            config ??= new AppConfig();
            config.ApplyDefaults();

            var apiKey = config.ApiKey;
            var envKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (!string.IsNullOrWhiteSpace(envKey))
                apiKey = envKey;

            _apiKey = apiKey ?? string.Empty;
            _model = string.IsNullOrWhiteSpace(config.Model) ? DefaultModel : config.Model!;

            _http = new()
            {
                BaseAddress = new Uri("https://api.openai.com/v1/"),
                Timeout = TimeSpan.FromSeconds(45)
            };
        }

        public Task<string> SendAsync(string prompt) => AskAsync(prompt);

        public async Task<string> AskAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            if (!IsApiKeyConfigured())
            {
                return "Ошибка конфигурации: OpenAI API key не настроен. " +
                       "Укажите ключ в файле Config/appsettings.json в поле \"ApiKey\".";
            }

            var request = new ChatCompletionRequest
            {
                Model = _model,
                Messages =
                [
                    new ChatMessage
                    {
                        Role = "system",
                        Content = "You are a helpful assistant inside Autodesk Revit."
                    },
                    new ChatMessage
                    {
                        Role = "user",
                        Content = input
                    }
                ],
                Temperature = 0.2
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(request, JsonOptions),
                    Encoding.UTF8,
                    "application/json")
            };

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            httpRequest.Headers.Accept.Clear();
            httpRequest.Headers.Accept.ParseAdd("application/json");

            HttpResponseMessage response;
            try
            {
                response = await _http.SendAsync(httpRequest).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                return "Ошибка: превышено время ожидания ответа от сервера OpenAI.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }

            var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var message = $"Error: OpenAI API вернул статус {(int)response.StatusCode} ({response.ReasonPhrase}).";

                try
                {
                    var errorDoc = JsonSerializer.Deserialize<OpenAiErrorResponse>(responseText, JsonOptions);
                    var apiMessage = errorDoc?.Error?.Message;
                    if (!string.IsNullOrWhiteSpace(apiMessage))
                        message += " " + apiMessage;
                }
                catch
                {
                    // ignore
                }

                return message;
            }

            try
            {
                var completion = JsonSerializer.Deserialize<ChatCompletionResponse>(responseText, JsonOptions);
                var choices = completion?.Choices;

                if (choices != null && choices.Length > 0)
                {
                    var msg = choices[0]?.Message;
                    var content = msg?.Content;

                    if (!string.IsNullOrWhiteSpace(content))
                        return content!.Trim();
                }

                return "(empty)";
            }
            catch (Exception ex)
            {
                return $"Error: failed to parse OpenAI response. {ex.Message}";
            }
        }

        private bool IsApiKeyConfigured()
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                return false;

            if (string.Equals(_apiKey.Trim(), PlaceholderApiKey, StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        #region DTOs

        private sealed class ChatCompletionRequest
        {
            [JsonPropertyName("model")]
            public string? Model { get; set; }

            [JsonPropertyName("messages")]
            public ChatMessage[] Messages { get; set; } = [];

            [JsonPropertyName("temperature")]
            public double Temperature { get; set; }
        }

        private sealed class ChatMessage
        {
            [JsonPropertyName("role")]
            public string? Role { get; set; }

            [JsonPropertyName("content")]
            public string? Content { get; set; }
        }

        private sealed class ChatCompletionResponse
        {
            [JsonPropertyName("choices")]
            public ChatChoice[]? Choices { get; set; }
        }

        private sealed class ChatChoice
        {
            [JsonPropertyName("message")]
            public ChatMessage? Message { get; set; }
        }

        private sealed class OpenAiErrorResponse
        {
            [JsonPropertyName("error")]
            public OpenAiError? Error { get; set; }
        }

        private sealed class OpenAiError
        {
            [JsonPropertyName("message")]
            public string? Message { get; set; }
        }

        #endregion
    }
}
