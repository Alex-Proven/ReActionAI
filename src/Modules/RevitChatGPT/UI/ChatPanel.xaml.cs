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
        private const double InputMinHeight = 32.0;
        private const double SendButtonHeight = 24.0;

        public ChatPanel()
        {
            InitializeComponent();

            ApplyRevitTheme(false);

            InputBox.GotFocus += InputBox_GotFocus;
            InputBox.LostFocus += InputBox_LostFocus;
            InputBox.TextChanged += InputBox_TextChanged;
            InputBox.KeyDown += InputBox_KeyDown;

            SendButton.Click += SendButton_Click;
            PlusButton.Click += PlusButton_Click;

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
                SendMessage();
            }
        }

        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Кнопка + нажата");
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void SendMessage()
        {
            var text = InputBox.Text.Trim();
            if (string.IsNullOrEmpty(text))
                return;

            AddUserMessage(text);

            // Пока для теста — эхо ×3, чтобы были длинные сообщения
            var triple = text + " " + text + " " + text;
            AddBotMessage(triple);

            InputBox.Text = string.Empty;
            UpdateInputHeight();
        }

        private void AddUserMessage(string text)
        {
            // Перенос по слогам
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

            MessagesPanel.Children.Add(bubble);
            ScrollToBottom();
        }

        private void AddBotMessage(string text)
        {
            // Перенос по слогам
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

            MessagesPanel.Children.Add(bubble);
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            MessagesScrollViewer?.ScrollToEnd();
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
            PlusButton.Height = InputMinHeight;
            InputBorder.MinHeight = InputMinHeight;
            InputBorder.Height = Double.NaN;
            SendButton.Height = SendButtonHeight;

            InputBorder.VerticalAlignment = VerticalAlignment.Center;
            SendButton.VerticalAlignment = VerticalAlignment.Center;

            var borderBrush = isDark
                ? new SolidColorBrush(Color.FromRgb(85, 85, 85))
                : new SolidColorBrush(Color.FromRgb(208, 208, 208));

            InputBorder.BorderBrush = borderBrush;
            PlusButton.BorderBrush = borderBrush;
            SendButton.BorderBrush = borderBrush;

            if (isDark)
            {
                InputBorder.Background = new SolidColorBrush(Color.FromRgb(50, 50, 50));
                PlusButton.Background = new SolidColorBrush(Color.FromRgb(45, 45, 45));
                SendButton.Background = Brushes.White;

                if (SendButton.Content is TextBlock tbDark)
                    tbDark.Foreground = Brushes.Black;
            }
            else
            {
                InputBorder.Background = Brushes.White;
                PlusButton.Background = new SolidColorBrush(Color.FromRgb(242, 242, 242));
                SendButton.Background = new SolidColorBrush(Color.FromRgb(242, 242, 242));

                if (SendButton.Content is TextBlock tbLight)
                    tbLight.Foreground = Brushes.Black;
            }

            UpdateInputHeight();
        }
    }
}
