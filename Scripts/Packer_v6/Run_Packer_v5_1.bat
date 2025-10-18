@echo off
title ReActionAI Archiver v5.1
echo =====================================
echo ==     ReActionAI Archiver v5.1     ==
echo =====================================
echo.

set SCRIPT_DIR=%~dp0
powershell -ExecutionPolicy Bypass -File "%SCRIPT_DIR%pack_by_list_v5_1.ps1"
pause
