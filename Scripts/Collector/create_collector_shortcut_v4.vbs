' create_collector_shortcut_v2.vbs
On Error Resume Next

Dim fso, shell, here, batPath, desktop, lnk
Set fso   = CreateObject("Scripting.FileSystemObject")
Set shell = CreateObject("WScript.Shell")

here    = fso.GetParentFolderName(WScript.ScriptFullName)
batPath = fso.BuildPath(here, "collect-pack-upload_v2.bat")

If Not fso.FileExists(batPath) Then
  MsgBox "Не найден батник: " & batPath, vbCritical, "ReActionAI Collector"
  WScript.Quit 1
End If

desktop = shell.SpecialFolders("Desktop")

Set lnk = shell.CreateShortcut(fso.BuildPath(desktop, "ReActionAI Collector.lnk"))
lnk.TargetPath       = batPath            ' < напрямую на .bat (как в V1)
lnk.WorkingDirectory = here
lnk.IconLocation     = "%SystemRoot%\System32\shell32.dll,44"
lnk.Description      = "Собрать и упаковать данные проекта ReActionAI"
lnk.Save

If Err.Number <> 0 Then
  MsgBox "Ошибка создания ярлыка: " & Err.Description, vbCritical, "ReActionAI Collector"
Else
  MsgBox "Ярлык создан на рабочем столе.", vbInformation, "ReActionAI Collector"
End If