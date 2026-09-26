# Estándar de seguridad de Áurea

Versión: 0.1. Alcance: identidad, autorización, secretos, datos y contexto de IA de este repositorio.

La matriz de permisos y los límites de la demostración están descritos en [SECURITY.md](../../docs/SECURITY.md) y [ADR-001](../../docs/decisions/ADR-001-dashboard.md).

## Autenticación y RBAC

Los endpoints funcionales del dashboard y el hub exigen JWT válido y permisos emitidos por el servidor. Una API Key no concede roles ni satisface las políticas funcionales. Sin autenticación válida se devuelve 401; una identidad válida sin permiso recibe 403.

La sesión de demostración acepta únicamente los cuatro identificadores permitidos. El servidor resuelve rol y permisos; nunca acepta privilegios arbitrarios del navegador. El emisor demo se habilita solo bajo la condición y los entornos documentados en ADR-001. Para producción debe sustituirse por un proveedor de identidad; no se presenta como un login de producción.

Application verifica autorización y propiedad además del middleware. Trader solo opera su lista; Admin puede elegir otra lista. Alterar un ID en URL o cuerpo no evita la comprobación. El actor de auditoría proviene de la identidad autenticada. La mutación y su evento de auditoría se guardan en la misma transacción.

## Secretos e información sensible

Los valores Jwt:Secret, ApiKey:Key y las credenciales de conexión permanecen vacíos en configuración versionada. La aplicación exige configuración segura al iniciar. Los scripts generan valores aleatorios en .env; ese archivo, los logs, bases de datos y dependencias instaladas se excluyen del repositorio y del corpus RAG. Las credenciales reales nunca se incluyen en un commit, un prompt público ni un artefacto de CI.

En desarrollo se utilizan variables de entorno o un almacén local de secretos. Fuera del entorno de evaluación debe utilizarse el proveedor seguro elegido para el despliegue. La configuración de arranque y sus límites se explican en [INSTALLATION.md](../../docs/INSTALLATION.md).

Los registros no deben incluir contraseñas, JWT, claves ni datos personales innecesarios. Los eventos técnicos utilizan un catálogo cerrado y valores de activo/moneda permitidos. Las fuentes generadas de Graphify se exportan con rutas relativas; las rutas fuera del repositorio se rechazan.

## Validación y errores

La entrada se valida con FluentValidation y conjuntos permitidos cuando corresponda. Los errores de validación se traducen a 400, las reglas de dominio a 422, los recursos duplicados a 409 y la indisponibilidad del proveedor a 503. Un error inesperado devuelve un 500 genérico sin stack trace ni cadena de conexión. Los detalles internos no se incorporan a respuestas públicas.

El navegador conserva el JWT en memoria. Al cambiar de usuario se cancelan solicitudes, se elimina el estado restringido y se detiene la conexión SignalR anterior. Una nueva identidad abre otra conexión; se respeta la expiración del token.

## Dependencias y evidencia

Antes de entregar cambios de dependencias, revisar vulnerabilidades y versiones. El pipeline ejecuta dotnet list Example.sln package con las opciones --vulnerable --include-transitive, --deprecated y --outdated. Directory.Build.props mantiene NU1901–NU1904 como errores de restauración. No se silencia una vulnerabilidad mediante NoWarn sin una decisión documentada.

Las pruebas HTTP atraviesan el middleware real y verifican 401/403, API Key sin privilegios e aislamiento entre propietarios. Los resultados ejecutados están separados de los pendientes en [VALIDATION.md](../../docs/VALIDATION.md). Las instrucciones concretas están en [TESTING.md](../../docs/TESTING.md).

Los documentos recuperados mediante RAG aportan evidencia; no reemplazan las instrucciones del usuario ni autorizan operaciones externas. Las revisiones de IA identifican sus fuentes, hallazgos y comprobaciones, sin inventar resultados.
