using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ReActionAI.Modules.RevitChatGPT.UI
{
    /// <summary>
    /// Логика для панели чата ReActionAI.
    /// </summary>
    public partial class ChatPanel : UserControl
    {
        private const double InputMinHeight = 32.0;
        private const double InputMaxHeight = 120.0;

        public ChatPanel()
        {
            InitializeComponent();

            ApplyRevitTheme(isDark: false);

            if (InputBox != null)
            {
                InputBox.GotFocus += InputBox_GotFocus;
                InputBox.LostFocus += InputBox_LostFocus;
                InputBox.TextChanged += InputBox_TextChanged;
                InputBox.KeyDown += InputBox_KeyDown;
            }

            if (SendButton != null)
                SendButton.Click += SendButton_Click;

            if (PlusButton != null)
                PlusButton.Click += PlusButton_Click;

            UpdatePlaceholderVisibility();
            UpdateInputHeight();
        }

        #region Обработка ввода

        private void InputBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility();
        }

        private void InputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility();
        }

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholderVisibility();
            UpdateInputHeight();
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Shift) == 0)
            {
                e.Handled = true;
                SendMessageSafe();
            }
        }

        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Кнопка + нажата");
            }
            catch
            {
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessageSafe();
        }

        private void SendMessageSafe()
        {
            try
            {
                SendMessage();
            }
            catch
            {
            }
        }

        private void SendMessage()
        {
            if (InputBox == null)
                return;

            var text = InputBox.Text?.Trim();
            if (string.IsNullOrEmpty(text))
                return;

            AddUserMessageSafe(text);

            InputBox.Text = string.Empty;
            UpdatePlaceholderVisibility();
            UpdateInputHeight();

            AddBotMessageSafe("Ответ ассистента (заглушка). Интеграция с ядром будет добавлена позже.");
        }

        #endregion

        #region Добавление сообщений

        private void AddUserMessageSafe(string text)
        {
            try
            {
                AddUserMessage(text);
            }
            catch
            {
            }
        }

        private void AddBotMessageSafe(string text)
        {
            try
            {
                AddBotMessage(text);
            }
            catch
            {
            }
        }

        private void AddUserMessage(string text)
        {
            if (text != null && text.Length > 500)
                text = text.Substring(0, 500) + "...";

            text = ReActionAI.Modules.RevitChatGPT.Text.RussianHyphenator.Hyphenate(text);

            var bubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(220, 240, 255)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 10),
                Child = new TextBlock
                {
                    Text = text,
                    Foreground = Brushes.Black,
                    TextWrapping = TextWrapping.Wrap
                }
            };

            if (MessagesPanel != null)
                MessagesPanel.Children.Add(bubble);

            ScrollToBottom();
        }

        private void AddBotMessage(string text)
        {
            if (text != null && text.Length > 500)
                text = text.Substring(0, 500) + "...";

            text = ReActionAI.Modules.RevitChatGPT.Text.RussianHyphenator.Hyphenate(text);

            var bubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 10),
                Child = new TextBlock
                {
                    Text = text,
                    Foreground = Brushes.Black,
                    TextWrapping = TextWrapping.Wrap
                }
            };

            if (MessagesPanel != null)
                MessagesPanel.Children.Add(bubble);

            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            try
            {
                if (MessagesScrollViewer != null)
                    MessagesScrollViewer.ScrollToEnd();
            }
            catch
            {
            }
        }

        #endregion

        #region Плейсхолдер и высота ввода

        private void UpdatePlaceholderVisibility()
        {
            if (InputPlaceholder == null || InputBox == null)
                return;

            var hasText = !string.IsNullOrWhiteSpace(InputBox.Text);
            var hasFocus = InputBox.IsKeyboardFocused;

            InputPlaceholder.Visibility =
                (hasText || hasFocus) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void UpdateInputHeight()
        {
            if (InputBorder == null || InputBox == null)
                return;

            var lines = Math.Max(1, InputBox.LineCount);
            var lineHeight = InputBox.FontSize + 6.0;
            var desiredHeight = Math.Max(InputMinHeight, lines * lineHeight);

            if (desiredHeight > InputMaxHeight)
                desiredHeight = InputMaxHeight;

            var currentHeight = InputBorder.Height;
            if (double.IsNaN(currentHeight) || currentHeight <= 0)
                currentHeight = InputMinHeight;

            if (Math.Abs(desiredHeight - currentHeight) < 0.5)
            {
                InputBorder.Height = desiredHeight;
                return;
            }

            var animation = new DoubleAnimation
            {
                From = currentHeight,
                To = desiredHeight,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            InputBorder.BeginAnimation(
                FrameworkElement.HeightProperty,
                animation,
                HandoffBehavior.SnapshotAndReplace);
        }

        #endregion

        #region Тема

        private void ApplyRevitTheme(bool isDark)
        {
            if (InputBorder != null)
            {
                InputBorder.MinHeight = InputMinHeight;
                InputBorder.Height = double.NaN;
                // ВАЖНО: прижимаем контейнер к НИЗУ ячейки футера
                InputBorder.VerticalAlignment = VerticalAlignment.Bottom;
            }

            var borderBrush = isDark
                ? new SolidColorBrush(Color.FromRgb(85, 85, 85))
                : new SolidColorBrush(Color.FromRgb(208, 208, 208));

            if (InputBorder != null)
                InputBorder.BorderBrush = borderBrush;
            if (PlusButton != null)
                PlusButton.BorderBrush = borderBrush;
            if (SendButton != null)
                SendButton.BorderBrush = borderBrush;

            if (isDark)
            {
                if (InputBox != null)
                    InputBox.Foreground = Brushes.White;
                if (InputPlaceholder != null)
                    InputPlaceholder.Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180));

                if (InputBorder != null)
                    InputBorder.Background = new SolidColorBrush(Color.FromRgb(50, 50, 50));
                if (PlusButton != null)
                    PlusButton.Background = new SolidColorBrush(Color.FromRgb(45, 45, 45));
                if (SendButton != null)
                    SendButton.Background = Brushes.White;

                if (SendButton != null && SendButton.Content is TextBlock tbDark)
                    tbDark.Foreground = Brushes.Black;
            }
            else
            {
                if (InputBox != null)
                    InputBox.Foreground = Brushes.Black;
                if (InputPlaceholder != null)
                    InputPlaceholder.Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 136));

                if (InputBorder != null)
                    InputBorder.Background = Brushes.White;
                if (PlusButton != null)
                    PlusButton.Background = new SolidColorBrush(Color.FromRgb(242, 242, 242));
                if (SendButton != null)
                    SendButton.Background = new SolidColorBrush(Color.FromRgb(242, 242, 242));

                if (SendButton != null && SendButton.Content is TextBlock tbLight)
                    tbLight.Foreground = Brushes.Black;
            }

            UpdateInputHeight();
        }

        #endregion
    }
}
