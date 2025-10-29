# ReActionAI.Modules.RevitChatGPT (Revit 2024) — интеграционный модуль

Структура для встраивания ChatGPT‑клиента в проект **ReActionAI** как модуль `src/Modules/RevitChatGPT`.

## Состав
- Проект модуля: `ReActionAI.Modules.RevitChatGPT.csproj` (net48, WPF, Revit 2024)
- Проект абстракций: `ReActionAI.Abstractions.csproj` (netstandard2.0) — `IReActionModule`, `IKnowledgeBase`, `IChatService`
- Add-in: `ReActionAI.Modules.RevitChatGPT.addin` (копирование в `%AppData%\Autodesk\Revit\Addins\2024`)
- Конфиг: `Config/appsettings.json` (API ключ и модель)

## Интеграция с Core/KB
- Модуль реализует `IReActionModule` (через проект Abstractions).
- `ChatClient` также реализует `IChatService`, чтобы ядро могло использовать его напрямую.
- При появлении настоящего `ReActionAI.Core` — замените ссылку на локальные абстракции на ссылку на Core/Abstractions вашего решения.

## Сборка и запуск
1. Добавьте оба проекта в ваше решение `ReActionAI.sln` и назначьте зависимость модуля от Abstractions.
2. Укажите `ApiKey` в `Config/appsettings.json`.
3. Соберите. DLL и `.addin` автоматически скопируются в `%AppData%\Autodesk\Revit\Addins\2024`.
4. Запустите Revit 2024 → вкладка **ReActionAI** → **AI Tools** → **ChatGPT**.

## Форматы файлов
- Код и конфиги: **UTF-8 (без BOM)**, переводы строк **CRLF**.
