# Requisitos y aceptación

Fuente: Candidate Take-Home Assignment: Real-Time Financial Dashboard with Dynamic RBAC.
La referencia temporal del ejercicio es 1-2 horas; se registra el tiempo real sin falsear resultados.

| ID | Requisito | Evidencia requerida |
| --- | --- | --- |
| REQ-01 | CoinGecko con BTC, ETH y SOL, USD/EUR, métricas seleccionables | Proveedor real + tests deterministas de respuesta |
| REQ-02 | Actualizaciones del servidor mediante SignalR | Dos clientes, reconexión y recuperación de estado |
| REQ-03 | Caché, rate limiting y reintentos limitados; conservar datos reales antiguos o informar 503, sin simulación en la entrega | Tests 429/timeout/stale; SimulationIsNotUsedWhenDisabled valida cotizaciones e histórico |
| REQ-04 | JWT, Viewer/Trader/Admin, selector de usuarios demo | Validación HTTP real 401/403/expiración/firma |
| REQ-05 | Permisos de interfaz y backend | Matriz por endpoint; API Key no concede permisos humanos |
| REQ-06 | Crear, editar y eliminar elementos de seguimiento | Propiedad Trader, acceso global Admin, unicidad, persistencia |
| REQ-07 | Gráfico financiero interactivo | Chart.js, activo y período 24h/7d |
| REQ-08 | Auditoría y registros del sistema solo Admin | Actor/propietario, transacción de negocio, eventos técnicos seguros y filtros |
| REQ-09 | Umbral global editable por Admin | Valor inicial 5%, validación, aplicación y notificación |
| REQ-10 | Repositorio y arranque de un comando | Compose + script + README español |
| REQ-11 | AI_PROMPTS.md | Prompts y error real de IA con corrección y regresión |
| REQ-12 | Suite automatizada | Unitarios, HTTP, persistencia, frontend y E2E |
| REQ-13 | Graphify y RAG durante todo el proceso | Grafo, recuperación con fuentes, vigencia y checkpoint |

Viewer consulta mercado/umbrales; Trader además gestiona su lista; Admin gestiona todas las listas, umbrales y auditoría.
Hay cuatro identidades predefinidas: Viewer, Trader A, Trader B y Admin.
Precio, variación 24h, capitalización, volumen y volatilidad son métricas distintas.
Volatilidad: desviación muestral de 24 retornos logarítmicos horarios × sqrt(24) × 100, con 25 cierres consecutivos positivos; en otro caso null.
La selección de moneda cotiza el activo en esa moneda; no se presenta como un servicio FX independiente.



## Trazabilidad de implementación y evidencia

El [enunciado traducido](ASSIGNMENT.es.md) permite contrastar el alcance. Todos los requisitos siguientes tienen implementación y validación; [VALIDATION.md](VALIDATION.md) enlaza la ejecución aprobada y distingue los entornos utilizados.

| Requisitos | Implementación | Evidencia verificable |
|---|---|---|
| REQ-01, 03 | [Proveedor y caché](../Infraestructure/Adapters/MarketData/CoinGeckoMarketDataProvider.cs) | [Nulos, concurrencia, límites, fallos e históricos](../Test/Markets/CoinGeckoProviderTests.cs) y [volatilidad](../Test/Markets/VolatilityCalculatorTests.cs) |
| REQ-02 | [Hub](../Api/Api/Realtime/MarketHub.cs) y [actualización periódica](../Api/Api/Realtime/MarketBroadcastWorker.cs) | [Dos clientes y reconexión](../Test/Integration/SignalRTests.cs); E2E de umbral entre sesiones |
| REQ-04, 05 | [Políticas JWT](../Api/Api/Program.cs), [propiedad](../Core/Application/Security/AccessRules.cs), [sesión Angular](../Frontend/financial-dashboard/src/app/core/session/session.store.ts) | [HTTP real 401/403](../Test/Integration/RbacHttpTests.cs), [cancelación y limpieza de sesión](../Frontend/financial-dashboard/src/app/core/session/session.store.spec.ts) |
| REQ-06, 08 | [Casos de seguimiento](../Core/Application/UseCases/Watchlists), [auditoría](../Core/Application/UseCases/AuditEvents) | [CRUD y actor Admin](../Test/Integration/RbacHttpTests.cs), [reinicio](../Test/Integration/PersistenceRestartTests.cs), smoke SQL Server y E2E |
| REQ-07 | [Gráfico Chart.js](../Frontend/financial-dashboard/src/app/features/market/presentation/price-chart/price-chart.ts) | [E2E: moneda, período y métricas](../Frontend/financial-dashboard/e2e/dashboard.spec.ts) |
| REQ-09 | [Cambiar umbral](../Core/Application/UseCases/DashboardSettings/UpdateThresholds/UpdateThresholdsCommand.cs) | Validación HTTP y E2E de difusión a otra sesión por SignalR |
| REQ-10 | [Compose](../compose.yaml), [Windows](../start.ps1), [Unix](../start.sh) | [CI desde checkout limpio](../.github/workflows/verify.yml) y [smoke de persistencia](../tools/smoke.mjs) |
| REQ-11 | [AI_PROMPTS.md](../AI_PROMPTS.md) | Error real con traza/fallo observado, corrección y prueba de regresión |
| REQ-12 | [Backend](../Test), [frontend](../Frontend/financial-dashboard), [CI](../.github/workflows/verify.yml) | Suite y resultados del commit público en Actions; informes TRX y Playwright |
| REQ-13 | [Contexto Graphify/FTS](../tools/context.py), [generación de grafo y checkpoint](AI_CONTEXT.md) | [Evaluaciones de recuperación/vigencia](../tools/test_context.py) y artefacto evidencia-contexto de CI |


Cierre: REQ-01…REQ-13 tienen implementación y comprobaciones asociadas. [VALIDATION.md](VALIDATION.md) distingue los entornos y enlaza la CI pública del commit evaluado.

## Cierre de la revisión literal

[SystemEvents](../Core/Application/UseCases/SystemEvents) cubre los registros técnicos. [PermissionMatrixTests](../Test/Integration/PermissionMatrixTests.cs) completa los casos por operación; [SystemEventsTests](../Test/SystemEvents/SystemEventsTests.cs) verifica seguridad y persistencia. [COMPLETION.md](COMPLETION.md) detalla cada pendiente cerrado y la condición de acceso al repositorio.
