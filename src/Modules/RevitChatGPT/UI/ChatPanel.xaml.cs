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
        /// <summary>
        /// Минимальная высота контейнера ввода (одна строка).
        /// </summary>
        private const double InputMinHeight = 32.0;

        /// <summary>
        /// Максимальная высота контейнера ввода (под ~20 строк).
        /// </summary>
        private const double InputMaxHeight = 400.0;

        /// <summary>
        /// Максимальное количество строк, которое учитываем при автоувеличении.
        /// После этого высота не растёт, включается скролл.
        /// </summary>
        private const int InputMaxLines = 20;

        /// <summary>
        /// Дополнительный запас по высоте к размеру шрифта на строку.
        /// </summary>
        private const double LineExtraPadding = 6.0;

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

                // По умолчанию скролл скрыт — включаем только после 20 строк
                InputBox.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
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

        private void SendMessageSafe()
        {
            try
            {
                SendMessage();
            }
            catch
            {
                // Любые проблемы не должны рушить Revit
            }
        }

        private void SendMessage()
        {
            if (InputBox == null)
                return;

            var text = InputBox.Text?.Trim();
            if (string.IsNullOrEmpty(text))
                return;

            // Здесь уже гарантированно не null
            AddUserMessageSafe(text);

            // Очистка поля ввода
            InputBox.Text = string.Empty;
            UpdatePlaceholderVisibility();
            UpdateInputHeight();

            // Временный ответ бота-заглушка
            AddBotMessageSafe("Ответ ассистента (заглушка). Интеграция с ядром будет добавлена позже.");
        }

        #endregion

        #region Добавление сообщений

        /// <summary>
        /// Безопасная обёртка, гасит исключения и отсекает null/пустую строку.
        /// </summary>
        private void AddUserMessageSafe(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            try
            {
                // text! — здесь мы гарантируем компилятору, что null уже отсеян
                AddUserMessage(text!);
            }
            catch
            {
                // Гасим, чтобы не уронить Revit
            }
        }

        /// <summary>
        /// Безопасная обёртка, гасит исключения и отсекает null/пустую строку.
        /// </summary>
        private void AddBotMessageSafe(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            try
            {
                AddBotMessage(text!);
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
            if (text.Length > 500)
                text = text.Substring(0, 500) + "...";

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

            MessagesPanel?.Children.Add(bubble);

            ScrollToBottom();
        }

        private void AddBotMessage(string text)
        {
            if (text.Length > 500)
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

            MessagesPanel?.Children.Add(bubble);

            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            try
            {
                MessagesScrollViewer?.ScrollToEnd();
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
                (hasText || hasFocus) ? Visibility.Collapsed : Visibility.Hidden;
        }

        /// <summary>
        /// Плавно подстраивает высоту овального контейнера ввода
        /// под количество строк, с ограничением до 20 строк.
        /// Скролл включается только после 20-й строки.
        /// </summary>
        private void UpdateInputHeight()
        {
            if (InputBorder == null || InputBox == null)
                return;

            // Сколько строк реально в текстбоксе
            var totalLines = Math.Max(1, InputBox.LineCount);

            // Управляем скроллом:
            // до 20 строк — скролл скрыт, после 20 — показываем
            if (totalLines > InputMaxLines)
                InputBox.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            else
                InputBox.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

            // Ограничиваем количество строк, по которым растёт высота
            var lines = Math.Min(totalLines, InputMaxLines);

            // Высота одной строки + небольшой запас
            var lineHeight = InputBox.FontSize + LineExtraPadding;

            // Высота под N строк + общий небольшой запас,
            // чтобы верхняя строка не попадала под скругление/бордер
            var desiredHeight = lines * lineHeight + 4.0;

            if (desiredHeight < InputMinHeight)
                desiredHeight = InputMinHeight;

            if (desiredHeight > InputMaxHeight)
                desiredHeight = InputMaxHeight;

            var currentHeight = InputBorder.Height;
            if (double.IsNaN(currentHeight) || currentHeight <= 0)
                currentHeight = InputMinHeight;

            // Если различие маленькое — просто присваиваем
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

        #region Тема (светлая/тёмная)

        private void ApplyRevitTheme(bool isDark)
        {
            if (InputBorder != null)
            {
                InputBorder.MinHeight = InputMinHeight;
                InputBorder.Height = double.NaN;
                // Прижимаем контейнер к низу ячейки футера
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
