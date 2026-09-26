# Arquitectura

Se conserva Clean Architecture y CQRS de la plantilla. Domain contiene entidades, contratos y reglas puras; Application ejecuta casos de uso y autorización por propietario; Persistence implementa repositorios EF Core; Adapters obtiene y almacena temporalmente datos de CoinGecko. API compone servicios, autentica JWT y expone controladores y SignalR. Domain y Application no referencian EF Core ni SignalR.

Angular organiza Mercado y Seguimiento por funcionalidad, con domain/application/infrastructure/presentation. Las páginas consumen fachadas, que coordinan Signals, clientes HTTP y operaciones cancelables. Administración reutiliza Seguimiento con un selector de propietario. El token vive en memoria; cada cambio de identidad cancela solicitudes y descarta estado privado.

Los cambios de lista y configuración incluyen un AuditEvent dentro del mismo SaveChanges transaccional. SQL Server conserva los datos en un volumen. Los mensajes públicos del hub llevan moneda, procedencia y fecha; la conexión exige JWT y se cierra al expirar.

El único generador de arquitectura es tools/context.py sync con Graphify 0.9.67. Su resultado canónico es architecture/graphify-out/graph.json. SQLite FTS recupera fragmentos verificables y expande relaciones de ese grafo. Los nodos documentales se identifican como extraídos; no se atribuyen inferencias a otro LLM.

Consultar CONTRACTS.md, decisions/ADR-001-dashboard.md y REQUIREMENTS.md para contratos, excepciones y aceptación.

SystemEvents incorpora registros técnicos persistidos. El puerto de dominio recibe tipos de evento; API compone una cola singleton y un escritor con scopes EF. Application consulta mediante CQRS y exige system-logs.read. Angular mantiene domain/application/infrastructure/presentation para esta funcionalidad. La escritura técnica es asíncrona y acotada; AuditEvents mantiene la transacción de negocio.
