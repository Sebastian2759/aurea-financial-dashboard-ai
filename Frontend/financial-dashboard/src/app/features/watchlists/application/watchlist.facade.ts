import { DestroyRef, Injectable, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, switchMap, catchError, of, tap } from 'rxjs';
import { WatchlistApi } from '../infrastructure/watchlist.api';
import { Owner, WatchlistDraft, WatchlistItem } from '../domain/watchlist';
import { SessionStore } from '../../../core/session/session.store';
import { errorMessage } from '../../../core/http/api';
@Injectable()
export class WatchlistFacade {
  private api = inject(WatchlistApi);
  private destroy = inject(DestroyRef);
  readonly session = inject(SessionStore);
  readonly items = signal<WatchlistItem[]>([]);
  readonly owners = signal<Owner[]>([]);
  readonly owner = signal('me');
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly success = signal('');
  private requests = new Subject<string>();
  constructor() {
    this.requests
      .pipe(
        tap(() => {
          this.items.set([]);
          this.loading.set(true);
          this.error.set('');
        }),
        switchMap((owner) =>
          this.api.list(owner).pipe(
            catchError((e) => {
              this.error.set(errorMessage(e));
              return of([]);
            }),
          ),
        ),
        takeUntilDestroyed(),
      )
      .subscribe((items) => {
        this.items.set(items);
        this.loading.set(false);
      });
    if (this.session.can('watchlist.manage-all'))
      this.api
        .owners()
        .pipe(takeUntilDestroyed())
        .subscribe({
          next: (owners) => this.owners.set(owners),
          error: (e) => this.error.set(errorMessage(e)),
        });
    this.reload();
  }
  selectOwner(owner: string) {
    this.owner.set(owner);
    this.success.set('');
    this.reload();
  }
  reload() {
    this.requests.next(this.owner());
  }
  save(draft: WatchlistDraft, id?: string, onSuccess?: () => void) {
    this.saving.set(true);
    this.error.set('');
    this.success.set('');
    this.api
      .save(this.owner(), draft, id)
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.success.set('Lista actualizada. El cambio quedó registrado en auditoría.');
          onSuccess?.();
          this.reload();
        },
        error: (e) => {
          this.saving.set(false);
          this.error.set(errorMessage(e));
        },
      });
  }
  remove(item: WatchlistItem, onSuccess: () => void) {
    this.saving.set(true);
    this.error.set('');
    this.api
      .remove(this.owner(), item.id)
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.success.set('Activo eliminado del seguimiento.');
          onSuccess();
          this.reload();
        },
        error: (e) => {
          this.saving.set(false);
          this.error.set(errorMessage(e));
        },
      });
  }
}
