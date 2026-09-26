import { Component, inject } from '@angular/core';
import { DEMO_USERS, SessionStore } from '../../core/session/session.store';
@Component({
  selector: 'app-role-switcher',
  templateUrl: './role-switcher.html',
  styleUrl: './role-switcher.scss',
})
export class RoleSwitcher {
  readonly session = inject(SessionStore);
  readonly users = DEMO_USERS;
}
