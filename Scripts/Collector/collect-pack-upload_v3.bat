@echo off
chcp 65001 >nul
setlocal EnableExtensions EnableDelayedExpansion
title ReActionAI Collector v3.0 (ZIP + Google Drive)

rem ====== НАСТРОЙКИ ======
rem Укажи путь к папке на Google Drive (можно сетевой диск G: или локальную "Мой диск")
set "GDRIVE_DEST=G:\Мой диск\ReActionAI\FullPacks"

rem ====== ПУТИ ======
set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%\..") do set "SCRIPTS_DIR=%%~fI"
for %%I in ("%SCRIPTS_DIR%\..") do set "REPO_DIR=%%~fI"

rem Проверим наличие решения
if not exist "%REPO_DIR%\ReActionAI.sln" (
  echo [ERROR] Не найдено решение: "%REPO_DIR%\ReActionAI.sln"
  echo        Проверь, что батник лежит в ReActionAI\Scripts\Collector
  pause & exit /b 1
)

rem ====== ВРЕМЕННЫЕ КАТАЛОГИ И ФАЙЛЫ ======
for /f %%T in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd_HHmmss"') do set "STAMP=%%T"
set "WORKDIR=%TEMP%\ReActionAI_FullPack_%STAMP%"
set "STAGE=%WORKDIR%\stage"
set "ZIP=%WORKDIR%\ReActionAI_FullPack_%STAMP%.zip"
set "LOG=%WORKDIR%\Collector.log"
mkdir "%WORKDIR%" 2>nul
echo [INFO] SCRIPT_DIR   = "%SCRIPT_DIR%"   >  "%LOG%"
echo [INFO] SCRIPTS_DIR  = "%SCRIPTS_DIR%" >> "%LOG%"
echo [INFO] REPO_DIR     = "%REPO_DIR%"    >> "%LOG%"
echo [INFO] WORKDIR      = "%WORKDIR%"     >> "%LOG%"
echo [INFO] STAGE        = "%STAGE%"       >> "%LOG%"
echo [INFO] GDRIVE_DEST  = "%GDRIVE_DEST%" >> "%LOG%"
echo.

echo [1/4] Подготовка staging-каталога...
mkdir "%STAGE%" 2>nul

rem Копируем чистую копию репозитория без мусора
echo [2/4] Копирование файлов (robocopy)...
robocopy "%REPO_DIR%" "%STAGE%" /MIR ^
  /XD .git .vs .idea .vscode packages node_modules obj bin TestResults ^
  /XF *.user *.suo *.lock *.tmp Thumbs.db desktop.ini > "%WORKDIR%\robocopy.log"
if errorlevel 8 (
  echo [ERROR] robocopy завершился с кодом ^>=8. См. "%WORKDIR%\robocopy.log" >> "%LOG%"
  echo [ERROR] robocopy failed. Смотри лог.
  pause & exit /b 2
) else (
  echo [OK] Robocopy завершён. >> "%LOG%"
)

rem Список файлов
dir /b /s "%STAGE%" > "%WORKDIR%\filelist.txt"

rem Упаковка через PowerShell Compress-Archive
echo [3/4] Упаковка в ZIP...
powershell -NoProfile -Command ^
  "Compress-Archive -Path '%STAGE%\*' -DestinationPath '%ZIP%' -CompressionLevel Optimal -Force" >> "%LOG%" 2>&1
if not exist "%ZIP%" (
  echo [ERROR] Не удалось создать ZIP: "%ZIP%" >> "%LOG%"
  echo [ERROR] ZIP не создан.
  pause & exit /b 3
) else (
  echo [OK] ZIP создан: "%ZIP%" >> "%LOG%"
)

rem Копирование на Google Drive
echo [4/4] Копирование на Google Drive...
if not exist "%GDRIVE_DEST%" mkdir "%GDRIVE_DEST%" 2>nul
copy /Y "%ZIP%" "%GDRIVE_DEST%\" >nul
if errorlevel 1 (
  echo [ERROR] Не удалось скопировать ZIP в "%GDRIVE_DEST%". >> "%LOG%"
  echo [ERROR] Копирование на Google Drive не удалось.
  pause & exit /b 4
)

echo.
echo [DONE] Архив: "%GDRIVE_DEST%\ReActionAI_FullPack_%STAMP%.zip"
echo [INFO] Лог:   "%LOG%"
echo [INFO] Список: "%WORKDIR%\filelist.txt"
echo.
pause
exit /b 0