# ReActionAI Archiver v5.1 (universal)
# Работает даже без System.IO.Compression.FileSystem
# Создаёт ZIP через чистый PowerShell Stream API

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host "`n====================================="
Write-Host "==  ReActionAI Archiver v5.1 (universal) =="
Write-Host "=====================================`n"

# --- Пути ---
$scriptDir   = Split-Path -Parent $MyInvocation.MyCommand.Definition
$requestPath = Join-Path $scriptDir 'ReActionAI_request.txt'

if (-not (Test-Path -LiteralPath $requestPath)) {
    throw "[ERROR] Request file not found: $requestPath"
}

# --- Чтение параметров KEY: VALUE ---
$pars = @{}
Get-Content -LiteralPath $requestPath | ForEach-Object {
    $line = $_.Trim()
    if ($line -match '^\s*$') { return }
    if ($line -notmatch '^\s*([^:]+)\s*:\s*(.+)\s*$') { return }
    $k = $matches[1].Trim().ToUpper()
    $v = $matches[2].Trim()
    $pars[$k] = $v
}

$archivePath = $pars['ARCHIVE']
$uploadPath  = $pars['UPLOAD_TO']
$purgeRaw    = $pars['PURGE_OLD']
$keepRaw     = $pars['KEEP_N']

# --- Проверка путей ---
if ([string]::IsNullOrWhiteSpace($archivePath)) { throw "[ERROR] ARCHIVE: путь не указан" }
if (-not (Test-Path -LiteralPath $archivePath)) { throw "[ERROR] ARCHIVE: путь не найден: $archivePath" }

if ([string]::IsNullOrWhiteSpace($uploadPath)) { throw "[ERROR] UPLOAD_TO: путь не указан" }
if (-not (Test-Path -LiteralPath $uploadPath)) {
    New-Item -ItemType Directory -Path $uploadPath | Out-Null
}

# --- Параметры очистки ---
$purge = if ($null -ne $purgeRaw) { $purgeRaw.ToUpper() } else { "" }
$purge = @("1","TRUE","YES","Y") -contains $purge

if ([string]::IsNullOrWhiteSpace($keepRaw)) {
    $keepN = 3
}
elseif (-not [int]::TryParse($keepRaw.Trim(), [ref]$keepN)) {
    $keepN = 3
}
if ($keepN -lt 0) { $keepN = 0 }

# --- Пути архивов ---
$zipName = 'ReActionAI_latest.zip'
$zipPath = Join-Path $uploadPath $zipName

Write-Host "[INFO] Source : $archivePath"
Write-Host "[INFO] Target : $zipPath"
Write-Host "[INFO] Purge old dated: $purge; KEEP_N: $keepN`n"

# --- Исключаем временные каталоги ---
$exclude = @('\.git($|\\)', '\.vs($|\\)', 'bin($|\\)', 'obj($|\\)', 'node_modules($|\\)', '\.idea($|\\)', '\.vscode($|\\)')

function Should-Exclude([string]$path) {
    foreach ($rx in $exclude) {
        if ($path -match $rx) { return $true }
    }
    return $false
}

# --- Удаляем старый архив ---
if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force -ErrorAction SilentlyContinue
}

# --- Создание архива вручную через Stream API ---
try {
    Add-Type -AssemblyName 'System.IO.Compression'
    $fileStream = [System.IO.File]::Create($zipPath)
    $zipArchive = New-Object System.IO.Compression.ZipArchive($fileStream, [System.IO.Compression.ZipArchiveMode]::Create, $false)

    $baseLen = ($archivePath.TrimEnd('\')).Length + 1
    Get-ChildItem -LiteralPath $archivePath -Recurse -File | ForEach-Object {
        $full = $_.FullName
        if (Should-Exclude $full) { return }

        $entryName = $full.Substring($baseLen)
        $entry = $zipArchive.CreateEntry($entryName, [System.IO.Compression.CompressionLevel]::Optimal)

        $entryStream = $entry.Open()
        $sourceStream = [System.IO.File]::OpenRead($full)
        $sourceStream.CopyTo($entryStream)
        $sourceStream.Close()
        $entryStream.Close()
    }

    $zipArchive.Dispose()
    $fileStream.Close()

    Write-Host "`n[OK] Archive created successfully." -ForegroundColor Green
}
catch {
    Write-Host "[ERROR] Archive creation failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# --- Очистка старых архивов ---
if ($purge) {
    Write-Host "`n[INFO] Purging old archives..."
    Get-ChildItem -LiteralPath $uploadPath -Filter 'ReActionAI_*.zip' -File -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -Skip $keepN |
        Remove-Item -Force -ErrorAction SilentlyContinue
}

Write-Host "`n[OK] Done. Archive path: $zipPath"
Write-Host "=====================================`n"