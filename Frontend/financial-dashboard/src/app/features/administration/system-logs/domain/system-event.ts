export interface SystemEvent {
  id: string;
  code: string;
  level: 'Information' | 'Warning' | 'Error';
  component: 'Api' | 'Market';
  message: string;
  currency: string | null;
  coinId: string | null;
  createdAtUtc: string;
}
export interface SystemEventPage {
  items: SystemEvent[];
  total: number;
  page: number;
  pageSize: number;
}
export interface SystemEventFilters {
  level: string;
  component: string;
  from: string;
  to: string;
}
