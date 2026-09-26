import assert from 'node:assert/strict';
import {mkdir,writeFile} from 'node:fs/promises';
const base=process.env.DASHBOARD_URL??'http://localhost:8080';
const sessionResponse=await fetch(base+'/api/v1/auth/demo-sessions',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({userId:'viewer'})});
assert.equal(sessionResponse.status,200);
const session=(await sessionResponse.json()).data;
const headers={Authorization:'Bearer '+session.accessToken};
const quoteResponse=await fetch(base+'/api/v1/markets?currency=usd',{headers,signal:AbortSignal.timeout(120000)});
assert.equal(quoteResponse.status,200);
const snapshot=(await quoteResponse.json()).data.snapshot;
assert.equal(snapshot.source,'coingecko','Esta comprobación exige datos reales, no acepta el respaldo demo.');
assert.deepEqual(snapshot.quotes.map(q=>q.coinId).sort(),['bitcoin','ethereum','solana']);
for(const quote of snapshot.quotes){assert.ok(quote.price>0);assert.ok(Number.isFinite(Date.parse(quote.updatedAt)));}
const historyResponse=await fetch(base+'/api/v1/markets/bitcoin/history?currency=usd&days=1',{headers,signal:AbortSignal.timeout(120000)});
assert.equal(historyResponse.status,200);
const history=(await historyResponse.json()).data.history;
assert.equal(history.source,'coingecko','El histórico debe proceder del proveedor real.');
assert.ok(history.points.length>=24);
assert.ok(history.points.every(point=>point.price>0&&Number.isFinite(Date.parse(point.time))));
await mkdir('test-results',{recursive:true});
const evidence={checkedAtUtc:new Date().toISOString(),base,source:snapshot.source,isStale:snapshot.isStale,
  fetchedAt:snapshot.fetchedAt,quotes:snapshot.quotes,history:{source:history.source,points:history.points.length,first:history.points[0],last:history.points.at(-1)}};
await writeFile('test-results/coingecko-live.json',JSON.stringify(evidence,null,2));
console.log(JSON.stringify({status:'passed',check:'real-coingecko-through-dashboard',assets:snapshot.quotes.length,historyPoints:history.points.length}));
