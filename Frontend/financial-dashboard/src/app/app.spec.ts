import { money, percent } from './shared/formatting/format';
import { exceedsThreshold } from './features/market/domain/market';
describe('Representación de datos financieros', () => {
  it('preserva métricas ausentes y distingue cero de ausencia', () => {
    expect(money(null)).toBe('No disponible');
    expect(percent(undefined)).toBe('No disponible');
    expect(money(0)).not.toBe('No disponible');
    expect(percent(0)).toBe('0 %');
  });
  it('solo alerta por encima del umbral con valores disponibles', () => {
    expect(exceedsThreshold(null, 5)).toBe(false);
    expect(exceedsThreshold(5, 5)).toBe(false);
    expect(exceedsThreshold(5.01, 5)).toBe(true);
  });
});
