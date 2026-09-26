# Estándar de arquitectura de Áurea

Versión: 0.1. Alcance: backend .NET y organización del frontend de este repositorio.

Este documento conserva las convenciones aplicables de la plantilla y las decisiones de [ADR-001](../../docs/decisions/ADR-001-dashboard.md). La implementación y sus límites se describen en [ARCHITECTURE.md](../../docs/ARCHITECTURE.md). Una regla de revisión no implica que exista un servicio de agentes o una aprobación automática desplegada.

## Capas y dependencias

Se conservan Core/Domain, Core/Application, Infraestructure/Adapters, Infraestructure/Persistence, Infraestructure/Infraestructure y Api/Api. Las pruebas residen en Test.

| Capa | Dependencias permitidas dentro del proyecto |
|---|---|
| Domain | Ninguna otra capa |
| Application | Domain |
| Adapters | Application, Domain, Infraestructure |
| Persistence | Domain, Infraestructure |
| Infraestructure, helper de DI | Domain |
| Api | Application, Domain, Adapters, Persistence |

Domain y Application no deben depender de EF Core, SignalR, controladores ni implementaciones de infraestructura. Los contratos que cruzan esas fronteras pertenecen a Domain. Una interfaz nueva debe representar una frontera real; no se crean abstracciones solamente para envolver otra clase.

## Casos de uso y contratos

Las operaciones de Application se agrupan en UseCases/<PluralFeature>/<UseCaseName> con Request, Validator, Command o Query y Response. Command/Query implementa su IRequestHandler; no se introduce una clase Handler separada ni una carpeta genérica Services para ocultar lógica de aplicación.

La validación de entradas utiliza FluentValidation. Las invariantes puras permanecen en el dominio. Los casos de uso devuelven ResponseBase<T>; los controladores delegan en el mediador y convierten la respuesta con ToResult. HTTP y SignalR utilizan DTOs, sin exponer entidades EF ni respuestas crudas del proveedor.

Los repositorios concretos heredan de RepositoryGeneric<T>. Se añade un contrato específico solo cuando las operaciones genéricas no cubren la necesidad. Para el mapeo convencional de entidades a DTOs se conserva IMapperAdapter y la convención de nombres; no se duplica un perfil por entidad cuando la convención lo resuelve.

## Composición y transacciones

DependencyInjectionHelper registra los pares que resuelve por convención. Los ciclos de vida de Typed HttpClient, caché, identidad por solicitud y cola de eventos requieren composición explícita; estas excepciones están justificadas en ADR-001.

Las mutaciones de seguimiento o umbrales y su AuditEvent se guardan en la misma transacción. SystemEvents tiene un escritor asíncrono separado y una cola acotada: no se presenta como auditoría transaccional ni como cola durable.

## Frontend por funcionalidad

Angular agrupa Mercado y Seguimiento en domain/application/infrastructure/presentation. Los componentes consumen fachadas; domain contiene modelos y reglas independientes de Angular. Las fachadas coordinan Signals, solicitudes cancelables y adaptadores. La administración de listas reutiliza Seguimiento con selección de propietario.

## Contexto verificable y revisión

El único generador es tools/context.py: Graphify extrae relaciones y el mismo procedimiento construye el índice SQLite FTS. No se introduce otro grafo paralelo. Las fuentes recuperadas incluyen ruta, líneas y hash; las relaciones extraídas no se presentan como inferencias de un LLM. Los artefactos se regeneran en cada checkout y en CI.

Antes de editar, recuperar contexto vigente; después, ejecutar las pruebas pertinentes, sincronizar y registrar un checkpoint con evidencia real. Una revisión de arquitectura identifica archivo, regla, efecto y comprobación propuesta. Las comprobaciones ejecutadas se distinguen de recomendaciones o pendientes.

La ejecución automática real se define en [verify.yml](../../.github/workflows/verify.yml). Las instrucciones de verificación están en [TESTING.md](../../docs/TESTING.md), junto con [seguridad](security.md) y [estilo](clean-code.md).
