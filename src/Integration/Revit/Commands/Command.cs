using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ReActionAI.Integration.Revit.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            var uiApp = commandData.Application;

            App.EnsureThemeSubscription(uiApp);

            try
            {
                // Открываем нашу док-панель по ID из App
                DockablePane pane = uiApp.GetDockablePane(App.ChatPaneId);
                pane.Show();   // В Revit 2024 другого метода нет

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = "Не удалось открыть док-панель ChatGPT: " + ex.Message;
                return Result.Failed;
            }
        }
    }
}
