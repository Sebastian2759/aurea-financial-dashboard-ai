import { Component, DestroyRef, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ThresholdsFacade } from './thresholds.facade';
import { errorMessage } from '../../../core/http/api';
@Component({
  selector: 'app-thresholds-page',
  imports: [FormsModule],
  templateUrl: './thresholds-page.html',
})
export class ThresholdsPage {
  readonly vm = inject(ThresholdsFacade);
  private destroy = inject(DestroyRef);
  readonly busy = signal(false);
  readonly loading = signal(true);
  readonly ready = signal(false);
  readonly error = signal('');
  readonly success = signal('');
  value = 5;
  constructor() {
    this.load();
  }
  load() {
    this.loading.set(true);
    this.ready.set(false);
    this.error.set('');
    this.vm
      .refresh()
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe({
        next: () => {
          this.value = this.vm.value();
          this.ready.set(true);
          this.loading.set(false);
        },
        error: (e) => {
          this.error.set(errorMessage(e));
          this.loading.set(false);
        },
      });
  }
  save() {
    if (!this.ready() || this.busy()) return;
    this.busy.set(true);
    this.error.set('');
    this.success.set('');
    this.vm
      .update(this.value)
      .pipe(takeUntilDestroyed(this.destroy))
      .subscribe({
        next: () => {
          this.busy.set(false);
          this.success.set('Umbral guardado y enviado a todos los usuarios conectados.');
        },
        error: (e) => {
          this.busy.set(false);
          this.error.set(errorMessage(e));
        },
      });
  }
}
