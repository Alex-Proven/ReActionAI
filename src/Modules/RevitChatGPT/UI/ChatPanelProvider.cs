using Autodesk.Revit.UI;

namespace ReActionAI.Modules.RevitChatGPT.UI
{
    public class ChatPanelProvider : IDockablePaneProvider
    {
        private readonly ChatPanel _panel;

        public ChatPanelProvider(ChatPanel panel)
        {
            _panel = panel;
        }

        // При желании можно получать доступ к панели извне
        public ChatPanel Panel => _panel;

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = _panel;

            data.InitialState = new DockablePaneState
            {
                DockPosition = DockPosition.Right
            };
        }
    }
}
