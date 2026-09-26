# Registro de uso de IA

Este registro combina **resúmenes de instrucciones aplicadas** y citas identificadas. No es una transcripción completa del chat. Los errores históricos se distinguen de los casos controlados de prueba y de las mejoras solicitadas para publicar.

## Preparación de la entrega pública · 25/09/2026

**Solicitud del usuario, extracto textual:** «me gusta esa respuesta para el readme del repositorio y tambien que coloques una imagen del graphify». También solicitó instrucciones de instalación detalladas, un repositorio público nuevo y excluir archivos ajenos a la prueba.

**Instrucción aplicada, resumen:** preparar una copia de publicación con historial nuevo y fechas reales; explicar decisiones y límites, documentar dependencias de ejecución/desarrollo por separado, incorporar una captura real y mapas basados en relaciones comprobadas, revisar secretos y regenerar contexto en CI antes de acreditar la entrega.

**Contexto utilizado:** matriz de requisitos, scripts de arranque, Compose, contratos, índice FTS5 y grafo base. Se comprobaron los hashes de sus 291 fuentes y del grafo/índice antes de preparar los cambios. Las lecturas recuperaron documentos de validación, el workflow y el arranque. La evidencia local confirmó SQL Server y cinco recorridos de navegador.

**Hallazgos concretos de revisión:** el grafo exportaba rutas absolutas del equipo; se incorporó normalización a rutas relativas y regresiones que rechazan rutas ajenas. La copia requería su propio nombre Compose para no compartir un volumen con credenciales distintas. Se eliminaron metadatos de plantilla ajenos a la evaluación, checkpoints históricos y enlaces a evidencia privada. Son hallazgos de preparación, no errores artificiales inventados para cumplir el enunciado.

**Resultado revisable:** README, INSTALLATION.md, AI_CONTEXT.md, dos SVG con procedencia, captura real sin alterar, cambios acotados de configuración y generador. La CI pública reconstruye el grafo, ejecuta recuperación y pruebas, y publica artefactos. [VALIDATION.md](docs/VALIDATION.md) identifica qué se ejecutó; el texto de un prompt no se usa como prueba de éxito.

**Ejemplo de prompt de revisión versionado:** [review-v1.md](.llmops/prompts/review-v1.md). Pide contrastar seguridad, arquitectura y tests con fuentes recuperadas; su salida debe aportar hallazgos y evidencia verificable.

## Revisión local solicitada: datos reales obligatorios

Instrucción del usuario: la vista presentada debe consultar el proveedor financiero real, sin datos simulados. Contexto: `context.py query` recuperó configuración de mercado, requisitos, ADR, Compose y validaciones anteriores.

Error real del agente: para abrir la aplicación en Windows se eligió BrowserHost, que entonces sustituía CoinGecko por un proveedor de pruebas. El aviso visible «Datos simulados de demostración» permitió detectar que la vista no demostraba la integración real solicitada. Se corrigió BrowserHost para conservar CoinGecko, deshabilitar la simulación, generar una firma de sesión aleatoria y conservar SQLite en archivo. Compose también deshabilita la simulación; un fallo sin datos reales previos produce 503.

Verificación: `verify-live.mjs` pasó en localhost con tres cotizaciones y 25 puntos históricos de fuente `coingecko`; se comprobó la pantalla. El test existente de simulación deshabilitada ahora comprueba cotizaciones e histórico. Pasan las 89 pruebas backend. Las restricciones de TLS de este entorno y el transporte local de respuestas reales están descritos en `docs/VALIDATION.md`.

La implementación fue asistida por el agente de desarrollo. Graphify 0.9.67 extrae relaciones AST; SQLite FTS recupera fragmentos con fuentes y el grafo amplía sus dependencias. No existe un LLM dentro de la aplicación financiera.

## Prompts relevantes y contexto

| Versión | Instrucción aplicada | Contexto usado | Resultado |
|---|---|---|---|
| implementación-v1 | Implementar el plan acordado sobre la plantilla conservando Clean Architecture/CQRS; Angular por funcionalidades; Graphify/RAG antes y después de cada unidad | Enunciado, plantilla, estándares, ADR-001 y matriz REQ-01…13 | Módulos de negocio, contratos HTTP/SignalR y demo reproducible |
| seguridad-v1 | Verificar 401/403 con middleware real, identidad del servidor, aislamiento Trader A/B y auditoría de Admin | AccessRules, Program, JWT, casos de uso y pruebas HTTP recuperados | Suite de autorización y limpieza de sesión |
| datos-v1 | Conservar nulos, distinguir simulación y dato antiguo, deduplicar peticiones y limitar reintentos | CoinGeckoMarketDataProvider, VolatilityCalculator, tests de proveedor | Caché compartida, timestamp de origen y pruebas de regresión |
| interfaz-v1 | Crear una interfaz española legible, con estados visibles y componentes por funcionalidad; cancelar datos privados al cambiar de usuario | Contratos, session.store, feature facades y requisitos de roles | Mercado, gráfico, seguimiento, umbrales y auditoría |
| entrega-v1 | Un evaluador debe poder iniciar frontend, API y SQL Server con un comando, sin ejecutar migraciones a mano | Dockerfiles, Compose, migraciones y healthchecks | start.ps1/start.sh, smoke de reinicio y CI |
| revisión-v1 | Aplicar `.llmops/prompts/review-v1.md` y registrar evidencia sin declarar ejecutadas las validaciones pendientes | Paquetes RAG, grafo y resultados de tests | Revisión estructurada; resultados actuales en CI y VALIDATION.md |

