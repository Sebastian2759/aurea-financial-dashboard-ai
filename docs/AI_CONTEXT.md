# Desarrollo con IA, Graphify y recuperación local

El objetivo es que cada cambio parta de requisitos y fuentes comprobables, y pueda retomarse después de una interrupción. La IA participa como agente de desarrollo y revisión. El dashboard no necesita un modelo de lenguaje, una cuenta de IA ni una base vectorial para funcionar.

![Ciclo de recuperación de contexto](assets/rag-workflow.svg)

## Qué está implementado

El único procedimiento de extracción e indexación es [`tools/context.py`](../tools/context.py). Usa **Graphify 0.9.67**, instalado mediante el paquete `graphifyy==0.9.67` de [`tools/requirements.txt`](../tools/requirements.txt), y **SQLite FTS5** para recuperar fragmentos de texto. Los lanzadores de Windows y Unix preparan un entorno virtual y fijan `PYTHONHASHSEED=0`.

| Parte | Comportamiento comprobable en el código |
|---|---|
| Corpus | Requisitos, ADR, estándares, código, pruebas y archivos de configuración permitidos. Excluye `.env`, `appsettings*`, dependencias, compilados, caché, estado e informes de ejecución. Los archivos de más de 250 KB no se indexan. |
| Graphify | `sync` ejecuta la extracción oficial con `--code-only --force --max-workers 1 --no-cluster`. El adaptador añade documentos Markdown como nodos de fuente explícitos; no inventa relaciones semánticas entre ellos. |
| Fragmentación | Ventanas de hasta 70 líneas, avanzando 55. Cada fragmento conserva ruta, líneas y SHA-256 del archivo completo. |
| Recuperación textual | FTS5 con `unicode61 remove_diacritics 2`, términos unidos por `OR`, un diccionario pequeño de sinónimos español/inglés y orden BM25. El límite predeterminado es 8 fragmentos. |
| Expansión del grafo | Busca nodos pertenecientes a los archivos recuperados y devuelve hasta 30 relaciones incidentes. Conserva `EXTRACTED` o `INFERRED`; no hace una expansión recursiva ni ordena esas relaciones por relevancia. |
| Paquete de contexto | Incluye fragmentos, hasta 30 símbolos por archivo, relaciones, requisitos, ADR, estándares de arquitectura/seguridad/pruebas y checkpoint disponible. Se escribe en `.llmops/state/last-context.json`. |
| Vigencia | Antes de consultar verifica hashes del corpus, del grafo y de SQLite contra el manifiesto. Si falta el índice o cambió una fuente, falla con un mensaje que indica ejecutar `sync`. |
| Continuidad | El checkpoint guarda tarea, paso completado, siguiente acción, evidencia, versión de Graphify y hashes. `resume` valida fuentes, versión y campos mínimos antes de devolver el siguiente paso. |

Esto es recuperación aumentada por grafo para el agente: **la recuperación es local, textual y determinista; la generación queda a cargo del agente de desarrollo**. No hay embeddings ni una llamada a un LLM escondida en `context.py`.

## Una imagen basada en el grafo real

![Vista seleccionada del grafo de Graphify](assets/graphify-map.svg)

La imagen es una **vista editorial resumida** del artefacto `docs/architecture/graphify-out/graph.json` generado durante el desarrollo, no una captura de la interfaz de Graphify. Se eligieron ocho símbolos y seis relaciones existentes para mostrar dos preguntas útiles: dónde se aplica la regla de propietario y qué entradas solicitan una cotización.

Metadatos del snapshot utilizado, conservados como procedencia de esta ilustración:

- Generación del manifiesto: **2026-09-25T20:55:36.376096+00:00**.
- Graphify: **0.9.67**.
- Corpus: **291 archivos**.
- Grafo: **1.505 nodos** —incluidos 21 documentos— y **3.562 aristas**.
- Procedencia de aristas: **3.407 `EXTRACTED`**, **155 `INFERRED`**.
- SHA-256 del grafo: `3afbf00633e27965b3741f49888f07e57576ff120716e31ca052cf2fa2eb379f`.

