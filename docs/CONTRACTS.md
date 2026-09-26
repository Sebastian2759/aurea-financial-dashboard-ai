# Contratos públicos

Prefijo `/api/v1`. HTTP devuelve `ResponseBase<T>` con `isSuccess`, `statusCode`, `message`, `data` y `errors`. Errores de validación/excepción utilizan ProblemDetails; la interfaz admite ambos. Sin JWT válido: 401; permiso o propietario no autorizado: 403; recurso inexistente bajo propietario autorizado: 404; duplicado: 409.

| Método y ruta | Entrada | data | Permiso |
|---|---|---|---|
| POST `/auth/demo-sessions` | `{userId}` | accessToken, expiresAt, userId, name, role, permissions | Solo perfil demo |
| GET `/auth/me` | — | identidad y permisos | JWT |
| GET `/markets?currency=usd` | usd/eur | `{snapshot}` | market.read |
| GET `/markets/{coinId}/history?currency=usd&days=1` | bitcoin/ethereum/solana, 1/7 días | `{history}` | market.read |
| GET `/watchlists/owners` | — | `{owners}` | watchlist.manage-all |
| GET `/watchlists/{ownerId}/items` | ownerId o me | `{items}` | watchlist.read + propiedad |
| POST `/watchlists/{ownerId}/items` | `{coinId,note?}` | `{item}` | watchlist.write + propiedad |
| PUT `/watchlists/{ownerId}/items/{id}` | `{coinId,note?}` | `{item}` | watchlist.write + propiedad |
| DELETE `/watchlists/{ownerId}/items/{id}` | — | `{removed}` | watchlist.write + propiedad |
| GET `/dashboard/thresholds` | — | `{volatilityThreshold}` | thresholds.read |
| PUT `/dashboard/thresholds` | `{volatilityThreshold}` entre .01 y 100 | `{volatilityThreshold}` | thresholds.write |
| GET `/audit-events` | page=1,pageSize=20,actorId?,action? | items,total,page,pageSize | audit.read |

## SignalR

Hub JWT `/hubs/market`. El token se admite en query string exclusivamente en esta ruta para transportes de navegador; nginx no registra su URL. La conexión cierra al expirar el token.

- Cliente invoca `Subscribe(currency)` con usd/eur; se reemplaza su grupo anterior.
- Servidor envía `MarketSnapshotUpdated(snapshot)`: currency, quotes, fetchedAt, source, isStale, message. Cada quote contiene precio/métricas nullable y updatedAt de origen.
- Servidor envía `ThresholdsUpdated({volatilityThreshold})` después del commit.
- La reconexión restablece la suscripción y recupera cotizaciones/umbrales actuales. Cambiar de usuario crea una conexión nueva y cancela solicitudes previas.

Acciones de auditoría: `watchlist.add`, `watchlist.update`, `watchlist.remove`, `thresholds.update`. Fechas UTC con zona explícita. Las listas y auditoría se recuperan por HTTP autorizado, nunca mediante grupos públicos del hub.

## Límite deliberado

La notificación de umbral sucede después de persistir. Un fallo del transporte no revierte el cambio: queda registrado en log y la recarga/reconexión recupera el valor. La demo es de una instancia de API; escalar a varias requiere un backplane de SignalR y caché distribuida.
# Registros técnicos

`GET /api/v1/system-events?page=1&pageSize=20&level=Warning&component=Market&from=2026-01-01T00:00:00Z&to=2027-01-01T00:00:00Z`

Requiere JWT con `system-logs.read` (solo Admin). Devuelve `ResponseBase<{items,total,page,pageSize}>`; cada elemento contiene id, code, level, component, message, currency, coinId y createdAtUtc. Los filtros son opcionales. Niveles: Information/Warning/Error; orígenes: Api/Market; pageSize entre 1 y 100; fechas inclusivas. Sin JWT: 401; Viewer/Trader: 403; filtro inválido: 400. No admite escritura desde el navegador.