## Error real producido por IA: nulos de CoinGecko

La primera implementación generada utilizaba `JsonElement.TryGetDecimal` y `TryGetDouble` sin comprobar `ValueKind`. Esos métodos lanzan `InvalidOperationException` cuando el valor JSON es `null`.

**Efecto:** una respuesta de CoinGecko válida con capitalización, volumen o precio nulo terminaba en el manejador de errores y activaba el respaldo demo. No se preservaba la fuente real.

**Detección:** la primera ejecución de `NullMetricsRemainNullWithoutSwitchingToSimulation` falló: esperaba `coingecko` y recibió `demo`. También falló la prueba de deduplicación: esperaba cuatro llamadas (mercados y tres históricos), pero solo hubo dos porque la lectura falló en el primer activo.

**Corrección:** verificar `JsonValueKind.Number` antes de leer números, y `JsonValueKind.String` antes de interpretar la fecha. Los valores ausentes se conservan como `null`; Angular muestra «No disponible».

**Regresión ejecutada:** `Test/Markets/CoinGeckoProviderTests.cs`, pruebas `NullMetricsRemainNullWithoutSwitchingToSimulation` y `ConcurrentReadersShareCacheAndFailurePreservesLastRealTimestamp`, ambas pasan después de corregir el código. El test fallido fue observado durante esta implementación, no es un error artificial insertado para satisfacer el documento.

## Otros hallazgos reales

- CI detectó una carrera en el recorrido de edición: la prueba pulsaba Editar y escribía sobre el formulario todavía visible de Agregar, antes de que Angular renderizara la nueva selección. La traza confirmó que el PUT enviaba la nota antigua. Se reforzó el formulario con controles reactivos e inicialización síncrona; una nueva ejecución demostró que también era necesario esperar el estado visible «Editar seguimiento» y su valor inicial antes de escribir. La prueba conserva la comprobación de que la nueva nota persiste; no usa pausas fijas ni reintentos para ocultar el fallo.
- La auditoría de restauración de CI detectó `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 vulnerable en las pruebas. Se añadió el bundle 3.0.5; la siguiente auditoría no encontró paquetes NuGet vulnerables. No se deshabilitó el control NU1903.
- La configuración inicial de WebApplicationFactory llegaba después de la lectura temprana de la clave JWT. Se ajustó la configuración del host de pruebas, manteniendo la exigencia de una clave válida en la aplicación.
- Las fechas `datetime2` no preservan `DateTime.Kind`. La revisión del navegador mostró auditoría en hora UTC interpretada como local; el contrato JSON ahora marca explícitamente UTC y la prueba de reinicio exige el sufijo `Z`.
- La revisión detectó que un cierre horario inválido podía ser sustituido por un precio anterior de la misma hora. Se corrigió la selección y se añadió `InvalidFinalCloseCannotReuseAnEarlierPriceFromTheSameHour`.

## Ampliación de criterios solicitada

La nueva ejecución de CI verificó CoinGecko real, ambos scripts de arranque, persistencia de SQL Server y cuatro recorridos de navegador, pero detectó que Viewer no conservaba el umbral enviado por SignalR. Una regresión controlada reprodujo el error: iniciar GET con 5, recibir SignalR con 1.25 y terminar aquel GET devolvía incorrectamente a 5. Se añadió una revisión de estado que descarta respuestas anteriores a notificaciones o cambios de sesión. El formulario también espera la carga inicial antes de permitir editar. Las tres regresiones pasan; se mantiene el E2E original sin aumentar esperas ni añadir reintentos.

La revisión de artefactos detectó además que JSON generado en Windows utilizaba CRLF; la publicación normalizaba a LF y alteraba los hashes. El generador ahora fija UTF-8/LF y dispone de una prueba de bytes canónicos.

Prompt aplicado: «Completar registros del sistema para Admin, toda la matriz HTTP de permisos, corrupción del índice RAG y recuperación tras interrupción; demostrar CoinGecko real y actualizar la entrega». Contexto: PDF original, matriz, estándares, grafo y paquetes RAG recuperados antes de cada unidad.

Se añadieron eventos técnicos con catálogo cerrado, cola acotada, migración SQL y vista administrativa. La ampliación de pruebas verifica cada ruta funcional sin JWT o con API Key, las tres mutaciones denegadas a Viewer y CRUD/aislamiento de ambos Traders. La recuperación usa dos procesos: el primero guarda un checkpoint y termina abruptamente; el segundo verifica hashes y retoma la siguiente acción. La corrupción del índice y la interrupción son pruebas controladas, no errores históricos de IA.

Un error real de esta ampliación apareció al compilar el test del validador: la IA utilizó `Validate(new(...))`, ambiguo entre Request y ValidationContext. El compilador emitió CS0121; se corrigió usando `new GetSystemEventsRequest(...)`. Las 89 pruebas backend posteriores pasaron.


## Límites de la evidencia

Las pruebas con SQLite no acreditan ejecución sobre SQL Server. El despliegue local con Docker sí comprobó SQL Server, persistencia tras reinicio y cinco E2E. Esta publicación se vuelve a validar desde un checkout limpio en su propia CI. Ver [VALIDATION.md](docs/VALIDATION.md) para resultados, fechas y límites; los resultados históricos no se atribuyen automáticamente a un commit nuevo.
