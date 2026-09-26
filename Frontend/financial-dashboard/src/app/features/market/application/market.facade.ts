import { Injectable, computed, effect, inject, signal, untracked } from '@angular/core';
import { MarketApi } from '../infrastructure/market.api';
import { ASSETS, Currency, History, Metric, Snapshot } from '../domain/market';
import { SessionStore } from '../../../core/session/session.store';
import { MarketRealtime } from '../../../core/realtime/market-realtime.service';
import { errorMessage } from '../../../core/http/api';
import { ThresholdsFacade } from '../../administration/thresholds/thresholds.facade';
@Injectable({ providedIn: 'root' })
export class MarketFacade {
  private api = inject(MarketApi);
  private session = inject(SessionStore);
  readonly realtime = inject(MarketRealtime);
  readonly thresholds = inject(ThresholdsFacade);
  readonly currency = signal<Currency>('usd');
  readonly asset = signal('bitcoin');
  readonly days = signal(1);
  readonly selectedAssets = signal(ASSETS.map((x) => x.id));
  readonly metrics = signal<Metric[]>([
    'price',
    'change24h',
    'marketCap',
    'volume24h',
    'volatility24h',
  ]);
  readonly snapshot = signal<Snapshot | null>(null);
  readonly history = signal<History | null>(null);
  readonly loading = signal(false);
  readonly historyLoading = signal(false);
  readonly error = signal('');
  readonly historyError = signal('');
  private readonly refreshCount = signal(0);
  readonly quotes = computed(
    () => this.snapshot()?.quotes.filter((q) => this.selectedAssets().includes(q.coinId)) ?? [],
  );
  constructor() {
    this.session.changed$.subscribe(() => {
      this.snapshot.set(null);
      this.history.set(null);
      this.error.set('');
      this.historyError.set('');
    });
    this.realtime.snapshots$.subscribe((snapshot) => {
      if (snapshot.currency === this.currency()) {
        this.snapshot.set(snapshot);
        this.error.set('');
      }
    });
    this.realtime.recovered$.subscribe(() => this.refresh());
    effect((onCleanup) => {
      const user = this.session.session(),
        currency = this.currency();
      this.refreshCount();
      if (!user) return;
      untracked(() => {
        this.loading.set(true);
        this.error.set('');
        if (this.snapshot()?.currency !== currency) this.snapshot.set(null);
      });
      void this.realtime.subscribe(currency);
      const subscription = this.api.snapshot(currency).subscribe({
        next: (s) => {
          this.snapshot.set(s);
          this.loading.set(false);
        },
        error: (e) => {
          this.error.set(errorMessage(e));
          this.loading.set(false);
        },
      });
      const settings = this.thresholds.refresh().subscribe({ error: () => {} });
      onCleanup(() => {
        subscription.unsubscribe();
        settings.unsubscribe();
      });
    });
    effect((onCleanup) => {
      const user = this.session.session(),
        currency = this.currency(),
        asset = this.asset(),
        days = this.days();
      this.snapshot();
      this.refreshCount();
      if (!user) return;
      untracked(() => {
        this.historyLoading.set(true);
        this.historyError.set('');
        if (this.history()?.currency !== currency || this.history()?.coinId !== asset)
          this.history.set(null);
      });
      const request = this.api.history(asset, currency, days).subscribe({
        next: (h) => {
          this.history.set(h);
          this.historyLoading.set(false);
        },
        error: (e) => {
          this.historyError.set(errorMessage(e));
          this.historyLoading.set(false);
        },
      });
      onCleanup(() => request.unsubscribe());
    });
  }
  refresh() {
    this.refreshCount.update((n) => n + 1);
  }
  toggleAsset(id: string) {
    this.selectedAssets.update((items) =>
      items.includes(id) ? items.filter((x) => x !== id) : [...items, id],
    );
  }
  toggleMetric(key: Metric) {
    this.metrics.update((items) =>
      items.includes(key) ? items.filter((x) => x !== key) : [...items, key],
    );
  }
}
