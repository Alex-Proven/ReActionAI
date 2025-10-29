# Make-ChatMini-Image.ps1 v2.5
# Универсальный упаковщик проекта: создаёт ZIP с исключениями, опционально PNG и Base64.
# Запускать из корня репозитория ReActionAI.

[CmdletBinding()]
param(
    # Корень проекта (по умолчанию — текущая папка)
    [string]$ProjectRoot = (Get-Location).Path,
    # Имя без расширения
    [string]$OutName = "ReActionAI_latest",
    # Папка для вывода
    [string]$OutDir = (Join-Path (Get-Location).Path "image_out"),
    # Сделать копию архива под видом картинки .png
    [switch]$AsPng,
    # Сгенерировать Base64-файл
    [switch]$AsBase64
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# === Вспомогательные функции ===
function Write-Info($msg) { Write-Host "[INFO] $msg" -ForegroundColor DarkCyan }
function Write-Ok($msg)   { Write-Host "[OK]   $msg" -ForegroundColor Green }
function Write-Warn($msg) { Write-Host "[WARN] $msg" -ForegroundColor Yellow }
function Write-Err($msg)  { Write-Host "[ERROR] $msg" -ForegroundColor Red }

# --- Подготовка путей ---
$ProjectRoot = (Resolve-Path $ProjectRoot).Path
if (-not (Test-Path $OutDir)) {
    New-Item -ItemType Directory -Path $OutDir -Force | Out-Null
}

$zipPath  = Join-Path $OutDir "$OutName.zip"
$pngPath  = Join-Path $OutDir "$OutName.png"
$b64Path  = Join-Path $OutDir "$OutName.base64.txt"

Write-Host ""
Write-Host "=== ReActionAI Archiver / Mini-Image ===" -ForegroundColor White
Write-Info  "Root   : $ProjectRoot"
Write-Info  "Output : $OutDir"
Write-Info  "Name   : $OutName"
Write-Host ""

# --- Список исключений ---
$excludeRoots = @(
    '\.git($|\\)',
    '\.vs($|\\)',
    '\.idea($|\\)',
    '\.vscode($|\\)',
    'bin($|\\)',
    'obj($|\\)',
    'packages($|\\)',
    'node_modules($|\\)',
    'image_out($|\\)',
    'archives($|\\)'
)
$excludeRegex = ($excludeRoots -join '|')

# --- Сбор файлов ---
Write-Info "Collecting files (excluding: .git, .vs, bin, obj, packages, node_modules, image_out, archives)..."
$files = Get-ChildItem -LiteralPath $ProjectRoot -Recurse -File -Force -ErrorAction SilentlyContinue |
         Where-Object { $_.FullName -notmatch $excludeRegex }

if (-not $files) {
    Write-Err "Файлы не найдены (после фильтрации). Проверь корень проекта."
    exit 1
}

# --- Удаляем старые артефакты ---
foreach ($p in @($zipPath, $pngPath, $b64Path)) {
    if (Test-Path $p) { Remove-Item $p -Force -ErrorAction SilentlyContinue }
}

# --- Упаковка ---
Write-Info "Creating ZIP archive..."
try {
    Compress-Archive -Path ($files.FullName) -DestinationPath $zipPath -CompressionLevel Optimal -Force
    if (!(Test-Path $zipPath)) { throw "Не удалось создать ZIP." }
    Write-Ok "ZIP создан: $zipPath"
} catch {
    Write-Err "Ошибка при создании архива: $_"
    exit 1
}

# --- PNG-копия (маскировка) ---
if ($AsPng) {
    Copy-Item -LiteralPath $zipPath -Destination $pngPath -Force
    if (Test-Path $pngPath) { Write-Ok "PNG-копия создана: $pngPath" } else { Write-Warn "PNG не создан." }
}

# --- Base64 .txt (опционально) ---
if ($AsBase64) {
    Write-Info "Генерирую Base64..."
    [byte[]]$bytes = [System.IO.File]::ReadAllBytes($zipPath)
    $b64 = [Convert]::ToBase64String($bytes)
    Set-Content -LiteralPath $b64Path -Value $b64 -Encoding UTF8
    if (Test-Path $b64Path) { Write-Ok "Base64 сохранён: $b64Path" } else { Write-Warn "Base64 не создан." }
}

Write-Host ""
Write-Ok "Готово!"
Write-Info ("Артефакты: " + ((Get-ChildItem $OutDir | Select-Object -ExpandProperty Name) -join ', '))