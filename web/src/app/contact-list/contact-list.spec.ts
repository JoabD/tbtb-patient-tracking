import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Subject, of, throwError } from 'rxjs';

import { ContactResponse, PatientListItem } from '../models/api.models';
import { PatientApiService } from '../services/patient-api.service';
import { ContactList } from './contact-list';

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
  contactCount: 2,
  lastContact: null,
};

function contact(overrides: Partial<ContactResponse>): ContactResponse {
  return {
    id: 'c1',
    patientId: patient.id,
    gestorUsername: 'gestor.demo',
    contactDate: '2026-09-20T15:30:00Z',
    channel: 'Call',
    resultCode: 'Contacted',
    observations: null,
    createdAt: '2026-09-20T15:31:00Z',
    ...overrides,
  };
}

describe('ContactList', () => {
  let api: jasmine.SpyObj<PatientApiService>;
  let fixture: ComponentFixture<ContactList>;
  let element: HTMLElement;

  async function create(): Promise<void> {
    await TestBed.configureTestingModule({
      imports: [ContactList],
      providers: [{ provide: PatientApiService, useValue: api }],
    }).compileComponents();
    fixture = TestBed.createComponent(ContactList);
    fixture.componentRef.setInput('patient', patient);
    element = fixture.nativeElement;
    fixture.detectChanges();
  }

  beforeEach(() => {
    api = jasmine.createSpyObj<PatientApiService>('PatientApiService', ['listContacts']);
  });

  it('pide los contactos del paciente y los muestra en el orden recibido, con etiquetas en español', async () => {
    api.listContacts.and.returnValue(
      of([
        contact({ id: 'c2', channel: 'WhatsApp', resultCode: 'NoAnswer', observations: 'No respondió' }),
        contact({ id: 'c1', channel: 'Call', resultCode: 'Contacted' }),
      ]),
    );

    await create();

    expect(api.listContacts).toHaveBeenCalledOnceWith(patient.id);
    const rows = element.querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain('WhatsApp');
    expect(rows[0].textContent).toContain('No contesta');
    expect(rows[0].textContent).toContain('No respondió');
    expect(rows[1].textContent).toContain('Llamada');
    expect(rows[1].textContent).toContain('—');
  });

  it('avisa cuando el paciente no tiene contactos', async () => {
    api.listContacts.and.returnValue(of([]));

    await create();

    expect(element.querySelector('table')).toBeNull();
    expect(element.textContent).toContain('Este paciente todavía no tiene contactos.');
  });

  it('muestra un aviso general si el paciente ya no existe (404)', async () => {
    api.listContacts.and.returnValue(
      throwError(() => new HttpErrorResponse({ status: 404, error: { detail: 'El paciente indicado no existe.' } })),
    );

    await create();

    expect(element.querySelector('.banner-error')?.textContent).toContain('El paciente indicado no existe.');
  });

  it('cancela la petición en curso si el componente se destruye', async () => {
    const pending = new Subject<ContactResponse[]>();
    api.listContacts.and.returnValue(pending);
    await create();
    expect(pending.observed).toBeTrue();

    fixture.destroy();

    expect(pending.observed).toBeFalse();
  });
});
