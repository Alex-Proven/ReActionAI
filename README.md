# ReActionAI

> Когнитивная надстройка для Revit: CTO/HIC/AOC + LLM + KB.
> Этот репозиторий — «новое ядро» AI. ReAction используется как донор модулей.

## Цели
- Модульная архитектура: Core / Modules / LLM / KB
- Совместимость: .NET Framework 4.8 (Revit) и .NET 6.0+ (сервисы)
- Векторные схемы (SVG), тёмный стиль, акцент #00AEEF

## Проекты
- `src/ReActionAI` — основная библиотека (multi-target: net48; net6.0)
- `tests/ReActionAI.Tests` — модульные тесты (net6.0)

## Быстрый старт
```bash
git init
git add .
git commit -m "chore: bootstrap ReActionAI skeleton"
git branch -M master
git remote add origin https://github.com/Alex-Proven/ReActionAI.git
git push -u origin master
```

## Лицензия
MIT (замените при необходимости).
