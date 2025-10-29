using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System;
using System.Windows;

namespace ReActionAI.Modules.RevitChatGPT
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var window = new UI.ChatWindow();
                window.Show();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("ReActionAI", $"Failed to open Chat window: {ex.Message}");
                return Result.Failed;
            }
        }
    }
}
