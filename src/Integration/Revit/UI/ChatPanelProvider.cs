using Autodesk.Revit.UI;

namespace ReActionAI.Integration.Revit.UI
{
    public class ChatPanelProvider : IDockablePaneProvider
    {
        private readonly ChatPanel _panel;

        public ChatPanelProvider()
        {
            _panel = new ChatPanel();
        }

        public ChatPanel Panel => _panel;

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = _panel;
            data.InitialState = new DockablePaneState
            {
                DockPosition = DockPosition.Tabbed
            };
        }

        public void AddMessage(string author, string? message)
        {
            _panel.AddMessage(author, message);
        }
    }
}
