# Seguridad

Los endpoints del dashboard y el hub usan exclusivamente JWT y permisos emitidos por el servidor. La API Key de la plantilla conserva su esquema, pero no satisface ninguna política funcional del dashboard. JWT ausente, alterado o expirado produce 401; identidad válida sin permiso produce 403.

La sesión demo acepta cuatro IDs conocidos. No acepta roles ni permisos del cliente. Se habilita solo en Demo/Development/Testing; la excepción está justificada en ADR-001. Para producción hay que sustituir el emisor demo por un proveedor de identidad.

Application verifica identidad y propiedad además del middleware. Trader solo opera su lista; Admin puede seleccionar propietario. Un ID de elemento ajeno no evita ese control. Los cambios y la auditoría se guardan juntos; el actor siempre es la identidad autenticada.

JWT y claves de API están vacíos en configuración versionada. Los scripts generan secretos aleatorios en .env, excluido de Git y del corpus. Compose publica únicamente la web sobre 127.0.0.1. El hub no registra la URL con access_token en Nginx. No se registran contraseñas ni tokens en la aplicación.

Cambiar usuario limpia permisos y estado, cancela HTTP y detiene la conexión previa. El token se guarda en memoria y expira; la nueva identidad abre otra conexión. Los errores inesperados devuelven ProblemDetails genérico.

Las pruebas de Test/Integration/RbacHttpTests.cs atraviesan el middleware real. La auditoría de dependencias se ejecuta en verify.yml con vulnerable/deprecated/outdated; NU1901–NU1904 siguen siendo errores de restauración. Consultar VALIDATION.md para el resultado observado.

SystemEvents exige system-logs.read, concedido únicamente a Admin. El catálogo no acepta mensajes ni excepciones arbitrarios y restringe activo/moneda a valores conocidos. PermissionMatrixTests comprueba todas las rutas funcionales sin JWT y con API Key; SystemEventsTests comprueba el rechazo de Viewer/Trader y que valores no permitidos no se almacenen.
