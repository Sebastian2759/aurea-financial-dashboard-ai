# Instalación y ejecución de Áurea

Esta guía permite iniciar el frontend Angular, la API .NET y SQL Server desde una copia de `Sebastian2759/aurea-financial-dashboard-ai`. Una vez instalado Docker y descargado el repositorio, el arranque requiere un solo comando: `./start.ps1` en PowerShell o `sh start.sh` en Linux/macOS Intel.

## 1. Preparar el equipo

Para evaluar la aplicación se necesita Docker con motor de **contenedores Linux x64** y el complemento **Docker Compose v2 o posterior**, con las opciones `--wait` y `--wait-timeout`. Compose v5 también sirve: la verificación local utilizó v5.5.1. El comando es `docker compose`, sin guion. Docker Desktop incluye Engine, CLI y Compose; en Linux también puede usarse Docker Engine con el complemento Compose. [Instalación oficial de Compose](https://docs.docker.com/compose/install/).

No hace falta instalar Node.js, Angular CLI, .NET, SQL Server ni Python para abrir el dashboard: los Dockerfiles descargan y compilan las dependencias dentro de contenedores. Git es opcional si se descarga el ZIP. Se necesita un navegador y conexión a Internet durante la construcción y para consultar CoinGecko.

| Entorno | Preparación |
| --- | --- |
| Windows x64 | Instalar Docker Desktop, completar la configuración de WSL 2 y virtualización, abrir Docker Desktop y esperar a que el motor esté listo. Seleccionar contenedores Linux. Es el entorno local comprobado. |
| Linux x64 | Instalar Docker Engine y Compose. El usuario que inicia el proyecto debe poder ejecutar `docker info`. |
| macOS Intel | Usar Docker Desktop con motor Linux x64. No se ha realizado una validación local de esta entrega en macOS. |
| Apple Silicon o Windows/Linux ARM | Esta configuración de SQL Server no es una ruta compatible. Utilizar un equipo o una máquina Linux x64 con CPU Intel/AMD. No se ofrece emulación ARM como solución validada. |

Microsoft limita el soporte de los contenedores SQL Server a Linux sobre CPU Intel/AMD x86-64; las capas de emulación como Rosetta o QEMU no están probadas ni soportadas. [Compatibilidad de SQL Server en contenedores](https://learn.microsoft.com/en-us/sql/linux/install-upgrade/quickstart-install-docker?view=sql-server-ver17).

Como margen práctico para este proyecto, se recomiendan **6 GB disponibles para Docker y 16 GB de RAM en el equipo** durante la construcción. Es una recomendación operativa, no un mínimo medido de la aplicación. SQL Server por sí solo requiere al menos 2 GB de RAM y Docker Desktop en Windows indica 8 GB de RAM del sistema; también debe quedar memoria para el sistema operativo y el navegador. Reserva espacio para varios GB de imágenes, cachés y datos. [Requisitos de SQL Server](https://learn.microsoft.com/en-us/sql/linux/install-upgrade/quickstart-install-docker?view=sql-server-ver17), [requisitos e instalación de Docker Desktop en Windows](https://docs.docker.com/desktop/setup/install/windows-install/).

Comprueba la instalación desde una terminal nueva:

```text
docker info --format '{{.OSType}} {{.Architecture}}'
docker compose version
```

El motor debe responder `linux` y una arquitectura x64 (`x86_64`/`amd64`). Los scripts comprueban que Docker y Compose respondan; no sustituyen esta comprobación de arquitectura. Si `docker` no se reconoce después de instalarlo, abre de nuevo la terminal.

## 2. Descargar e iniciar

Descarga el ZIP desde GitHub y extráelo, o clona el repositorio:

```sh
git clone https://github.com/Sebastian2759/aurea-financial-dashboard-ai.git
cd aurea-financial-dashboard-ai
```

La raíz correcta contiene `compose.yaml`, `start.ps1` y `start.sh`. Abre allí una terminal. Con Docker iniciado, ejecuta **uno** de estos comandos según tu sistema:

**Windows PowerShell**

```powershell
.\start.ps1
```

**Linux o macOS Intel**

```sh
sh start.sh
```

El script realiza estas acciones:

1. Comprueba que Docker está disponible.
2. Crea `.env` si todavía no existe, con credenciales aleatorias locales y el puerto 8080.
3. Descarga SQL Server `2022-CU22-ubuntu-22.04` y las imágenes de construcción; compila la API .NET 10 y Angular 22.2.0.
4. Inicia SQL Server; la API inicializa la base y aplica las migraciones; después se inicia Nginx con el frontend.
5. Espera a que los tres servicios superen sus comprobaciones de salud y muestra `Dashboard listo: http://localhost:8080`.

La primera ejecución tarda más porque descarga imágenes y paquetes. El tiempo depende de la conexión y del equipo. El límite de 300 segundos configurado con `--wait-timeout` corresponde a la espera de disponibilidad de los servicios; no es una promesa de duración total de las descargas y la construcción. [Opciones de `docker compose up`](https://docs.docker.com/reference/cli/docker/compose/up/).

Abre **[http://localhost:8080](http://localhost:8080)**. Solo se publica el frontend, vinculado a `127.0.0.1`; la API y SQL Server se comunican en la red interna de Compose. No hay que abrir el puerto 1433 ni iniciar otra instancia de SQL Server en el equipo.

Para comprobar los contenedores:

```sh
docker compose ps
```

Los servicios `database`, `api` y `frontend` deben aparecer activos y saludables. La ruta [http://localhost:8080/health/ready](http://localhost:8080/health/ready) comprueba la disponibilidad de la aplicación y su base de datos; no garantiza que CoinGecko esté respondiendo en ese momento.

## 3. Configuración local y datos reales

**Deja que el script cree `.env`.** `.env.example` documenta los nombres de las variables, pero sus secretos están vacíos: no debe copiarse sin completar como configuración de ejecución. Si `.env` ya existe, el script lo conserva.

| Variable | Uso |
| --- | --- |
| `DASHBOARD_PORT` | Puerto local del frontend; valor inicial `8080`. |
| `SQL_PASSWORD` | Contraseña generada para SQL Server. Debe seguir correspondiendo a la base persistida. |
| `JWT_SECRET` | Secreto generado para firmar las sesiones de evaluación. |
| `API_KEY` | Configuración heredada de la plantilla; no otorga permisos de usuario del dashboard. |
| `COINGECKO_API_KEY` | Clave Demo opcional de CoinGecko; vacía por defecto. |

`.env` está excluido de Git. Guarda sus valores localmente y no los pegues en incidencias, capturas ni documentación. No borres ni regeneres `SQL_PASSWORD` mientras conservas la base de datos: cambiar una variable no cambia la contraseña dentro de un volumen SQL Server ya inicializado.

El backend consulta CoinGecko para Bitcoin, Ethereum y Solana en USD/EUR. Se puede intentar el acceso público sin clave, sujeto a disponibilidad y límites del proveedor. La aplicación también permite una clave **Demo de CoinGecko**, que se envía desde el backend mediante `x-cg-demo-api-key`; no debe usarse una clave Pro con esta configuración. [Autenticación oficial de la API Demo](https://docs.coingecko.com/demo/reference/authentication).

Para agregar la clave después del primer arranque, edita únicamente la línea `COINGECKO_API_KEY=` de `.env` y vuelve a ejecutar el script de inicio. Compose aplicará el cambio a la API. Antes del primer arranque, también puedes definir esa variable en la terminal; el script la copiará al `.env` que genera. Si `.env` ya existe, edita ese archivo en lugar de esperar que el script lo sobrescriba.

**La entrega no usa cotizaciones simuladas:** `compose.yaml` fija `Market__AllowDemoFallback=false`; el ajuste también está deshabilitado en las configuraciones de desarrollo de esta edición. Los datos simulados y mocks se reservan para pruebas automatizadas aisladas. Si CoinGecko falla, se conserva el último dato real disponible con la indicación **Datos desactualizados**; si no existe un dato válido previo, la consulta devuelve 503 y la interfaz muestra el error. La etiqueta **Sesión demo** se refiere a las identidades de evaluación, no al origen de las cotizaciones.

Los mensajes de conexión SignalR y la antigüedad del dato son estados diferentes. **En vivo** indica conexión con el servidor; la fecha del proveedor y el aviso de datos desactualizados indican la vigencia de la cotización. Las consultas periódicas no obligan al proveedor a producir un precio nuevo cada minuto.

### Recorrido breve de evaluación

1. Como **Viewer**, consulta mercado e histórico, cambia moneda, activos visibles y métricas.
2. Como **Trader A**, agrega, edita y elimina elementos en Seguimiento.
3. Cambia a **Trader B** y comprueba que su lista es independiente.
4. Como **Admin**, selecciona listas ajenas, modifica el umbral global y consulta Auditoría y Registros del sistema.
5. Abre una segunda ventana como Viewer y cambia el umbral desde Admin: debe llegar por SignalR.

## 4. Detener, reanudar y conservar datos

Desde la misma raíz del proyecto, para detener los servicios sin eliminar los contenedores:

```sh
docker compose stop
```

Para reanudarlos y esperar su disponibilidad:

```sh
docker compose up --detach --wait --wait-timeout 300
```

También puedes volver a ejecutar `start.ps1` o `start.sh`; conservarán `.env` y el volumen. Si cambiaste código, el script de inicio incluye la reconstrucción de imágenes.

Las listas, umbrales, auditoría y registros técnicos persisten en el volumen de Compose `sql-data`, cuyo nombre Docker será `aurea-dashboard-ai_sql-data` con la configuración predeterminada. `docker compose down` elimina los contenedores y la red del proyecto, pero conserva este volumen. **No uses `down -v` ni elimines el volumen si quieres conservar los datos.** [Persistencia con volúmenes](https://docs.docker.com/engine/storage/volumes/), [comportamiento de `compose down`](https://docs.docker.com/reference/cli/docker/compose/down/).

### Una segunda copia en el mismo equipo

Esta edición fija `name: aurea-dashboard-ai` en Compose y mantiene sus contenedores y volumen separados de la edición anterior `aurea-dashboard`. Las dos ediciones usan inicialmente el puerto 8080: si la anterior está abierta, el primer intento de esta edición puede crear `.env` y después fallar por puerto ocupado. Conserva ese archivo, cambia `DASHBOARD_PORT=8081` y vuelve a iniciar; no hay que borrar contenedores ni datos de la otra edición.

Dos carpetas de esta misma edición comparten el nombre `aurea-dashboard-ai` y, por tanto, pueden apuntar al mismo volumen aunque sus `.env` sean distintos. Para una tercera copia o una evaluación independiente, utiliza otro nombre **antes del primer arranque**:

```powershell
$env:COMPOSE_PROJECT_NAME = 'aurea-dashboard-ai-evaluation'
.\start.ps1
```

```sh
export COMPOSE_PROJECT_NAME=aurea-dashboard-ai-evaluation
sh start.sh
```

Mantén ese valor para los comandos posteriores. Puedes agregar `COMPOSE_PROJECT_NAME=aurea-dashboard-ai-evaluation` al `.env` generado para conservar la selección en otras terminales. Si las dos copias funcionan simultáneamente, cada una necesita un `DASHBOARD_PORT` distinto. Cambiar el nombre crea un entorno independiente; no migra la base de datos anterior. [Aislamiento y precedencia del nombre del proyecto](https://docs.docker.com/compose/how-tos/project-name/).

## 5. Resolver problemas de arranque

Empieza por consultar el estado y los registros:

```sh
docker compose ps
docker compose logs --tail 100 database api frontend
```

Antes de compartir registros, revisa su contenido y elimina cualquier dato sensible. No hace falta compartir `.env` ni la configuración expandida de Compose.

| Síntoma | Acción |
| --- | --- |
| Docker no responde | Abre Docker Desktop, espera a que el motor esté listo y confirma `docker info`. En Windows debe utilizar contenedores Linux. |
| `unknown flag: --wait` | Actualiza el complemento Compose. Instalar solo el ejecutable antiguo `docker-compose` no satisface el comando usado por los scripts. |
| Puerto 8080 ocupado | Conserva el `.env` generado, cambia `DASHBOARD_PORT=8081` y vuelve a ejecutar el script. Abre entonces `http://localhost:8081`. |
| SQL Server no está saludable | Revisa sus registros, memoria disponible y arquitectura x64. Comprueba que se conserva la contraseña del `.env` con el que se creó el volumen. |
| Fallo de autenticación SQL tras copiar el repositorio | Comprueba si otra copia usa el mismo nombre Compose. Recupera la configuración correspondiente al volumen o usa un proyecto independiente; no borres la base para ocultar el problema. |
| Espera agotada | Consulta los registros. Si los servicios siguen inicializándose, repite `docker compose up --detach --wait --wait-timeout 300`. Esto no borra la base. |
| 429, timeout o 503 de mercado | Revisa el mensaje de Mercado y, como Admin, los Registros del sistema. Espera el siguiente intento o usa Reintentar; verifica la disponibilidad de CoinGecko y la clave Demo opcional. No habilites la simulación para aprobar la evaluación. |

### PowerShell bloquea el script descargado

Revisa primero `start.ps1`. Si el bloqueo corresponde a la política de ejecución del archivo descargado, puedes lanzarlo con una excepción limitada a ese proceso:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\start.ps1
```

No es necesario cambiar la política global de Windows. En equipos administrados, las políticas de la organización pueden prevalecer. [Ámbitos y precedencia de las políticas de PowerShell](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_execution_policies).

### Error `Get "https://mcr.microsoft.com/v2/": EOF`

Este mensaje señala un corte al acceder al registro que distribuye SQL Server y las imágenes .NET. No demuestra un fallo de la aplicación ni que deba reinstalarse Docker. Reintenta primero la descarga oficial:

```sh
docker pull mcr.microsoft.com/mssql/server:2022-CU22-ubuntu-22.04
```

Si persiste, compara el acceso HTTPS por IPv4 e IPv6. En Windows usa `curl.exe` para evitar el alias de PowerShell; en Linux/macOS usa `curl`:

```powershell
curl.exe --ipv4 --fail --show-error --connect-timeout 10 --max-time 20 https://mcr.microsoft.com/v2/
curl.exe --ipv6 --fail --show-error --connect-timeout 10 --max-time 20 https://mcr.microsoft.com/v2/
```

Que IPv6 falle en una red sin conectividad IPv6 no basta para diagnosticar Docker. Revisa también el proxy configurado y, si es posible, compara en otra red. En la validación Windows de esta entrega, MCR respondió por IPv4 y cortó IPv6; una preferencia IPv4 temporal permitió descargar las imágenes, y después se verificó la restauración de la configuración anterior. Fue una medida específica de ese equipo, no un requisito de instalación.

No desactives globalmente IPv6, TLS, el firewall o el antivirus, ni añadas registros inseguros o IP fijas a `hosts` como solución automática. Cuando la descarga funcione, vuelve a ejecutar el script de inicio. Conserva `.env` y el volumen existente.

## 6. Herramientas opcionales para desarrollo y pruebas

Esta sección no es necesaria para recorrer el dashboard en Docker.

| Herramienta | Versión usada o contrato del repositorio | Uso |
| --- | --- | --- |
| SDK .NET | 10.0; `global.json` parte de 10.0.100 y permite bandas posteriores mediante `latestFeature` | Compilar y probar backend fuera de Docker. |
| Node.js | 24.21.0 en Docker; `package.json` acepta `>=24.15.0 <25` | Desarrollo Angular y Playwright. |
| Angular | 22.2.0, fijado en `package.json` | Se instala mediante `npm ci`; no requiere CLI global. |
| Python | 3.12 para el procedimiento de contexto | Graphify, SQLite FTS y recuperación RAG. |
| Graphify | `graphifyy==0.9.67` en `tools/requirements.txt` | Generación del grafo mediante el procedimiento único `tools/context.py`. |

Desde la raíz, las pruebas del backend:

```sh
dotnet test Test/Test.csproj
```

En `Frontend/financial-dashboard`, instala lo resuelto en el archivo de bloqueo y verifica el frontend:

```sh
npm ci
npm run typecheck
npm test
npm run build
```

Con el dashboard iniciado en Docker, instala Chromium y activa expresamente la comprobación de datos reales antes de ejecutar los cinco recorridos de navegador:

```powershell
npx playwright install chromium
$env:DASHBOARD_URL = 'http://localhost:8080'
$env:REQUIRE_LIVE_DATA = 'true'
npm run e2e
```

```sh
npx playwright install chromium
DASHBOARD_URL=http://localhost:8080 REQUIRE_LIVE_DATA=true npm run e2e
```

Los E2E crean y eliminan datos de evaluación y modifican temporalmente el umbral. Úsalos sobre una instalación destinada a pruebas. `tools/smoke.mjs` espera una lista inicial vacía para Trader A; no debe repetirse a ciegas sobre una base ya utilizada. No elimines datos personales para conseguir un resultado verde. La prueba de CoinGecko depende del proveedor externo y no confunde `source=coingecko` con una garantía de frescura.

### Graphify y RAG

Los scripts de contexto crean `.venv`, instalan la dependencia fijada y trabajan localmente. En Windows, `python` debe resolver a Python 3.12; en Unix, debe hacerlo `python3`. Se necesita acceso a paquetes Python durante la primera preparación. Esta edición genera localmente el grafo, su manifiesto, el índice SQLite y el checkpoint; no incluye un checkpoint antiguo ni un grafo presentado como vigente después de clonar.

Desde la raíz, el orden inicial es sincronizar, recuperar una consulta, registrar un checkpoint, probar el recuperador y comprobar su recuperación. Las pruebas de contexto necesitan que ya exista ese checkpoint inicial. Avanza únicamente cuando el comando anterior termine correctamente. El checkpoint del ejemplo registra la sincronización y la consulta, no anticipa el resultado de las pruebas; para otra tarea, sustituye su descripción y evidencia por las reales.

```powershell
.\tools\context.ps1 sync
.\tools\context.ps1 query 'AccessRules RequireOwner WatchlistWrite'
.\tools\context.ps1 checkpoint --task 'Preparar contexto local' --completed 'Grafo e indice regenerados; consulta ejecutada' --next 'Ejecutar pruebas de recuperacion' --evidence 'Consulta de autorizacion ejecutada sin error'
& .\.venv\Scripts\python.exe tools/test_context.py
.\tools\context.ps1 status
.\tools\context.ps1 resume
```

```sh
sh tools/context.sh sync
sh tools/context.sh query 'AccessRules RequireOwner WatchlistWrite'
sh tools/context.sh checkpoint --task 'Preparar contexto local' --completed 'Grafo e indice regenerados; consulta ejecutada' --next 'Ejecutar pruebas de recuperacion' --evidence 'Consulta de autorizacion ejecutada sin error'
.venv/bin/python tools/test_context.py
sh tools/context.sh status
sh tools/context.sh resume
```

`sync` genera `docs/architecture/graphify-out/graph.json`, su manifiesto y `.llmops/cache/context.sqlite`. `checkpoint` registra el estado en `.llmops/state/checkpoint.json`; solo después existe un punto local que `resume` puede comprobar. El workflow de CI puede conservar el grafo y el manifiesto como artefactos de su propia ejecución.

La recuperación comprueba hashes y rechaza fuentes desactualizadas. Después de editar documentación o código, ejecuta las pruebas pertinentes, vuelve a realizar `sync` y registra un nuevo checkpoint con los resultados y el siguiente paso. No actualices un checkpoint con un éxito supuesto. Estas herramientas apoyan el desarrollo; el dashboard no depende de otro servicio de inferencia ni necesita una clave de un proveedor LLM para ejecutarse.

## 7. Alcance de la validación disponible

La base de esta entrega se comprobó el **25 de septiembre de 2026** en Windows con Docker Linux 29.8.0 y Compose 5.5.1: arranque de los tres servicios, permisos HTTP, conservación de un elemento de seguimiento y de un evento técnico después de reiniciar API/SQL Server, acceso real a CoinGecko y **cinco E2E Chromium aprobados**. La ejecución completa registró nueve etapas aprobadas.

La muestra de CoinGecko validada contenía tres activos y 24 puntos de histórico Bitcoin. Se identificó como real y también como desactualizada (`isStale=true`); por tanto, acredita integración con el proveedor y tratamiento explícito de antigüedad, no una garantía de datos recién emitidos. El éxito de esa instalación tampoco garantiza la disponibilidad futura de servicios externos o la compatibilidad de equipos ARM.

Consulta [VALIDATION.md](VALIDATION.md) para el detalle de evidencia y [../AI_PROMPTS.md](../AI_PROMPTS.md) para los prompts y revisiones utilizados durante el desarrollo.
