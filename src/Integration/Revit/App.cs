using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System;
using ReActionAI.Integration.Revit.UI;

namespace ReActionAI.Integration.Revit
{
    public class App : IExternalApplication
    {
        // Общий GUID панели чата – должен совпадать с GUID в OpenPanelCommand
        private static readonly Guid ChatPanelGuid = new("6F2F9E1C-5A5C-4DB4-9E8A-2B840D433E52");

        private ChatPanelProvider? _chatPanelProvider;

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                var panel = application.CreateRibbonPanel("ReActionAI");

                // Кнопка ассистента
                var buttonData = new PushButtonData(
                    "ReActionAI_AssistantButton",
                    "Ассистент",
                    typeof(App).Assembly.Location,
                    "ReActionAI.Integration.Revit.Commands.OpenPanelCommand"
                );

                panel.AddItem(buttonData);

                // Регистрация Dockable Pane
                _chatPanelProvider = new ChatPanelProvider();

                var dpId = new DockablePaneId(ChatPanelGuid);

                application.RegisterDockablePane(
                    dpId,
                    "ReActionAI – Ассистент",
                    _chatPanelProvider
                );

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("ReActionAI", $"Ошибка загрузки: {ex.Message}");
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
