import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormGroup } from '@angular/forms';
import { Subject, of, throwError } from 'rxjs';

import { ContactResponse, PatientListItem } from '../models/api.models';
import { GestorContext } from '../services/gestor-context';
import { PatientApiService } from '../services/patient-api.service';
import { ContactForm } from './contact-form';

const patient: PatientListItem = {
  id: '22222222-2222-2222-2222-222222222222',
  fullName: 'Luis Quispe',
  documentType: 'Dni',
  documentNumber: '45678912',
  country: 'PE',
  city: 'Lima',
  phone: '+51987654321',
  email: null,
  treatmentStartDate: '2026-08-15',
  trackingStatus: 'Reachable',
  isActive: true,
  createdAt: '2026-09-01T10:00:00Z',
  contactCount: 0,
  lastContact: null,
};

describe('ContactForm', () => {
  let api: jasmine.SpyObj<PatientApiService>;
  let fixture: ComponentFixture<ContactForm>;
  let element: HTMLElement;

  beforeEach(async () => {
    api = jasmine.createSpyObj<PatientApiService>('PatientApiService', ['registerContact']);
    await TestBed.configureTestingModule({
      imports: [ContactForm],
      providers: [{ provide: PatientApiService, useValue: api }],
    }).compileComponents();

    fixture = TestBed.createComponent(ContactForm);
    fixture.componentRef.setInput('patient', patient);
    element = fixture.nativeElement;
    fixture.detectChanges();
  });

  function form(): FormGroup {
    return (fixture.componentInstance as unknown as { form: FormGroup }).form;
  }

  function fillValidForm(): void {
    form().patchValue({
      gestorUsername: 'gestor.demo',
      channel: 'Call',
      resultCode: 'NoAnswer',
      observations: '',
    });
  }

  function submit(): void {
    element.querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
  }

  it('muestra el paciente al que se le registra el contacto', () => {
    expect(element.textContent).toContain('Luis Quispe');
  });

  it('no llama al API si faltan canal, resultado o gestor', () => {
    submit();

    expect(api.registerContact).not.toHaveBeenCalled();
    expect(element.querySelectorAll('.field-error').length).toBeGreaterThanOrEqual(3);
  });

  it('muestra un aviso general cuando el API responde 404 (el paciente ya no existe)', () => {
    api.registerContact.and.returnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 404,
            error: { detail: 'El paciente indicado no existe.' },
          }),
      ),
    );
    fillValidForm();

    submit();

    expect(element.querySelector('.banner-error')?.textContent).toContain(
      'El paciente indicado no existe.',
    );
  });

  it('muestra el error del API debajo del campo cuando responde 400', () => {
    api.registerContact.and.returnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 400,
            error: { errors: { contactDate: ['La fecha del contacto no puede ser futura.'] } },
          }),
      ),
    );
    fillValidForm();

    submit();

    const field = element.querySelector('#contact-contactDate')!.closest('.field')!;
    expect(field.textContent).toContain('La fecha del contacto no puede ser futura.');
  });

  it('cancela la petición en curso si el componente se destruye', () => {
    const pending = new Subject<ContactResponse>();
    api.registerContact.and.returnValue(pending);
    fillValidForm();

    submit();
    expect(pending.observed).toBeTrue();

    fixture.destroy();

    expect(pending.observed).toBeFalse();
  });

  it('envía el contacto al paciente elegido con la fecha en ISO 8601 y emite el resultado', () => {
    const created = { id: 'c1', patientId: patient.id } as ContactResponse;
    api.registerContact.and.returnValue(of(created));
    const emitted: ContactResponse[] = [];
    fixture.componentInstance.registered.subscribe((c) => emitted.push(c));
    fillValidForm();

    submit();

    const [patientId, request] = api.registerContact.calls.mostRecent().args;
    expect(patientId).toBe(patient.id);
    expect(request.contactDate).toMatch(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$/);
    expect(request.observations).toBeNull();
    expect(emitted).toEqual([created]);
    expect(TestBed.inject(GestorContext).username()).toBe('gestor.demo');
  });

  it('avisa al pulsar Cancelar', () => {
    let cancelled = 0;
    fixture.componentInstance.cancelled.subscribe(() => cancelled++);

    const cancel = Array.from(element.querySelectorAll('button')).find((b) =>
      b.textContent?.includes('Cancelar'),
    );
    cancel!.click();

    expect(cancelled).toBe(1);
  });
});
