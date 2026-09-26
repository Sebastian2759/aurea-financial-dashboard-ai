# Estándar de pruebas de Áurea

Versión: 0.1. Alcance: backend, frontend, integración y recuperación de contexto.

Las instrucciones reproducibles están en [TESTING.md](../../docs/TESTING.md). El pipeline efectivo está en [verify.yml](../../.github/workflows/verify.yml). Este documento define qué revisar; no atribuye ejecuciones o porcentajes sin evidencia.

## Herramientas y ubicación

El backend utiliza xunit.v3, FluentAssertions, Moq y coverlet.collector según Test/Test.csproj. Las pruebas residen en Test, agrupadas por funcionalidad, integración o arquitectura. Los archivos usan el sufijo Tests y describen la clase o comportamiento comprobado.

El frontend utiliza Vitest para pruebas unitarias y Playwright para recorridos de navegador. Las pruebas de contexto están en tools/test_context.py y utilizan unittest. No se introduce otro framework equivalente sin una necesidad justificada.

## Cobertura por cambio

Todo caso de uso nuevo debe comprobar su respuesta y las entradas válidas e inválidas que cambian el resultado. Cuando intervengan permisos, mapeo o persistencia, también debe verificar esas fronteras. El recorrido de controlador comprueba el contrato HTTP real; las pruebas de integración pueden cubrir varios de estos aspectos sin duplicar tests que solo repitan la implementación.

Para cambios en RBAC, las pruebas deben atravesar el middleware y comprobar ausencia o invalidez del JWT, permisos insuficientes, API Key sin privilegios y propiedad de recursos. Una prueba de interfaz que oculta un botón no sustituye la comprobación del servidor.

Las operaciones de seguimiento y umbrales comprueban sus eventos de auditoría. Los cambios de sesión verifican limpieza de información restringida. Los valores nulos, la volatilidad, la fecha/procedencia del mercado y la recuperación tras fallos merecen casos específicos cuando resulten afectados.

## Persistencia y servicios externos

Las pruebas unitarias sustituyen dependencias en fronteras mediante contratos; no simulan DbContext para afirmar que EF Core persiste correctamente. Las pruebas HTTP pueden utilizar SQLite relacional aislado, identificándolo como tal. La aceptación de la entrega también requiere SQL Server en Docker y comprobar que los cambios sobreviven al reinicio.

Las pruebas aisladas de mercado pueden emplear respuestas controladas. Deben distinguirse de la verificación contra CoinGecko real: el arranque de entrega no permite datos simulados y conserva la procedencia y antigüedad del último dato válido.

## Graphify, RAG y continuidad

Antes de ejecutar las pruebas de contexto en un checkout nuevo: instalar las dependencias de herramientas, sincronizar el grafo e índice, ejecutar una consulta y crear un checkpoint con la evidencia realmente obtenida. La secuencia completa está en TESTING.md.

Las pruebas de contexto verifican recuperación con procedencia, rechazo de fuentes o índice desactualizados, integridad del índice, persistencia de checkpoint tras interrupción y privacidad de rutas exportadas. Las fuentes deben recuperarse desde el estado actual del repositorio, no desde un checkpoint histórico copiado de otra máquina.

## Resultado de la revisión

Ejecutar las comprobaciones pertinentes al alcance y registrar comando, resultado y limitaciones. No afirmar un porcentaje de cobertura sin medirlo ni confundir número de tests con calidad de cobertura. No existe un umbral porcentual automático de aceptación configurado para esta entrega.

Un fallo de compilación, autorización, aislamiento o persistencia impide declarar terminado el requisito afectado. Si una comprobación no pudo ejecutarse, registrarla como pendiente y explicar la condición necesaria para correrla. [REQUIREMENTS.md](../../docs/REQUIREMENTS.md) relaciona criterios con código y evidencia; [VALIDATION.md](../../docs/VALIDATION.md) identifica el estado verificable.
