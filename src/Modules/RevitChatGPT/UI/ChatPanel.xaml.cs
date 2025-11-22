using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ReActionAI.Modules.RevitChatGPT.UI
{
    public partial class ChatPanel : UserControl
    {
        private const double InputMinHeight = 36.0;
        private const double InputMaxHeight = 400.0;
        private const int InputMaxLines = 20;
        private const double LineExtraPadding = 6.0;

        // Тестовые абзацы ответа бота
        private const string EchoParagraph1 =
            "Иногда достаточно одного маленького штриха, чтобы интерфейс наконец-то обрёл гармонию. " +
            "Такой штрих делает его более живым и показывает заботу о каждом движении пользователя.";

        private const string EchoParagraph2 =
            "А порой именно незаметные детали формируют самое приятное впечатление от проекта. " +
            "Эти маленькие штрихи создают ощущение завершённости и вдохновляют двигаться дальше ещё увереннее.";

        // Межабзацный интервал для всех сообщений (бот и пользователь)
        private const double ParagraphSpacing = 6.0;

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

                // Скроллбар не показываем, но скролл колесом работает
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

        private void InputBox_GotFocus(object sender, RoutedEventArgs e) => UpdatePlaceholderVisibility();
        private void InputBox_LostFocus(object sender, RoutedEventArgs e) => UpdatePlaceholderVisibility();

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholderVisibility();
            UpdateInputHeight();
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Enter без Shift – отправка сообщения
            // Shift+Enter – новая строка внутри того же сообщения
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
                // Заглушка для будущего функционала кнопки "+"
                MessageBox.Show("Кнопка + нажата");
            }
            catch
            {
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e) => SendMessageSafe();

        private void SendMessageSafe()
        {
            try
            {
                SendMessage();
            }
            catch
            {
                // Любые проблемы не должны уронить Revit
            }
        }

        private void SendMessage()
        {
            if (InputBox == null)
                return;

            string? text = InputBox.Text?.TrimEnd();
            if (string.IsNullOrWhiteSpace(text))
                return;

            // Сообщение пользователя
            AddUserMessageSafe(text);

            // Очистка ввода
            InputBox.Text = string.Empty;
            UpdatePlaceholderVisibility();
            UpdateInputHeight();

            // Эхо-ответ бота: два абзаца с «полуторным» интервалом
            AddBotMessageSafe(EchoParagraph1, EchoParagraph2);
        }

        #endregion

        #region Добавление сообщений

        private void AddUserMessageSafe(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            try
            {
                AddUserMessage(text!);
            }
            catch
            {
            }
        }

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
            }
        }

        // Новый безопасный метод для двух абзацев
        private void AddBotMessageSafe(string? paragraph1, string? paragraph2)
        {
            if (string.IsNullOrWhiteSpace(paragraph1) && string.IsNullOrWhiteSpace(paragraph2))
                return;

            try
            {
                AddBotMessage(paragraph1 ?? string.Empty, paragraph2 ?? string.Empty);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Сообщение пользователя с одинаковыми «абзацными» интервалами, как у бота.
        /// Каждая строка (Enter / Shift+Enter / перенос из Word) считается отдельным абзацем.
        /// </summary>
        private void AddUserMessage(string text)
        {
            if (text.Length > 1000)
                text = text.Substring(0, 1000) + "...";

            // Нормализуем переводы строк и разбиваем по каждой строке
            var lines = text
                .Replace("\r\n", "\n")
                .Split('\n')
                .Select(l => l.TrimEnd('\r'))
                .ToArray();

            if (lines.Length <= 1)
            {
                // Один абзац — старое поведение
                string hyphenated = ReActionAI.Modules.RevitChatGPT.Text.RussianHyphenator.Hyphenate(text);

                var singleBubble = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(220, 240, 255)),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(10),
                    Margin = new Thickness(0, 0, 0, 10),
                    Child = new TextBlock
                    {
                        Text = hyphenated,
                        Foreground = Brushes.Black,
                        TextWrapping = TextWrapping.Wrap
                    }
                };

                MessagesPanel?.Children.Add(singleBubble);
                ScrollToBottom();
                return;
            }

            // Несколько строк — каждая как абзац с интервалом ParagraphSpacing
            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                line = ReActionAI.Modules.RevitChatGPT.Text.RussianHyphenator.Hyphenate(line);

                var tb = new TextBlock
                {
                    Text = line,
                    Foreground = Brushes.Black,
                    TextWrapping = TextWrapping.Wrap
                };

                if (i > 0)
                    tb.Margin = new Thickness(0, ParagraphSpacing, 0, 0);

                stack.Children.Add(tb);
            }

            var bubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(220, 240, 255)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 10),
                Child = stack
            };

            MessagesPanel?.Children.Add(bubble);
            ScrollToBottom();
        }

        // Старый универсальный вариант (одна строка текста бота)
        private void AddBotMessage(string text)
        {
            if (text.Length > 2000)
                text = text.Substring(0, 2000) + "...";

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

        // Новый вариант – два абзаца с «полуторным» интервалом для бота
        private void AddBotMessage(string paragraph1, string paragraph2)
        {
            if (paragraph1.Length > 2000)
                paragraph1 = paragraph1.Substring(0, 2000) + "...";
            if (paragraph2.Length > 2000)
                paragraph2 = paragraph2.Substring(0, 2000) + "...";

            paragraph1 = ReActionAI.Modules.RevitChatGPT.Text.RussianHyphenator.Hyphenate(paragraph1);
            paragraph2 = ReActionAI.Modules.RevitChatGPT.Text.RussianHyphenator.Hyphenate(paragraph2);

            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            if (!string.IsNullOrWhiteSpace(paragraph1))
            {
                stack.Children.Add(new TextBlock
                {
                    Text = paragraph1,
                    Foreground = Brushes.Black,
                    TextWrapping = TextWrapping.Wrap
                });
            }

            if (!string.IsNullOrWhiteSpace(paragraph2))
            {
                stack.Children.Add(new TextBlock
                {
                    Text = paragraph2,
                    Foreground = Brushes.Black,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, ParagraphSpacing, 0, 0)
                });
            }

            var bubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 10),
                Child = stack
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

        #region Плейсхолдер и авто-высота ввода

        private void UpdatePlaceholderVisibility()
        {
            if (InputPlaceholder == null || InputBox == null)
                return;

            bool hasText = !string.IsNullOrWhiteSpace(InputBox.Text);
            bool hasFocus = InputBox.IsKeyboardFocused;

            InputPlaceholder.Visibility =
                (hasText || hasFocus) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void UpdateInputHeight()
        {
            if (InputBorder == null || InputBox == null)
                return;

            int totalLines = Math.Max(1, InputBox.LineCount);

            InputBox.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

            int lines = Math.Min(totalLines, InputMaxLines);
            double lineHeight = InputBox.FontSize + LineExtraPadding;

            double desiredHeight = lines * lineHeight + 8.0;
            if (desiredHeight < InputMinHeight)
                desiredHeight = InputMinHeight;
            if (desiredHeight > InputMaxHeight)
                desiredHeight = InputMaxHeight;

            double currentHeight = InputBorder.Height;
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

                if (SendButton?.Content is TextBlock tbd)
                    tbd.Foreground = Brushes.Black;
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

                if (SendButton?.Content is TextBlock tbl)
                    tbl.Foreground = Brushes.Black;
            }

            UpdateInputHeight();
        }

        #endregion
    }
}
