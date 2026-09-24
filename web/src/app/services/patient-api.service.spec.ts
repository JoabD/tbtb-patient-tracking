import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { RegisterContactRequest, RegisterPatientRequest } from '../models/api.models';
import { PatientApiService } from './patient-api.service';

describe('PatientApiService', () => {
  let service: PatientApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(PatientApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('lista pacientes con página y tamaño como parámetros', () => {
    service.listPatients(2, 25).subscribe();

    const req = http.expectOne((r) => r.url === '/api/patients');
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('page')).toBe('2');
    expect(req.request.params.get('pageSize')).toBe('25');
    req.flush({ items: [], page: 2, pageSize: 25, totalCount: 0 });
  });

  it('registra un paciente con POST /api/patients', () => {
    const body = { fullName: 'Ana' } as RegisterPatientRequest;
    service.registerPatient(body).subscribe();

    const req = http.expectOne('/api/patients');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(body);
    req.flush({});
  });

  it('registra un contacto con POST /api/patients/{id}/contacts', () => {
    const body = { channel: 'Call' } as RegisterContactRequest;
    service.registerContact('abc-123', body).subscribe();

    const req = http.expectOne('/api/patients/abc-123/contacts');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(body);
    req.flush({});
  });
});
