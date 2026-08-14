import re, json, urllib.parse, urllib.request
from pathlib import Path
from collections import defaultdict, deque

ROOT=Path('Resources')/'Locale'; SRC=ROOT/'en-US'; DST=ROOT/'pt-BR'
PAT_FTL=re.compile(r'^(?P<indent>\s*)(?P<key>[^\s=]+)\s*=\s*(?P<value>.*)$')
PAT_YAML=re.compile(r'^(?P<indent>\s*)(?P<key>[^:#\n]+?)\s*:\s*(?P<value>.*)$')
PAT_PLACEHOLDER=re.compile(r'\{\s*[^{}]+\s*\}')
PAT_TAG=re.compile(r'<[^>]+>|\[[^\]]+\]')


def read(p):
    for e in ['utf-8','utf-8-sig','utf-16','utf-16-le','utf-16-be','cp1252','latin-1']:
        try: return p.read_text(encoding=e)
        except: pass
    return p.read_text(encoding='utf-8', errors='replace')

def norm(v): return re.sub(r'\s+',' ', v.strip())

def is_plain_word(v):
    if not v.strip(): return False
    if PAT_PLACEHOLDER.search(v) or PAT_TAG.search(v): return False
    if re.match(r'^[\d\s\-._:/+%#?=&]+$', v.strip()): return False
    words=re.findall(r"[\w'-]+", v.strip())
    if len(words)!=1:
        return False
    return re.search(r'[A-Za-z]', v) is not None

def protect(t):
    toks=[]
    def repl(m):
        k=f'__TOK{len(toks)}__'; toks.append(m.group(0)); return k
    out=PAT_PLACEHOLDER.sub(repl,t)
    out=PAT_TAG.sub(repl,out)
    return out,toks

def restore(t,toks):
    return re.sub(r'__TOK(\d+)__', lambda m: toks[int(m.group(1))], t)

def trans(v):
    if not v: return v
    p,toks=protect(v)
    q=urllib.parse.quote(p)
    url=f'https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=pt&dt=t&q={q}'
    with urllib.request.urlopen(url,timeout=12) as r:
        data=json.loads(r.read().decode('utf-8'))
    return restore(''.join(x[0] for x in data[0] if x and x[0]), toks)

count=0; changed=0; shown=0
for src in SRC.rglob('*'):
    if not src.is_file() or src.suffix.lower() not in {'.ftl','.yml','.yaml'}: continue
    dst=DST/src.relative_to(SRC)
    if not dst.exists(): continue
    is_yaml=src.suffix.lower() in {'.yml','.yaml'}
    sp=PAT_YAML if is_yaml else PAT_FTL
    dp=PAT_YAML if is_yaml else PAT_FTL
    sm=defaultdict(deque)
    for l in read(src).splitlines():
        m=sp.match(l)
        if not m: continue
        v=m.group('value')
        if not v.strip(): continue
        sm[m.group('key').strip()].append(v)
    for l in read(dst).splitlines():
        m=dp.match(l)
        if not m: continue
        dv=m.group('value').strip()
        if not dv: continue
        k=m.group('key').strip()
        if not sm.get(k): continue
        sv=sm[k].popleft()
        if norm(sv)!=norm(dv): continue
        if not is_plain_word(sv): continue
        count+=1
        t=trans(sv)
        if t!=sv:
            changed+=1
            if shown<40:
                print(f'{src.relative_to(SRC)}\t{k}::{sv}=>{t}')
                shown+=1

print('count',count)
print('changed',changed)
