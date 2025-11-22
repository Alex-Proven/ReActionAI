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

        // Межабзацный интервал для всех сообщений (бот и пользователь)
        private const double ParagraphSpacing = 6.0;

        // Текстовые блоки демо-ответа бота
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

            // Демонстрационный богатый ответ бота
            AddDemoBotResponseSafe();
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

        /// <summary>
        /// Сообщение пользователя с абзацным интервалом между строками,
        /// как и в сообщениях бота.
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

        /// <summary>
        /// Богатый демонстрационный ответ бота:
        /// заголовки, цитата, список, код-блок, абзацные интервалы.
        /// </summary>
        private void AddDemoBotResponse()
        {
            if (MessagesPanel == null)
                return;

            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            // Заголовок "Введение"
            var introHeader = new TextBlock
            {
                Text = " Введение",
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap
            };
            stack.Children.Add(introHeader);

            // Вводный абзац
            var introText = new TextBlock
            {
                Text = Hyphenate(DemoIntroParagraph),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, ParagraphSpacing, 0, 0)
            };
            stack.Children.Add(introText);

            // Разделитель
            stack.Children.Add(CreateSeparator());

            // Заголовок для цитаты
            var quoteHeader = new TextBlock
            {
                Text = " Цитата из проекта",
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, ParagraphSpacing, 0, 0)
            };
            stack.Children.Add(quoteHeader);

            // Цитата-блок
            var quoteBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(248, 248, 248)),
                BorderThickness = new Thickness(3, 0, 0, 0),
                BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                Padding = new Thickness(8, 6, 8, 6),
                Margin = new Thickness(0, ParagraphSpacing, 0, 0)
            };

            var quoteStack = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

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

            // Разделитель
            stack.Children.Add(CreateSeparator());

            // Заголовок "Основная часть"
            var mainHeader = new TextBlock
            {
                Text = " Основная часть",
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, ParagraphSpacing, 0, 0)
            };
            stack.Children.Add(mainHeader);

            // Основной абзац
            var mainText = new TextBlock
            {
                Text = Hyphenate(DemoMainParagraph),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, ParagraphSpacing, 0, 0)
            };
            stack.Children.Add(mainText);

            // Список
            var listStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, ParagraphSpacing, 0, 0)
            };

            foreach (var item in DemoListItems)
            {
                var tb = new TextBlock
                {
                    Text = "• " + Hyphenate(item),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 2, 0, 0)
                };
                listStack.Children.Add(tb);
            }

            stack.Children.Add(listStack);

            // Разделитель
            stack.Children.Add(CreateSeparator());

            // Заголовок "Пример технического блока"
            var codeHeader = new TextBlock
            {
                Text = " Пример технического блока",
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, ParagraphSpacing, 0, 0)
            };
            stack.Children.Add(codeHeader);

            // Код-блок
            var codeBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(210, 210, 210)),
                Padding = new Thickness(8, 6, 8, 6),
                Margin = new Thickness(0, ParagraphSpacing, 0, 0)
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

            // Итоговый баббл
            var bubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
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
