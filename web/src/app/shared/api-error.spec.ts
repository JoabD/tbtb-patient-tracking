import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup } from '@angular/forms';

import { applyApiError, toApiError } from './api-error';

describe('api-error', () => {
  describe('toApiError', () => {
    it('trata el estado 0 como error de red', () => {
      const result = toApiError(new HttpErrorResponse({ status: 0 }));
      expect(result.kind).toBe('network');
    });

    it('lee los errores por campo de un 400', () => {
      const result = toApiError(
        new HttpErrorResponse({ status: 400, error: { errors: { phone: ['El teléfono es obligatorio.'] } } }),
      );
      expect(result.kind).toBe('validation');
      expect(result.fieldErrors['phone']).toEqual(['El teléfono es obligatorio.']);
    });

    it('usa el detalle del 409 como mensaje', () => {
      const result = toApiError(new HttpErrorResponse({ status: 409, error: { detail: 'Ya existe.' } }));
      expect(result.kind).toBe('conflict');
      expect(result.message).toBe('Ya existe.');
    });

    it('usa el detalle del 404 como mensaje', () => {
      const result = toApiError(new HttpErrorResponse({ status: 404, error: { detail: 'El paciente indicado no existe.' } }));
      expect(result.kind).toBe('notFound');
      expect(result.message).toBe('El paciente indicado no existe.');
    });

    it('trata cualquier otro estado como error inesperado', () => {
      expect(toApiError(new HttpErrorResponse({ status: 500 })).kind).toBe('unexpected');
      expect(toApiError(new Error('boom')).kind).toBe('unexpected');
    });
  });

  describe('applyApiError', () => {
    it('marca el control que corresponde a cada campo y avisa de que hay campos por revisar', () => {
      const form = new FormGroup({ phone: new FormControl(''), city: new FormControl('') });
      const message = applyApiError(form, {
        kind: 'validation',
        message: 'x',
        fieldErrors: { phone: ['Teléfono inválido.'] },
      });

      expect(form.controls.phone.errors?.['server']).toBe('Teléfono inválido.');
      expect(form.controls.phone.touched).toBeTrue();
      expect(form.controls.city.errors).toBeNull();
      expect(message).toBe('Revise los campos marcados.');
    });

    it('acepta claves con el prefijo de binding de ASP.NET ("$.treatmentStartDate")', () => {
      const form = new FormGroup({ treatmentStartDate: new FormControl('') });
      applyApiError(form, { kind: 'validation', message: 'x', fieldErrors: { '$.treatmentStartDate': ['Fecha inválida.'] } });
      expect(form.controls.treatmentStartDate.errors?.['server']).toBe('Fecha inválida.');
    });

    it('devuelve como mensaje general los errores que no pertenecen a un campo', () => {
      const form = new FormGroup({ phone: new FormControl('') });
      const message = applyApiError(form, {
        kind: 'validation',
        message: 'x',
        fieldErrors: { '': ['No se pueden registrar contactos de un paciente inactivo.'] },
      });
      expect(message).toBe('No se pueden registrar contactos de un paciente inactivo.');
    });

    it('para 409, 404 y red devuelve el mensaje del error', () => {
      const form = new FormGroup({});
      expect(applyApiError(form, { kind: 'conflict', message: 'Duplicado.', fieldErrors: {} })).toBe('Duplicado.');
    });
  });
});
