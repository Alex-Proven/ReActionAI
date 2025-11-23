using System.Windows.Controls;

namespace ReActionAI.Views
{
    public partial class AssistantPanel : UserControl
    {
        public AssistantPanel()
        {
            InitializeComponent();

            // Заголовок — через перенос по слогам
            HeaderTitle.Text =
                ReActionAI.Modules.RevitChatGPT.Text.RussianHyphenator.Hyphenate(
                    "ReActionAI — когнитивный ассистент для BIM-проектирования");

            // Описание — тоже через Hyphenator (чуть укороченный текст)
            HeaderDescription.Text =
                ReActionAI.Modules.RevitChatGPT.Text.RussianHyphenator.Hyphenate(
                    "Поддержка проектных решений, оптимизация процессов и повышение эффективности работы инженера.");
        }
    }
}
