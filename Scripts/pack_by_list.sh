#!/usr/bin/env bash
set -euo pipefail

# ==== НАСТРОЙ ВЕРХНИЕ ПЕРЕМЕННЫЕ ====
PROJECT_ROOT="${PROJECT_ROOT:-$HOME/Projects/ReActionAI}"
OUT_DIR="${OUT_DIR:-$HOME/Temp}"
RCLONE_REMOTE="${RCLONE_REMOTE:-MyDrive:Inbox}"
ZIP_NAME="${ZIP_NAME:-ReActionAI_request.zip}"
PYTHON_BIN="${PYTHON_BIN:-python3}"
PACKER_SCRIPT="${PACKER_SCRIPT:-$(dirname "$0")/pack_by_list.py}"

get_clipboard() {
  if command -v wl-paste >/dev/null 2>&1; then
    wl-paste -n
  elif command -v xclip >/dev/null 2>&1; then
    xclip -selection clipboard -o
  else
    echo "Не удалось прочитать буфер обмена: установи wl-clipboard или xclip." >&2
    exit 1
  fi
}

FILELIST_SRC="$(get_clipboard)"
if [[ -z "$FILELIST_SRC" ]]; then
  echo "Буфер пуст. Скопируй ссылку/путь на filelist.txt и запусти снова." >&2
  exit 1
fi

mkdir -p "$OUT_DIR"
OUT_ZIP="$OUT_DIR/$ZIP_NAME"

echo "[INFO] root: $PROJECT_ROOT"
echo "[INFO] list: $FILELIST_SRC"
echo "[INFO] out : $OUT_ZIP"
echo "[INFO] rclone: $RCLONE_REMOTE"
echo

"$PYTHON_BIN" "$PACKER_SCRIPT" \
  --root "$PROJECT_ROOT" \
  --list "$FILELIST_SRC" \
  --out "$OUT_ZIP" \
  --manifest manifest.json \
  --preserve-times \
  --rclone-remote "$RCLONE_REMOTE"
