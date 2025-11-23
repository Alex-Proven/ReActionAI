using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Documents;

namespace ReActionAI.Modules.RevitChatGPT.UI
{
    /// <summary>
    /// Панель чата ReActionAI с двумя вариантами оформления:
    /// 1) монохромный (вариант 1),
    /// 2) мягкие цветные акценты (вариант 2).
    /// Переключение временно выполняется по кнопке "+".
    /// </summary>
    public partial class ChatPanel : UserControl
    {
        private const double InputMinHeight = 36.0;
        private const double InputMaxHeight = 400.0;
        private const int InputMaxLines = 20;
        private const double LineExtraPadding = 6.0;

        // Межабзацный интервал для всех сообщений (бот и пользователь)
        private const double ParagraphSpacing = 6.0;

        #region Темы

        private enum ChatTheme
        {
            Mono,   // вариант 1 — чёрно-белый
            Color   // вариант 2 — текущий цветной
        }

        // Стартуем с уже проверенного цветного варианта 2
        private ChatTheme _currentTheme = ChatTheme.Color;

        // Палитра цветной темы (вариант 2)
        private static readonly SolidColorBrush AccentBrushColor =
            new SolidColorBrush(Color.FromRgb(0x3A, 0x78, 0xD1));     // #3A78D1
        private static readonly SolidColorBrush AccentLightBrushColor =
            new SolidColorBrush(Color.FromRgb(0xD9, 0xE7, 0xFA));     // #D9E7FA
        private static readonly SolidColorBrush QuoteBorderBrushColor =
            new SolidColorBrush(Color.FromRgb(0xA8, 0xC4, 0xF0));     // мягкий голубой
        private static readonly SolidColorBrush QuoteBackgroundBrushColor =
            new SolidColorBrush(Color.FromRgb(0xF5, 0xF7, 0xFC));     // почти белый с голубым
        private static readonly SolidColorBrush CodeBackgroundBrushColor =
            new SolidColorBrush(Color.FromRgb(0xF5, 0xF7, 0xFA));     // чуть холоднее
        private static readonly SolidColorBrush CodeBorderBrushColor =
            new SolidColorBrush(Color.FromRgb(0xCC, 0xD3, 0xE5));     // серо-голубой
        private static readonly SolidColorBrush UserBubbleBackgroundColor =
            new SolidColorBrush(Color.FromRgb(220, 240, 255));        // как в варианте 2
        private static readonly SolidColorBrush BotBubbleBackgroundColor =
            new SolidColorBrush(Color.FromRgb(240, 240, 240));

