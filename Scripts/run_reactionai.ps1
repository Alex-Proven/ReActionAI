param([string]$UriArg)

# v4:
# - Централизованная папка архивов по умолчанию (G:\Мой диск\ReActionAI\archives, fallback C:\Users\User\MyDrive\ReActionAI\archives)
# - Перед сохранением нового архива удаляются ВСЕ *.zip в целевой папке
# - Параметры можно переопределять через URL: root, include, exclude, target, out, desc

Add-Type -AssemblyName System.Web | Out-Null
try { $u = [Uri]$UriArg } catch { Write-Error "Bad URI: $UriArg"; exit 1 }
$q = [System.Web.HttpUtility]::ParseQueryString($u.Query)

$here   = Split-Path -Parent $MyInvocation.MyCommand.Path
$mode   = ($u.Host)

# Общие
$desc   = $q['desc'];   if ([string]::IsNullOrWhiteSpace($desc))   { $desc='Auto from ChatGPT' }

# Централизованные папки
$central1 = 'G:\Мой диск\ReActionAI\archives'
$central2 = 'C:\Users\User\MyDrive\ReActionAI\archives'

# Опциональный запуск внешнего раннера
$exe = Join-Path $here 'ReActionAI.exe'
$bat = Join-Path $here 'Run_ReActionAI_Archiver.bat'

function Start-ReActionAI([string]$reqPath) {
  if (Test-Path $exe) {
    Start-Process -FilePath $exe -ArgumentList @('--request', $reqPath) -WorkingDirectory $here
  } elseif (Test-Path $bat) {
    Start-Process -FilePath $bat -ArgumentList @($reqPath) -WorkingDirectory $here
  }
}

if ($mode -eq 'collect') {
  $root    = $q['root'];   if ([string]::IsNullOrWhiteSpace($root)) { $root = $here }
  $include = $q['include']
  $exclude = $q['exclude']
  $target  = $q['target']

  # Цель по умолчанию: централизованная папка архивов
  if ([string]::IsNullOrWhiteSpace($target)) {
    if (Test-Path $central1) { $target = $central1 }
    else { $target = $central2 }
  }

  $out     = $q['out']

  $root = $root.Trim('"')
  if (-not (Test-Path $root)) { Write-Error "ROOT not found: $root"; exit 1 }

  # Убедиться, что папка-назначение существует
  if (-not (Test-Path $target)) { New-Item -ItemType Directory -Path $target -Force | Out-Null }

  # === Очистка старых архивов ===
  try {
    Get-ChildItem -Path $target -Filter '*.zip' -File -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue
    Write-Host "Удалены старые архивы в $target"
  } catch {
    Write-Warning ("Не удалось очистить папку архива: " + $_.Exception.Message)
  }

  # Формирование имени файла архива
  $ts = Get-Date -Format 'yyyy-MM-dd_HH-mm-ss'
  $projectName = Split-Path $root -Leaf
  if ([string]::IsNullOrWhiteSpace($projectName)) { $projectName = 'ReActionAI' }

  $zipName = if ([string]::IsNullOrWhiteSpace($out)) { "${projectName}_${ts}.zip" } else { $out }
  if (-not $zipName.ToLower().EndsWith('.zip')) { $zipName += '.zip' }
  $zipPath = if ([IO.Path]::IsPathRooted($zipName)) { $zipName } else { Join-Path $target $zipName }

  # Сбор включаемых файлов
  $incs = @()
  if (-not [string]::IsNullOrWhiteSpace($include)) {
    $include.Split(';') | ForEach-Object {
      $p = $_.Trim()
      if ($p) {
        if (-not [IO.Path]::IsPathRooted($p)) { $p = Join-Path $root $p }
        $incs += (Get-ChildItem -LiteralPath $p -Recurse -File -ErrorAction SilentlyContinue)
      }
    }
  } else {
    $incs = Get-ChildItem -LiteralPath $root -Recurse -File -ErrorAction SilentlyContinue
  }

  # Применение исключений
  $exRules = @()
  if (-not [string]::IsNullOrWhiteSpace($exclude)) {
    $exclude.Split(';') | ForEach-Object {
      $p = $_.Trim()
      if ($p) {
        if (-not [IO.Path]::IsPathRooted($p)) { $p = Join-Path $root $p }
        $exRules += $p
      }
    }
  }

  if ($exRules.Count -gt 0) {
    $incs = $incs | Where-Object {
      $full = $_.FullName
      -not ($exRules | Where-Object { $full.StartsWith($_) -or ($full -like ($_ -replace '\\','\\') + '*') })
    }
  }

  if ($incs.Count -eq 0) { Write-Error "Нечего архивировать (include пуст)"; exit 1 }

  # Создание архива
  if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
  Compress-Archive -Path ($incs | ForEach-Object { $_.FullName }) -DestinationPath $zipPath -Force

  # Request-файл (на будущее)
  $req = Join-Path $here 'ReActionAI_request_auto.txt'
  @(
    '# ReActionAI Request Auto (collect v4)'
    'ARCHIVE = "' + $zipPath + '"'
    'TARGET = "'  + $target  + '"'
    'DESCRIPTION = "' + $desc + '"'
  ) | Out-File -FilePath $req -Encoding utf8 -Force

  Start-ReActionAI $req
  Write-Host "ZIP создан: $zipPath"
  exit 0
}

Write-Error "Unknown mode. Use reactionai://collect?... "
exit 1
