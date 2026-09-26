import { Component, input, output } from '@angular/core';
import { DatePipe } from '@angular/common';
import { WatchlistItem } from '../../domain/watchlist';
import { ASSETS } from '../../../market/domain/market';
@Component({
  selector: 'app-watchlist-table',
  imports: [DatePipe],
  templateUrl: './watchlist-table.html',
})
export class WatchlistTable {
  readonly items = input.required<WatchlistItem[]>();
  readonly busy = input(false);
  readonly edit = output<WatchlistItem>();
  readonly remove = output<WatchlistItem>();
  name(id: string) {
    return ASSETS.find((a) => a.id === id)?.name ?? id;
  }
}
