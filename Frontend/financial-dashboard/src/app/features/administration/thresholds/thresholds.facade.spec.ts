import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { Subject } from 'rxjs';
import { ThresholdsFacade } from './thresholds.facade';
import { MarketRealtime } from '../../../core/realtime/market-realtime.service';
import { SessionStore } from '../../../core/session/session.store';
import { ThresholdsPage } from './thresholds-page';

describe('Orden de las actualizaciones del umbral', () => {
  let facade: ThresholdsFacade;
  let http: HttpTestingController;
  let updates: Subject<number>;
  let sessions: Subject<void>;
  beforeEach(() => {
    updates = new Subject<number>();
    sessions = new Subject<void>();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: MarketRealtime, useValue: { thresholds$: updates } },
        { provide: SessionStore, useValue: { changed$: sessions } },
      ],
    });
    facade = TestBed.inject(ThresholdsFacade);
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => {
    http.verify();
    TestBed.resetTestingModule();
  });
  it('una lectura antigua no sobrescribe el umbral recibido por SignalR', () => {
    facade.refresh().subscribe();
    const old = http.expectOne('/api/v1/dashboard/thresholds');
    updates.next(1.25);
    old.flush({ data: { volatilityThreshold: 5 } });
    expect(facade.value()).toBe(1.25);
  });
  it('una respuesta de la sesión anterior no repuebla el estado', () => {
    facade.refresh().subscribe();
    const old = http.expectOne('/api/v1/dashboard/thresholds');
    sessions.next();
    old.flush({ data: { volatilityThreshold: 9 } });
    expect(facade.value()).toBe(5);
  });
  it('impide guardar antes de cargar y conserva la edición posterior', () => {
    const page = TestBed.runInInjectionContext(() => new ThresholdsPage());
    const initial = http.expectOne('/api/v1/dashboard/thresholds');
    expect(page.ready()).toBe(false);
    page.value = 1.25;
    page.save();
    http.expectNone((request) => request.method === 'PUT');
    initial.flush({ data: { volatilityThreshold: 5 } });
    expect(page.ready()).toBe(true);
    page.value = 1.25;
    page.save();
    const saved = http.expectOne('/api/v1/dashboard/thresholds');
    expect(saved.request.body).toEqual({ volatilityThreshold: 1.25 });
    saved.flush({ data: { volatilityThreshold: 1.25 } });
    expect(facade.value()).toBe(1.25);
  });
});
