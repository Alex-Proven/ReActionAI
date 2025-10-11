@echo off
:: Run_Packer.bat — ярлык для запуска PowerShell-скрипта архиватора
:: Версия от 2025-10-07

setlocal
:: Укажи путь к папке, где лежит pack_by_list.ps1
set SCRIPT_DIR=%~dp0

:: Если нужно, можно изменить путь к PowerShell (для Windows 10/11 по умолчанию работает)
set POWERSHELL_EXE=powershell.exe

echo.
echo Запуск архиватора ReActionAI...
echo.

%POWERSHELL_EXE% -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%pack_by_list.ps1"

echo.
echo ------------------------------------------
echo Работа завершена.
pause
endlocal
