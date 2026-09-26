import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs';
import { API, ApiResponse } from '../../../core/http/api';
import { Currency, History, Snapshot } from '../domain/market';
@Injectable({ providedIn: 'root' })
export class MarketApi {
  private http = inject(HttpClient);
  snapshot(currency: Currency) {
    return this.http
      .get<ApiResponse<{ snapshot: Snapshot }>>(`${API}/markets`, { params: { currency } })
      .pipe(map((r) => r.data.snapshot));
  }
  history(coinId: string, currency: Currency, days: number) {
    return this.http
      .get<ApiResponse<{ history: History }>>(`${API}/markets/${coinId}/history`, {
        params: { currency, days },
      })
      .pipe(map((r) => r.data.history));
  }
}
