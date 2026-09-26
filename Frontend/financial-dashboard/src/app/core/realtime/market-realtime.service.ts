import { Injectable, inject, signal } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { SessionStore } from '../session/session.store';
import { Currency, Snapshot } from '../../features/market/domain/market';
@Injectable({ providedIn: 'root' })
export class MarketRealtime {
  private session = inject(SessionStore);
  private connection?: HubConnection;
  private currency: Currency = 'usd';
  private generation = 0;
  private retry?: ReturnType<typeof setTimeout>;
  readonly status = signal<'connecting' | 'connected' | 'reconnecting' | 'disconnected'>(
    'disconnected',
  );
  readonly snapshots$ = new Subject<Snapshot>();
  readonly thresholds$ = new Subject<number>();
  readonly recovered$ = new Subject<void>();
  constructor() {
    this.session.registerCleanup(() => this.stop());
  }
  async connect(token: string) {
    await this.stop();
    if (this.session.session()?.accessToken !== token) return;
    const generation = ++this.generation;
    const connection = new HubConnectionBuilder()
      .withUrl('/hubs/market', { accessTokenFactory: () => token })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(LogLevel.Error)
      .build();
    this.connection = connection;
    const active = () =>
      generation === this.generation && this.session.session()?.accessToken === token;
    connection.on('MarketSnapshotUpdated', (snapshot: Snapshot) => {
      if (active()) this.snapshots$.next(snapshot);
    });
    connection.on('ThresholdsUpdated', (settings: { volatilityThreshold: number }) => {
      if (active()) this.thresholds$.next(settings.volatilityThreshold);
    });
    connection.onreconnecting(() => {
      if (active()) this.status.set('reconnecting');
    });
    connection.onreconnected(async () => {
      if (active()) {
        this.status.set('connected');
        await this.subscribe(this.currency);
        this.recovered$.next();
      }
    });
    const start = async () => {
      if (!active()) return;
      this.status.set('connecting');
      try {
        await connection.start();
        if (active()) {
          this.status.set('connected');
          await this.subscribe(this.currency);
          this.recovered$.next();
        }
      } catch {
        if (active()) {
          this.status.set('disconnected');
          this.retry = setTimeout(() => void start(), 10000);
        }
      }
    };
    connection.onclose(() => {
      if (active()) {
        this.status.set('disconnected');
        this.retry = setTimeout(() => void start(), 10000);
      }
    });
    await start();
  }
  async subscribe(currency: Currency) {
    this.currency = currency;
    if (this.connection?.state === HubConnectionState.Connected) {
      try {
        await this.connection.invoke('Subscribe', currency);
      } catch {
        this.status.set('disconnected');
      }
    }
  }
  async stop() {
    ++this.generation;
    clearTimeout(this.retry);
    const previous = this.connection;
    this.connection = undefined;
    this.status.set('disconnected');
    if (previous) await previous.stop();
  }
}
