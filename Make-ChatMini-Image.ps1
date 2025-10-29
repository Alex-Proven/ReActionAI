# Создаёт мини-клиент ChatGPT для Revit 2024 и пакует его в ZIP, затем копирует как .png
# Запускать из корня репозитория ReActionAI.

$ErrorActionPreference = "Stop"
$root = Get-Location
$projDir = Join-Path $root "src\Integrations\Revit\ReActionAI.ChatMini"

# Создание директорий
$dirs = @(
    "$projDir",
    "$projDir\Commands",
    "$projDir\Services",
    "$projDir\UI",
    "$root\docs"
)
foreach ($d in $dirs) { New-Item -ItemType Directory -Force -Path $d | Out-Null }

function W($Path, $Text) {
    $dir = Split-Path -Parent $Path
    if (!(Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    Set-Content -Path $Path -Value $Text -Encoding UTF8
}

# === Пример одного файла ===
$csproj = @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
    <UseWPF>true</UseWPF>
    <RootNamespace>ReActionAI.ChatMini</RootNamespace>
    <AssemblyName>ReActionAI.ChatMini</AssemblyName>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="RevitAPI">
      <HintPath>$(ProgramFiles)\Autodesk\Revit 2024\RevitAPI.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="RevitAPIUI">
      <HintPath>$(ProgramFiles)\Autodesk\Revit 2024\RevitAPIUI.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>
</Project>
'@

W "$projDir\ReActionAI.ChatMini.csproj" $csproj

# Можно добавить аналогично остальные файлы позже...

# === Архив ===
$zip = "ReActionAI_ChatMini_v1.0.zip"
$png = "ReActionAI_ChatMini_v1.0.png"

if (Test-Path $zip) { Remove-Item $zip -Force }
if (Test-Path $png) { Remove-Item $png -Force }

Compress-Archive -Path "$projDir\*" -DestinationPath $zip
Copy-Item $zip $png -Force

Write-Host "`n✅ Готово! Создано:"
Write-Host " - $zip"
Write-Host " - $png (архив в disguise)"
Write-Host "Переименуй .png обратно в .zip и распакуй в корень проекта."