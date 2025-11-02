<# ReActionAI • FixPack_v6 • deploy.ps1 #>

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# 1) Пути
$repoRoot   = Split-Path -Parent $PSScriptRoot
$repoRoot   = Split-Path -Parent $repoRoot
$sln        = Join-Path $repoRoot 'ReActionAI.sln'
$binDebug   = Join-Path $repoRoot 'src\Modules\RevitChatGPT\bin\x64\Debug\net48'
$addinRoot  = Join-Path $env:APPDATA 'Autodesk\Revit\Addins\2024\ReActionAI'

Write-Host "`n=== FixPack_v6: restore + build + deploy ===`n"

# 2) Очистка bin/obj
Write-Host "[1/4] Cleaning old builds..."
Get-ChildItem $repoRoot -Recurse -Include bin,obj -Directory |
  ForEach-Object { Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue }

# 3) Восстановление пакетов NuGet
Write-Host "[2/4] Restoring NuGet packages..."
dotnet restore $sln

# 4) Сборка решения Debug|x64
Write-Host "[3/4] Building solution (Debug|x64)..."
$msbuild = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
if (-not (Test-Path $msbuild)) { $msbuild = "msbuild" }
& $msbuild $sln /m /t:Build /p:Configuration=Debug /p:Platform=x64

# 5) Копирование .dll и .addin в %AppData%
Write-Host "[4/4] Deploying addin..."
New-Item -ItemType Directory -Force -Path $addinRoot | Out-Null

$srcDll = Join-Path $binDebug 'ReActionAI.Modules.RevitChatGPT.dll'
$srcAddin = Join-Path $repoRoot 'ReActionAI.Modules.RevitChatGPT.addin'

if (-not (Test-Path $srcDll)) {
    throw "Module DLL not found: $srcDll"
}

Copy-Item $srcDll $addinRoot -Force
Copy-Item $srcAddin $addinRoot -Force

Write-Host "`n=== FixPack_v6 completed successfully ===`n"
Write-Host "Revit Addin deployed to:`n$addinRoot`n"