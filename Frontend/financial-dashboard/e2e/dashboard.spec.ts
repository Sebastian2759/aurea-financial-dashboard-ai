import {test,expect,Page} from '@playwright/test';
import {money} from '../src/app/shared/formatting/format';
async function role(page:Page,user:string){await page.getByLabel('SESIÓN DEMO').selectOption(user);await expect(page.getByRole('heading',{name:'El mercado, en perspectiva.'})).toBeVisible();}
test('mercado, aislamiento, CRUD administrativo, auditoría y limpieza de sesión',async({page})=>{
  await page.goto('/');await expect(page.getByRole('heading',{name:'El mercado, en perspectiva.'})).toBeVisible();
  await expect(page.getByRole('link',{name:'Auditoría',exact:true})).toHaveCount(0);
  await page.getByRole('button',{name:'EUR',exact:true}).click();await expect(page.locator('canvas')).toHaveAttribute('aria-label',/eur/);
  await page.getByRole('button',{name:'7 días',exact:true}).click();await expect(page.getByRole('button',{name:'7 días',exact:true})).toHaveAttribute('aria-pressed','true');
  await page.getByLabel('Capitalización',{exact:true}).uncheck();await expect(page.getByRole('columnheader',{name:'Capitalización',exact:true})).toHaveCount(0);
  await role(page,'trader-a');await page.getByRole('link',{name:'Seguimiento',exact:true}).click();
  await page.getByLabel('Activo',{exact:true}).selectOption('ethereum');await page.getByLabel('Nota (opcional)').fill('E2E: observar el volumen');await page.getByRole('button',{name:'Agregar a mi lista',exact:true}).click();
  await expect(page.getByRole('cell',{name:'E2E: observar el volumen',exact:true})).toBeVisible();
  await role(page,'trader-b');await page.getByRole('link',{name:'Seguimiento',exact:true}).click();await expect(page.getByText('Tu lista está lista para empezar.')).toBeVisible();
  await role(page,'admin');await page.getByRole('link',{name:'Seguimiento',exact:true}).click();await page.getByLabel('Lista de seguimiento de').selectOption('trader-a');
  await page.getByRole('row').filter({hasText:'E2E: observar el volumen'}).getByRole('button',{name:'Editar',exact:true}).click();
  await expect(page.getByRole('heading',{name:'Editar seguimiento',exact:true})).toBeVisible();
  await expect(page.getByLabel('Nota (opcional)')).toHaveValue('E2E: observar el volumen');
  await page.getByLabel('Nota (opcional)').fill('E2E: editado por Admin');
  await page.getByRole('button',{name:'Guardar cambios',exact:true}).click();
  await expect(page.getByRole('cell',{name:'E2E: editado por Admin',exact:true})).toBeVisible();
  await page.getByRole('row').filter({hasText:'E2E: editado por Admin'}).getByRole('button',{name:'Eliminar',exact:true}).click();await page.getByRole('button',{name:'Eliminar activo',exact:true}).click();await expect(page.getByText('Activo eliminado del seguimiento.')).toBeVisible();
  await page.getByRole('link',{name:'Auditoría',exact:true}).click();await page.getByRole('combobox',{name:'Actor',exact:true}).selectOption('admin');await page.getByRole('button',{name:'Aplicar filtros'}).click();
  await expect(page.getByRole('cell',{name:'Activo eliminado',exact:true})).toBeVisible();
  await role(page,'viewer');await expect(page.getByRole('link',{name:'Auditoría',exact:true})).toHaveCount(0);await expect(page.getByText('E2E: editado por Admin',{exact:true})).toHaveCount(0);
});
test('el umbral llega a otra sesión por SignalR',async({page,browser})=>{
  const context=await browser.newContext({baseURL:process.env['DASHBOARD_URL']??'http://localhost:8080'});const viewer=await context.newPage();await viewer.goto('/');await expect(viewer.getByText('En vivo',{exact:true})).toBeVisible();
  await page.goto('/');await role(page,'admin');await page.getByRole('link',{name:'Umbrales',exact:true}).click();await page.getByLabel('Volatilidad máxima (%)').fill('1.25');await page.getByRole('button',{name:'Guardar umbral'}).click();
  await expect(page.getByText('Umbral guardado y enviado a todos los usuarios conectados.')).toBeVisible();await expect(viewer.getByText('1.25 %',{exact:true})).toBeVisible();
  await page.getByLabel('Volatilidad máxima (%)').fill('5');await page.getByRole('button',{name:'Guardar umbral'}).click();await expect(viewer.getByText('5 %',{exact:true})).toBeVisible();await context.close();
});
test('la vista móvil no desborda el documento',async({page})=>{
  await page.setViewportSize({width:390,height:844});await page.goto('/');await expect(page.locator('canvas')).toBeVisible();
  expect(await page.evaluate(()=>document.documentElement.scrollWidth<=window.innerWidth)).toBe(true);
});

test('solo Admin consulta y filtra registros técnicos persistidos',async({page})=>{
  await page.goto('/');
  await expect(page.getByRole('heading',{name:'El mercado, en perspectiva.'})).toBeVisible();
  await expect(page.getByRole('link',{name:'Registros del sistema',exact:true})).toHaveCount(0);
  await role(page,'admin');
  await page.getByRole('link',{name:'Registros del sistema',exact:true}).click();
  await expect(page.getByRole('heading',{name:'Estado y eventos del sistema.'})).toBeVisible();
  await page.getByLabel('Nivel',{exact:true}).selectOption('Information');
  await page.getByLabel('Origen',{exact:true}).selectOption('Api');
  await page.getByRole('button',{name:'Aplicar filtros'}).click();
  await expect(page.getByRole('cell',{name:'application.started',exact:true}).first()).toBeVisible();
  await page.screenshot({path:'test-results/system-logs-admin.png',fullPage:true});
  await role(page,'viewer');
  await expect(page.getByRole('link',{name:'Registros del sistema',exact:true})).toHaveCount(0);
  await page.goto('/administration/system-logs');
  await expect(page.getByRole('heading',{name:'El mercado, en perspectiva.'})).toBeVisible();
  await expect(page.getByRole('cell',{name:'application.started',exact:true})).toHaveCount(0);
});

test('las cotizaciones reales y su histórico se representan en pantalla',async({page})=>{
  test.skip(process.env['REQUIRE_LIVE_DATA']!=='true','Comprobación externa activada expresamente en CI.');
  const response=page.waitForResponse(r=>r.url().includes('/api/v1/markets?currency=usd')&&r.request().method()==='GET');
  await page.goto('/');
  const marketResponse=await response;
  expect(marketResponse.status(),'CoinGecko debe estar disponible; no se acepta simulación ni una respuesta 503.').toBe(200);
  const snapshot=(await marketResponse.json()).data.snapshot;
  expect(snapshot.source).toBe('coingecko');
  for(const quote of snapshot.quotes){
    expect(Number.isFinite(Date.parse(quote.updatedAt))).toBe(true);
    await expect(page.locator('.quote-card').filter({hasText:quote.name}).locator('.quote-price')).toHaveText(money(quote.price,'usd'));
  }
  await expect(page.locator('.chart-caption')).toContainText('Histórico de CoinGecko');
  await expect(page.locator('canvas')).toBeVisible();
  await page.screenshot({path:'test-results/market-live-coingecko.png',fullPage:true});
});
