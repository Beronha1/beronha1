# -*- coding: utf-8 -*-
from __future__ import annotations

import argparse
import json
import re
import urllib.parse
import urllib.request
from collections import defaultdict, deque
from pathlib import Path

ROOT = Path('Resources') / 'Locale'
SRC_ROOT = ROOT / 'en-US'
DST_ROOT = ROOT / 'pt-BR'
ENCODINGS = ['utf-8', 'utf-8-sig', 'utf-16', 'utf-16-le', 'utf-16-be', 'cp1252', 'latin-1']

PAT_FTL = re.compile(r'^(?P<indent>\s*)(?P<key>[^\s=]+)\s*=\s*(?P<value>.*)$')
PAT_YAML = re.compile(r'^(?P<indent>\s*)(?P<key>[^:#\n]+?)\s*:\s*(?P<value>.*)$')
PAT_PLACEHOLDER = re.compile(r'\{\s*[^{}]+\s*\}')
PAT_TAG = re.compile(r'<[^>]+>|\[[^\]]+\]')
PAT_VERSION = re.compile(r'^v\d+(?:\.\d+)*(?:\.[0-9]+)?$')
PAT_WORDS = re.compile(r'[A-Za-z\u00C0-\u024F\u0400-\u04FF]')
PAT_CYR = re.compile(r'[\u0400-\u04FF]')
PAT_LOWER = re.compile(r'[a-z\u00E0-\u00F8\u00F9-\u00FF]')
PAT_TOKEN = re.compile(r'__TOK(\d+)__')

STOPWORDS = {
    ' the ', ' and ', ' to ', ' of ', ' in ', ' for ', ' is ', ' are ', ' you ', ' your ',
    ' that ', ' this ', ' with ', ' not ', ' please ', ' when ', ' from ', ' about ', 'into', ' after ',
    ' before ', ' while ', ' where ', ' there ', ' here ', ' have ', ' has ', ' had ', ' was ', ' were ',
    ' on ', ' like ', ' cannot ', ' cant ', ' do not ', ' can not ', ' how ', ' need ',
    ' open ', ' close ', ' confirm ', ' enabled ', ' disabled ', ' cancel '
}

def read_text(path: Path) -> str:
    for enc in ENCODINGS:
        try:
            return path.read_text(encoding=enc)
        except UnicodeDecodeError:
            continue
        except Exception:
            break
    return path.read_text(encoding='utf-8', errors='replace')


def normalize_value(value: str) -> str:
    return re.sub(r'\s+', ' ', value.strip())


def is_dataset_name_file(rel: Path) -> bool:
    p = rel.as_posix().lower()
    return p.startswith('datasets/names/') or '/datasets/names/' in p


def looks_translatable(raw: str, aggressive: bool = False) -> bool:
    raw = raw.strip()
    if not raw:
        return False

    cleaned = PAT_PLACEHOLDER.sub('', raw)
    cleaned = PAT_TAG.sub('', cleaned).strip()
    if not cleaned:
        return False

    if PAT_VERSION.fullmatch(cleaned):
        return False

    # Keep technical/system values and shouty acronyms untouched.
    if not PAT_WORDS.search(cleaned):
        return False

    # Cyrillic fallback should still be translated.
    if PAT_CYR.search(cleaned):
        return True

    if aggressive:
        return bool(PAT_WORDS.search(cleaned))

    # Avoid all-caps chants/onomatopoeia/acronyms.
    letters = re.findall(r'[A-Za-z]', cleaned)
    if letters and not PAT_LOWER.search(cleaned):
        return False

    lower = (' ' + cleaned.lower() + ' ')
    if any(stop in lower for stop in STOPWORDS):
        return True

    words = re.findall(r"[\w'-]+", cleaned)
    word_count = len(words)
    if word_count <= 1:
        return False
    if word_count >= 3:
        return True

    # Short 2-word mechanical fragments should stay as-is unless long.
    if word_count == 2:
        if len(cleaned) >= 24:
            return True
        # Examples: "Power Cell", "Fooga Booga"
        return False

    if len(cleaned) > 25:
        return True
    if re.search(r'[\.!\?;:]', cleaned):
        return len(cleaned) > 15

    return False


def protect_tokens(text: str):
    tokens: list[str] = []

    def repl(match: re.Match[str]) -> str:
        marker = f'__TOK{len(tokens)}__'
        tokens.append(match.group(0))
        return marker

    out = PAT_PLACEHOLDER.sub(repl, text)
    out = PAT_TAG.sub(repl, out)
    return out, tokens


def restore_tokens(text: str, tokens: list[str]) -> str:
    def repl(match: re.Match[str]) -> str:
        return tokens[int(match.group(1))]

    return PAT_TOKEN.sub(repl, text)


def translate_value(text: str) -> str:
    if not text:
        return text

    protected, tokens = protect_tokens(text)
    encoded = urllib.parse.quote(protected)
    url = f'https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=pt&dt=t&q={encoded}'

    try:
        with urllib.request.urlopen(url, timeout=10) as response:
            payload = response.read().decode('utf-8')
        data = json.loads(payload)
        translated = ''.join(part[0] for part in data[0] if part and part[0])
        return restore_tokens(translated, tokens)
    except Exception:
        return text


