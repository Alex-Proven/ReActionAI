# ReActionAI — Packer v5.1-latest

Готово к распаковке в корень проекта `ReActionAI/`:

```
Scripts/Packer_v5_1_latest/
├─ Run_Packer_v5_1_latest.bat
├─ pack_by_list_v5_1_latest.ps1
└─ ReActionAI_request.txt
```

## Использование
1. Отредактируйте `ReActionAI_request.txt` (пути `ARCHIVE`, `UPLOAD_TO`).
2. Запустите `Run_Packer_v5_1_latest.bat`.
3. Получите `ReActionAI_latest.zip` в `UPLOAD_TO`.

## Чистка датированных архивов
- По умолчанию — выключена.
- Чтобы включить, раскомментируйте в `ReActionAI_request.txt` строки:
  ```
  PURGE_OLD: TRUE
  KEEP_N: 0
  ```
  `KEEP_N` — сколько датированных архивов хранить (0 = не хранить ни одного).

## Исключения из архива
`.git`, `.vs`, `bin`, `obj`, `archives`, `logs`, `*.user`, `*.suo`, `*.zip`.
