# Revisión contextual — v1

Entrada: paquete de `tools/context.py query`, diff o unidad de trabajo, evidencia de pruebas.

1. Verifica hashes del corpus, grafo e índice. Si falla vigencia, detente y regenera.
2. Relaciona el cambio con REQ-01…REQ-13 y sus criterios de aceptación.
3. Seguridad: JWT, permisos en middleware y casos de uso, propietario tomado de identidad autorizada, cancelación de la sesión anterior, secretos fuera del corpus.
4. Arquitectura: dependencias internas, CQRS, DTO propios y fachadas por funcionalidad.
5. Pruebas: HTTP atraviesa middleware; errores de proveedor, nulos, reconexión, persistencia, RAG obsoleto. Distingue una comprobación ejecutada de una prueba solo escrita.
6. Registra hallazgos verificables (archivo/línea, impacto, evidencia y corrección). No atribuyas a otro agente una revisión propia ni inventes consumo de tokens o duración de inferencia.
7. Ejecuta pruebas pertinentes, regenera grafo/índice y guarda checkpoint con pendiente concreto.

Salida: JSON según `review.schema.json`. Una limitación del entorno permanece abierta hasta tener evidencia verificable.
