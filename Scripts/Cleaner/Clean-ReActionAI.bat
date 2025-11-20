@echo off
chcp 65001 >nul
setlocal EnableExtensions EnableDelayedExpansion
title ReActionAI Cleaner v1.0

rem ===== PATHS =====
set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%\..") do set "SCRIPTS_DIR=%%~fI"
for %%I in ("%SCRIPTS_DIR%\..") do set "REPO_DIR=%%~fI"

if not exist "%REPO_DIR%\ReActionAI.sln" (
  echo [ERROR] Solution file not found: "%REPO_DIR%\ReActionAI.sln"
  echo         Make sure the cleaner is inside ReActionAI\Scripts\Cleaner
  pause
  exit /b 1
)

echo.
echo [INFO] SCRIPT_DIR  = "%SCRIPT_DIR%"
echo [INFO] SCRIPTS_DIR = "%SCRIPTS_DIR%"
echo [INFO] REPO_DIR    = "%REPO_DIR%"
echo.

rem ===== [1/3] Remove .vs =====
echo [1/3] Removing .vs folder...

if exist "%REPO_DIR%\.vs" (
  echo       Removing "%REPO_DIR%\.vs"
  rmdir /S /Q "%REPO_DIR%\.vs"
) else (
  echo       .vs folder not found, skipping.
)

rem ===== [2/3] Remove all bin =====
echo.
echo [2/3] Searching and removing bin folders...

for /f "delims=" %%D in ('dir "%REPO_DIR%" /ad /b /s ^| findstr /i "\\bin$"') do (
  echo       Removing "%%D"
  rmdir /S /Q "%%D"
)

rem ===== [3/3] Remove all obj =====
echo.
echo [3/3] Searching and removing obj folders...

for /f "delims=" %%D in ('dir "%REPO_DIR%" /ad /b /s ^| findstr /i "\\obj$"') do (
  echo       Removing "%%D"
  rmdir /S /Q "%%D"
)

echo.
echo ==========================================
echo Cleanup finished (.vs + bin + obj)
echo ==========================================
pause