using Autodesk.Revit.UI;

namespace ReActionAI.Integration.Revit.UI
{
    /// <summary>
    /// Провайдер док-панели Revit для панели ToolsPanel.
    /// </summary>
    public class ChatPanelProvider : IDockablePaneProvider
    {
        private readonly ToolsPanel _panel;

        public ChatPanelProvider(ToolsPanel panel)
        {
            _panel = panel;
        }

        /// <summary>
        /// Доступ к панели.
        /// </summary>
        public ToolsPanel Panel => _panel;

        /// <summary>
        /// Настройка док-панели Revit.
        /// </summary>
        public void SetupDockablePane(DockablePaneProviderData data)
        {
            // WPF-контрол панель
            data.FrameworkElement = _panel;

            // Первое состояние док-панели Revit
            data.InitialState = new DockablePaneState
            {
                DockPosition = DockPosition.Right
            };

            // Видимость по умолчанию
            data.VisibleByDefault = true;
        }
    }
}
