import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Subject, of, throwError } from 'rxjs';

import { PagedResponse, PatientListItem } from '../models/api.models';
import { PatientApiService } from '../services/patient-api.service';
import { PatientTable } from './patient-table';

function patient(overrides: Partial<PatientListItem>): PatientListItem {
  return {
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
    contactCount: 0,
    lastContact: null,
    ...overrides,
  };
}

function page(items: PatientListItem[], totalCount = items.length): PagedResponse<PatientListItem> {
  return { items, page: 1, pageSize: 25, totalCount };
}

function buttonWithText(root: Element, text: string): HTMLButtonElement {
  return Array.from(root.querySelectorAll('button')).find((b) => b.textContent?.includes(text)) as HTMLButtonElement;
}

describe('PatientTable', () => {
  let api: jasmine.SpyObj<PatientApiService>;
  let fixture: ComponentFixture<PatientTable>;
  let element: HTMLElement;

  async function create(): Promise<void> {
    await TestBed.configureTestingModule({
      imports: [PatientTable],
      providers: [{ provide: PatientApiService, useValue: api }],
    }).compileComponents();
    fixture = TestBed.createComponent(PatientTable);
    element = fixture.nativeElement;
    fixture.detectChanges();
  }

  beforeEach(() => {
    api = jasmine.createSpyObj<PatientApiService>('PatientApiService', ['listPatients']);
  });

  it('muestra solo el último contacto de cada paciente y el total dentro del botón', async () => {
    api.listPatients.and.returnValue(
      of(
        page([
          patient({
            contactCount: 3,
            lastContact: {
              contactDate: '2026-09-20T15:30:00Z',
              channel: 'WhatsApp',
              resultCode: 'NoAnswer',
              gestorUsername: 'gestor.demo',
            },
          }),
          patient({ id: 'p2', fullName: 'Luis Quispe', contactCount: 0 }),
        ]),
      ),
    );

    await create();

    const rows = element.querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain('Cédula 1032456789');
    expect(rows[0].textContent).toContain('WhatsApp · No contesta · gestor.demo');
    expect(rows[0].textContent).toContain('Ver contactos (3)');
    expect(rows[1].textContent).toContain('Sin contactos');
  });

  it('pide la primera página de 25 al iniciar', async () => {
    api.listPatients.and.returnValue(of(page([])));

    await create();

    expect(api.listPatients).toHaveBeenCalledOnceWith(1, 25);
    expect(element.textContent).toContain('Todavía no hay pacientes registrados.');
  });

  it('deshabilita registrar contacto en un paciente inactivo y lo emite en uno activo', async () => {
    const active = patient({ id: 'active' });
    const inactive = patient({ id: 'inactive', fullName: 'Marta Vera', isActive: false });
    api.listPatients.and.returnValue(of(page([active, inactive])));
    await create();
    const emitted: PatientListItem[] = [];
    fixture.componentInstance.contactRequested.subscribe((p) => emitted.push(p));

    const rows = element.querySelectorAll('tbody tr');
    expect(buttonWithText(rows[1], 'Registrar contacto').disabled).toBeTrue();
    buttonWithText(rows[0], 'Registrar contacto').click();

    expect(emitted).toEqual([active]);
  });

  it('deshabilita ver contactos si el paciente no tiene y lo emite si tiene', async () => {
    const withContacts = patient({ id: 'with', contactCount: 2 });
    const without = patient({ id: 'without', contactCount: 0 });
    api.listPatients.and.returnValue(of(page([withContacts, without])));
    await create();
    const emitted: PatientListItem[] = [];
    fixture.componentInstance.contactsRequested.subscribe((p) => emitted.push(p));

    const rows = element.querySelectorAll('tbody tr');
    expect(buttonWithText(rows[1], 'Ver contactos').disabled).toBeTrue();
    buttonWithText(rows[0], 'Ver contactos').click();

    expect(emitted).toEqual([withContacts]);
  });

  it('muestra un aviso general si el API no responde', async () => {
    api.listPatients.and.returnValue(throwError(() => new HttpErrorResponse({ status: 0 })));

    await create();

    expect(element.querySelector('.banner-error')?.textContent).toContain('No se pudo conectar con el servidor');
  });

  it('cancela la petición anterior al pedir otra y no deja que una respuesta vieja pise a la nueva', async () => {
    const first = new Subject<PagedResponse<PatientListItem>>();
    const second = new Subject<PagedResponse<PatientListItem>>();
    api.listPatients.and.returnValues(first, second);
    await create();

    fixture.componentInstance.reload(2);
    expect(first.observed).toBeFalse();

    second.next(page([patient({ fullName: 'Respuesta nueva' })]));
    fixture.detectChanges();
    first.next(page([patient({ fullName: 'Respuesta vieja' })]));
    fixture.detectChanges();

    expect(element.textContent).toContain('Respuesta nueva');
    expect(element.textContent).not.toContain('Respuesta vieja');
  });

  it('cancela la petición en curso si el componente se destruye', async () => {
    const pending = new Subject<PagedResponse<PatientListItem>>();
    api.listPatients.and.returnValue(pending);
    await create();
    expect(pending.observed).toBeTrue();

    fixture.destroy();

    expect(pending.observed).toBeFalse();
  });

  it('calcula las páginas a partir del total', async () => {
    api.listPatients.and.returnValue(of(page([patient({})], 60)));

    await create();

    expect(element.querySelector('.pager')?.textContent).toContain('Página 1 de 3');
  });
});
