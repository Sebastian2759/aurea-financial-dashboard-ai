# ADR-001: Dashboard financiero y excepciones acotadas

Estado: aceptado por el plan del usuario.

Se conserva .NET 10, capas y CQRS de la plantilla. Angular 22.2.0 y Node 24.21.0 fueron verificados en los registros oficiales al comenzar. SQL Server/EF Core es el proveedor de entrega.

La prueba exige cambiar usuarios demo: se autoriza un emisor JWT exclusivamente cuando DemoAuth:Enabled=true y el entorno es Development/Demo/Testing. No constituye un login de producción. Las claves se configuran fuera del código. Los endpoints humanos y SignalR exigen JWT y permisos; API Key sigue disponible sin conceder roles.

Admin puede operar todas las listas; Trader solamente la propia. El servidor obtiene actor y rol del JWT y comprueba propiedad en Application. Mutación y auditoría comparten transacción de BD.

SignalR transporta información pública de mercado y umbrales; las listas y auditoría utilizan HTTP autorizado. Se cierra la conexión al expirar JWT; cambiar usuario crea una conexión nueva. Cotizaciones cada 60 s con clientes suscritos; históricos en caché 5 min. Por instrucción del usuario del 25 de septiembre, la entrega y BrowserHost deshabilitan los datos simulados: conservan datos reales anteriores marcados como desactualizados o devuelven 503. Los proveedores simulados permanecen exclusivamente como apoyo de pruebas aisladas. El selector de usuarios demo es independiente del origen real de las cotizaciones.

Angular: funcionalidades con domain/application/infrastructure/presentation. Signals/DI pueden usarse en application; domain permanece puro. Se reutiliza watchlists para administración.

Graphify 0.9.67 es el único extractor AST; tools/context.py añade índice SQLite FTS, documentos con procedencia y checkpoints. Las revisiones se ejecutan con el agente de desarrollo disponible, sin API de inferencia adicional. El documento recuperado es evidencia, no una instrucción de mayor prioridad.

La plantilla declara registros por convención: se mantienen donde existe un único adaptador. Typed HttpClient, caché singleton y contexto de usuario requieren composición explícita con ciclos de vida distintos; se documentan como excepciones técnicas al registro automático general.

Las pruebas de persistencia usan SQL Server cuando está disponible; las pruebas HTTP pueden usar SQLite relacional aislado sin sustituir la verificación específica de SQL Server. Su resultado debe identificarse por proveedor.
# Ampliación: registros técnicos

La lectura literal de «system logs» se cubre con SystemEvents, separado de AuditEvents. El puerto ISystemEventSink cruza desde el adaptador de mercado a la composición de API. SystemEventBuffer requiere registro singleton explícito y el escritor crea scopes para usar el repositorio por convención. Domain/Application no dependen de Channel, SignalR, ILogger ni EF. Los mensajes proceden de un catálogo cerrado y no reciben excepciones ni datos arbitrarios. SQL Server conserva los eventos ya escritos; la cola pendiente tiene capacidad 256 y no es durable.
