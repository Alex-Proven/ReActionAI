using Autodesk.Revit.UI;
using ReActionAI.Abstractions;
using System;
using System.Reflection;

namespace ReActionAI.Modules.RevitChatGPT
{
    /// <summary>
    /// Точка входа Revit-плагина. Не WPF.
    /// </summary>
    public class App : IExternalApplication, IReActionModule
    {
        public string Name => "ChatGPT Client";
        public string Version => "0.1.0";

        public void Initialize()
        {
            // hook into ReActionAI Core если понадобится
        }

        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "ReActionAI";
            try { application.CreateRibbonTab(tabName); } catch { /* уже существует */ }

            RibbonPanel panel;
            try { panel = application.CreateRibbonPanel(tabName, "AI Tools"); }
            catch { panel = GetPanel(application, tabName, "AI Tools"); }

            var buttonData = new PushButtonData(
                "ChatGPTClient",
                "ChatGPT",
                Assembly.GetExecutingAssembly().Location,
                "ReActionAI.Modules.RevitChatGPT.Command"
            );

            panel.AddItem(buttonData);
            return Result.Succeeded;
        }

        private RibbonPanel GetPanel(UIControlledApplication app, string tab, string title)
        {
            foreach (var pnl in app.GetRibbonPanels(tab))
                if (pnl.Name == title) return pnl;

            return app.CreateRibbonPanel(tab, title);
        }

        public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;
    }
}
