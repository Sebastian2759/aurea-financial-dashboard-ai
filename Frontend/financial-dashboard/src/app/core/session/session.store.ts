import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Subject, firstValueFrom } from 'rxjs';
import { API, ApiResponse, errorMessage } from '../http/api';
export interface Session {
  accessToken: string;
  expiresAt: string;
  userId: string;
  name: string;
  role: string;
  permissions: string[];
}
export const DEMO_USERS = [
  { id: 'viewer', name: 'Viewer', description: 'Solo consulta' },
  { id: 'trader-a', name: 'Trader A', description: 'Seguimiento personal' },
  { id: 'trader-b', name: 'Trader B', description: 'Otra lista personal' },
  { id: 'admin', name: 'Admin', description: 'Administración completa' },
];
@Injectable({ providedIn: 'root' })
export class SessionStore {
  private http = inject(HttpClient);
  private router = inject(Router);
  readonly session = signal<Session | null>(null);
  readonly busy = signal(false);
  readonly error = signal('');
  readonly changed$ = new Subject<void>();
  private cleanups: (() => Promise<void>)[] = [];
  private revision = 0;
  private expiry?: ReturnType<typeof setTimeout>;
  can(permission: string) {
    return this.session()?.permissions.includes(permission) ?? false;
  }
  registerCleanup(cleanup: () => Promise<void>) {
    this.cleanups.push(cleanup);
  }
  async switchTo(userId: string) {
    const revision = ++this.revision;
    this.busy.set(true);
    this.error.set('');
    this.changed$.next();
    this.session.set(null);
    clearTimeout(this.expiry);
    await Promise.all(this.cleanups.map((cleanup) => cleanup()));
    try {
      const response = await firstValueFrom(
        this.http.post<ApiResponse<Session>>(`${API}/auth/demo-sessions`, { userId }),
      );
      if (revision !== this.revision) return;
      this.session.set(response.data);
      this.expiry = setTimeout(
        () => this.expire(),
        Math.max(0, Date.parse(response.data.expiresAt) - Date.now()),
      );
      await this.router.navigateByUrl('/market');
    } catch (error) {
      if (revision === this.revision) this.error.set(errorMessage(error));
    } finally {
      if (revision === this.revision) this.busy.set(false);
    }
  }
  expire() {
    ++this.revision;
    clearTimeout(this.expiry);
    this.changed$.next();
    this.session.set(null);
    this.busy.set(false);
    this.error.set('La sesión expiró. Selecciona un usuario para continuar.');
    void Promise.all(this.cleanups.map((cleanup) => cleanup()));
  }
}
