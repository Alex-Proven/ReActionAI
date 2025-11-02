using System;
using System.Linq;
using Autodesk.Revit.UI;
using ReActionAI.Modules.RevitChatGPT.UI;

namespace ReActionAI.Modules.RevitChatGPT
{
    public class App : IExternalApplication
    {
        private const string TabName   = "ReActionAI";
        private const string PanelName = "AI Tools";

        public Result OnStartup(UIControlledApplication application)
        {
            try { application.CreateRibbonTab(TabName); } catch { }
            var ribbonPanel = application.GetRibbonPanels(TabName)
                .FirstOrDefault(p => string.Equals(p.Name, PanelName, StringComparison.OrdinalIgnoreCase))
                ?? application.CreateRibbonPanel(TabName, PanelName);

            try
            {
                application.RegisterDockablePane(ChatDock.PaneId, ChatDock.PanelTitle, new ChatDock());
            }
            catch (InvalidOperationException)
            {
                // already registered — ignore
            }

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;
    }
}
