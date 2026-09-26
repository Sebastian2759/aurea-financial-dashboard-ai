"""Contexto local: Graphify oficial + SQLite FTS + checkpoints verificables."""
from __future__ import annotations
import argparse
import hashlib
import importlib.metadata
import json
import os
from pathlib import Path, PureWindowsPath
import re
import shutil
import sqlite3
import subprocess
import sys
from datetime import datetime, timezone

ROOT = Path(__file__).resolve().parents[1]
CACHE = ROOT / '.llmops/cache'
STATE = ROOT / '.llmops/state'
GRAPH = ROOT / 'docs/architecture/graphify-out/graph.json'
SUFFIXES = {'.cs', '.csproj', '.props', '.ts', '.html', '.scss', '.css', '.md', '.json', '.yaml', '.yml', '.py', '.ps1', '.sh', '.mjs', '.cjs', '.conf', '.sln'}
SKIP = {'.git', 'node_modules', 'bin', 'obj', '.angular', 'dist', 'coverage', 'test-results', '__pycache__', '.venv', 'graphify-out'}

def now():
    return datetime.now(timezone.utc).isoformat()

def atomic_json(path, data):
    path.parent.mkdir(parents=True, exist_ok=True)
    tmp = path.with_suffix(path.suffix + '.tmp')
    tmp.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding='utf-8', newline='\n')
    tmp.replace(path)

def corpus():
    result = {}
    for base, dirs, names in os.walk(ROOT):
        dirs[:] = sorted(d for d in dirs if d not in SKIP)
        for name in sorted(names):
            p = Path(base) / name
            rel = p.relative_to(ROOT).as_posix()
            if rel.startswith(('.llmops/cache/', '.llmops/state/', '.llmops/evaluations/', 'docs/architecture/')):
                continue
            if name.startswith(('.env', 'appsettings')) or name in {'package-lock.json', 'packages.lock.json'}:
                continue
            if (p.suffix not in SUFFIXES and name != 'Dockerfile') or p.stat().st_size > 250_000:
                continue
            result[rel] = hashlib.sha256(p.read_bytes()).hexdigest()
    return result

def freshness():
    manifest = CACHE / 'manifest.json'
    if not manifest.exists() or not GRAPH.exists() or not (CACHE/'context.sqlite').exists():
        return False, ['missing-index']
    old = json.loads(manifest.read_text(encoding='utf-8'))
    current = corpus()
    changed = sorted(k for k in current.keys() | old['files'].keys() if current.get(k) != old['files'].get(k))
    if old.get('graphHash') != hashlib.sha256(GRAPH.read_bytes()).hexdigest():
        changed.append('graph-hash')
    if old.get('indexHash') != hashlib.sha256((CACHE/'context.sqlite').read_bytes()).hexdigest():
        changed.append('index-hash')
    return not changed, changed

def sync():
    version = importlib.metadata.version('graphifyy')
    if version != '0.9.67':
        raise RuntimeError(f'Graphify esperado 0.9.67; encontrado {version}')
    # Graphify's hash-seed bootstrap re-execs via os.execvpe on Windows.
    # Use its dispatch entrypoint directly; the project launchers set PYTHONHASHSEED=0.
    from graphify.cli import dispatch_command
    # Rebuild the canonical graph. Document nodes added by our indexer are not AST
    # inputs; reusing the old graph would retain them as skipped extraction sources.
    for generated in (GRAPH, GRAPH.parent/'manifest.json'):
        generated.unlink(missing_ok=True)
    previous_args = sys.argv
    sys.argv = ['graphify', 'extract', str(ROOT), '--code-only', '--force', '--max-workers', '1', '--no-cluster', '--out', str(ROOT/'docs/architecture')]
    try:
        dispatch_command('extract')
    except SystemExit as exc:
        if exc.code not in (None, 0):
            raise RuntimeError('Graphify falló: '+str(exc.code)) from exc
    finally:
        sys.argv = previous_args
    # Canonical generated text must retain the same bytes after Git checkout on
    # Windows and Linux; otherwise a valid index appears corrupt after publishing.
    extracted_manifest = GRAPH.parent/'manifest.json'
    atomic_json(extracted_manifest, json.loads(extracted_manifest.read_text(encoding='utf-8')))
    index()

