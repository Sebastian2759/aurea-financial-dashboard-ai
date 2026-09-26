import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, takeUntil, throwError } from 'rxjs';
import { SessionStore } from '../session/session.store';
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const session = inject(SessionStore);
  if (!request.url.startsWith('/api/') || request.url.endsWith('/demo-sessions'))
    return next(request);
  const token = session.session()?.accessToken;
  const authenticated = token
    ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : request;
  return next(authenticated).pipe(
    takeUntil(session.changed$),
    catchError((error) => {
      if (error.status === 401 && token === session.session()?.accessToken) session.expire();
      return throwError(() => error);
    }),
  );
};
