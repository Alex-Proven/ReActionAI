using System;
using System.Reflection;
using Autodesk.Revit.UI;

namespace ReActionAI.Modules.RevitChatGPT
{
    /// Добавляет панель «AI Tools» и кнопку «Pack Project»
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            const string tabName = "ReActionAI";
            const string panelName = "AI Tools";

            try { application.CreateRibbonTab(tabName); } catch { /* таб уже есть */ }

            RibbonPanel? panel = null;
            foreach (var p in application.GetRibbonPanels(tabName))
                if (p.Name.Equals(panelName, StringComparison.OrdinalIgnoreCase)) { panel = p; break; }
            panel ??= application.CreateRibbonPanel(tabName, panelName);

            string asm = Assembly.GetExecutingAssembly().Location;
            var btnData = new PushButtonData(
                "PackProjectButton",
                "Pack Project",
                asm,
                "ReActionAI.Modules.RevitChatGPT.Commands.PackProjectCommand"
            );
            var btn = panel.AddItem(btnData) as PushButton;
            if (btn != null)
                btn.ToolTip = "Собрать ZIP/PNG/Base64 через Make-ChatMini-Image.ps1";

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;
    }
}