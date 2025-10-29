using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ReActionAI.Modules.RevitChatGPT.UI
{
    public partial class ChatWindow : Window
    {
        public ChatWindow() => InitializeComponent();

        private void AppendMessage(string senderName, string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            var bubble = new TextBlock
            {
                Text = $"{senderName}: {message}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(4),
                Foreground = Brushes.White
            };

            ChatHistoryPanel.Children.Add(bubble);
            ChatScrollViewer.ScrollToEnd();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var userMessage = UserInputBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(userMessage)) return;

            AppendMessage("Вы", userMessage!);
            UserInputBox.Text = string.Empty;

            AppendMessage("ChatGPT", "Принято. Ответ формируется...");
        }

        private void UserInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                SendButton_Click(sender, e);
                e.Handled = true;
            }
        }
    }
}