Los conteos describen ese snapshot de desarrollo del 25 de septiembre de 2026, previo al paquete público. La publicación añade documentación y endurece el manejo de rutas, por lo que un `sync` posterior puede cambiar conteos, IDs o hashes. La imagen no se usa para afirmar que un índice local está vigente: esa comprobación corresponde al comando `status`.

El repositorio público conserva código, herramientas y esta ilustración sin rutas personales. Los archivos generados del grafo, manifiestos y checkpoints históricos no se distribuyen como estado vigente. El comando `sync` crea el grafo canónico a partir de tu checkout; el workflow `verify.yml` también lo regenera y publica el artefacto descargable **`evidencia-contexto`** en GitHub Actions. Así puede inspeccionarse el resultado de una ejecución, en lugar de confiar en una captura.

### Cómo se seleccionaron las relaciones

Se leyeron directamente `nodes` y `edges` del JSON; se contaron las entradas y sus etiquetas de procedencia. Para dibujar se seleccionaron estas seis aristas, sin añadir dependencias ni un extractor nuevo:

| Origen | Destino | Relación y procedencia | Ubicación que registra la arista |
|---|---|---|---|
| `AddWatchlistItemCommand.Handle()` | `AccessRules.RequireOwner()` | `calls`, `EXTRACTED` | `Core/Application/UseCases/Watchlists/AddWatchlistItem/AddWatchlistItemCommand.cs`, L21 |
| `GetWatchlistQuery.Handle()` | `AccessRules.RequireOwner()` | `calls`, `EXTRACTED` | `Core/Application/UseCases/Watchlists/GetWatchlist/GetWatchlistQuery.cs`, L21 |
| `UpdateWatchlistItemCommand.Handle()` | `AccessRules.RequireOwner()` | `calls`, `EXTRACTED` | `Core/Application/UseCases/Watchlists/UpdateWatchlistItem/UpdateWatchlistItemCommand.cs`, L21 |
| `RemoveWatchlistItemCommand.Handle()` | `AccessRules.RequireOwner()` | `calls`, `EXTRACTED` | `Core/Application/UseCases/Watchlists/RemoveWatchlistItem/RemoveWatchlistItemCommand.cs`, L21 |
| `MarketBroadcastWorker.ExecuteAsync()` | `GetMarketSnapshotRequest` | `calls`, `INFERRED`, puntuación 0,85 | `Api/Api/Realtime/MarketBroadcastWorker.cs`, L19 |
| `MarketHub.Subscribe()` | `GetMarketSnapshotRequest` | `calls`, `INFERRED`, puntuación 0,85 | `Api/Api/Realtime/MarketHub.cs`, L17 |

La etiqueta `INFERRED` procede de Graphify y se muestra con línea discontinua. Esa puntuación es metadato del extractor, no una probabilidad calibrada ni prueba de comportamiento en ejecución. Incluso una relación `EXTRACTED` requiere revisar su fuente para evaluar significado y seguridad. El grafo orienta la lectura; las pruebas comprueban el comportamiento.

Los IDs exactos usados son:

```text
core_application_usecases_watchlists_addwatchlistitem_addwatchlistitemcommand_application_usecases_watchlists_addwatchlistitem_addwatchlistitemcommand_handle
core_application_usecases_watchlists_getwatchlist_getwatchlistquery_application_usecases_watchlists_getwatchlist_getwatchlistquery_handle
core_application_usecases_watchlists_updatewatchlistitem_updatewatchlistitemcommand_application_usecases_watchlists_updatewatchlistitem_updatewatchlistitemcommand_handle
core_application_usecases_watchlists_removewatchlistitem_removewatchlistitemcommand_application_usecases_watchlists_removewatchlistitem_removewatchlistitemcommand_handle
core_application_security_accessrules_application_security_accessrules_requireowner
api_api_realtime_marketbroadcastworker_api_realtime_marketbroadcastworker_executeasync
api_api_realtime_markethub_api_realtime_markethub_subscribe
core_application_usecases_markets_getmarketsnapshot_getmarketsnapshotrequest_application_usecases_markets_getmarketsnapshot_getmarketsnapshotrequest
```

