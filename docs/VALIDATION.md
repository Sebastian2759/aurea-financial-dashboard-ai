# Evidencia de validación

El estado verificable de esta entrega pública está en [Verificar entrega](https://github.com/Sebastian2759/aurea-financial-dashboard-ai/actions/workflows/verify.yml). Cada ejecución identifica su commit y conserva sus resultados. El historial nuevo no reutiliza como propias las ejecuciones del repositorio de trabajo anterior.

## Verificación del repositorio público

El workflow ejecuta dos trabajos independientes:

| Trabajo | Qué comprueba | Evidencia |
|---|---|---|
| application | Backend HTTP/RBAC, proveedor y volatilidad; frontend, typecheck y build; ambos scripts; SQL Server; persistencia tras reinicio; CoinGecko real; cinco recorridos de Chromium | Pasos de Actions y artefacto `evidencia-aplicacion` |
| context | Graphify oficial, recuperación con fuentes, hashes, checkpoint, índice alterado y recuperación después de interrupción | Pasos de Actions y artefacto `evidencia-contexto` |

Consulta la ejecución del commit que estés evaluando. Un test que existe en el código no equivale a un test ejecutado; una insignia pendiente no significa aprobación. Los comandos para repetir cada conjunto están en [TESTING.md](TESTING.md).

## Verificación local previa de la aplicación

El **25/09/2026 entre las 22:25:45 y 22:32:20, America/Bogota**, se ejecutó el mismo código de aplicación en Windows con Docker Desktop 4.92.0, motor Linux 29.8.0, Compose 5.5.1 y WSL 2.7.14.0. Se aprobaron nueve etapas de validación y cinco E2E sin omisiones:

1. Motor Docker Linux disponible.
2. Arranque de Angular/Nginx, API .NET y SQL Server.
3. Permisos y persistencia inicial.
4. Reinicio real de API y SQL Server.
5. Healthchecks después del reinicio.
6. Recuperación del seguimiento y del mismo registro técnico anterior.
7. Cotizaciones e histórico de CoinGecko.
8. Preparación de Chromium.
9. Cinco recorridos de navegador con `REQUIRE_LIVE_DATA=true`.

Se verificó además HTTP 200 en `/health/ready` y `/market`, y la pantalla de mercado. La captura del README procede de ese despliegue local. En la copia pública el nombre de Compose es `aurea-dashboard-ai` para aislar sus contenedores/volumen; el perfil Development también deshabilita simulación y las herramientas de contexto normalizan rutas para su publicación. Esos ajustes se vuelven a comprobar en la CI pública.

La descarga inicial de Microsoft Container Registry falló por EOF. Una prueba temporal de preferencia IPv4 permitió descargar las imágenes; la configuración original de Windows fue restaurada y verificada antes del arranque completo. Es evidencia de aquel equipo, no un requisito para todos los evaluadores ni una garantía sobre futuras descargas.

## Datos reales, frescura y límites

La comprobación local de las **03:29:20 UTC del 26/09/2026** registró `source=coingecko` para BTC/ETH/SOL y un histórico de Bitcoin de 24 puntos. Las cotizaciones tenían fecha **03:26:20 UTC** y `isStale=true`. Esto acredita origen real, **no frescura**. `verify-live.mjs` valida la fuente y los valores, no exige `isStale=false`.

La aplicación informa datos desactualizados y conserva sus fechas. Un fallo sin respuesta real anterior produce 503. La disponibilidad y límites de CoinGecko son externos. La captura del README conserva el aviso visible; no se editaron sus valores ni se ocultó el estado.

Los tests HTTP usan SQLite relacional y atraviesan middleware, autorización, validadores y persistencia; **no sustituyen** las comprobaciones de SQL Server con Compose. Las pruebas del proveedor emplean respuestas controladas para reproducir nulos, errores, concurrencia y tiempos de espera. Los E2E y `verify-live.mjs` exigen CoinGecko real cuando `REQUIRE_LIVE_DATA=true`.

La difusión del umbral entre dos sesiones se verifica en navegador; los tests de SignalR también comprueban conexiones y reconexión. Estas comprobaciones no representan una prueba de carga ni una certificación de disponibilidad permanente.

## Integridad del proceso de IA

El grafo y SQLite se generan con el procedimiento único `tools/context.py`; la recuperación rechaza fuentes o índices alterados. El checkpoint se crea sobre el corpus actual y registra evidencia concreta. Los artefactos históricos del equipo de desarrollo y sus rutas privadas no se versionan en esta entrega.

El mapa de Graphify del README es una selección identificada del grafo base del 25/09/2026. Los conteos pueden cambiar al regenerar con las nuevas herramientas/documentos; la CI conserva el grafo completo de la revisión que realmente ejecutó.

Los errores de IA históricos y las regresiones controladas se distinguen en [AI_PROMPTS.md](../AI_PROMPTS.md). Las 1–2 horas indicadas por el enunciado son una referencia de alcance, no una afirmación sobre la duración total de esta entrega ampliada.
