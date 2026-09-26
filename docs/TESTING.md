# Pruebas y reproducción de resultados

Los comandos parten de la **raíz del repositorio**, salvo cuando un bloque cambia explícitamente a `Frontend/financial-dashboard`. Instala SDK .NET 10 y Node 24.21.0 para las pruebas de aplicación; Python 3.12 se usa solamente para Graphify/RAG. La [guía de instalación](INSTALLATION.md) distingue estas herramientas del arranque del dashboard, que requiere Docker.

| Conjunto | Pruebas definidas | Alcance |
| --- | ---: | --- |
| Backend | 89 | Dominio, HTTP, RBAC, persistencia relacional, proveedor, SignalR y arquitectura. |
| Frontend | 8 | Datos, sesión, guard y respuestas HTTP antiguas frente a actualizaciones SignalR. |
| Contexto Graphify/RAG | 9 | Fuentes, hashes, corrupción, checkpoint, recuperación tras interrupción y dos regresiones de privacidad de rutas. |
| Navegador Chromium | 5 | Roles/CRUD/auditoría, umbral por SignalR, vista móvil, registros técnicos y datos reales representados. |

Los conteos describen las suites de esta revisión; no declaran que se hayan ejecutado en el equipo del lector. Para los resultados del commit concreto consulta [VALIDATION.md](VALIDATION.md) y su ejecución de GitHub Actions.

## Backend

El mismo comando funciona en PowerShell y Unix:

```sh
dotnet test Test/Test.csproj --logger "trx;LogFileName=backend.trx" --results-directory test-results
```

Las pruebas incluyen 13 rutas con JWT ausente/API Key, CRUD y aislamiento de ambos Traders, denegaciones de Viewer, registros técnicos seguros, cola real, filtros y persistencia tras reiniciar el host de prueba. Las pruebas HTTP usan WebApplicationFactory y SQLite relacional y atraviesan el middleware real. Las respuestas controladas del proveedor permiten reproducir nulos, errores, concurrencia y timeouts. Este conjunto no reemplaza las comprobaciones de SQL Server o CoinGecko real con Compose.

## Frontend

Desde la raíz, en PowerShell o Unix:

```sh
cd Frontend/financial-dashboard
npm ci
npm run typecheck
npm test
npm run build
```

`npm test` usa la configuración de Vitest de servicios incluida en `package.json`. `npm run typecheck` y `npm run build` comprueban también los tipos y la compilación de Angular; no se cuentan como tests adicionales. Para el servidor de desarrollo consulta el [README del frontend](../Frontend/financial-dashboard/README.md).

## Cinco E2E con datos reales

Utiliza una instalación **destinada a pruebas** con las listas de Traders en el estado inicial. Los recorridos crean y eliminan un elemento Ethereum de Trader A, comprueban que Trader B está vacío y modifican temporalmente el umbral. Si ya recorriste manualmente la aplicación, usa otra instalación aislada según [INSTALLATION.md](INSTALLATION.md); no borres datos personales para aprobar tests.

Los bloques siguientes comienzan en la raíz y dejan el dashboard iniciado. `npm ci` y la preparación de Chromium se necesitan en el equipo que ejecuta Playwright, aunque la aplicación esté en Docker.

**Windows PowerShell**

```powershell
.\start.ps1
$env:DASHBOARD_URL = 'http://localhost:8080'
$env:REQUIRE_LIVE_DATA = 'true'
Set-Location Frontend/financial-dashboard
npm ci
npx playwright install chromium
npm run e2e
```

**Linux / macOS Intel**

```sh
sh start.sh
cd Frontend/financial-dashboard
npm ci
npx playwright install chromium
DASHBOARD_URL=http://localhost:8080 REQUIRE_LIVE_DATA=true npm run e2e
```

Sustituye el puerto de `DASHBOARD_URL` si cambiaste `DASHBOARD_PORT` en `.env`. En Linux, si faltan bibliotecas del navegador, la CI utiliza `npx playwright install --with-deps chromium` para preparar sus dependencias del sistema.

El resultado esperado de la suite completa es **cinco pruebas aprobadas y ninguna omitida**. Sin `REQUIRE_LIVE_DATA=true`, Playwright omite expresamente el caso externo de CoinGecko. Un test omitido no acredita esa integración.

La prueba real exige HTTP 200, `source=coingecko`, fechas válidas, precios representados en pantalla y el histórico del proveedor. **Origen real no significa dato fresco:** el test no exige `isStale=false`. La UI presenta explícitamente los datos desactualizados y sus fechas. Un fallo del proveedor debe quedar visible como fallo de validación; no se habilita simulación para ocultarlo.

Los resultados están en `Frontend/financial-dashboard/playwright-report` y `Frontend/financial-dashboard/test-results`. Para abrir el informe, desde la carpeta del frontend:

```sh
npx playwright show-report
```

## SQL Server, permisos y persistencia tras reinicio

Esta comprobación requiere Docker y Node, con la aplicación ya iniciada. Se ejecuta desde la raíz y **una sola vez sobre una lista inicial vacía de Trader A**. `tools/smoke.mjs` inserta Bitcoin con la nota `smoke-persistence-check`; repetir el primer paso sobre esa misma lista puede fallar por duplicado. El script verifica 401 sin JWT, 403 para Viewer en auditoría, aislamiento de Trader B y la creación auditada.

