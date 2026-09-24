import { HttpErrorResponse } from '@angular/common/http';
import { FormGroup } from '@angular/forms';

export type ApiErrorKind = 'validation' | 'conflict' | 'notFound' | 'network' | 'unexpected';

export interface ApiError {
  kind: ApiErrorKind;
  /** Mensaje general para mostrar al usuario. */
  message: string;
  /** Errores por campo (solo en 400). Las claves llegan en camelCase, igual que los controles del formulario. */
  fieldErrors: Record<string, string[]>;
}

/** Cuerpo de un ProblemDetails / ValidationProblemDetails de ASP.NET Core. */
interface ProblemBody {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

/** Traduce cualquier error de una llamada HTTP a algo que la interfaz sabe mostrar. */
export function toApiError(error: unknown): ApiError {
  if (!(error instanceof HttpErrorResponse)) {
    return unexpected();
  }

  const body: ProblemBody =
    typeof error.error === 'object' && error.error !== null ? error.error : {};

  switch (error.status) {
    case 0:
      return {
        kind: 'network',
        message: 'No se pudo conectar con el servidor. Verifique que el API esté en ejecución.',
        fieldErrors: {},
      };
    case 400:
      return {
        kind: 'validation',
        message: body.detail ?? 'Hay datos que no son válidos.',
        fieldErrors: body.errors ?? {},
      };
    case 404:
      return {
        kind: 'notFound',
        message: body.detail ?? 'No se encontró el recurso solicitado.',
        fieldErrors: {},
      };
    case 409:
      return {
        kind: 'conflict',
        message: body.detail ?? 'La operación entra en conflicto con datos existentes.',
        fieldErrors: {},
      };
    default:
      return unexpected();
  }
}

function unexpected(): ApiError {
  return {
    kind: 'unexpected',
    message: 'Ocurrió un error inesperado. Intente de nuevo.',
    fieldErrors: {},
  };
}

/**
 * Pone los errores por campo del API sobre los controles del formulario y devuelve el mensaje general
 * que debe mostrarse arriba del formulario. Los errores cuya clave no corresponde a un control
 * (por ejemplo los generales, con clave vacía) se incluyen en ese mensaje.
 */
export function applyApiError(form: FormGroup, apiError: ApiError): string {
  const unmatched: string[] = [];
  let matched = 0;

  for (const [key, messages] of Object.entries(apiError.fieldErrors)) {
    const control = form.get(normalizeKey(key));
    if (control) {
      control.setErrors({ server: messages[0] });
      control.markAsTouched();
      matched++;
    } else {
      unmatched.push(...messages);
    }
  }

  if (apiError.kind !== 'validation') {
    return apiError.message;
  }
  if (unmatched.length > 0) {
    return unmatched.join(' ');
  }
  return matched > 0 ? 'Revise los campos marcados.' : apiError.message;
}

/** Los errores de binding de ASP.NET pueden venir como "$.treatmentStartDate"; se dejan como "treatmentStartDate". */
function normalizeKey(key: string): string {
  const clean = key.replace(/^\$\./, '');
  return clean.charAt(0).toLowerCase() + clean.slice(1);
}
