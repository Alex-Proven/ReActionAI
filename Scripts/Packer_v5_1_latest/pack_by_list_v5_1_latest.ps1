# ReActionAI Archiver v5.1-latest — фиксированное имя + управляемая чистка
# Encoding: UTF-8 (no BOM)
$ErrorActionPreference = "Stop"

# Пути
$scriptDir   = Split-Path -Parent $MyInvocation.MyCommand.Definition
$requestPath = Join-Path $scriptDir "ReActionAI_request.txt"

if (!(Test-Path $requestPath)) { throw "[ERROR] Request file not found: $requestPath" }

# Чтение параметров KEY: VALUE
$params = @{}
Get-Content $requestPath | ForEach-Object {
    $line = $_.Trim()
    if ($line -match '^\s*#') { return }
    if ($line -notmatch '.+:') { return }
    $k,$v = $line -split ":", 2
    $params[$k.Trim().ToUpper()] = $v.Trim()
}

$archivePath = $params["ARCHIVE"]
$uploadPath  = $params["UPLOAD_TO"]
$purgeRaw    = $params["PURGE_OLD"]
$keepNraw    = $params["KEEP_N"]

if (!(Test-Path $archivePath)) { throw "[ERROR] Source folder not found: $archivePath" }
if (!(Test-Path $uploadPath))  { New-Item -ItemType Directory -Force -Path $uploadPath | Out-Null }

# Настройки чистки
$purge = $false
if ($purgeRaw) { $purge = ($purgeRaw.ToUpper() -in @("TRUE","YES","1")) }
[int]$keepN = 0
if ($keepNraw) { [void][int]::TryParse($keepNraw, [ref]$keepN); if ($keepN -lt 0) { $keepN = 0 } }

$zipName = "ReActionAI_latest.zip"
$zipPath = Join-Path $uploadPath $zipName

Write-Host "`n====================================="
Write-Host "==  ReActionAI Archiver v5.1-latest =="
Write-Host "=====================================`n"
Write-Host "[INFO] Source: $archivePath"
Write-Host "[INFO] Target: $zipPath"
Write-Host "[INFO] Purge old dated: $purge; KEEP_N: $keepN"

# Удаляем текущий latest, если есть
if (Test-Path $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force -ErrorAction SilentlyContinue
}

# Создаём архив через .NET Zip API
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::Open($zipPath, [System.IO.Compression.ZipArchiveMode]::Create)
$base = (Resolve-Path $archivePath).Path

# Исключения
$exclude = @(
    '\\\.git(\\|$)', '\\\.vs(\\|$)',
    '\\bin(\\|$)',   '\\obj(\\|$)',
    '\\archives(\\|$)', '\\logs(\\|$)'
)
function Skip($p){
    foreach($rx in $exclude){ if($p -imatch $rx){ return $true } }
    $n=[IO.Path]::GetFileName($p)
    if($n -like '*.user' -or $n -like '*.suo' -or $n -like '*.zip'){return $true}
    return $false
}

Get-ChildItem -LiteralPath $archivePath -Recurse -Force -File | ForEach-Object {
    if (Skip $_.FullName) { return }
    $rel = $_.FullName.Substring($base.Length).TrimStart('\','/')
    $rel = $rel -replace '\\','/'
    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
        $zip, $_.FullName, $rel, [System.IO.Compression.CompressionLevel]::Optimal
    ) | Out-Null
}
$zip.Dispose()

# Чистка датированных архивов по желанию
if ($purge) {
    $dated = Get-ChildItem -LiteralPath $uploadPath -Filter "ReActionAI_*.zip" -File -ErrorAction SilentlyContinue |
             Sort-Object LastWriteTime -Descending
    if ($dated.Count -gt $keepN) {
        $dated | Select-Object -Skip $keepN | Remove-Item -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "[OK] Archive created: $zipPath" -ForegroundColor Green
