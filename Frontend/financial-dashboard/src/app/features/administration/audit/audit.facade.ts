import { Injectable, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, catchError, of, switchMap, tap } from 'rxjs';
import { AuditApi, AuditPage } from './audit.api';
import { errorMessage } from '../../../core/http/api';
@Injectable()
export class AuditFacade {
  private api = inject(AuditApi);
  private requests = new Subject<{ page: number; actor: string; action: string }>();
  readonly data = signal<AuditPage>({ items: [], total: 0, page: 1, pageSize: 10 });
  readonly error = signal('');
  readonly loading = signal(false);
  constructor() {
    this.requests
      .pipe(
        tap(() => {
          this.loading.set(true);
          this.error.set('');
        }),
        switchMap((q) =>
          this.api.list(q.page, q.actor, q.action).pipe(
            catchError((e) => {
              this.error.set(errorMessage(e));
              return of({ items: [], total: 0, page: q.page, pageSize: 10 });
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
  load(page = 1, actor = '', action = '') {
    this.requests.next({ page, actor, action });
  }
}
