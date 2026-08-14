import re, json, urllib.parse, urllib.request
from pathlib import Path
from collections import defaultdict, deque

ROOT = Path('Resources') / 'Locale'
SRC_ROOT = ROOT / 'en-US'
DST_ROOT = ROOT / 'pt-BR'
ENCODINGS = ['utf-8','utf-8-sig','utf-16','utf-16-le','utf-16-be','cp1252','latin-1']

PAT_FTL = re.compile(r'^(?P<indent>\s*)(?P<key>[^\s=]+)\s*=\s*(?P<value>.*)$')
PAT_YAML = re.compile(r'^(?P<indent>\s*)(?P<key>[^:#\n]+?)\s*:\s*(?P<value>.*)$')
PAT_PLACEHOLDER = re.compile(r'\{\s*[^{}]+\s*\}')
PAT_TAG = re.compile(r'<[^>]+>|\[[^\]]+\]')
PAT_VERSION = re.compile(r'^v\d+(?:\.\d+)*(?:\.[0-9]+)?$')


def read_text(path):
    for e in ENCODINGS:
        try:
            return path.read_text(encoding=e)
        except Exception:
            pass
    return path.read_text(encoding='utf-8', errors='replace')


def normalize(v):
    return re.sub(r'\s+',' ', v.strip())


def translate_value(text):
    if not text:
        return text
    cleaned = PAT_PLACEHOLDER.sub('@@', text)
    cleaned = PAT_TAG.sub('@@', cleaned)
    encoded = urllib.parse.quote(cleaned)
    url = f'https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=pt&dt=t&q={encoded}'
    try:
        with urllib.request.urlopen(url, timeout=10) as response:
            data = json.loads(response.read().decode('utf-8'))
        return ''.join(part[0] for part in data[0] if part and part[0])
    except Exception:
        return text


def parse_entries(lines,is_yaml):
    pat = PAT_YAML if is_yaml else PAT_FTL
    for idx, line in enumerate(lines):
        m = pat.match(line)
        if not m:
            continue
        value = m.group('value')
        if not value.strip():
            continue
        yield idx, m, m.group('key'), value

updated=0
checked=0
files_touched=0
for src in SRC_ROOT.rglob('*'):
    if not src.is_file() or src.suffix.lower() not in {'.ftl','.yml','.yaml'}:
        continue

    rel = src.relative_to(SRC_ROOT)
    rels = rel.as_posix().lower()
    if rels.startswith('datasets/names/'):
        continue

    dst = DST_ROOT / rel
    if not dst.exists():
        continue

    is_yaml = src.suffix.lower() in {'.yml','.yaml'}
    src_lines = read_text(src).splitlines()
    dst_lines = read_text(dst).splitlines()

    smap = defaultdict(deque)
    for _,_, key, v in parse_entries(src_lines, is_yaml):
        smap[key].append(v)

    touched=False
    for idx, m, key, dst_val in parse_entries(dst_lines, is_yaml):
        if not smap.get(key):
            continue
        src_val = smap[key].popleft()
        if normalize(src_val) != normalize(dst_val):
            continue
        if PAT_VERSION.fullmatch(src_val.strip()):
            continue

        clean = PAT_PLACEHOLDER.sub('', src_val)
        clean = PAT_TAG.sub('', clean).strip()
        if not clean:
            continue
        if ' ' in clean:
            continue
        if len(clean) < 3:
            continue
        if clean.isupper():
            continue
        # avoid common title-case names
        if clean[0].isupper() and clean[1:].islower() and len(clean) > 3:
            continue

        checked += 1
        tr = translate_value(src_val)
        if tr == src_val:
            continue

        if is_yaml:
            curr = m.group('value')
            quote = ''
            if curr.startswith('"') and curr.endswith('"'):
                quote = '"'
            elif curr.startswith("'") and curr.endswith("'"):
                quote = "'"
            if quote:
                tr = f'{quote}{tr}{quote}' if not (tr.startswith(quote) and tr.endswith(quote)) else tr
            dst_lines[idx] = f"{m.group('indent')}{m.group('key')}: {tr}"
        else:
            dst_lines[idx] = f"{m.group('indent')}{m.group('key')}= {tr}"

        updated += 1
        touched = True

    if touched:
        files_touched += 1
        text = '\n'.join(dst_lines)
        if text and not text.endswith('\n'):
            text += '\n'
        dst.write_text(text, encoding='utf-8')

print(f'checked={checked}')
print(f'updated={updated}')
print(f'files_touched={files_touched}')
