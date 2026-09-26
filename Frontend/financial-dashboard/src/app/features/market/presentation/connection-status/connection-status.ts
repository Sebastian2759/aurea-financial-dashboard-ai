import { Component, input } from '@angular/core';
@Component({ selector: 'app-connection-status', templateUrl: './connection-status.html' })
export class ConnectionStatus {
  readonly status = input.required<string>();
  readonly labels: Record<string, string> = {
    connected: 'En vivo',
    connecting: 'Conectando',
    reconnecting: 'Reconectando',
    disconnected: 'Sin conexión',
  };
}
