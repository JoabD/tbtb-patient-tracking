import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { of } from 'rxjs';

import { App } from './app';
import {
  ContactResponse,
  PagedResponse,
  PatientListItem,
  PatientResponse,
} from './models/api.models';
import { PatientForm } from './patient-form/patient-form';
import { PatientApiService } from './services/patient-api.service';

const patient: PatientListItem = {
  id: 'p1',
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
  contactCount: 1,
  lastContact: {
    contactDate: '2026-09-20T15:30:00Z',
    channel: 'Call',
    resultCode: 'Contacted',
    gestorUsername: 'gestor.demo',
  },
};

const list: PagedResponse<PatientListItem> = {
  items: [patient],
  page: 1,
  pageSize: 25,
  totalCount: 1,
};

describe('App', () => {
  let api: jasmine.SpyObj<PatientApiService>;
  let fixture: ComponentFixture<App>;
  let element: HTMLElement;

  beforeEach(async () => {
    api = jasmine.createSpyObj<PatientApiService>('PatientApiService', [
      'listPatients',
      'registerPatient',
      'registerContact',
      'listContacts',
    ]);
    api.listPatients.and.returnValue(of(list));
    api.listContacts.and.returnValue(
      of([
        {
          id: 'c1',
          patientId: 'p1',
          gestorUsername: 'gestor.demo',
          contactDate: '2026-09-20T15:30:00Z',
          channel: 'Call',
          resultCode: 'Contacted',
          observations: null,
          createdAt: '2026-09-20T15:31:00Z',
        } satisfies ContactResponse,
      ]),
    );

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [{ provide: PatientApiService, useValue: api }],
    }).compileComponents();

    fixture = TestBed.createComponent(App);
    element = fixture.nativeElement;
    fixture.detectChanges();
  });

  function button(text: string, root: Element = element): HTMLButtonElement {
    return Array.from(root.querySelectorAll('button')).find((b) =>
      b.textContent?.includes(text),
    ) as HTMLButtonElement;
  }

  it('muestra al iniciar solo la lista de pacientes y el botón de nuevo paciente, sin ningún modal', () => {
    expect(element.querySelector('dialog')).toBeNull();
    expect(element.querySelectorAll('tbody tr').length).toBe(1);
    expect(button('Nuevo paciente')).toBeTruthy();
  });

  it('abre el formulario de nuevo paciente en un modal', () => {
    button('Nuevo paciente').click();
    fixture.detectChanges();

    const dialog = element.querySelector('dialog')!;
    expect(dialog.open).toBeTrue();
    expect(dialog.querySelector('h2')?.textContent).toContain('Nuevo paciente');
    expect(dialog.querySelector('app-patient-form')).not.toBeNull();
  });

  it('al registrar un paciente cierra el modal, avisa y recarga la primera página', () => {
    button('Nuevo paciente').click();
    fixture.detectChanges();
    api.listPatients.calls.reset();

    const created = { fullName: 'Marta Vera' } as PatientResponse;
    fixture.debugElement
      .query(By.directive(PatientForm))
      .componentInstance.registered.emit(created);
    fixture.detectChanges();

    expect(element.querySelector('dialog')).toBeNull();
    expect(element.querySelector('.notice')?.textContent).toContain(
      'Paciente Marta Vera registrado correctamente.',
    );
    expect(api.listPatients).toHaveBeenCalledOnceWith(1, 25);
  });

  it('abre el formulario de contacto del paciente elegido en un modal', () => {
    button('Registrar contacto').click();
    fixture.detectChanges();

    const dialog = element.querySelector('dialog')!;
    expect(dialog.open).toBeTrue();
    expect(dialog.querySelector('h2')?.textContent).toContain('Registrar contacto');
    expect(dialog.textContent).toContain('Ana Rodríguez');
  });

  it('abre la lista completa de contactos del paciente en un modal', () => {
    button('Ver contactos').click();
    fixture.detectChanges();

    const dialog = element.querySelector('dialog')!;
    expect(dialog.open).toBeTrue();
    expect(dialog.querySelector('h2')?.textContent).toContain('Contactos del paciente');
    expect(api.listContacts).toHaveBeenCalledOnceWith('p1');
    expect(dialog.querySelectorAll('tbody tr').length).toBe(1);
  });
});
