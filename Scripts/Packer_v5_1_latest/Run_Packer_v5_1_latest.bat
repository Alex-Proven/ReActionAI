@echo off
title ReActionAI Archiver v5.1-latest
echo ==========================================
echo ==  ReActionAI Archiver v5.1 (latest)   ==
echo ==========================================
echo.
set "SCRIPT_DIR=%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%pack_by_list_v5_1_latest.ps1"
echo.
pause
