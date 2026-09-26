import { Component, input, output, OnChanges, SimpleChanges } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ASSETS } from '../../../market/domain/market';
import { WatchlistDraft, WatchlistItem } from '../../domain/watchlist';
@Component({
  selector: 'app-watchlist-item-form',
  imports: [ReactiveFormsModule],
  templateUrl: './watchlist-item-form.html',
  styleUrl: './watchlist-item-form.scss',
})
export class WatchlistItemForm implements OnChanges {
  readonly item = input<WatchlistItem | null>(null);
  readonly busy = input(false);
  readonly save = output<WatchlistDraft>();
  readonly cancel = output<void>();
  readonly assets = ASSETS;
  readonly form = new FormGroup({
    coinId: new FormControl('bitcoin', { nonNullable: true, validators: [Validators.required] }),
    note: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(240)] }),
  });
  ngOnChanges(changes: SimpleChanges) {
    // Initialize synchronously before editing is displayed. An asynchronous
    // effect with ngModel could overwrite the first keystrokes with old data.
    if (changes['item']) {
      this.form.reset({ coinId: this.item()?.coinId ?? 'bitcoin', note: this.item()?.note ?? '' });
    }
    if (this.busy()) this.form.disable({ emitEvent: false });
    else this.form.enable({ emitEvent: false });
  }
  submit() {
    if (this.form.invalid || this.busy()) return;
    const draft = this.form.getRawValue();
    this.save.emit({ coinId: draft.coinId, note: draft.note.trim() || null });
  }
}
