using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace ReActionAI.Integration.Revit.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class OpenPanelCommand : IExternalCommand
    {
        // Тот же GUID, что и в App.ChatPanelGuid
        private static readonly Guid ChatPanelGuid = new("6F2F9E1C-5A5C-4DB4-9E8A-2B840D433E52");

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var dpId = new DockablePaneId(ChatPanelGuid);
                var pane = commandData.Application.GetDockablePane(dpId);

                pane.Show();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("ReActionAI", $"Ошибка: {ex.Message}");
                return Result.Failed;
            }
        }
    }
}
