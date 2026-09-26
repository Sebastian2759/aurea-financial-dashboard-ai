# Criterios de entrega

La matriz de [REQUIREMENTS.md](REQUIREMENTS.md) vincula cada requisito con su implementación y comprobaciones. La ejecución pública de [CI](https://github.com/Sebastian2759/aurea-financial-dashboard-ai/actions/workflows/verify.yml) permite contrastar el resultado del commit evaluado.

| Criterio | Implementación y comprobación |
|---|---|
| Mercado real | CoinGecko, BTC/ETH/SOL, USD/EUR, métricas y gráfico; verificación externa en API y navegador |
| Roles y propiedad | JWT emitido por servidor; 401/403, aislamiento Trader A/B, operaciones Admin y rechazo de API Key |
| Seguimiento | CRUD, unicidad por propietario, notas, SQL Server y recuperación después de reiniciar |
| Auditoría | Actor y propietario; transacción junto a cambios de negocio |
| Registros técnicos | Vista Admin, filtros y paginación; cola acotada y catálogo seguro; persistencia del evento escrito |
| Tiempo real | SignalR, suscripción por moneda, reconexión y umbral difundido entre sesiones |
| Resiliencia | Nulos preservados, reintentos limitados, caché, fecha de origen y estado desactualizado; sin fallback simulado en ejecución |
| Instalación | Un comando después de clonar/extraer; secretos generados, migraciones y healthchecks automáticos |
| IA verificable | Prompts versionados, errores reales documentados, Graphify, FTS5, hashes y recuperación de checkpoint |
| Acceso del entrevistador | Repositorio público, documentación en español y CI consultable |

Los resultados observados y sus límites figuran en [VALIDATION.md](VALIDATION.md). Los casos de proveedor controlados y las pruebas de corrupción del índice se identifican como tests, no como datos reales ni errores históricos de IA.

La cotización USD/EUR expresa el precio del activo en esa moneda; no implementa un servicio FX independiente. Graphify y RAG amplían el proceso de desarrollo acordado y no añaden requisitos de instalación al evaluador.
