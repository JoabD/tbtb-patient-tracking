import { Injectable, signal } from '@angular/core';

/**
 * Recuerda el usuario del gestor mientras la pestaña esté abierta, para no volver a escribirlo en cada
 * formulario. No hay autenticación: es solo una comodidad de la interfaz y vive en memoria.
 */
@Injectable({ providedIn: 'root' })
export class GestorContext {
  readonly username = signal('');
}
