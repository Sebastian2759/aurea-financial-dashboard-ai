# Contexto del proyecto

Áurea implementa un dashboard financiero con RBAC sobre una plantilla .NET 10. El alcance y los criterios están en REQUIREMENTS.md; las decisiones en decisions/ADR-001-dashboard.md. Este repositorio público es Sebastian2759/aurea-financial-dashboard-ai.

El evaluador requiere Docker con contenedores Linux x64 y el plugin Compose con --wait. start.ps1 o start.sh genera configuración demo y levanta Angular, API y SQL Server. La simulación financiera está deshabilitada. Graphify/RAG son herramientas de desarrollo, no dependencias del dashboard.

Después de clonar, generar el grafo/índice con tools/context.py sync, recuperar fuentes con query y crear un checkpoint con evidencia real antes de usar resume o ejecutar la suite de contexto. Los launchers tools/context.ps1 y tools/context.sh preparan Graphify 0.9.67 en .venv. AI_CONTEXT.md describe el procedimiento.

Cada cambio requiere fuentes vigentes, pruebas pertinentes y una regeneración del contexto. VALIDATION.md distingue ejecución local, CI, datos reales y datos frescos. Los resultados de esta revisión pública se consultan en Actions, con artefactos de aplicación y contexto.

El grafo, índice, resultados locales y checkpoints de una máquina no se incluyen como estado vigente de otra. La CI reconstruye y verifica su propio contexto. Las fechas de resultados y migraciones conservan su significado real; el historial público empieza con la publicación del 25/09/2026, America/Bogota.
