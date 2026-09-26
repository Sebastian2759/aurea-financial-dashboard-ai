import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { defer, tap } from 'rxjs';
import { API, ApiResponse } from '../../../core/http/api';
import { MarketRealtime } from '../../../core/realtime/market-realtime.service';
import { SessionStore } from '../../../core/session/session.store';
@Injectable({ providedIn: 'root' })
export class ThresholdsFacade {
  private http = inject(HttpClient);
  readonly value = signal(5);
  private revision = 0;
  private readSequence = 0;
  constructor() {
    inject(MarketRealtime).thresholds$.subscribe((value) => this.accept(value));
    inject(SessionStore).changed$.subscribe(() => this.accept(5));
  }
  private accept(value: number) {
    ++this.revision;
    this.value.set(value);
  }
  refresh() {
    return defer(() => {
      const revision = this.revision;
      const sequence = ++this.readSequence;
      return this.http
        .get<ApiResponse<{ volatilityThreshold: number }>>(`${API}/dashboard/thresholds`)
        .pipe(
          tap((r) => {
            if (revision === this.revision && sequence === this.readSequence)
              this.value.set(r.data.volatilityThreshold);
          }),
        );
    });
  }
  update(volatilityThreshold: number) {
    return defer(() => {
      const revision = this.revision;
      return this.http
        .put<ApiResponse<{ volatilityThreshold: number }>>(`${API}/dashboard/thresholds`, {
          volatilityThreshold,
        })
        .pipe(
          tap((r) => {
            if (revision === this.revision) this.accept(r.data.volatilityThreshold);
          }),
        );
    });
  }
}
