using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ReActionAI.Modules.RevitChatGPT.UI
{
    public partial class ChatPanel : UserControl
    {
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
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Enter без Shift — отправка сообщения
            if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Shift) == 0)
            {
                e.Handled = true;
                SendMessage();
            }
        }

        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            // Заглушка
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

            // ЭХО × 3 (в 3 раза длиннее)
            string triple = text + " " + text + " " + text;
            AddBotMessage(triple);

            InputBox.Text = string.Empty;
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

        // ------------------- ТЕМА REVIT -------------------

        /// <summary>
        /// Применение темы Revit к панели (isDark = true для тёмной темы).
        /// Здесь же задаём высоту элементов.
        /// </summary>
        private void ApplyRevitTheme(bool isDark)
        {
            // Высоты элементов
            const double inputHeight = 32; // высота кнопки "+" и контейнера
            const double sendHeight = 24; // кнопка отправки — меньше!

            PlusButton.Height = inputHeight;
            InputBorder.Height = inputHeight;

            // Уменьшенная кнопка отправки — больше не будет сплюснутой
            SendButton.Height = sendHeight;

            // Центрирование
            InputBorder.VerticalAlignment = VerticalAlignment.Center;
            SendButton.VerticalAlignment = VerticalAlignment.Center;

            // --- СТИЛИ ТЕМЫ ---
            InputBorder.Background = isDark
                ? new SolidColorBrush(Color.FromRgb(50, 50, 50))
                : Brushes.White;

            InputBorder.BorderBrush = isDark
                ? new SolidColorBrush(Color.FromRgb(80, 80, 80))
                : new SolidColorBrush(Color.FromRgb(220, 220, 220));

            if (isDark)
            {
                SendButton.Background = Brushes.White;
                SendButton.BorderBrush = Brushes.White;

                if (SendButton.Content is TextBlock tbDark)
                    tbDark.Foreground = Brushes.Black;
            }
            else
            {
                SendButton.Background = new SolidColorBrush(Color.FromRgb(242, 242, 242));
                SendButton.BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224));

                if (SendButton.Content is TextBlock tbLight)
                    tbLight.Foreground = Brushes.Black;
            }

            PlusButton.Background = isDark
                ? new SolidColorBrush(Color.FromRgb(45, 45, 45))
                : new SolidColorBrush(Color.FromRgb(242, 242, 242));

            PlusButton.BorderBrush = isDark
                ? new SolidColorBrush(Color.FromRgb(80, 80, 80))
                : new SolidColorBrush(Color.FromRgb(224, 224, 224));

            if (PlusButton.Content is TextBlock p)
                p.Foreground = isDark ? Brushes.White : Brushes.Black;
        }
    }
}