        // Палитра монохромной темы (вариант 1)
        private static readonly SolidColorBrush AccentBrushMono =
            new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));     // тёмно-серый
        private static readonly SolidColorBrush AccentLightBrushMono =
            new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));     // светло-серый
        private static readonly SolidColorBrush QuoteBorderBrushMono =
            new SolidColorBrush(Color.FromRgb(0xC0, 0xC0, 0xC0));
        private static readonly SolidColorBrush QuoteBackgroundBrushMono =
            new SolidColorBrush(Color.FromRgb(0xF5, 0xF5, 0xF5));
        private static readonly SolidColorBrush CodeBackgroundBrushMono =
            new SolidColorBrush(Color.FromRgb(0xF2, 0xF2, 0xF2));
        private static readonly SolidColorBrush CodeBorderBrushMono =
            new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD));
        private static readonly SolidColorBrush UserBubbleBackgroundMono =
            new SolidColorBrush(Color.FromRgb(230, 230, 230));
        private static readonly SolidColorBrush BotBubbleBackgroundMono =
            new SolidColorBrush(Color.FromRgb(245, 245, 245));

        // Свойства, которые отдают нужные кисти в зависимости от текущей темы
        private Brush AccentBrush => _currentTheme == ChatTheme.Color ? AccentBrushColor : AccentBrushMono;
        private Brush AccentLightBrush => _currentTheme == ChatTheme.Color ? AccentLightBrushColor : AccentLightBrushMono;
        private Brush QuoteBorderBrush => _currentTheme == ChatTheme.Color ? QuoteBorderBrushColor : QuoteBorderBrushMono;
        private Brush QuoteBackgroundBrush => _currentTheme == ChatTheme.Color ? QuoteBackgroundBrushColor : QuoteBackgroundBrushMono;
        private Brush CodeBackgroundBrush => _currentTheme == ChatTheme.Color ? CodeBackgroundBrushColor : CodeBackgroundBrushMono;
        private Brush CodeBorderBrush => _currentTheme == ChatTheme.Color ? CodeBorderBrushColor : CodeBorderBrushMono;
        private Brush UserBubbleBackground => _currentTheme == ChatTheme.Color ? UserBubbleBackgroundColor : UserBubbleBackgroundMono;
        private Brush BotBubbleBackground => _currentTheme == ChatTheme.Color ? BotBubbleBackgroundColor : BotBubbleBackgroundMono;

        #endregion

        #region Демо-текст

        private const string DemoIntroParagraph =
            "Иногда интерфейсу не хватает всего одного аккуратного штриха, чтобы он стал понятнее и " +
            "приятнее в использовании. Такие детали формируют ощущение стабильности и продуманности системы.";

        private const string DemoQuoteHeader =
            "Выписка из технического раздела стандарта:";

        private const string DemoQuoteText =
            "Участие в координации BIM-моделей: проверка моделей на наличие коллизий, " +
            "контроль корректности параметров, настройка графики видов, разработка шаблонов " +
            "и организация библиотек семейств проекта.";

        private const string DemoMainParagraph =
            "А порой именно незаметные мелочи определяют впечатление от всей системы. " +
            "Чтобы пользователь чувствовал себя уверенно, в интерфейсе важно соблюдать несколько простых принципов:";

        private static readonly string[] DemoListItems =
        {
            "Структурность и предсказуемость поведения элементов.",
            "Привычную логику расположения управления и навигации.",
            "Читаемость текста и понятные визуальные акценты.",
            "Ровные отступы и аккуратные ритмы интерфейса."
        };

        private const string DemoCodeBlockText =
            "Параметры рендера сообщения:\n" +
            "- Абзацный интервал: 1.5\n" +
            "- Цитаты: Markdown-style\n" +
            "- Разметка: StackPanel + TextBlocks\n" +
            "- Моноширинный текст: FontFamily = Consolas";

        private const string DemoFinalParagraph =
            "Маленькие штрихи создают ощущение завершённости и помогают пользователю чувствовать контроль. " +
            "Когда система общается с ним так же аккуратно, как он работает в Revit, это вдохновляет двигаться дальше.";

        #endregion

        public ChatPanel()
        {
            InitializeComponent();

            // стартовая тема — цветная, как в последнем пуше
            ApplyTheme(ChatTheme.Color);

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
            // Shift+Enter – новая строка
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
                // Временно: переключаем тему по кнопке "+"
                ToggleTheme();
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

            // Демонстрационный богатый ответ бота
            AddDemoBotResponseSafe();
        }

        #endregion

        #region Сообщения

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

        private void AddDemoBotResponseSafe()
        {
            try
            {
                AddDemoBotResponse();
            }
            catch
            {
            }
        }

        private void AddUserMessage(string text)
        {
            if (text.Length > 1000)
                text = text.Substring(0, 1000) + "...";

            var lines = text
                .Replace("\r\n", "\n")
                .Split('\n')
                .Select(l => l.TrimEnd('\r'))
                .ToArray();

            if (lines.Length <= 1)
            {
                string hyphenated = Hyphenate(text);

                var singleBubble = new Border
                {
                    Tag = "user",                     // помечаем как пользователь
                    Background = UserBubbleBackground,
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

            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                line = Hyphenate(line);

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
                Tag = "user",
                Background = UserBubbleBackground,
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 10),
                Child = stack
            };

            MessagesPanel?.Children.Add(bubble);
            ScrollToBottom();
        }

        private void AddDemoBotResponse()
        {
            if (MessagesPanel == null)
                return;

            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            // Введение
            var introHeader = new TextBlock
            {
                Text = " Введение",
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                Foreground = AccentBrush
            };
            stack.Children.Add(introHeader);

            var introText = new TextBlock
            {
                Text = Hyphenate(DemoIntroParagraph),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, ParagraphSpacing, 0, 0)
            };
            stack.Children.Add(introText);

            stack.Children.Add(CreateSeparator());

            // Цитата
            var quoteHeader = new TextBlock
            {
                Text = " Цитата из проекта",
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, ParagraphSpacing, 0, 0),
                Foreground = AccentBrush
            };
            stack.Children.Add(quoteHeader);

            var quoteBorder = new Border
            {
                Background = QuoteBackgroundBrush,
                BorderThickness = new Thickness(4, 0, 0, 0),
                BorderBrush = QuoteBorderBrush,
                Padding = new Thickness(10, 6, 8, 8),
                Margin = new Thickness(0, ParagraphSpacing, 0, 0),
                CornerRadius = new CornerRadius(4)
            };

            var quoteStack = new StackPanel { Orientation = Orientation.Vertical };

            var quoteTitle = new TextBlock
            {
                Text = DemoQuoteHeader,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap
            };
            quoteStack.Children.Add(quoteTitle);

            var quoteText = new TextBlock
            {
                Text = Hyphenate(DemoQuoteText),
                FontStyle = FontStyles.Italic,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 0, 0)
            };
            quoteStack.Children.Add(quoteText);

            quoteBorder.Child = quoteStack;
            stack.Children.Add(quoteBorder);

            stack.Children.Add(CreateSeparator());

            // Основная часть
            var mainHeader = new TextBlock
            {
                Text = " Основная часть",
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, ParagraphSpacing, 0, 0),
                Foreground = AccentBrush
            };
            stack.Children.Add(mainHeader);

            var mainText = new TextBlock
            {
                Text = Hyphenate(DemoMainParagraph),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, ParagraphSpacing, 0, 0)
            };
            stack.Children.Add(mainText);

            var listStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, ParagraphSpacing, 0, 0)
            };

            foreach (var item in DemoListItems)
            {
                var tb = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 2, 0, 0)
                };

                var bulletRun = new Run("◆ ")
                {
                    Foreground = AccentBrush
                };
                var textRun = new Run(Hyphenate(item));

                tb.Inlines.Add(bulletRun);
                tb.Inlines.Add(textRun);

                listStack.Children.Add(tb);
            }

            stack.Children.Add(listStack);

            stack.Children.Add(CreateSeparator());

            // Код-блок
            var codeHeader = new TextBlock
            {
                Text = " Пример технического блока",
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, ParagraphSpacing, 0, 0),
                Foreground = AccentBrush
            };
            stack.Children.Add(codeHeader);

            var codeBorder = new Border
            {
                Background = CodeBackgroundBrush,
                BorderThickness = new Thickness(1),
                BorderBrush = CodeBorderBrush,
                Padding = new Thickness(10, 6, 10, 6),
                Margin = new Thickness(0, ParagraphSpacing, 0, 0),
                CornerRadius = new CornerRadius(6),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 4,
                    ShadowDepth = 0,
                    Opacity = 0.08
                }
            };

            var codeText = new TextBlock
            {
                Text = DemoCodeBlockText,
                FontFamily = new FontFamily("Consolas"),
                TextWrapping = TextWrapping.Wrap
            };
            codeBorder.Child = codeText;

            stack.Children.Add(codeBorder);

            // Финальный абзац
            var finalText = new TextBlock
            {
                Text = Hyphenate(DemoFinalParagraph),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, ParagraphSpacing, 0, 0)
            };
            stack.Children.Add(finalText);

            var bubble = new Border
            {
                Tag = "bot",                  // помечаем как бот
                Background = BotBubbleBackground,
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 10),
                Child = stack
            };

            MessagesPanel.Children.Add(bubble);
            ScrollToBottom();
        }

        private static string Hyphenate(string text)
        {
            return ReActionAI.Modules.RevitChatGPT.Text.RussianHyphenator.Hyphenate(text);
        }

        private FrameworkElement CreateSeparator()
        {
            return new Border
            {
                Height = 1,
                Margin = new Thickness(0, ParagraphSpacing, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(230, 230, 230))
            };
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

        #region Применение темы

        private void ToggleTheme()
        {
            _currentTheme = _currentTheme == ChatTheme.Color
                ? ChatTheme.Mono
                : ChatTheme.Color;

            ApplyTheme(_currentTheme);
        }

        private void ApplyTheme(ChatTheme theme)
        {
            _currentTheme = theme;

            // пока у нас только светлая ревитовская тема
            ApplyRevitTheme(isDark: false);

            // перекрашиваем уже существующие сообщения согласно новой теме
            RecolorExistingMessages();
        }

        private void RecolorExistingMessages()
        {
            if (MessagesPanel == null)
                return;

            foreach (var child in MessagesPanel.Children)
            {
                if (child is DependencyObject d)
                    RecolorElementRecursive(d);
            }
        }

        private void RecolorElementRecursive(DependencyObject element)
        {
            if (element == null)
                return;

            // Border: пузырьки, цитата, код-блок
            if (element is Border border)
            {
                if (border.Tag is string tag)
                {
                    if (string.Equals(tag, "user", StringComparison.OrdinalIgnoreCase))
                        border.Background = UserBubbleBackground;
                    else if (string.Equals(tag, "bot", StringComparison.OrdinalIgnoreCase))
                        border.Background = BotBubbleBackground;
                }

                // рамка цитаты / код-блока
                if (border.BorderBrush is SolidColorBrush bb)
                {
                    if (bb.Color == QuoteBorderBrushColor.Color || bb.Color == QuoteBorderBrushMono.Color)
                        border.BorderBrush = QuoteBorderBrush;
                    else if (bb.Color == CodeBorderBrushColor.Color || bb.Color == CodeBorderBrushMono.Color)
                        border.BorderBrush = CodeBorderBrush;
                }

                // фон цитаты / код-блока
                if (border.Background is SolidColorBrush bg)
                {
                    if (bg.Color == QuoteBackgroundBrushColor.Color || bg.Color == QuoteBackgroundBrushMono.Color)
                        border.Background = QuoteBackgroundBrush;
                    else if (bg.Color == CodeBackgroundBrushColor.Color || bg.Color == CodeBackgroundBrushMono.Color)
                        border.Background = CodeBackgroundBrush;
                }
            }

            // TextBlock: заголовки, обычный текст, элементы списка
            if (element is TextBlock tb)
            {
                if (tb.Foreground is SolidColorBrush fg &&
                    (fg.Color == AccentBrushColor.Color || fg.Color == AccentBrushMono.Color))
                {
                    tb.Foreground = AccentBrush;
                }

                // Отдельно перекрашиваем Inline-элементы (маркеры ◆)
                foreach (var inline in tb.Inlines)
                {
                    if (inline is Run run && run.Foreground is SolidColorBrush rfg &&
                        (rfg.Color == AccentBrushColor.Color || rfg.Color == AccentBrushMono.Color))
                    {
                        run.Foreground = AccentBrush;
                    }
                }
            }

            // Рекурсивный обход визуального дерева
            int count = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                RecolorElementRecursive(child);
            }
        }

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
