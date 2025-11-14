# README — Collector Scripts (v4)

##  Назначение
Папка *Collector* содержит служебные скрипты, используемые для:
Сборки ZIP-архива проекта
Резервного копирования/выгрузки архива (через внешние инструменты — MacroDroid, Google Drive и т.п.)
Создания ярлыка для быстрого запуска сборки

Collector не участвует в работе Revit-плагина, а нужен только для упаковки проекта.

---

##  Состав папки

collect-pack-upload_v4.bat — основной сценарий сборки архива проекта
create_collector_shortcut_v4.vbs — скрипт создания ярлыка для батника

---

##  collect-pack-upload_v4.bat

BAT-скрипт выполняет:

определение корня репозитория ReActionAI относительно папки Scripts/Collector
создание папки Collector_Output (если её нет)
формирование имени архива с датой/временем
упаковку всего репозитория ReActionAI в ZIP с помощью PowerShell (Compress-Archive)

*Важно:* выгрузка на Google Drive/облако выполняется внешними средствами (например, MacroDroid на телефоне), сам BAT только собирает ZIP.

---

##  create_collector_shortcut_v4.vbs

VBS-скрипт:

создаёт ярлык *ReActionAI Collector v4* на рабочем столе
привязывает ярлык к collect-pack-upload_v4.bat

---

##  Версия

Текущая версия скриптов Collector: *v4*.  
Старые версии (v2, v3) удалены как устаревшие.

---

##  Размещение

Папка Collector должна находиться по пути:

```text
ReActionAI/Scripts/Collector/