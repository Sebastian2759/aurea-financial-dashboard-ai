# Convenciones de código de Áurea

Versión: 0.1. Alcance: código mantenido en este repositorio.

Estas reglas complementan [arquitectura](architecture.md). La documentación y la interfaz usan español; los identificadores de código usan inglés.

## Nombres y ubicación

Las entidades se ubican en Core/Domain/Entities, con nombre singular y sufijo Entity, y heredan de EntityBase. No se duplican sus propiedades comunes de auditoría. Las invariantes puras pueden vivir en la entidad; la validación de entrada corresponde al Validator del caso de uso.

Las configuraciones de persistencia se ubican en Infraestructure/Persistence/Configuration y utilizan el sufijo Configuration. Se conserva una configuración por entidad; AppDbContext se reserva para composición y aspectos transversales. Los nombres de tablas son plurales.

Los DTOs pertenecen a Application, utilizan el sufijo Dto y mantienen el nombre base de la entidad cuando se usa mapeo por convención. Los casos de uso no exponen entidades directamente. Cada carpeta de caso de uso conserva Request, Validator, Command o Query y Response con el mismo nombre base.

En Angular, cada componente mantiene juntos su código, plantilla, estilos y pruebas cuando correspondan. Se agrupa por funcionalidad y se conservan los límites de domain/application/infrastructure/presentation descritos en [ARCHITECTURE.md](../../docs/ARCHITECTURE.md).

## Diseño y legibilidad

- Usar nombres que expresen intención y evitar abreviaturas ambiguas.
- Preferir clases sealed cuando no estén diseñadas para herencia y respuestas inmutables cuando corresponda.
- Mantener los métodos enfocados; dividir lógica cuando ayude a comprender reglas o responsabilidades, sin imponer un umbral de complejidad que no se mida.
- Explicar decisiones en comentarios cuando el motivo no sea evidente; evitar repetir lo que hace una línea.
- No introducir dependencias o interfaces sin una necesidad concreta.
- Mantener juntos los casos de carga, vacío, error y datos desactualizados que afectan al usuario; no convertir valores financieros nulos en cero.

## Formato y revisión

Conservar el estilo existente y los archivos de configuración de cada proyecto. Para revisar formato .NET puede utilizarse dotnet format Example.sln --verify-no-changes después de restaurar dependencias. No afirmar que este comando se ejecutó si no hay evidencia; el conjunto automático vigente está en [verify.yml](../../.github/workflows/verify.yml).

Los cambios deben ser acotados y explicar el comportamiento, su propósito y cómo se comprobó. Las reglas de [seguridad](security.md), [pruebas](testing.md) y las decisiones de [ADR-001](../../docs/decisions/ADR-001-dashboard.md) completan la revisión.
