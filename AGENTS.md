# Desarrollo del dashboard

El enunciado y las decisiones están en docs/REQUIREMENTS.md y docs/decisions/ADR-001-dashboard.md.
Antes de editar funcionalidades ejecutar `python tools/context.py query "tarea"`.
Si el índice está desactualizado, ejecutar `python tools/context.py sync` antes de continuar.
Después de una unidad de trabajo: pruebas pertinentes, sync y checkpoint con evidencia real.
Para retomar: `python tools/context.py resume`; si hay fuentes modificadas, revisar sus diferencias y recuperar contexto vigente antes de actualizar el checkpoint.
No inventar resultados de pruebas, fuentes, métricas ni errores de IA.
Conservar Clean Architecture, CQRS y las convenciones de la plantilla.
Los estándares aplicables están en .llmops/standards/. Graphify tiene un único generador: tools/context.py.
Los secretos, logs de ejecución y dependencias instaladas no forman parte del corpus.
La interfaz y documentación se escriben en español; los identificadores, en inglés.
