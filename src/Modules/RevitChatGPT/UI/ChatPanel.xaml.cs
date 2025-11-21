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

        // Максимальная высота текстового ввода (после чего включается скролл)
        private const double InputMaxHeight = 96.0;

        // Скорость анимации изменения высоты (секунды) – если вернём анимацию
        private const double HeightAnimationDuration = 0.10;

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

        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Кнопка \"+\" пока не реализована. Здесь в будущем будет выбор режимов или вложений.",
                "ReActionAI",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessageSafe();
        }

        private void SendMessageSafe()
        {
            try
            {
                var text = InputBox?.Text;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    // TODO: здесь должен быть вызов сервиса отправки сообщения
                    MessageBox.Show(
                        $"Отправлено сообщение:\n\n{text}",
                        "ReActionAI",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    InputBox!.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при отправке сообщения: {ex.Message}",
                    "ReActionAI",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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

        private void UpdatePlaceholderVisibility()
        {
            if (InputPlaceholder == null || InputBox == null)
                return;

            InputPlaceholder.Visibility =
                string.IsNullOrWhiteSpace(InputBox.Text) && !InputBox.IsKeyboardFocused
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        // ВАЖНО: финальная версия – без раздувания высоты, с корректным вычислением
        private void UpdateInputHeight()
        {
            if (InputBox == null || InputBorder == null)
                return;

            // Обновляем разметку, чтобы ActualWidth и DesiredSize были корректными
            InputBox.UpdateLayout();

            // Ширина для измерения: ширина контейнера минус запас под отступы и кнопку
            var measureWidth = InputBorder.ActualWidth > 40
                ? InputBorder.ActualWidth - 40
                : InputBorder.ActualWidth;

            if (measureWidth <= 0)
            {
                // Если ширина ещё не рассчитана (панель только что создалась),
                // просто выставляем минимальную высоту и выходим.
                InputBorder.Height = InputMinHeight;
                return;
            }

            InputBox.Measure(new Size(measureWidth, double.PositiveInfinity));
            var desiredHeight = InputBox.DesiredSize.Height;

            // Ограничиваем высоту диапазоном [InputMinHeight; InputMaxHeight]
            var targetHeight = Math.Max(InputMinHeight, Math.Min(InputMaxHeight, desiredHeight));

            InputBorder.Height = targetHeight;
        }

        public void SetTheme(bool isDark)
        {
            ApplyRevitTheme(isDark);
        }

        private void ApplyRevitTheme(bool isDark)
        {
            // Геометрия
            if (PlusButton != null)
            {
                PlusButton.Height = InputMinHeight;
                PlusButton.VerticalAlignment = VerticalAlignment.Center;
            }

            if (InputBorder != null)
            {
                InputBorder.MinHeight = InputMinHeight;
                // Больше НЕ задаём Height = double.NaN – это и раздувало контейнер
                InputBorder.VerticalAlignment = VerticalAlignment.Center;
            }

            if (SendButton != null)
            {
                SendButton.Height = SendButtonHeight;
                SendButton.VerticalAlignment = VerticalAlignment.Center;
            }

            // Общий цвет границ
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
                // Тёмная тема: фон кнопки отправки совпадает с фоном поля ввода
                var inputBgDark = new SolidColorBrush(Color.FromRgb(50, 50, 50));


                if (InputBorder != null)
                    InputBorder.Background = inputBgDark;

                if (PlusButton != null)
                    PlusButton.Background = new SolidColorBrush(Color.FromRgb(45, 45, 45));

                if (SendButton != null)
                    SendButton.Background = inputBgDark;

                if (SendButton != null && SendButton.Content is TextBlock tbDark)
                    tbDark.Foreground = Brushes.Black;
            }
            else
            {
                // Светлая тема: фон кнопки отправки совпадает с фоном поля ввода
                Brush inputBgLight = Brushes.White;

                if (InputBorder != null)
                    InputBorder.Background = inputBgLight;

                if (PlusButton != null)
                    PlusButton.Background = new SolidColorBrush(Color.FromRgb(242, 242, 242));

                if (SendButton != null)
                    SendButton.Background = inputBgLight;

                if (SendButton != null && SendButton.Content is TextBlock tbLight)
                    tbLight.Foreground = Brushes.Black;
            }

            UpdateInputHeight();
        }
    }
}
