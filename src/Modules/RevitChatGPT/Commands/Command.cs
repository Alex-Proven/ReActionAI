using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using ReActionAI.Modules.RevitChatGPT.UI;

namespace ReActionAI.Modules.RevitChatGPT.Commands
{
    [Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            var uiApp = commandData.Application;

            try
            {
                var pane = uiApp.GetDockablePane(ChatDock.PaneId); // <-- PaneId
                pane.Show();
                return Result.Succeeded;
            }
            catch
            {
                message = "Панель ChatGPT не зарегистрирована. Проверь OnStartup и GUID ChatDock.PaneId.";
                return Result.Failed;
            }
        }
    }
}
