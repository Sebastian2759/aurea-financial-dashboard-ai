import { Component, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SystemLogsFacade } from '../application/system-logs.facade';
import { SystemEventFilters } from '../domain/system-event';
@Component({
  selector: 'app-system-logs-page',
  providers: [SystemLogsFacade],
  imports: [DatePipe, FormsModule],
  templateUrl: './system-logs-page.html',
  styleUrl: './system-logs-page.scss',
})
export class SystemLogsPage {
  readonly vm = inject(SystemLogsFacade);
  filters: SystemEventFilters = { level: '', component: '', from: '', to: '' };
  readonly levels: Record<string, string> = {
    Information: 'Información',
    Warning: 'Advertencia',
    Error: 'Error',
  };
  readonly components: Record<string, string> = { Api: 'Aplicación', Market: 'Mercado' };
  load(page = 1) {
    this.vm.load(page, this.filters);
  }
}
