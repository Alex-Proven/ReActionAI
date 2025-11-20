using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ReActionAI.Modules.RevitChatGPT.UI
{
    public partial class ChatPanel : UserControl
    {
        // Минимальная высота овального контейнера ввода
        private const double InputMinHeight = 32.0;

        // Высота кнопки отправки (меньше контейнера)
        private const double SendButtonHeight = 24.0;

        public ChatPanel()
        {
            InitializeComponent();

            ApplyRevitTheme(false);

            if (InputBox != null)
            {
                InputBox.GotFocus += InputBox_GotFocus;
                InputBox.LostFocus += InputBox_LostFocus;
                InputBox.TextChanged += InputBox_TextChanged;
                InputBox.KeyDown += InputBox_KeyDown;
            }

            if (SendButton != null)
            {
                SendButton.Click += SendButton_Click;
            }

            if (PlusButton != null)
            {
                PlusButton.Click += PlusButton_Click;
            }

            UpdatePlaceholderVisibility();
            UpdateInputHeight();
        }

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
            // Enter без Shift — отправка
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
                // Не даём исключению уйти в Revit
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessageSafe();
        }

        /// <summary>
        /// Безопасная обёртка отправки сообщения.
        /// Любое исключение гасится, чтобы не уронить Revit.
        /// </summary>
        private void SendMessageSafe()
        {
            try
            {
                SendMessageInternal();
            }
            catch
            {
                // TODO: логировать при необходимости, но Revit не должен падать
            }
        }

        private void SendMessageInternal()
        {
            var text = InputBox != null ? InputBox.Text : string.Empty;
            text = text == null ? string.Empty : text.Trim();

            if (string.IsNullOrEmpty(text))
                return;

            AddUserMessageSafe(text);

            // Временный эхо-ответ, чтобы видеть длинные сообщения
            var triple = text + " " + text + " " + text;
            AddBotMessageSafe(triple);

            if (InputBox != null)
            {
                InputBox.Text = string.Empty;
            }

            UpdateInputHeight();
        }

        private void AddUserMessageSafe(string text)
        {
            try
            {
                AddUserMessage(text);
            }
            catch
            {
                // Гасим, чтобы не уронить Revit
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
                // Гасим, чтобы не уронить Revit
            }
        }

        private void AddUserMessage(string text)
        {
            // Временная защита: обрезаем слишком длинный текст,
            // чтобы не провоцировать падение Revit из-за WPF-разметки.
            if (text != null && text.Length > 500)
            {
                text = text.Substring(0, 500) + "...";
            }

            // Перенос по слогам (сейчас RussianHyphenator — безопасная заглушка)
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
            {
                MessagesPanel.Children.Add(bubble);
            }

            ScrollToBottom();
        }

        private void AddBotMessage(string text)
        {
            if (text != null && text.Length > 500)
            {
                text = text.Substring(0, 500) + "...";
            }

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
            {
                MessagesPanel.Children.Add(bubble);
            }

            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            try
            {
                if (MessagesScrollViewer != null)
                {
                    MessagesScrollViewer.ScrollToEnd();
                }
            }
            catch
            {
                // На всякий случай — не даём исключению уйти в Revit
            }
        }

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
            var maxHeight = 120.0;

            if (desiredHeight > maxHeight)
                desiredHeight = maxHeight;

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

        private void ApplyRevitTheme(bool isDark)
        {
            if (PlusButton != null)
            {
                PlusButton.Height = InputMinHeight;
                PlusButton.VerticalAlignment = VerticalAlignment.Center;
            }

            if (InputBorder != null)
            {
                InputBorder.MinHeight = InputMinHeight;
                InputBorder.Height = double.NaN;
                InputBorder.VerticalAlignment = VerticalAlignment.Center;
            }

            if (SendButton != null)
            {
                SendButton.Height = SendButtonHeight;
                SendButton.VerticalAlignment = VerticalAlignment.Center;
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
    }
}