Si usas el puerto predeterminado 8080, estos comandos funcionan en PowerShell y Unix:

```sh
node tools/smoke.mjs
docker compose restart api database
docker compose up --detach --wait --wait-timeout 300
node tools/smoke.mjs --verify-restart
node tools/verify-live.mjs
```

Con otro puerto, define `DASHBOARD_URL` en la misma terminal antes de empezar: `$env:DASHBOARD_URL = 'http://localhost:8081'` en PowerShell o `export DASHBOARD_URL=http://localhost:8081` en Unix. Si usaste otro nombre Compose, conserva también `COMPOSE_PROJECT_NAME` en esta terminal o en `.env`.

Avanza al siguiente comando solo si el anterior termina correctamente. Tras el reinicio, `--verify-restart` comprueba la fila de seguimiento y el mismo ID de evento técnico anterior. `verify-live.mjs` exige CoinGecko real tanto para las tres cotizaciones como para el histórico Bitcoin y escribe `test-results/coingecko-live.json`; siempre rechaza una fuente simulada, sin depender de `REQUIRE_LIVE_DATA`. Su evidencia conserva `isStale` para no confundir procedencia con frescura.

Esta secuencia conserva los contenedores y el volumen. Si una fase falla, consulta su evidencia y reanuda desde el paso pertinente; no elimines el volumen ni repitas automáticamente la inserción inicial.

## Graphify, RAG y recuperación

El repositorio no incluye un índice SQLite, un grafo ni un checkpoint antiguos como evidencia vigente. Los wrappers crean `.venv`, instalan `tools/requirements.txt` y generan el contexto desde las fuentes actuales. En Windows `python` debe resolver a Python 3.12; en Unix debe hacerlo `python3`.

El orden es **sync → query → checkpoint → pruebas → status → resume**. Las pruebas de recuperación necesitan ese checkpoint inicial. Los textos de evidencia siguientes describen únicamente la sincronización y la consulta completadas, no el éxito futuro de las pruebas. Ejecuta cada paso solo después de revisar el resultado del anterior.

**Windows PowerShell, desde la raíz**

```powershell
.\tools\context.ps1 sync
.\tools\context.ps1 query 'AccessRules RequireOwner WatchlistWrite'
.\tools\context.ps1 checkpoint --task 'Preparar contexto local' --completed 'Grafo e indice regenerados; consulta ejecutada' --next 'Ejecutar pruebas de recuperacion' --evidence 'Consulta de autorizacion ejecutada sin error'
& .\.venv\Scripts\python.exe tools/test_context.py
.\tools\context.ps1 status
.\tools\context.ps1 resume
```

**Linux / macOS, desde la raíz**

```sh
sh tools/context.sh sync
sh tools/context.sh query 'AccessRules RequireOwner WatchlistWrite'
sh tools/context.sh checkpoint --task 'Preparar contexto local' --completed 'Grafo e indice regenerados; consulta ejecutada' --next 'Ejecutar pruebas de recuperacion' --evidence 'Consulta de autorizacion ejecutada sin error'
.venv/bin/python tools/test_context.py
sh tools/context.sh status
sh tools/context.sh resume
```

Las **nueve pruebas de contexto** comprueban:

- Exportación de rutas relativas portables y rechazo de rutas fuera del repositorio: las dos regresiones de privacidad añadidas en esta edición.
- Serialización JSON con bytes UTF-8/LF reproducibles y fuentes/procedencia esperadas.
- Rechazo de código modificado, grafo alterado e índice SQLite corrupto.
- Checkpoint con información verificable, recuperación desde otro proceso tras una interrupción abrupta y rechazo de fuentes incompatibles con el checkpoint.

`sync` no crea un checkpoint; `resume` contrasta el checkpoint existente con las fuentes actuales. Un índice recién regenerado no demuestra que se hayan revisado los cambios respecto al checkpoint anterior. Después de una tarea, revisa las fuentes, ejecuta las pruebas pertinentes, sincroniza y registra resultados reales junto al próximo paso.

## Qué ejecuta y conserva la CI

El [workflow de verificación](../.github/workflows/verify.yml) separa dos trabajos:

- **application:** 89 pruebas backend, ocho frontend, typecheck/build, arranque con `start.ps1`, arranque Unix idempotente conservando `.env`, smoke inicial, reinicio de API/SQL Server, persistencia, CoinGecko real y cinco E2E con `REQUIRE_LIVE_DATA=true`. El artefacto `evidencia-aplicacion` contiene TRX, Playwright, capturas y JSON de comprobación.
- **context:** instala Graphify, regenera el grafo/índice, consulta autorización, crea el checkpoint y ejecuta nueve pruebas. Al aprobar, `evidencia-contexto` conserva grafo, manifiesto, checkpoint y salidas de sincronización, consulta, estado y pruebas de esa ejecución.

Los trabajos de GitHub Actions utilizan Linux; la ejecución de `start.ps1` allí no representa por sí sola una prueba de Docker Desktop en Windows. [VALIDATION.md](VALIDATION.md) documenta por separado la verificación local Windows y sus límites.
