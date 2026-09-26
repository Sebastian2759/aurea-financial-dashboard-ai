import { TestBed } from '@angular/core/testing';
import { provideHttpClient, HttpClient, withInterceptors } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { Session, SessionStore } from './session.store';
import { authInterceptor } from '../http/auth.interceptor';
import { permissionGuard } from '../authorization/permission.guard';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
const ticket = (role: string): Session => ({
  accessToken: role + '-token',
  expiresAt: new Date(Date.now() + 3600000).toISOString(),
  userId: role.toLowerCase(),
  name: role,
  role,
  permissions: role === 'Admin' ? ['market.read', 'audit.read'] : ['market.read'],
});
describe('Sesión y autorización', () => {
  let store: SessionStore;
  let http: HttpTestingController;
  const router = {
    navigateByUrl: vi.fn().mockResolvedValue(true),
    createUrlTree: vi.fn().mockReturnValue('market-route'),
  };
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: Router, useValue: router },
      ],
    });
    store = TestBed.inject(SessionStore);
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => {
    store.expire();
    http.verify({ ignoreCancelled: true });
    TestBed.resetTestingModule();
  });
  it('cancela consultas y elimina la identidad Admin antes de emitir un token Viewer', async () => {
    store.session.set(ticket('Admin'));
    const cleanup = vi.fn().mockResolvedValue(undefined);
    store.registerCleanup(cleanup);
    TestBed.inject(HttpClient).get('/api/v1/audit-events').subscribe();
    const pending = http.expectOne('/api/v1/audit-events');
    expect(pending.request.headers.get('Authorization')).toBe('Bearer Admin-token');
    const switching = store.switchTo('viewer');
    expect(store.session()).toBeNull();
    expect(pending.cancelled).toBe(true);
    expect(store.can('audit.read')).toBe(false);
    await Promise.resolve();
    await Promise.resolve();
    const login = http.expectOne('/api/v1/auth/demo-sessions');
    expect(login.request.headers.has('Authorization')).toBe(false);
    expect(login.request.body).toEqual({ userId: 'viewer' });
    login.flush({ data: ticket('Viewer') });
    await switching;
    expect(cleanup).toHaveBeenCalledOnce();
    expect(store.session()?.role).toBe('Viewer');
    expect(store.can('audit.read')).toBe(false);
  });
  it('bloquea la ruta de auditoría para Viewer', () => {
    store.session.set(ticket('Viewer'));
    const route = { data: { permission: 'audit.read' } } as unknown as ActivatedRouteSnapshot;
    const result = TestBed.runInInjectionContext(() =>
      permissionGuard(route, {} as RouterStateSnapshot),
    );
    expect(result).toBe('market-route');
    store.session.set(ticket('Admin'));
    expect(
      TestBed.runInInjectionContext(() => permissionGuard(route, {} as RouterStateSnapshot)),
    ).toBe(true);
  });
  it('expira la identidad cuando el backend devuelve 401', () => {
    store.session.set(ticket('Admin'));
    TestBed.inject(HttpClient)
      .get('/api/v1/audit-events')
      .subscribe({ error: () => {} });
    http.expectOne('/api/v1/audit-events').flush({}, { status: 401, statusText: 'Unauthorized' });
    expect(store.session()).toBeNull();
    expect(store.error()).toContain('expiró');
  });
});
