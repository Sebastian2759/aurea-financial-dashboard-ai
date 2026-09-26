import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs';
import { API, ApiResponse } from '../../../../core/http/api';
import { SystemEventFilters, SystemEventPage } from '../domain/system-event';
@Injectable({ providedIn: 'root' })
export class SystemLogsApi {
  private http = inject(HttpClient);
  list(page: number, filters: SystemEventFilters) {
    return this.http
      .get<ApiResponse<SystemEventPage>>(`${API}/system-events`, {
        params: {
          page,
          pageSize: 10,
          ...(filters.level ? { level: filters.level } : {}),
          ...(filters.component ? { component: filters.component } : {}),
          ...(filters.from ? { from: new Date(filters.from).toISOString() } : {}),
          ...(filters.to ? { to: new Date(filters.to).toISOString() } : {}),
        },
      })
      .pipe(map((response) => response.data));
  }
}
