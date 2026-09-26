import assert from 'node:assert/strict';
import {mkdir,writeFile,readFile} from 'node:fs/promises';
const base=process.env.DASHBOARD_URL??'http://localhost:8080';
async function call(path,token,method='GET',body){const response=await fetch(base+path,{method,headers:{...(token?{Authorization:`Bearer ${token}`} :{}),...(body?{'Content-Type':'application/json'}:{})},...(body?{body:JSON.stringify(body)}:{})});const text=await response.text();return {status:response.status,data:text?JSON.parse(text):null};}
async function login(userId){const result=await call('/api/v1/auth/demo-sessions',null,'POST',{userId});assert.equal(result.status,200);return result.data.data.accessToken;}
const a=await login('trader-a'),b=await login('trader-b'),admin=await login('admin'),viewer=await login('viewer');
assert.equal((await call('/api/v1/markets')).status,401);
assert.equal((await call('/api/v1/audit-events',viewer)).status,403);
assert.equal((await call('/api/v1/watchlists/trader-a/items',b)).status,403);
const items=await call('/api/v1/watchlists/me/items',a);assert.equal(items.status,200);
const note='smoke-persistence-check';
if(process.argv.includes('--verify-restart')){
  assert.ok(items.data.data.items.some(item=>item.note===note),'La fila debe sobrevivir al reinicio de API/SQL Server');
  const saved=JSON.parse(await readFile('test-results/system-event-restart.json','utf8'));
  const logs=await call('/api/v1/system-events?component=Api&pageSize=100',admin);
  assert.equal(logs.status,200);
  assert.ok(logs.data.data.items.some(item=>item.id===saved.id),'El registro técnico anterior debe sobrevivir al reinicio de SQL Server');
}else{
  const added=await call('/api/v1/watchlists/me/items',a,'POST',{coinId:'bitcoin',note});assert.equal(added.status,201);
  const audit=await call('/api/v1/audit-events?actorId=trader-a',admin);assert.equal(audit.status,200);assert.ok(audit.data.data.total>=1);
  const logs=await call('/api/v1/system-events?component=Api&pageSize=100',admin);assert.equal(logs.status,200);
  const startup=logs.data.data.items.find(item=>item.code==='application.started');assert.ok(startup);
  await mkdir('test-results',{recursive:true});await writeFile('test-results/system-event-restart.json',JSON.stringify({id:startup.id}));
}
console.log(JSON.stringify({status:'passed',mode:process.argv.includes('--verify-restart')?'restart':'fresh-install',base}));
