import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs';
import { API, ApiResponse } from '../../../core/http/api';
export interface AuditEvent {
  id: string;
  actorId: string;
  ownerId: string | null;
  action: string;
  resource: string;
  before: string | null;
  after: string | null;
  createdAtUtc: string;
}
export interface AuditPage {
  items: AuditEvent[];
  total: number;
  page: number;
  pageSize: number;
}
@Injectable({ providedIn: 'root' })
export class AuditApi {
  private http = inject(HttpClient);
  list(page: number, actor: string, action: string) {
    return this.http
      .get<ApiResponse<AuditPage>>(`${API}/audit-events`, {
        params: {
          page,
          pageSize: 10,
          ...(actor ? { actorId: actor } : {}),
          ...(action ? { action } : {}),
        },
      })
      .pipe(map((r) => r.data));
  }
}
