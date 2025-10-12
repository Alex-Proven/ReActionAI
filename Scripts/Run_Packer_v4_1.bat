@echo off
cd /d "%~dp0"
powershell -ExecutionPolicy Bypass -File ".\pack_by_list_v4_1.ps1"
pause
