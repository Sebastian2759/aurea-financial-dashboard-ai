# Frontend de Áurea

Angular 22.2.0 con componentes standalone, Signals, RxJS, Chart.js y SignalR. El código se organiza por funcionalidades en `src/app/features`; Mercado y Seguimiento separan `domain`, `application`, `infrastructure` y `presentation`.

Para evaluar la aplicación completa usa el arranque Docker de la [guía de instalación](../../docs/INSTALLATION.md). Los comandos siguientes son para desarrollar o probar el frontend y se ejecutan desde esta carpeta con Node 24.21.0; `package.json` admite `>=24.15.0 <25`.

## Desarrollo local

```sh
npm ci
npm start
```

Abre [http://localhost:4200](http://localhost:4200). `npm start` carga `proxy.conf.json`, que envía `/api`, `/hubs` y `/health` a una API en `http://localhost:5080`. El frontend necesita esa API para crear sesiones y consultar datos.

Para usar la API de desarrollo, abre otra terminal en la raíz del repositorio y ejecuta:

```sh
dotnet run --project tools/BrowserHost
```

Ese host requiere SDK .NET 10 y usa SQLite local persistente con CoinGecko real. La evaluación de SQL Server se realiza con Docker Compose; no se acredita mediante este host de desarrollo.

## Comprobar el frontend

```sh
npm run typecheck
npm test
npm run build
```

`npm test` ejecuta los ocho tests de servicios con Vitest. `npm run build` genera la aplicación en `dist/financial-dashboard/browser`. Las dependencias y el CLI se instalan localmente con `npm ci`; no hace falta instalar Angular CLI de forma global.

## Playwright

Con la aplicación de Docker disponible en el puerto 8080 y una base destinada a evaluación, ejecuta los cinco recorridos Chromium. Cambia `DASHBOARD_URL` si configuraste otro puerto.

**PowerShell**

```powershell
npx playwright install chromium
$env:DASHBOARD_URL = 'http://localhost:8080'
$env:REQUIRE_LIVE_DATA = 'true'
npm run e2e
```

**Linux / macOS Intel**

```sh
npx playwright install chromium
DASHBOARD_URL=http://localhost:8080 REQUIRE_LIVE_DATA=true npm run e2e
```

`REQUIRE_LIVE_DATA=true` activa la prueba que exige cotizaciones e histórico de CoinGecko en pantalla. Sin esa variable, el caso externo se omite. Los recorridos crean datos de seguimiento y cambian temporalmente el umbral; usa una instalación de pruebas. Los reportes quedan en `playwright-report` y `test-results`.

Consulta [TESTING.md](../../docs/TESTING.md) para la secuencia completa, los requisitos del estado inicial y los límites de la evidencia; [VALIDATION.md](../../docs/VALIDATION.md) distingue pruebas definidas y resultados ejecutados.
