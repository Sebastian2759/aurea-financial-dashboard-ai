import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs';
import { API, ApiResponse } from '../../../core/http/api';
import { Owner, WatchlistDraft, WatchlistItem } from '../domain/watchlist';
@Injectable({ providedIn: 'root' })
export class WatchlistApi {
  private http = inject(HttpClient);
  owners() {
    return this.http
      .get<ApiResponse<{ owners: Owner[] }>>(`${API}/watchlists/owners`)
      .pipe(map((r) => r.data.owners));
  }
  list(owner: string) {
    return this.http
      .get<ApiResponse<{ items: WatchlistItem[] }>>(
        `${API}/watchlists/${encodeURIComponent(owner)}/items`,
      )
      .pipe(map((r) => r.data.items));
  }
  save(owner: string, draft: WatchlistDraft, id?: string) {
    const url = `${API}/watchlists/${encodeURIComponent(owner)}/items`;
    return id ? this.http.put(`${url}/${id}`, draft) : this.http.post(url, draft);
  }
  remove(owner: string, id: string) {
    return this.http.delete(`${API}/watchlists/${encodeURIComponent(owner)}/items/${id}`);
  }
}
