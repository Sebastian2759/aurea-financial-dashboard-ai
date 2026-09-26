import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { SessionStore } from '../session/session.store';
export const permissionGuard: CanActivateFn = (route) =>
  inject(SessionStore).can(route.data['permission']) || inject(Router).createUrlTree(['/market']);
