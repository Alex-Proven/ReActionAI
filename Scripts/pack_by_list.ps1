param(
  [string]$ProjectRoot = "$env:USERPROFILE\Projects\ReActionAI",
  [string]$OutDir = "$env:USERPROFILE\Downloads",  # Архив хранится в Загрузках
  [string]$ZipName = "ReActionAI_request.zip",
  [string]$PythonExe = "python"
)

# === Настройка логирования ===
$LogDir = Join-Path $PSScriptRoot "logs"
New-Item -ItemType Directory -Force -Path $LogDir | Out-Null
$Timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$LogFile = Join-Path $LogDir "packer_log_$Timestamp.txt"

Start-Transcript -Path $LogFile -Append | Out-Null

Write-Host "=== ReActionAI Archiver started ==="
Write-Host "Log file: $LogFile"
Write-Host ""

try {
    # === Считываем текст из буфера ===
    $ClipboardText = Get-Clipboard -Raw
    if ([string]::IsNullOrWhiteSpace($ClipboardText)) {
        Write-Error "❌ Буфер обмена пуст. Скопируй текст filelist (через 'Копировать код') и запусти снова."
        Stop-Transcript | Out-Null
        exit 1
    }

    # === Создаём локальный файл списка ===
    $FilelistPath = Join-Path -Path $PSScriptRoot -ChildPath "request_sources_from_clipboard.txt"
    Set-Content -Path $FilelistPath -Value $ClipboardText -Encoding UTF8

    Write-Host "[INFO] Создан файл: $FilelistPath"
    Write-Host "[INFO] Кол-во строк: $($ClipboardText.Split([Environment]::NewLine).Count)"
    Write-Host ""

    # === Подготовка путей ===
    New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
    $OutZip = Join-Path $OutDir $ZipName

    # Если файл уже существует — удаляем, чтобы не копился мусор
    if (Test-Path $OutZip) {
        Remove-Item $OutZip -Force
        Write-Host "[INFO] Удалён старый архив: $OutZip"
    }

    Write-Host "[INFO] root: $ProjectRoot"
    Write-Host "[INFO] list: $FilelistPath"
    Write-Host "[INFO] out : $OutZip"
    Write-Host ""

    # === Запуск Python-архиватора ===
    & $PythonExe ".\pack_by_list.py" `
      --root $ProjectRoot `
      --list $FilelistPath `
      --out $OutZip `
      --manifest manifest.json `
      --preserve-times

    Write-Host ""
    Write-Host "[OK] Архив успешно сохранён в: $OutZip"

} catch {
    Write-Error "❌ Произошла ошибка: $($_.Exception.Message)"
} finally {
    Stop-Transcript | Out-Null
    Write-Host ""
    Write-Host "[INFO] Лог сохранён: $LogFile"
    Write-Host "=== ReActionAI Archiver finished ==="
}
