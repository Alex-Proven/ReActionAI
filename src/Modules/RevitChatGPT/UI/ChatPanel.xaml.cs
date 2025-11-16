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

            SendButton.Click += SendButton_Click;
            PlusButton.Click += PlusButton_Click;
            InputBox.KeyDown += InputBox_KeyDown;
        }

        // ------------- События UI -------------

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.IsKeyDown(Key.LeftShift))
            {
                e.Handled = true;
                SendMessage();
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Кнопка + нажата");
        }

        // ------------- Отправка -------------

        private void SendMessage()
        {
            string text = InputBox.Text.Trim();
            if (string.IsNullOrEmpty(text))
                return;

            AddUserMessage(text);
            InputBox.Text = string.Empty;
        }

        private void AddUserMessage(string text)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(220, 240, 255)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 8)
            };

            border.Child = new TextBlock
            {
                Text = text,
                Foreground = Brushes.Black,
                TextWrapping = TextWrapping.Wrap
            };

            MessagesPanel.Children.Add(border);
            ScrollToBottom();
        }

        // ------------- Автоскролл -------------

        private void ScrollToBottom()
        {
            MessagesScrollViewer?.ScrollToEnd();
        }

        // ------------- Применение темы Revit -------------

        /// <summary>
        /// Светлая тема  => isDark = false  => чёрная круглая кнопка.
        /// Тёмная тема   => isDark = true   => белая круглая кнопка.
        /// Вызывается из App через рефлексию.
        /// </summary>
        private void ApplyRevitTheme(bool isDark)
        {
            // Капсула ввода
            InputBorder.Background = isDark
                ? new SolidColorBrush(Color.FromRgb(50, 50, 50))
                : new SolidColorBrush(Color.FromRgb(244, 244, 244));

            InputBorder.BorderBrush = isDark
                ? new SolidColorBrush(Color.FromRgb(90, 90, 90))
                : new SolidColorBrush(Color.FromRgb(224, 224, 224));

            // Кнопка отправки
            if (isDark)
            {
                // Тёмная тема → белый круг, чёрная стрелка
                SendButton.Background = Brushes.White;
                SendButton.BorderBrush = Brushes.White;

                if (SendButton.Content is TextBlock arrowDark)
                    arrowDark.Foreground = Brushes.Black;
            }
            else
            {
                // Светлая тема → чёрный круг, белая стрелка
                SendButton.Background = Brushes.Black;
                SendButton.BorderBrush = Brushes.Black;

                if (SendButton.Content is TextBlock arrowLight)
                    arrowLight.Foreground = Brushes.White;
            }

            // Кнопка "+"
            PlusButton.Background = isDark
                ? new SolidColorBrush(Color.FromRgb(60, 60, 60))
                : new SolidColorBrush(Color.FromRgb(242, 242, 242));

            PlusButton.BorderBrush = isDark
                ? new SolidColorBrush(Color.FromRgb(90, 90, 90))
                : new SolidColorBrush(Color.FromRgb(224, 224, 224));

            if (PlusButton.Content is TextBlock plus)
                plus.Foreground = isDark ? Brushes.White : Brushes.Black;
        }
    }
}
