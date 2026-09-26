export type Currency = 'usd' | 'eur';
export interface Quote {
  coinId: string;
  symbol: string;
  name: string;
  price: number | null;
  change24h: number | null;
  marketCap: number | null;
  volume24h: number | null;
  volatility24h: number | null;
  updatedAt: string | null;
}
export interface Snapshot {
  currency: Currency;
  quotes: Quote[];
  fetchedAt: string;
  source: string;
  isStale: boolean;
  message: string | null;
}
export interface PricePoint {
  time: string;
  price: number;
}
export interface History {
  coinId: string;
  currency: Currency;
  points: PricePoint[];
  sourceUpdatedAt: string;
  source: string;
  isStale: boolean;
}
export const ASSETS = [
  { id: 'bitcoin', name: 'Bitcoin', symbol: 'BTC' },
  { id: 'ethereum', name: 'Ethereum', symbol: 'ETH' },
  { id: 'solana', name: 'Solana', symbol: 'SOL' },
];
export const METRICS = [
  { key: 'price', label: 'Precio' },
  { key: 'change24h', label: 'Variación 24 h' },
  { key: 'marketCap', label: 'Capitalización' },
  { key: 'volume24h', label: 'Volumen 24 h' },
  { key: 'volatility24h', label: 'Volatilidad 24 h' },
] as const;
export type Metric = (typeof METRICS)[number]['key'];
export function exceedsThreshold(value: number | null, threshold: number) {
  return value !== null && value > threshold;
}
