import { Component, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuditFacade } from './audit.facade';
@Component({
  selector: 'app-audit-page',
  providers: [AuditFacade],
  imports: [DatePipe, FormsModule],
  templateUrl: './audit-page.html',
})
export class AuditPage {
  readonly vm = inject(AuditFacade);
  actor = '';
  action = '';
  readonly labels: Record<string, string> = {
    'watchlist.add': 'Activo agregado',
    'watchlist.update': 'Activo editado',
    'watchlist.remove': 'Activo eliminado',
    'thresholds.update': 'Umbral actualizado',
  };
  load(page = 1) {
    this.vm.load(page, this.actor, this.action);
  }
}
