@echo off
chcp 65001 >nul
title ReActionAI Collector v2.8
setlocal enabledelayedexpansion

echo =============================================
echo ==     ReActionAI Collector v2.8 (UTF8)     ==
echo =============================================

REM ---------- 1) Определяем директории ----------
set "SCRIPT_DIR=%~dp0"
call :normalize "%SCRIPT_DIR%" SCRIPT_DIR

set "SCRIPTS_DIR=%SCRIPT_DIR%\.."
call :normalize "%SCRIPTS_DIR%" SCRIPTS_DIR

set "REPO_DIR=%SCRIPTS_DIR%\.."
call :normalize "%REPO_DIR%" REPO_DIR

echo SCRIPT_DIR  = "%SCRIPT_DIR%"
echo SCRIPTS_DIR = "%SCRIPTS_DIR%"
echo REPO_DIR    = "%REPO_DIR%"
echo --------------------------------------------

REM ---------- 2) Проверим наличие решения ----------
if not exist "%REPO_DIR%\ReActionAI.sln" (
    echo [ERROR] Не найден файл решения "%REPO_DIR%\ReActionAI.sln"
    pause
    exit /b 1
)

REM ---------- 3) Таймштамп ----------
for /f "tokens=1-3 delims=:." %%a in ("%time%") do set "TIMESTAMP=%%a-%%b-%%c"
set "DATESTAMP=%date:~6,4%-%date:~3,2%-%date:~0,2%"
set "WORKDIR=%TEMP%\ReActionAI_FullPack_%DATESTAMP%_%TIMESTAMP%"
mkdir "%WORKDIR%" >nul 2>&1

echo [INFO] Рабочая папка: "%WORKDIR%"
echo [INFO] Сканирование проекта...
dir "%REPO_DIR%" /b /s > "%WORKDIR%\filelist.txt"

echo [OK] Список файлов сохранён: "%WORKDIR%\filelist.txt"
echo.
pause
exit /b 0


REM ============================================================
REM ===                 ВСПОМОГАТЕЛЬНАЯ ФУНКЦИЯ               ===
REM ============================================================
:normalize
pushd "%~1" >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Не удалось перейти в "%~1"
    exit /b 1
)
set "%2=%CD%"
popd >nul
exit /b 0