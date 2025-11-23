// File: src/Modules/RevitChatGPT/Commands/PackProjectCommand.cs
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ReActionAI.Integration.Revit.Commands
{
    /// <summary>
    /// Заглушка команды "Pack Project".
    /// Позже сюда добавим реальную упаковку проекта.
    /// Сейчас — просто сообщение + Success, чтобы команда корректно загружалась.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class PackProjectCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog.Show("ReActionAI", "Pack Project: команда загружена.\n(Функционал будет добавлен позже.)");
            return Result.Succeeded;
        }
    }
}