#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
pack_by_list.py — сбор архива строго по списку (и шаблонам), опциональная загрузка в Google Drive (rclone)
"""
import argparse, os, sys, zipfile, pathlib, hashlib, json, fnmatch, glob, urllib.request, shutil, subprocess
from datetime import datetime
from typing import List, Tuple, Set

def read_text(src: str) -> str:
    if src.startswith(("http://", "https://")):
        with urllib.request.urlopen(src) as resp:
            return resp.read().decode("utf-8", errors="replace")
    with open(src, "r", encoding="utf-8") as f:
        return f.read()

def parse_filelist(text: str) -> Tuple[List[str], List[str]]:
    """
    Возвращает (includes, excludes).
    Строки:
      - комментарии начинаются с '#'
      - пустые строки игнорируются
      - строки, начинающиеся с '!' — исключения (глоб-паттерн)
      - остальные — включения (относительные пути ИЛИ глоб-паттерны)
    """
    includes, excludes = [], []
    for raw in text.splitlines():
        line = raw.strip()
        if not line or line.startswith("#"):
            continue
        if line.startswith("!"):
            excludes.append(line[1:].strip())
        else:
            includes.append(line)
    return includes, excludes

def expand_patterns(root: pathlib.Path, patterns: List[str]) -> Set[pathlib.Path]:
    """
    Разворачивает шаблоны относительно root. Поддержка **, *, ?.
    """
    results: Set[pathlib.Path] = set()
    for pat in patterns:
        posix = pat.replace("\\", "/").lstrip("/")
        # Если это выглядит как конкретный файл, добавим напрямую, иначе — через glob
        maybe = root / posix
        if any(ch in posix for ch in ["*", "?", "["]):
            for p in root.glob(posix):
                if p.is_file():
                    results.add(p.resolve())
        else:
            if maybe.is_file():
                results.add(maybe.resolve())
    return results

def filter_excludes(root: pathlib.Path, files: Set[pathlib.Path], excludes: List[str]) -> Set[pathlib.Path]:
    if not excludes:
        return files
    out: Set[pathlib.Path] = set()
    for f in files:
        rel = f.relative_to(root).as_posix()
        skip = any(fnmatch.fnmatch(rel, ex) for ex in excludes)
        if not skip:
            out.add(f)
    return out

def sha256_of_file(path: pathlib.Path) -> str:
    h = hashlib.sha256()
    with open(path, "rb") as r:
        for chunk in iter(lambda: r.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()

def build_zip(root: pathlib.Path, files: List[pathlib.Path], out_zip: pathlib.Path, preserve_times: bool) -> None:
    with zipfile.ZipFile(out_zip, "w", compression=zipfile.ZIP_DEFLATED) as z:
        for src in files:
            arc = src.relative_to(root).as_posix()
            zi = zipfile.ZipInfo(arc)
            if preserve_times:
                st = src.stat()
                dt = datetime.fromtimestamp(st.st_mtime)
                zi.date_time = (dt.year, dt.month, dt.day, dt.hour, dt.minute, dt.second)
            else:
                # иначе zipfile сам подставит текущее время
                pass
            with open(src, "rb") as r:
                data = r.read()
            zi.compress_type = zipfile.ZIP_DEFLATED
            z.writestr(zi, data)

def upload_with_rclone(local_zip: pathlib.Path, remote_dir: str) -> bool:
    if shutil.which("rclone") is None:
        print("[WARN] rclone не найден в PATH.")
        return False
    target = f"{remote_dir.rstrip('/')}/{local_zip.name}"
    try:
        subprocess.check_call(["rclone", "copyto", str(local_zip), target])
        print(f"[OK] Загружено: {target}")
        return True
    except subprocess.CalledProcessError as e:
        print(f"[WARN] Не удалось загрузить через rclone: {e}")
        return False

def main():
    ap = argparse.ArgumentParser(description="Pack zip strictly by filelist (with globs), optional rclone upload.")
    ap.add_argument("--root", required=True, help="Корень проекта")
    ap.add_argument("--list", required=True, help="URL или путь к filelist.txt")
    ap.add_argument("--out", required=True, help="Путь к результирующему ZIP")
    ap.add_argument("--rclone-remote", help="Напр. 'MyDrive:Inbox' — загрузить в эту папку на Google Drive")
    ap.add_argument("--manifest", default="manifest.json", help="Имя манифеста внутри архива ('' чтобы не писать)")
    ap.add_argument("--preserve-times", action="store_true", help="Сохранять времена модификации файлов в zip")
    ap.add_argument("--dry-run", action="store_true", help="Только показать, что попадёт в архив")
    ap.add_argument("--fail-on-missing", action="store_true", help="Падать, если какие-то include не найдены")
    args = ap.parse_args()

    root = pathlib.Path(args.root).resolve()
    out_zip = pathlib.Path(args.out).resolve()
    out_zip.parent.mkdir(parents=True, exist_ok=True)

    text = read_text(args.list)
    includes, excludes = parse_filelist(text)

    # Разворачиваем include и оцениваем пропуски
    found_set = expand_patterns(root, includes)
    expanded_count = len(found_set)
    # Определяем, какие «конкретные» include не нашлись
    missing = []
    for pat in includes:
        posix = pat.replace("\\", "/").lstrip("/")
        if any(ch in posix for ch in ["*", "?", "["]):
            continue
        candidate = (root / posix)
        if not candidate.is_file():
            missing.append(pat)

    # Исключения
    final_set = filter_excludes(root, found_set, excludes)
    files = sorted(final_set)

    print("=== PACK BY LIST ===")
    print(f"[INFO] root: {root}")
    print(f"[INFO] out : {out_zip}")
    print(f"[INFO] includes ({len(includes)}):")
    for i in includes[:10]:
        print(f"  + {i}")
    if len(includes) > 10:
        print(f"  ... (+{len(includes)-10} ещё)")
    if excludes:
        print(f"[INFO] excludes ({len(excludes)}):")
        for e in excludes[:10]:
            print(f"  - {e}")
        if len(excludes) > 10:
            print(f"  ... (+{len(excludes)-10} ещё)")
    print(f"[INFO] найдено файлов до исключений: {expanded_count}")
    print(f"[INFO] к упаковке после исключений: {len(files)}")

    if missing:
        print("[WARN] Отсутствуют конкретно указанные файлы:")
        for m in missing:
            print(f"  ! {m}")
        if args.fail_on_missing:
            sys.exit(2)

    if args.dry_run:
        for f in files:
            print(f" - {f.relative_to(root).as_posix()}")
        print("[INFO] dry-run завершён.")
        return

    # Манифест
    manifest = {
        "created_utc": datetime.utcnow().isoformat() + "Z",
        "root": str(root),
        "out": str(out_zip),
        "count": len(files),
        "files": []
    }
    for f in files:
        rel = f.relative_to(root).as_posix()
        manifest["files"].append({
            "path": rel,
            "size": f.stat().st_size,
            "sha256": sha256_of_file(f)
        })

    # Пакуем
    build_zip(root, files, out_zip, preserve_times=args.preserve_times)
    print(f"[OK] Собран архив: {out_zip} ({os.path.getsize(out_zip)} байт)")

    # Вшиваем manifest.json при необходимости
    if args.manifest:
        with zipfile.ZipFile(out_zip, "a", compression=zipfile.ZIP_DEFLATED) as z:
            z.writestr(args.manifest, json.dumps(manifest, ensure_ascii=False, indent=2))

    # Загрузка
    if args.rclone_remote:
        print(f"[INFO] Загружаю через rclone → {args.rclone_remote}")
        upload_with_rclone(out_zip, args.rclone_remote)

if __name__ == "__main__":
    main()
