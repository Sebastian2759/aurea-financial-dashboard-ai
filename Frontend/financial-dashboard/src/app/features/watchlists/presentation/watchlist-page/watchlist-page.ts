import { Component, inject, signal } from '@angular/core';
import { WatchlistFacade } from '../../application/watchlist.facade';
import { WatchlistItem } from '../../domain/watchlist';
import { WatchlistItemForm } from '../watchlist-item-form/watchlist-item-form';
import { WatchlistTable } from '../watchlist-table/watchlist-table';
import { RemoveItemDialog } from '../remove-item-dialog/remove-item-dialog';
import { OwnerSelector } from '../../../administration/watchlist-management/owner-selector';
@Component({
  selector: 'app-watchlist-page',
  providers: [WatchlistFacade],
  imports: [WatchlistItemForm, WatchlistTable, RemoveItemDialog, OwnerSelector],
  templateUrl: './watchlist-page.html',
  styleUrl: './watchlist-page.scss',
})
export class WatchlistPage {
  readonly vm = inject(WatchlistFacade);
  readonly editing = signal<WatchlistItem | null>(null);
  readonly removing = signal<WatchlistItem | null>(null);
  ownerChanged(owner: string) {
    this.editing.set(null);
    this.removing.set(null);
    this.vm.selectOwner(owner);
  }
  save(draft: { coinId: string; note: string | null }) {
    this.vm.save(draft, this.editing()?.id, () => this.editing.set(null));
  }
  remove() {
    const item = this.removing();
    if (item) this.vm.remove(item, () => this.removing.set(null));
  }
}
