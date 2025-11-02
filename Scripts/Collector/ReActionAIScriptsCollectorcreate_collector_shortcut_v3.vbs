On Error Resume Next
Dim fso, shell, here, batPath, desktop, lnk
Set fso   = CreateObject("Scripting.FileSystemObject")
Set shell = CreateObject("WScript.Shell")

here    = fso.GetParentFolderName(WScript.ScriptFullName) ' ...\Scripts\Collector
batPath = fso.BuildPath(here, "collect-pack-upload_v3.bat")

If Not fso.FileExists(batPath) Then
  MsgBox "Не найден батник: " & batPath, vbCritical, "ReActionAI Collector"
  WScript.Quit 1
End If

desktop = shell.SpecialFolders("Desktop")
Set lnk = shell.CreateShortcut(fso.BuildPath(desktop, "ReActionAI Collector.lnk"))
lnk.TargetPath       = batPath
lnk.WorkingDirectory = here
lnk.IconLocation     = "%SystemRoot%\System32\shell32.dll, 44"
lnk.Description      = "Собрать, упаковать и выложить FullPack на Google Drive"
lnk.Save

MsgBox "Готово. Ярлык создан на рабочем столе." & vbCrLf & batPath, vbInformation, "ReActionAI Collector"