def parse_entries(lines: list[str], is_yaml: bool):
    pat = PAT_YAML if is_yaml else PAT_FTL
    for idx, line in enumerate(lines):
        m = pat.match(line)
        if not m:
            continue
        value = m.group('value')
        if not value.strip():
            continue
        yield idx, f"{m.group('indent')}{m.group('key')}", value


def scan_candidates(limit: int, skip_names: bool, aggressive: bool):
    out = []
    for src in SRC_ROOT.rglob('*'):
        if not src.is_file() or src.suffix.lower() not in {'.ftl', '.yml', '.yaml'}:
            continue

        rel = src.relative_to(SRC_ROOT)
        if skip_names and is_dataset_name_file(rel):
            continue
        dst = DST_ROOT / rel
        if not dst.exists():
            continue

        is_yaml = src.suffix.lower() in {'.yml', '.yaml'}
        src_lines = read_text(src).splitlines()
        dst_lines = read_text(dst).splitlines()

        src_map = defaultdict(deque)
        for _, key, src_val in parse_entries(src_lines, is_yaml):
            src_map[key].append(src_val)

        total = 0
        same = 0
        for _, key, dst_val in parse_entries(dst_lines, is_yaml):
            if not src_map.get(key):
                continue
            src_val = src_map[key].popleft()
            total += 1
            if normalize_value(src_val) == normalize_value(dst_val) and looks_translatable(src_val, aggressive):
                same += 1

        if same:
            out.append((same, total, src))

    out.sort(key=lambda x: x[0], reverse=True)
    return out[:limit]


def translate_file(src: Path, dst: Path, aggressive: bool) -> tuple[int, int]:
    is_yaml = src.suffix.lower() in {'.yml', '.yaml'}
    src_lines = read_text(src).splitlines()
    dst_lines = read_text(dst).splitlines()

    src_map = defaultdict(deque)
    for _, key, src_val in parse_entries(src_lines, is_yaml):
        src_map[key].append(src_val)

    candidates = []
    for idx, key, dst_val in parse_entries(dst_lines, is_yaml):
        if not src_map.get(key):
            continue
        src_val = src_map[key].popleft()
        if normalize_value(src_val) != normalize_value(dst_val):
            continue
        if not looks_translatable(src_val, aggressive):
            continue
        candidates.append((idx, src_val))

    changed = 0
    cache = {}
    for idx, src_val in candidates:
        if src_val in cache:
            trans = cache[src_val]
        else:
            trans = translate_value(src_val)
            cache[src_val] = trans

        if trans == src_val:
            continue

        line = dst_lines[idx]
        if is_yaml:
            m = PAT_YAML.match(line)
            if not m:
                continue
            current = m.group('value')
            quote = ''
            if current.startswith('"') and current.endswith('"'):
                quote = '"'
            elif current.startswith("'") and current.endswith("'"):
                quote = "'"
            if quote:
                trans = f'{quote}{trans}{quote}' if not (trans.startswith(quote) and trans.endswith(quote)) else trans
            dst_lines[idx] = f"{m.group('indent')}{m.group('key')}: {trans}"
        else:
            m = PAT_FTL.match(line)
            if not m:
                continue
            dst_lines[idx] = f"{m.group('indent')}{m.group('key')}= {trans}"

        changed += 1

    if changed:
        text = '\n'.join(dst_lines)
        if text and not text.endswith('\n'):
            text += '\n'
        dst.write_text(text, encoding='utf-8')

    line_changed, line_checked = 0, 0
    for i, (src_line, dst_line) in enumerate(zip(src_lines, dst_lines)):
        if src_line == dst_line:
            if not src_line.strip() or src_line.lstrip().startswith('#'):
                continue
            # skip key lines already handled in candidate path
            if PAT_FTL.match(src_line) or PAT_YAML.match(src_line):
                continue
            if not looks_translatable(src_line, aggressive):
                continue
            line_checked += 1
            trans = translate_value(src_line)
            if trans != src_line:
                dst_lines[i] = trans
                line_changed += 1

    if line_changed:
        text = '\n'.join(dst_lines)
        if text and not text.endswith('\n'):
            text += '\n'
        dst.write_text(text, encoding='utf-8')

    return changed + line_changed, len(candidates) + line_checked


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--batch', type=int, default=100)
    parser.add_argument('--skip-dataset-names', action='store_true')
    parser.add_argument('--aggressive', action='store_true')
    args = parser.parse_args()

    picks = scan_candidates(args.batch, args.skip_dataset_names, args.aggressive)
    print(f'FILES {len(picks)}')
    for same, total, path in picks:
        print(f'{same}/{total}\t{path.relative_to(SRC_ROOT)}')

    updated = 0
    checked = 0
    errors = 0
    for same, total, src in picks:
        dst = DST_ROOT / src.relative_to(SRC_ROOT)
        try:
            changed, n = translate_file(src, dst, args.aggressive)
            updated += changed
            checked += n
        except Exception as exc:
            errors += 1
            print(f'error\t{src.relative_to(SRC_ROOT)}\t{exc}')

    remaining = len(scan_candidates(1000, args.skip_dataset_names, args.aggressive))
    print(f'updated={updated}')
    print(f'checked={checked}')
    print(f'errors={errors}')
    print(f'remaining_estimate={remaining}')


if __name__ == '__main__':
    main()
