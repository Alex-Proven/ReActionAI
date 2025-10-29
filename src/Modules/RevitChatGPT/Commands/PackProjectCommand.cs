using System;
using System.Diagnostics;
using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ReActionAI.Modules.RevitChatGPT.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class PackProjectCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Определяем путь к корню проекта
                var assemblyPath = typeof(PackProjectCommand).Assembly.Location;
                var moduleDir = Path.GetDirectoryName(assemblyPath)!;
                var repoRoot = Directory.GetParent(moduleDir)!.Parent!.Parent!.Parent!.FullName;

                var ps1 = @"C:\Users\User\source\repos\ReActionAI\Make-ChatMini-Image.ps1";
                if (!File.Exists(ps1))
                {
                    TaskDialog.Show("ReActionAI", $"Скрипт не найден:\n{ps1}");
                    return Result.Failed;
                }

                // Аргументы PowerShell
                var args = $"-ExecutionPolicy Bypass -File \"{ps1}\" -AsPng -AsBase64";

                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = args,
                    WorkingDirectory = @"C:\Users\User\source\repos\ReActionAI",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var p = new Process { StartInfo = psi };
                p.Start();
                string stdOut = p.StandardOutput.ReadToEnd();
                string stdErr = p.StandardError.ReadToEnd();
                p.WaitForExit();

                string resultMsg;
                if (p.ExitCode == 0)
                {
                    resultMsg = $"✅ Упаковка завершена успешно!\n\n{stdOut}";
                }
                else
                {
                    resultMsg = $"⚠ Ошибка при упаковке (код {p.ExitCode}):\n\n{stdErr}\n\n{stdOut}";
                }

                TaskDialog.Show("ReActionAI — Pack Project", resultMsg);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}