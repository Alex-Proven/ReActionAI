param(
    [string]$RootPath
)

Write-Host "Удаление папок bin и obj..."  # <-- русское, PowerShell поддерживает
Write-Host "Корень:" 
Write-Host "  $RootPath"
Write-Host ""

Get-ChildItem -Path $RootPath -Directory -Recurse -Include "bin","obj" -ErrorAction SilentlyContinue |
    ForEach-Object {
        Write-Host "Удаляю: $($_.FullName)"
        Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
    }

Write-Host ""
Write-Host "Удаление bin/ и obj/ завершено."