def normalize_extracted_sources(sources):
    """Export repository-relative paths; reject foreign roots and traversal."""
    if not isinstance(sources, list):
        raise RuntimeError('Graphify debe entregar una lista de fuentes.')
    root = ROOT.resolve()
    normalized = []
    for source in sources:
        if not isinstance(source, str) or not source:
            raise RuntimeError('Graphify contiene una fuente inválida.')
        portable = source.replace('\\', '/')
        windows_path = PureWindowsPath(portable)
        # A Windows drive must never become an ordinary relative filename on Unix.
        # Drive-relative paths (C:file) also depend on machine state and are rejected.
        if windows_path.drive and (os.name != 'nt' or not windows_path.is_absolute()):
            raise RuntimeError('Graphify contiene una fuente fuera de la raíz del repositorio.')
        path = Path(portable)
        candidate = path if path.is_absolute() else root / path
        try:
            relative = candidate.resolve().relative_to(root)
        except (ValueError, OSError):
            # Do not include the rejected absolute path in published CI diagnostics.
            raise RuntimeError('Graphify contiene una fuente fuera de la raíz del repositorio.') from None
        if relative == Path('.'):
            raise RuntimeError('Graphify debe identificar un archivo, no la raíz del repositorio.')
        normalized.append(relative.as_posix())
    return normalized

def index():
    version = importlib.metadata.version('graphifyy')
    if not GRAPH.exists():
        raise RuntimeError('Graphify no produjo graph.json')
    CACHE.mkdir(parents=True, exist_ok=True)
    files = corpus()
    extracted = json.loads((GRAPH.parent/'manifest.json').read_text(encoding='utf-8'))
    for rel in files:
        if (ROOT/rel).suffix in {'.cs','.ts','.py'} and rel not in extracted:
            raise RuntimeError('Graphify no extrajo el archivo de código '+rel)
        if rel in extracted and (ROOT/rel).suffix in {'.cs','.ts','.py','.json'}:
            if extracted[rel].get('ast_hash') != hashlib.md5((ROOT/rel).read_bytes()).hexdigest():
                raise RuntimeError('Graphify está desactualizado para '+rel)
    graph = json.loads(GRAPH.read_text(encoding='utf-8'))
    graph['extracted_sources'] = normalize_extracted_sources(graph.get('extracted_sources', []))
    nodes = graph.get('nodes', [])
    edges = graph.get('links', graph.get('edges', []))
    # Documents are explicit source nodes, not invented semantic relations.
    nodes = [n for n in nodes if not str(n.get('id','')).startswith('doc:')]
    for rel, digest in files.items():
        if rel.endswith('.md'):
            nodes.append({'id':'doc:'+rel, 'label':rel, 'type':'document', 'source_file':rel, 'source_hash':digest, 'provenance':'EXTRACTED'})
    graph['nodes'] = nodes
    atomic_json(GRAPH, graph)
    tmp = CACHE/'context.tmp.sqlite'
    if tmp.exists():
        tmp.unlink()
    conn = sqlite3.connect(tmp)
    conn.execute('CREATE VIRTUAL TABLE chunks USING fts5(path UNINDEXED, start UNINDEXED, end UNINDEXED, hash UNINDEXED, text, tokenize="unicode61 remove_diacritics 2")')
    for rel, digest in files.items():
        lines = (ROOT/rel).read_text(encoding='utf-8-sig', errors='replace').splitlines()
        for start in range(0, max(1,len(lines)), 55):
            end = min(start+70,len(lines))
            conn.execute('INSERT INTO chunks VALUES(?,?,?,?,?)',(rel,start+1,end,digest,'\n'.join(lines[start:end])))
    conn.commit()
    conn.close()
    tmp.replace(CACHE/'context.sqlite')
    atomic_json(CACHE/'manifest.json', {'generatedAt':now(),'graphifyVersion':version,'files':files,'graphHash':hashlib.sha256(GRAPH.read_bytes()).hexdigest(),'indexHash':hashlib.sha256((CACHE/'context.sqlite').read_bytes()).hexdigest(),'nodes':len(nodes),'edges':len(edges)})
    print(json.dumps({'status':'current','files':len(files),'nodes':len(nodes),'edges':len(edges)}))

