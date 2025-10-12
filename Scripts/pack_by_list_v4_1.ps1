# ===== ReActionAI — Archiver V4.1 (Stable) =====
# Рабочая версия с датой и удалением запроса

$ErrorActionPreference = 'Stop'
$RequestFile = 'C:\Users\User\MyDrive\ReActionAI\ReActionAI_request.txt'

if (!(Test-Path $RequestFile)) {
    Write-Host "[ERROR] Request file not found: $RequestFile" -ForegroundColor Red
    exit 1
}

$ARCHIVE = $null; $UPLOAD_TO = $null; $DELETE_REQUEST_AFTER = $false
Get-Content $RequestFile | ForEach-Object {
    $line = $_.Trim()
    if (-not $line -or $line.StartsWith('#')) { return }
    if ($line -match '^(?i)ARCHIVE:\s*(.+)$') { $ARCHIVE = $matches[1].Trim('" ') }
    elseif ($line -match '^(?i)UPLOAD_TO:\s*(.+)$') { $UPLOAD_TO = $matches[1].Trim('" ') }
    elseif ($line -match '^(?i)DELETE_REQUEST_AFTER:\s*(.+)$') { $DELETE_REQUEST_AFTER = ($matches[1] -match '^(?i)(true|1|yes)$') }
}

if (-not (Test-Path $ARCHIVE)) { Write-Host "[ERROR] Archive path not found: $ARCHIVE" -ForegroundColor Red; exit 1 }
if (-not (Test-Path $UPLOAD_TO)) { New-Item -ItemType Directory -Force -Path $UPLOAD_TO | Out-Null }

$timestamp = Get-Date -Format 'yyyy-MM-dd_HH-mm-ss'
$zipName = "ReActionAI_$timestamp.zip"
$localZip = Join-Path $ARCHIVE $zipName
$destZip  = Join-Path $UPLOAD_TO $zipName

Write-Host "[INFO] Creating archive: $zipName"
Compress-Archive -Path (Join-Path $ARCHIVE '*') -DestinationPath $localZip -Force
Copy-Item $localZip -Destination $destZip -Force
Remove-Item $localZip -Force -ErrorAction SilentlyContinue

if ($DELETE_REQUEST_AFTER -and (Test-Path $RequestFile)) {
    Remove-Item $RequestFile -Force
    Write-Host "[INFO] Request file removed."
}

if (Test-Path $destZip) {
    Write-Host "`n✅ Upload completed successfully!"
    Write-Host "Archive uploaded to: $destZip"
} else {
    Write-Host "`n⚠️ Copy finished, but archive not found at destination."
}
