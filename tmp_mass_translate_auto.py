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


def looks_translatable(raw: str, aggressive: bool = True) -> bool:
    raw = raw.strip()
    if not raw:
        return False

    cleaned = PAT_PLACEHOLDER.sub('', raw)
    cleaned = PAT_TAG.sub('', cleaned).strip()
    if not cleaned:
        return False

    if PAT_VERSION.fullmatch(cleaned):
        return False

    if not PAT_WORDS.search(cleaned):
        return False

    if aggressive:
        # Keep old guard rails for short acronyms/symbols.
        if cleaned.isupper() and ' ' not in cleaned and len(cleaned) <= 4:
            return False
        return True

    words = re.findall(r"[\w'-]+", cleaned)
    if len(words) <= 1:
        return False
    if len(words) == 2:
        if len(cleaned) >= 24:
            return True
        return False

    if len(cleaned) > 25:
        return True
    if re.search(r'[\.\!\?;:]', cleaned):
        return len(cleaned) > 15

    return False


def protect_tokens(text: str):
    tokens = []

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
    return re.sub(r'__TOK(\d+)__', repl, text)


def translate_value(text: str) -> str:
    if not text:
        return text

    protected, tokens = protect_tokens(text)
    encoded = urllib.parse.quote(protected)
    # auto detect source language to catch English/Russian and similar mixed content.
    url = f'https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl=pt&dt=t&q={encoded}'

    try:
        with urllib.request.urlopen(url, timeout=12) as response:
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


updated = 0
checked = 0
errors = 0
files_touched = 0

for src in SRC_ROOT.rglob('*'):
    if not src.is_file() or src.suffix.lower() not in {'.ftl', '.yml', '.yaml'}:
        continue

    rel = src.relative_to(SRC_ROOT)
    if is_dataset_name_file(rel):
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

    changed = 0
    for idx, key, dst_val in parse_entries(dst_lines, is_yaml):
        if not src_map.get(key):
            continue

        src_val = src_map[key].popleft()
        if normalize_value(src_val) != normalize_value(dst_val):
            continue

        if not looks_translatable(src_val, aggressive=True):
            continue

        checked += 1
        trans = translate_value(src_val)
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
        updated += 1

    if changed:
        text = '\n'.join(dst_lines)
        if text and not text.endswith('\n'):
            text += '\n'
        dst.write_text(text, encoding='utf-8')
        files_touched += 1

print(f'checked={checked}')
print(f'updated={updated}')
print(f'files_touched={files_touched}')
print(f'errors={errors}')
