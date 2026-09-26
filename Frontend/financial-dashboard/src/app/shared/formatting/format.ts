export function money(value: number | null | undefined, currency = 'usd', compact = false): string {
  return value == null
    ? 'No disponible'
    : new Intl.NumberFormat('es-ES', {
        style: 'currency',
        currency: currency.toUpperCase(),
        notation: compact ? 'compact' : 'standard',
        maximumFractionDigits: 2,
      }).format(value);
}
export function percent(value: number | null | undefined): string {
  return value == null
    ? 'No disponible'
    : `${value > 0 ? '+' : ''}${new Intl.NumberFormat('es-ES', { maximumFractionDigits: 2 }).format(value)} %`;
}
