using System;
using Autodesk.Revit.UI;

namespace ReActionAI.Modules.RevitChatGPT.UI
{
    public class ChatDock : IDockablePaneProvider
    {
        public static readonly DockablePaneId PaneId =
            new DockablePaneId(new Guid("8BD3A77A-B739-4ACD-A9FD-7A7B1FC5B123"));

        public const string PanelTitle = "ChatGPT";

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = new ChatPanel();
            data.InitialState = new DockablePaneState
            {
                DockPosition = DockPosition.Right
            };
        }
    }
}