## Reproducir una consulta

Estas herramientas son opcionales para evaluar la aplicación y requieren **Python 3.12 con `venv` y `pip`**, además de acceso a Internet para la primera instalación. Desde la raíz del repositorio:

**Windows PowerShell**

```powershell
.\tools\context.ps1 sync
.\tools\context.ps1 status
.\tools\context.ps1 query "AccessRules RequireOwner WatchlistWrite" --limit 12
```

**Linux / macOS**

```sh
sh tools/context.sh sync
sh tools/context.sh status
sh tools/context.sh query "AccessRules RequireOwner WatchlistWrite" --limit 12
```

La consulta es uno de los casos de [`tools/test_context.py`](../tools/test_context.py): busca recuperar `AccessRules.cs` con hash, líneas, símbolos y relaciones. Su salida es JSON con fuentes reales; no una respuesta narrativa preescrita. `sync` regenera el grafo completo e índice local, por lo que puede modificar artefactos derivados en el checkout.

Si se edita una fuente después de `sync`, `query` devuelve error por índice desactualizado. Después de revisar la diferencia, se vuelve a ejecutar `sync` antes de consultar. No se corrige ese error alterando manualmente los hashes del manifiesto.

## Guardar y retomar trabajo con evidencia

Después de ejecutar las pruebas pertinentes y regenerar el contexto, se puede registrar un checkpoint. Este ejemplo muestra la sintaxis: **sustituye los campos por trabajo y resultados que realmente hayas realizado**.

```powershell
.\tools\context.ps1 checkpoint --task "Revisión de permisos" --completed "Paso comprobado" --next "Siguiente acción concreta" --evidence "Comando ejecutado, resultado y ruta de la evidencia"
.\tools\context.ps1 resume
```

En Unix se usan los mismos argumentos con `sh tools/context.sh`. `resume` requiere un checkpoint existente y compatible; un clon nuevo debe generar su contexto y registrar su propio trabajo. Un checkpoint no demuestra que un test pasó: conserva la referencia a una evidencia que debe poder revisarse.

Las evaluaciones de `tools/test_context.py` incluyen recuperación y procedencia, serialización canónica, cambios de código/grafo, corrupción de SQLite, información mínima del checkpoint, recuperación después de una salida abrupta y rechazo de fuentes cambiadas. Tras crear el contexto y un checkpoint válido se ejecutan con el Python del entorno virtual:

```powershell
.\.venv\Scripts\python.exe -m unittest discover -s tools -p test_context.py -v
```

```sh
.venv/bin/python -m unittest discover -s tools -p test_context.py -v
```

## Qué aporta durante una defensa técnica

Un ejemplo concreto sería añadir un filtro de seguimiento por activo. Antes de editar, se recuperan los casos de uso y las reglas de propietario; después se revisan los controladores, contratos y componentes implicados. La implementación debe conservar el aislamiento entre Traders. Las pruebas y la revisión verifican que el filtro no abre acceso a listas ajenas, y un nuevo checkpoint deja el resultado y el siguiente paso.

Graphify ayuda a localizar relaciones. FTS5 ayuda a encontrar texto relevante. Los hashes evitan reutilizar contexto alterado sin advertencia. Ninguna de esas piezas sustituye el criterio del desarrollador, la autorización en el servidor ni las pruebas de integración.

Los prompts, errores reales producidos durante el desarrollo y sus correcciones están registrados en [`AI_PROMPTS.md`](../AI_PROMPTS.md). Las definiciones de revisión y estándares de `.llmops/` organizan el trabajo del agente; no constituyen una plataforma de inferencia desplegada con el dashboard.
