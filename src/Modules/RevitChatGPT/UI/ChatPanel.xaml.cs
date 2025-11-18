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
            UpdateInputHeight();      // пересчитываем высоту при изменении текста
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
            var text = InputBox.Text.Trim();
            if (string.IsNullOrEmpty(text))
                return;

            AddUserMessage(text);

            // ЭХО × 3 (в 3 раза длиннее)
            var triple = text + " " + text + " " + text;
            AddBotMessage(triple);

            InputBox.Text = string.Empty;
            UpdateInputHeight();   // после очистки возвращаем высоту к минимуму
        }

        private void AddUserMessage(string text)
        {
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

            var hasText = !string.IsNullOrWhiteSpace(InputBox.Text);
            var hasFocus = InputBox.IsKeyboardFocused;

            InputPlaceholder.Visibility = (hasText || hasFocus)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        // ------------------- ДИНАМИЧЕСКАЯ ВЫСОТА ВВОДА -------------------

        /// <summary>
        /// Подстраивает высоту овального контейнера под количество строк текста.
        /// Минимум — InputMinHeight, дальше растёт с ростом LineCount.
        /// </summary>
        private void UpdateInputHeight()
        {
            if (InputBorder == null || InputBox == null)
                return;

            // Количество строк в TextBox (работает, когда контрол загружен)
            var lines = Math.Max(1, InputBox.LineCount);

            // Примерная высота строки: размер шрифта + небольшой запас
            var lineHeight = InputBox.FontSize + 6.0;

            // Рассчитываем желаемую высоту
            var desiredHeight = Math.Max(InputMinHeight, lines * lineHeight);

            // Ограничим разумной высотой, чтобы не занимало всю панель
            var maxHeight = 120.0;
            if (desiredHeight > maxHeight)
                desiredHeight = maxHeight;

            InputBorder.Height = desiredHeight;
        }

        // ------------------- ТЕМА REVIT + стиль рамок -------------------

        /// <summary>
        /// Применение темы Revit к панели (isDark = true для тёмной темы).
        /// Здесь задаём размеры кнопок и минимальную высоту контейнера.
        /// </summary>
        private void ApplyRevitTheme(bool isDark)
        {
            // Высота кнопки "+" остаётся фиксированной
            PlusButton.Height = InputMinHeight;

            // Контейнер: минимальная высота, остальное — динамически в UpdateInputHeight()
            InputBorder.MinHeight = InputMinHeight;
            InputBorder.Height = Double.NaN; // Auto, дальше подстроим в UpdateInputHeight

            // Кнопка отправки — меньше по высоте
            SendButton.Height = SendButtonHeight;

            // Центрирование по вертикали
            InputBorder.VerticalAlignment = VerticalAlignment.Center;
            SendButton.VerticalAlignment = VerticalAlignment.Center;

            // --- СТИЛЬ ТОНКОЙ КРОМКИ ---
            var borderBrush = isDark
                ? new SolidColorBrush(Color.FromRgb(85, 85, 85))    // #555
                : new SolidColorBrush(Color.FromRgb(208, 208, 208)); // #D0

            InputBorder.BorderBrush = borderBrush;
            PlusButton.BorderBrush = borderBrush;
            SendButton.BorderBrush = borderBrush;

            // ФОНЫ и текст
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

            // Пересчитать высоту после смены темы
            UpdateInputHeight();
        }
    }
}
