using System.Windows.Controls;

namespace ReActionAI.Integration.Revit.UI
{
    public partial class ToolsPanel : UserControl
    {
        public ToolsPanel()
        {
            InitializeComponent();

            // Заголовок — через перенос по слогам
            HeaderTitle.Text =
                ReActionAI.Integration.Revit.Text.RussianHyphenator.Hyphenate(
                    "ReActionAI — когнитивный ассистент для BIM-проектирования");

            // Описание — тоже через Hyphenator (чуть укороченный текст)
            HeaderDescription.Text =
                ReActionAI.Integration.Revit.Text.RussianHyphenator.Hyphenate(
                    "Поддержка проектных решений, оптимизация процессов и повышение эффективности работы инженера.");
        }
    }
}
