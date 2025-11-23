using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ReActionAI.Integration.Revit.Commands
{
    /// <summary>
    /// Заглушка команды Pack Project.
    /// Позже сюда добавим функционал упаковки ReActionAI-проекта.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class PackProjectCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog.Show(
                "ReActionAI",
                "Команда Pack Project загружена успешно.\nФункционал будет добавлен позже."
            );
            return Result.Succeeded;
        }
    }
}
