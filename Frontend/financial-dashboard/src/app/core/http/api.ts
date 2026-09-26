import { HttpErrorResponse } from '@angular/common/http';
export interface ApiResponse<T> {
  isSuccess: boolean;
  data: T;
  message: string;
  errors?: string[];
}
export const API = '/api/v1';
export function errorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    if (error.status === 0) return 'No se pudo conectar con el servidor. Vuelve a intentarlo.';
    if (error.status === 401) return 'La sesión expiró. Selecciona un usuario para continuar.';
    if (error.status === 403) return 'Tu usuario no tiene permiso para esta operación.';
    return error.error?.message ?? error.error?.detail ?? 'No se pudo completar la operación.';
  }
  return 'No se pudo completar la operación.';
}
