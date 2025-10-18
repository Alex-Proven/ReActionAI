# Packer v6 (ReActionAI)

Этот пакет устанавливается распаковкой **прямо в корень проекта**:
```
ReActionAI/
└─ Scripts/
   └─ Packer_v6/
      ├─ Run_Packer_v6.bat
      ├─ pack_by_list_v6.ps1
      └─ ReActionAI_request.txt
```

## Как использовать
1. Отредактируйте `Scripts/Packer_v6/ReActionAI_request.txt` (пути ARCHIVE и UPLOAD_TO).
2. Запустите `Scripts/Packer_v6/Run_Packer_v6.bat`.
3. Архив `ReActionAI_latest.zip` появится в `UPLOAD_TO`. Старый `latest` будет удалён.

## Примечания
- Скрипт исключает `.git`, `.vs`, `bin`, `obj`, `archives`, `logs`, `*.user`, `*.suo`, `*.zip`.
- `pack_by_list_v6.ps1` сохранён в UTF-8 (без BOM).