def query(question, limit=8):
    current, changed = freshness()
    if not current:
        raise RuntimeError('Índice desactualizado: '+', '.join(changed[:12])+'. Ejecutar sync.')
    words = re.findall(r'[\w-]+',question.lower(),re.UNICODE)
    stop = {'que','para','como','donde','the','and','con','los','las','del','una','por','how'}
    terms = [w for w in words if len(w)>2 and w not in stop]
    synonyms={'permisos':['permission','authorize','rbac'],'autorizacion':['permission','authorize','rbac'],'seguimiento':['watchlist'],'mercado':['market'],'auditoria':['audit'],'usuario':['user'],'propietario':['owner','ownerid'],'sesion':['session'],'umbral':['threshold'],'seguridad':['security','authorize'],'actualizacion':['snapshot','signalr']}
    terms += [v for t in terms[:] for v in synonyms.get(t,[])]
    match = ' OR '.join('"'+t.replace('"','')+'"' for t in terms)
    conn = sqlite3.connect(CACHE/'context.sqlite')
    conn.row_factory=sqlite3.Row
    rows = conn.execute('SELECT path,start,end,hash,text,bm25(chunks) AS score FROM chunks WHERE chunks MATCH ? ORDER BY score LIMIT ?', (match or '"REQ"',limit)).fetchall()
    conn.close()
    graph=json.loads(GRAPH.read_text(encoding='utf-8'))
    by_id={str(n['id']):n for n in graph.get('nodes',[])}
    paths={r['path'] for r in rows}
    selected={nid for nid,n in by_id.items() if str(n.get('source_file',n.get('file',''))).replace('\\','/') in paths}
    related=[]
    for e in graph.get('links',graph.get('edges',[])):
        a,b=str(e.get('source')),str(e.get('target'))
        if a in selected or b in selected:
            other=by_id.get(b if a in selected else a,{})
            related.append({'source':a,'target':b,'relation':e.get('relation',e.get('type')),'provenance':e.get('confidence',e.get('provenance')),'relatedFile':other.get('source_file',other.get('file'))})
    sources=[]
    for row in rows:
        source=dict(row)
        source['symbols']=[{'symbol':n.get('label',n.get('name',n['id'])),'location':n.get('source_location'),'provenance':n.get('confidence',n.get('provenance','EXTRACTED'))} for n in by_id.values() if n.get('source_file')==row['path']][:30]
        sources.append(source)
    def reference(rel):
        return {'path':rel,'hash':hashlib.sha256((ROOT/rel).read_bytes()).hexdigest(),'text':(ROOT/rel).read_text(encoding='utf-8-sig')}
    result={'question':question,'retrievedAt':now(),'sources':sources,'graphRelations':related[:30],
        'requirements':reference('docs/REQUIREMENTS.md'),
        'decisions':reference('docs/decisions/ADR-001-dashboard.md'),
        'standards':[reference('.llmops/standards/'+name+'.md') for name in ('architecture','security','testing')],
        'checkpoint':json.loads((STATE/'checkpoint.json').read_text(encoding='utf-8')) if (STATE/'checkpoint.json').exists() else None}
    atomic_json(STATE/'last-context.json',result)
    print(json.dumps(result,ensure_ascii=False,indent=2))
    return result

