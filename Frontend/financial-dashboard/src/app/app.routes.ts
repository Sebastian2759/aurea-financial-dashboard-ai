import { Routes } from '@angular/router';
import { permissionGuard } from './core/authorization/permission.guard';
export const routes: Routes = [
  {
    path: 'market',
    loadComponent: () =>
      import('./features/market/presentation/market-page/market-page').then((m) => m.MarketPage),
  },
  {
    path: 'watchlists',
    canActivate: [permissionGuard],
    data: { permission: 'watchlist.read' },
    loadComponent: () =>
      import('./features/watchlists/presentation/watchlist-page/watchlist-page').then(
        (m) => m.WatchlistPage,
      ),
  },
  {
    path: 'administration/thresholds',
    canActivate: [permissionGuard],
    data: { permission: 'thresholds.write' },
    loadComponent: () =>
      import('./features/administration/thresholds/thresholds-page').then((m) => m.ThresholdsPage),
  },
  {
    path: 'administration/audit',
    canActivate: [permissionGuard],
    data: { permission: 'audit.read' },
    loadComponent: () =>
      import('./features/administration/audit/audit-page').then((m) => m.AuditPage),
  },
  {
    path: 'administration/system-logs',
    canActivate: [permissionGuard],
    data: { permission: 'system-logs.read' },
    loadComponent: () =>
      import('./features/administration/system-logs/presentation/system-logs-page').then(
        (m) => m.SystemLogsPage,
      ),
  },
  { path: '**', redirectTo: 'market' },
];
