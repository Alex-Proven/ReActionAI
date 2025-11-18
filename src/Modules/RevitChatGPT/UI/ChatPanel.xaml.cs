using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

            // По умолчанию считаем светлую тему
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

        // ------------------- ВВОД -------------------

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

        // ------------------- ОТПРАВКА -------------------

        private void SendMessage()
        {
            string text = InputBox.Text.Trim();
            if (string.IsNullOrEmpty(text))
                return;

            AddUserMessage(text);

            string triple = text + " " + text + " " + text;
            AddBotMessage(triple);

            InputBox.Text = string.Empty;
            UpdateInputHeight();
        }

        private void AddUserMessage(string text)
        {
            var bubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(220, 240, 255)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 10)
            };

            bubble.Child = new TextBlock
            {
                Text = text,
                Foreground = Brushes.Black,
                TextWrapping = TextWrapping.Wrap
            };

            MessagesPanel.Children.Add(bubble);
            ScrollToBottom();
        }

        private void AddBotMessage(string text)
        {
            var bubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 10)
            };

            bubble.Child = new TextBlock
            {
                Text = text,
                Foreground = Brushes.Black,
                TextWrapping = TextWrapping.Wrap
            };

            MessagesPanel.Children.Add(bubble);
            ScrollToBottom();
        }

        // ------------------- АВТОСКРОЛЛ -------------------

        private void ScrollToBottom()
        {
            MessagesScrollViewer?.ScrollToEnd();
        }

        // ------------------- ПЛЕЙСХОЛДЕР -------------------

        private void UpdatePlaceholderVisibility()
        {
            if (InputPlaceholder == null || InputBox == null)
                return;

            bool hasText = !string.IsNullOrWhiteSpace(InputBox.Text);
            bool hasFocus = InputBox.IsKeyboardFocused;

            InputPlaceholder.Visibility = (hasText || hasFocus)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        // ------------------- ДИНАМИЧЕСКАЯ ВЫСОТА -------------------

        private void UpdateInputHeight()
        {
            if (InputBorder == null || InputBox == null)
                return;

            int lines = Math.Max(1, InputBox.LineCount);

            double lineHeight = InputBox.FontSize + 6.0;

            double desiredHeight = Math.Max(InputMinHeight, lines * lineHeight);

            double maxHeight = 120.0;
            if (desiredHeight > maxHeight)
                desiredHeight = maxHeight;

            InputBorder.Height = desiredHeight;
        }

        // ------------------- ТЕМА REVIT + стиль рамок -------------------

        private void ApplyRevitTheme(bool isDark)
        {
            // Размеры кнопок
            PlusButton.Height = InputMinHeight;
            SendButton.Height = SendButtonHeight;
            InputBorder.MinHeight = InputMinHeight;
            InputBorder.VerticalAlignment = VerticalAlignment.Center;
            SendButton.VerticalAlignment = VerticalAlignment.Center;

            // Тонкая аккуратная кромка (САМЫЙ ВАЖНЫЙ БЛОК)
            SolidColorBrush borderBrush = isDark
                ? new SolidColorBrush(Color.FromRgb(85, 85, 85))    // #555
                : new SolidColorBrush(Color.FromRgb(208, 208, 208)); // #D0

            InputBorder.BorderBrush = borderBrush;
            PlusButton.BorderBrush = borderBrush;
            SendButton.BorderBrush = borderBrush;

            // ФОНЫ
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
