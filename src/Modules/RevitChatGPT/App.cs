using System;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.UI;

namespace ReActionAI.Modules.RevitChatGPT
{
    /// <summary>
    /// Revit Application Add-in:
    /// - создаёт вкладку ReActionAI и панель AI Tools;
    /// - регистрирует док-панель ChatGPT (справа);
    /// - добавляет кнопки Pack Project и ChatGPT.
    /// </summary>
    public class App : IExternalApplication
    {
        private const string TabName   = "ReActionAI";
        private const string PanelName = "AI Tools";

        public Result OnStartup(UIControlledApplication application)
        {
            // 1) Вкладка
            try { application.CreateRibbonTab(TabName); } catch { /* уже существует */ }

            // 2) Панель
            RibbonPanel panel = application.GetRibbonPanels(TabName)
                .FirstOrDefault(p => string.Equals(p.Name, PanelName, StringComparison.OrdinalIgnoreCase))
                ?? application.CreateRibbonPanel(TabName, PanelName);

            // 3) Регистрация док-панели ChatGPT (DockedRight)
            try
            {
                var provider = new UI.ChatPaneProvider(); // класс провайдера содержимого
                application.RegisterDockablePane(UI.ChatDock.PaneId, UI.ChatDock.PaneTitle, provider);
            }
            catch
            {
                // Если уже зарегистрирована — игнорируем
            }

            // 4) Кнопки
            string asmPath = Assembly.GetExecutingAssembly().Location;

            // 4.1 Pack Project (у вас уже была — проверим, чтобы не дублировать)
            var packBtn = new PushButtonData(
                name: "PackProjectButton",
                text: "Pack Project",
                assemblyName: asmPath,
                className: "ReActionAI.Modules.RevitChatGPT.Commands.PackProjectCommand")
            {
                ToolTip = "Собрать ZIP/PNG/Base64 через Make-ChatMini-Image.ps1"
            };
            if (!panel.GetItems().OfType<PushButton>().Any(b => b.Name == "PackProjectButton"))
                panel.AddItem(packBtn);

            // 4.2 ChatGPT
            var chatBtn = new PushButtonData(
                name: "OpenChatWindowButton",
                text: "ChatGPT",
                assemblyName: asmPath,
                className: "ReActionAI.Modules.RevitChatGPT.Commands.OpenChatWindowCommand")
            {
                ToolTip = "Открыть панель ChatGPT (закреплена справа)"
            };
            if (!panel.GetItems().OfType<PushButton>().Any(b => b.Name == "OpenChatWindowButton"))
                panel.AddItem(chatBtn);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            // Здесь ничего очищать не требуется
            return Result.Succeeded;
        }
    }
}