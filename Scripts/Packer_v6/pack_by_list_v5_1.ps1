# === ReActionAI Archiver v5.1 ===
$ErrorActionPreference = "Stop"

# Определяем путь к текущей папке (где лежит сам скрипт)
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$requestPath = Join-Path $scriptDir "ReActionAI_request.txt"

# Проверка наличия файла запроса
if (!(Test-Path $requestPath)) {
    Write-Host "[ERROR] Request file not found: $requestPath" -ForegroundColor Red
    pause
    exit
}

# Чтение параметров из ReActionAI_request.txt
$content = Get-Content $requestPath | Where-Object { $_ -match ".+:.+" }
$params = @{}
foreach ($line in $content) {
    $key, $value = $line -split ":", 2
    $params[$key.Trim()] = $value.Trim()
}

$archivePath = $params["ARCHIVE"]
$uploadPath  = $params["UPLOAD_TO"]

Write-Host "`n====================================="
Write-Host "==     ReActionAI Archiver v5.1     =="
Write-Host "=====================================`n"
Write-Host "[INFO] Source path: $archivePath"
Write-Host "[INFO] Upload path: $uploadPath"

# Проверяем папку для архивации
if (!(Test-Path $archivePath)) {
    Write-Host "[ERROR] Source folder not found: $archivePath" -ForegroundColor Red
    pause
    exit
}

# Создаём имя архива
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$archiveName = "ReActionAI_$timestamp.zip"
$archiveFullPath = Join-Path $uploadPath $archiveName

# Удаляем старые архивы (кроме последнего)
if (Test-Path $uploadPath) {
    $oldZips = Get-ChildItem -Path $uploadPath -Filter "ReActionAI_*.zip" | Sort-Object LastWriteTime -Descending
    if ($oldZips.Count -gt 1) {
        $oldZips | Select-Object -Skip 1 | Remove-Item -Force
    }
} else {
    New-Item -ItemType Directory -Force -Path $uploadPath | Out-Null
}

# Создаём архив
Write-Host "`n[INFO] Creating archive..."
Compress-Archive -Path "$archivePath\*" -DestinationPath $archiveFullPath -Force

Write-Host "[OK] Archive created: $archiveFullPath" -ForegroundColor Green
pause
