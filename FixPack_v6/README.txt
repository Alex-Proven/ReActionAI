FixPack_v5_v2 for ReActionAI — Revit 2024
Поместите папку FixPack_v5_v2 в КОРЕНЬ репозитория (рядом с ReActionAI.sln)

Запуск:
  powershell -ExecutionPolicy Bypass -File .\FixPack_v5_v2\scripts\deploy.ps1

Действия:
  1) Чистит bin/obj
  2) Собирает Debug|x64
  3) Удаляет дубликаты .addin/.dll из корня Addins\2024
  4) Копирует DLL в Addins\2024\ReActionAI\
  5) Генерирует единый .addin (Application) в той же папке
  6) Чистит AddInCache
