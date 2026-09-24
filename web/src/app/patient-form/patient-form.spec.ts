import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormGroup } from '@angular/forms';
import { Subject, of, throwError } from 'rxjs';

import { PatientResponse } from '../models/api.models';
import { GestorContext } from '../services/gestor-context';
import { PatientApiService } from '../services/patient-api.service';
import { PatientForm } from './patient-form';

const createdPatient: PatientResponse = {
  id: '11111111-1111-1111-1111-111111111111',
  fullName: 'Ana Rodríguez',
  documentType: 'Cedula',
  documentNumber: '1032456789',
  country: 'CO',
  city: 'Bogotá',
  phone: '+573001234567',
  email: null,
  treatmentStartDate: '2026-09-01',
  trackingStatus: 'Reachable',
  isActive: true,
  createdAt: '2026-09-23T21:00:00Z',
};

describe('PatientForm', () => {
  let api: jasmine.SpyObj<PatientApiService>;
  let fixture: ComponentFixture<PatientForm>;
  let element: HTMLElement;

  beforeEach(async () => {
    api = jasmine.createSpyObj<PatientApiService>('PatientApiService', ['registerPatient']);
    await TestBed.configureTestingModule({
      imports: [PatientForm],
      providers: [{ provide: PatientApiService, useValue: api }],
    }).compileComponents();

    fixture = TestBed.createComponent(PatientForm);
    element = fixture.nativeElement;
    fixture.detectChanges();
  });

  function form(): FormGroup {
    return (fixture.componentInstance as unknown as { form: FormGroup }).form;
  }

  function fillValidForm(): void {
    form().setValue({
      fullName: 'Ana Rodríguez',
      documentType: 'Cedula',
      documentNumber: '1032456789',
      country: 'CO',
      city: 'Bogotá',
      phone: '+573001234567',
      email: '   ',
      treatmentStartDate: '2026-09-01',
      privacyAccepted: true,
      gestorUsername: 'gestor.demo',
    });
  }

  function submit(): void {
    element.querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
  }

  it('no llama al API y marca los campos obligatorios cuando el formulario está vacío', () => {
    submit();

    expect(api.registerPatient).not.toHaveBeenCalled();
    expect(element.querySelectorAll('.field-error').length).toBeGreaterThan(5);
    expect(element.textContent).toContain('Debe confirmarse la aceptación del aviso de privacidad.');
  });

  it('muestra un mensaje claro cuando el API responde 409 por documento duplicado', () => {
    const detail = 'Ya existe un paciente registrado con ese país, tipo y número de documento.';
    api.registerPatient.and.returnValue(
      throwError(() => new HttpErrorResponse({ status: 409, error: { title: 'Conflicto', detail } })),
    );
    fillValidForm();

    submit();

    expect(element.querySelector('.banner-error')?.textContent).toContain(detail);
    expect(element.querySelector('.banner-success')).toBeNull();
  });

  it('muestra el error del API debajo del campo cuando responde 400', () => {
    api.registerPatient.and.returnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 400,
            error: { errors: { phone: ['El teléfono debe tener entre 7 y 15 dígitos.'] } },
          }),
      ),
    );
    fillValidForm();

    submit();

    const phoneField = element.querySelector('#patient-phone')!.closest('.field')!;
    expect(phoneField.textContent).toContain('El teléfono debe tener entre 7 y 15 dígitos.');
    expect(element.querySelector('.banner-error')?.textContent).toContain('Revise los campos marcados.');
  });

  it('avisa que no se pudo conectar cuando hay un error de red', () => {
    api.registerPatient.and.returnValue(throwError(() => new HttpErrorResponse({ status: 0 })));
    fillValidForm();

    submit();

    expect(element.querySelector('.banner-error')?.textContent).toContain('No se pudo conectar con el servidor');
  });

  it('cancela la petición en curso si el componente se destruye', () => {
    const pending = new Subject<PatientResponse>();
    api.registerPatient.and.returnValue(pending);
    fillValidForm();

    submit();
    expect(pending.observed).toBeTrue();

    fixture.destroy();

    expect(pending.observed).toBeFalse();
  });

  it('envía el correo vacío como null, emite el paciente y recuerda el usuario del gestor', () => {
    api.registerPatient.and.returnValue(of(createdPatient));
    const emitted: PatientResponse[] = [];
    fixture.componentInstance.registered.subscribe((p) => emitted.push(p));
    fillValidForm();

    submit();

    expect(api.registerPatient.calls.mostRecent().args[0].email).toBeNull();
    expect(emitted).toEqual([createdPatient]);
    expect(TestBed.inject(GestorContext).username()).toBe('gestor.demo');
  });

  it('precarga el usuario del gestor recordado', () => {
    TestBed.inject(GestorContext).username.set('gestor.previo');

    const other = TestBed.createComponent(PatientForm);
    other.detectChanges();

    const otherForm = (other.componentInstance as unknown as { form: FormGroup }).form;
    expect(otherForm.getRawValue()['gestorUsername']).toBe('gestor.previo');
  });

  it('avisa al pulsar Cancelar', () => {
    let cancelled = 0;
    fixture.componentInstance.cancelled.subscribe(() => cancelled++);

    const cancel = Array.from(element.querySelectorAll('button')).find((b) => b.textContent?.includes('Cancelar'));
    cancel!.click();

    expect(cancelled).toBe(1);
  });
});
