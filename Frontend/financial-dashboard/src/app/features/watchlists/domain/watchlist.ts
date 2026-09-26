export interface WatchlistItem {
  id: string;
  ownerId: string;
  coinId: string;
  note: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
}
export interface WatchlistDraft {
  coinId: string;
  note: string | null;
}
export interface Owner {
  id: string;
  name: string;
  role: string;
}
