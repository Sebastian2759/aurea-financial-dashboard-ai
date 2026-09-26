"""Evaluaciones reproducibles de recuperación, vigencia y checkpoint."""
import contextlib
import io
import json
from pathlib import Path
import shutil
import subprocess
import sys
import uuid
import unittest
from unittest.mock import patch
import context as ctx

@contextlib.contextmanager
def isolated_context_directory():
    # Inherit workspace access on Windows, including the child recovery process.
    directory = ctx.CACHE / ('test-' + uuid.uuid4().hex)
    directory.mkdir()
    try:
        yield directory
    finally:
        resolved = directory.resolve()
        if resolved.parent != ctx.CACHE.resolve() or not resolved.name.startswith('test-'):
            raise RuntimeError('Directorio de prueba fuera de cache.')
        shutil.rmtree(resolved)

class RetrievalTests(unittest.TestCase):
    def test_extracted_sources_export_portable_repository_paths(self):
        sources = [str(ctx.ROOT/'tools/context.py'), 'tools\\test_context.py']
        self.assertEqual(['tools/context.py', 'tools/test_context.py'],
                         ctx.normalize_extracted_sources(sources))

    def test_extracted_sources_reject_paths_outside_repository(self):
        sources = [str(ctx.ROOT.parent/'private.py'), '../private.py', 'Z:private.py']
        for source in sources:
            with self.subTest(source_kind='absolute' if Path(source).is_absolute() else 'relative'):
                with self.assertRaisesRegex(RuntimeError, 'fuera de la raíz') as failure:
                    ctx.normalize_extracted_sources([source])
                self.assertNotIn(source, str(failure.exception))

    def test_atomic_json_has_canonical_utf8_lf_bytes(self):
        with isolated_context_directory() as directory:
            path = directory/'canonical.json'
            ctx.atomic_json(path, {'message': 'Recuperación', 'current': True})
            expected = json.dumps({'message': 'Recuperación', 'current': True}, ensure_ascii=False, indent=2).encode('utf-8')
            self.assertEqual(expected, path.read_bytes())
            self.assertNotIn(b'\r\n', path.read_bytes())

    def test_expected_sources_and_provenance(self):
        cases=[('AccessRules RequireOwner WatchlistWrite','AccessRules.cs'),('CoinGecko GetSnapshotAsync MarketSnapshotUpdated','CoinGeckoMarketDataProvider.cs'),('WatchlistItem OwnerId repository','Watchlist')]
        for question,expected in cases:
            with contextlib.redirect_stdout(io.StringIO()): result=ctx.query(question,12)
            self.assertTrue(any(expected in r['path'] for r in result['sources']),expected)
            self.assertTrue(all(r['hash'] and r['start']>0 and 'symbols' in r for r in result['sources']))
            self.assertTrue(result['requirements']['hash'])
            self.assertTrue(result['graphRelations'])

    def test_changed_code_and_tampered_graph_are_rejected(self):
        current=ctx.corpus(); key=next(k for k in current if k.endswith('.cs'))
        changed={**current,key:'different-content'}
        with patch.object(ctx,'corpus',return_value=changed):
            self.assertFalse(ctx.freshness()[0])
            with self.assertRaises(RuntimeError): ctx.query('watchlist')
        corrupt=ctx.CACHE/('test-'+uuid.uuid4().hex+'.json')
        try:
            corrupt.write_text('{}')
            with patch.object(ctx,'GRAPH',corrupt): self.assertIn('graph-hash',ctx.freshness()[1])
        finally:
            corrupt.unlink(missing_ok=True)

    def test_checkpoint_has_verifiable_resume_information(self):
        checkpoint=json.loads((ctx.STATE/'checkpoint.json').read_text(encoding='utf-8'))
        self.assertTrue(checkpoint['nextAction']);self.assertTrue(checkpoint['evidence']);self.assertTrue(checkpoint['indexManifestHash'])
        self.assertTrue(ctx.freshness()[0])

    def test_corrupted_sqlite_index_is_rejected_before_retrieval(self):
        with isolated_context_directory() as directory:
            cache = Path(directory)
            shutil.copyfile(ctx.CACHE/'manifest.json', cache/'manifest.json')
            shutil.copyfile(ctx.CACHE/'context.sqlite', cache/'context.sqlite')
            with (cache/'context.sqlite').open('ab') as stream:
                stream.write(b'controlled-index-corruption')
            with patch.object(ctx, 'CACHE', cache):
                self.assertIn('index-hash', ctx.freshness()[1])
                with self.assertRaisesRegex(RuntimeError, 'index-hash'):
                    ctx.query('watchlist')

    def test_checkpoint_survives_abrupt_process_exit_and_resumes_in_new_process(self):
        with isolated_context_directory() as directory:
            state = Path(directory)/'state'
            prefix = (
                'import sys,os,io,contextlib; from pathlib import Path; '
                f'sys.path.insert(0,{str(ctx.ROOT / "tools")!r}); import context as c; '
                f'c.STATE=Path({str(state)!r}); '
            )
            first = subprocess.run([sys.executable, '-X', 'utf8', '-c', prefix +
                "c.checkpoint('interrupted-task','Paso durable','Implementar siguiente paso',['Prueba controlada de recuperación']); os._exit(23)"],
                capture_output=True, text=True, encoding='utf-8', timeout=30)
            self.assertEqual(23, first.returncode, first.stderr)
            self.assertTrue((state/'checkpoint.json').exists())
            second = subprocess.run([sys.executable, '-X', 'utf8', '-c', prefix + 'c.resume()'],
                capture_output=True, text=True, encoding='utf-8', timeout=30)
            self.assertEqual(0, second.returncode, second.stderr)
            resumed = json.loads(second.stdout)
            self.assertEqual('interrupted-task', resumed['task'])
            self.assertEqual('Implementar siguiente paso', resumed['nextAction'])
            self.assertTrue(resumed['current'])
            self.assertEqual(len(ctx.corpus()), resumed['verifiedFiles'])
            self.assertEqual(['Prueba controlada de recuperación'], resumed['evidence'])

    def test_resume_rejects_checkpoint_with_changed_source_hash(self):
        with isolated_context_directory() as directory:
            with patch.object(ctx, 'STATE', Path(directory)):
                with contextlib.redirect_stdout(io.StringIO()):
                    ctx.checkpoint('task', 'done', 'next', ['controlled-test'])
                path = ctx.STATE/'checkpoint.json'
                saved = json.loads(path.read_text(encoding='utf-8'))
                saved['sources'][next(iter(saved['sources']))] = 'outdated-content'
                ctx.atomic_json(path, saved)
                with self.assertRaisesRegex(RuntimeError, 'fuentes cambiaron'):
                    ctx.resume()

if __name__=='__main__': unittest.main()
