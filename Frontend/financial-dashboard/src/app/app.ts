import { Component, effect, inject, untracked } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SessionStore, DEMO_USERS } from './core/session/session.store';
import { MarketRealtime } from './core/realtime/market-realtime.service';
import { Navigation } from './shell/navigation/navigation';
import { RoleSwitcher } from './shell/role-switcher/role-switcher';
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navigation, RoleSwitcher],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  readonly session = inject(SessionStore);
  private realtime = inject(MarketRealtime);
  readonly users = DEMO_USERS;
  constructor() {
    effect(() => {
      const user = this.session.session();
      if (user) untracked(() => void this.realtime.connect(user.accessToken));
    });
    void this.session.switchTo('viewer');
  }
}
