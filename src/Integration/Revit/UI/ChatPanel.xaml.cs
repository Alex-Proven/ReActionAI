using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ReActionAI.Integration.Revit.Services;

namespace ReActionAI.Integration.Revit.UI
{
    public partial class ChatPanel : UserControl
    {
        private readonly IChatClient _chatClient;

        public ChatPanel()
        {
            InitializeComponent();

            try
            {
                // Основной клиент, работающий через OpenAI
                _chatClient = new ChatClient();
            }
            catch (Exception ex)
            {
                // Если ChatClient упал при инициализации – не ломаем загрузку плагина
                _chatClient = new FallbackChatClient(ex.Message);

                AddMessage(
                    "Система",
                    "Не удалось инициализировать подключение к ассистенту. " +
                    "Панель работает в демонстрационном режиме (ответы формируются локально)."
                );
            }

            // Приветствие от Феникса
            AddMessage(
                "Феникс",
                "Привет! Я ассистент ReActionAI. Напишите вопрос по Revit или BIM."
            );
        }

        private void InputBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            InputPlaceholder.Visibility =
                string.IsNullOrWhiteSpace(InputBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private async void SendButton_OnClick(object sender, RoutedEventArgs e)
        {
            var text = InputBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return;

            // сообщение пользователя
            AddMessage("Вы", text);
            InputBox.Text = string.Empty;

            try
            {
                SendButton.IsEnabled = false;

                var reply = await _chatClient.SendAsync(text);

                if (string.IsNullOrWhiteSpace(reply))
                    reply = "Ответ от ассистента не получен.";

                AddMessage("Феникс", reply);
            }
            catch (Exception ex)
            {
                AddMessage(
                    "Система",
                    "Ошибка при обращении к ассистенту: " + ex.Message
                );
            }
            finally
            {
                SendButton.IsEnabled = true;
            }
        }

        // --- Публичный API панели (используется ChatPanelProvider) ---

        public void AddMessage(string author, string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            var block = new TextBlock
            {
                Text = $"{author}: {message}",
                Margin = new Thickness(0, 6, 0, 6),
                TextWrapping = TextWrapping.Wrap
            };

            MessagesPanel.Children.Add(block);
            MessagesScrollViewer.ScrollToEnd();
        }

        // --- Внутренний запасной клиент, если ChatClient не поднялся ---

        private sealed class FallbackChatClient : IChatClient
        {
            private readonly string _reason;

            public FallbackChatClient(string reason)
            {
                _reason = reason ?? string.Empty;
            }

            public Task<string> SendAsync(string prompt)
            {
                var msg =
                    "Сервис ассистента сейчас недоступен.\n" +
                    "Плагин загружен, но ChatClient не инициализировался.\n";

                if (!string.IsNullOrWhiteSpace(_reason))
                    msg += "Техническая информация: " + _reason;

                return Task.FromResult(msg);
            }
        }
    }
}