def checkpoint(task, completed, next_action, evidence):
    current, changed=freshness()
    if not current:
        raise RuntimeError('No se puede cerrar con índice desactualizado: '+str(changed[:5]))
    previous={}
    path=STATE/'checkpoint.json'
    if path.exists():
        previous=json.loads(path.read_text(encoding='utf-8'))
    manifest=json.loads((CACHE/'manifest.json').read_text(encoding='utf-8'))
    record={'task':task,'completed':completed,'nextAction':next_action,'evidence':evidence,'recordedAt':now(),'indexManifestHash':hashlib.sha256((CACHE/'manifest.json').read_bytes()).hexdigest(),'graphifyVersion':manifest['graphifyVersion'],'graphHash':manifest['graphHash'],'indexHash':manifest['indexHash'],'sources':manifest['files'],'status':'in-progress'}
    record['history']=previous.get('history',[])+([{k:v for k,v in previous.items() if k not in ('history','sources')} ] if previous else [])
    atomic_json(path,record)
    print(json.dumps(record,ensure_ascii=False,indent=2))

def resume():
    current, changed = freshness()
    if not current:
        raise RuntimeError('Contexto desactualizado: '+', '.join(changed[:12]))
    path = STATE/'checkpoint.json'
    if not path.exists():
        raise RuntimeError('No existe checkpoint para retomar.')
    saved = json.loads(path.read_text(encoding='utf-8'))
    sources = corpus()
    changed = sorted(k for k in sources.keys() | saved.get('sources', {}).keys()
                     if sources.get(k) != saved.get('sources', {}).get(k))
    if changed:
        raise RuntimeError('Las fuentes cambiaron desde el checkpoint: '+', '.join(changed[:12]))
    manifest = json.loads((CACHE/'manifest.json').read_text(encoding='utf-8'))
    if saved.get('graphifyVersion') != manifest['graphifyVersion']:
        raise RuntimeError('La versión de Graphify difiere del checkpoint.')
    if not saved.get('nextAction') or not saved.get('evidence'):
        raise RuntimeError('Checkpoint incompleto: faltan siguiente acción o evidencia.')
    result = {k: saved[k] for k in ('task', 'status', 'completed', 'nextAction', 'evidence', 'recordedAt')}
    result.update({'current': True, 'verifiedFiles': len(sources),
                   'contextRegenerated': saved.get('graphHash') != manifest['graphHash'] or saved.get('indexHash') != manifest['indexHash']})
    print(json.dumps(result, ensure_ascii=False, indent=2))
    return result

def main():
    p=argparse.ArgumentParser()
    sub=p.add_subparsers(dest='command',required=True)
    sub.add_parser('sync')
    sub.add_parser('index')
    sub.add_parser('status')
    sub.add_parser('resume')
    q=sub.add_parser('query'); q.add_argument('question'); q.add_argument('--limit',type=int,default=8)
    cp=sub.add_parser('checkpoint'); cp.add_argument('--task',required=True); cp.add_argument('--completed',required=True); cp.add_argument('--next',required=True); cp.add_argument('--evidence',action='append',default=[])
    args=p.parse_args()
    if args.command=='sync': sync()
    elif args.command=='index': index()
    elif args.command=='query': query(args.question,args.limit)
    elif args.command=='checkpoint': checkpoint(args.task,args.completed,args.next,args.evidence)
    elif args.command=='resume': resume()
    else:
        current,changed=freshness()
        print(json.dumps({'current':current,'changed':changed,'checkpoint':json.loads((STATE/'checkpoint.json').read_text(encoding='utf-8')) if (STATE/'checkpoint.json').exists() else None},ensure_ascii=False,indent=2))
        if not current: sys.exit(2)

if __name__=='__main__':
    try: main()
    except Exception as exc:
        print(str(exc),file=sys.stderr)
        sys.exit(1)
