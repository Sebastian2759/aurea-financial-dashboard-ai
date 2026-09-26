import { inject, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, catchError, of, switchMap, tap } from 'rxjs';
import { SystemLogsApi } from '../infrastructure/system-logs.api';
import { SystemEventFilters, SystemEventPage } from '../domain/system-event';
import { errorMessage } from '../../../../core/http/api';
import { SessionStore } from '../../../../core/session/session.store';
@Injectable()
export class SystemLogsFacade {
  private api = inject(SystemLogsApi);
  private requests = new Subject<{ page: number; filters: SystemEventFilters }>();
  readonly data = signal<SystemEventPage>({ items: [], total: 0, page: 1, pageSize: 10 });
  readonly error = signal('');
  readonly loading = signal(false);
  constructor() {
    inject(SessionStore)
      .changed$.pipe(takeUntilDestroyed())
      .subscribe(() => {
        this.data.set({ items: [], total: 0, page: 1, pageSize: 10 });
        this.error.set('');
      });
    this.requests
      .pipe(
        tap(() => {
          this.loading.set(true);
          this.error.set('');
        }),
        switchMap(({ page, filters }) =>
          this.api.list(page, filters).pipe(
            catchError((error) => {
              this.error.set(errorMessage(error));
              return of({ items: [], total: 0, page, pageSize: 10 });
            }),
          ),
        ),
        takeUntilDestroyed(),
      )
      .subscribe((data) => {
        this.data.set(data);
        this.loading.set(false);
      });
    this.load();
  }
  load(page = 1, filters: SystemEventFilters = { level: '', component: '', from: '', to: '' }) {
    this.requests.next({ page, filters: { ...filters } });
  }
}
