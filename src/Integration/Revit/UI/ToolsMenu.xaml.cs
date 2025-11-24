using System.Windows;
using System.Windows.Controls;

namespace ReActionAI.Integration.Revit.UI
{
    public partial class ToolsMenu : UserControl
    {
        public ToolsMenu()
        {
            InitializeComponent();
            MenuClose.Click += (s, e) => Visibility = Visibility.Collapsed;

            // Тоггл подменю настроек
            MenuItem1.Click += (s, e) =>
            {
                if (SettingsSubmenu != null)
                {
                    SettingsSubmenu.Visibility = SettingsSubmenu.Visibility == Visibility.Visible
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                }
            };

            MenuItem2.Click += (s, e) => MessageBox.Show("Пункт 2");
            MenuItem3.Click += (s, e) => MessageBox.Show("Пункт 3");
            MenuItem4.Click += (s, e) => MessageBox.Show("Доп. пункт 1");
            MenuItem5.Click += (s, e) => MessageBox.Show("Доп. пункт 2");
            MenuItem6.Click += (s, e) => MessageBox.Show("Доп. пункт 3");

            // Подменю опции
            SettingOption1.Click += (s, e) => MessageBox.Show("Тема");
            SettingOption2.Click += (s, e) => MessageBox.Show("Интеграция");
            SettingOption3.Click += (s, e) => MessageBox.Show("Дополнительно");
        }
    }
}
