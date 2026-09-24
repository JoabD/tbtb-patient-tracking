import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import {
  ContactResponse,
  PagedResponse,
  PatientListItem,
  PatientResponse,
  RegisterContactRequest,
  RegisterPatientRequest,
} from '../models/api.models';

/**
 * Único punto de acceso al API. Los componentes no llaman a HttpClient directamente.
 * La URL es relativa: en desarrollo el servidor de Angular la redirige al API (proxy.conf.json).
 */
@Injectable({ providedIn: 'root' })
export class PatientApiService {
  private readonly http = inject(HttpClient);
  private readonly patientsUrl = '/api/patients';

  listPatients(page: number, pageSize: number): Observable<PagedResponse<PatientListItem>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResponse<PatientListItem>>(this.patientsUrl, { params });
  }

  registerPatient(request: RegisterPatientRequest): Observable<PatientResponse> {
    return this.http.post<PatientResponse>(this.patientsUrl, request);
  }

  registerContact(patientId: string, request: RegisterContactRequest): Observable<ContactResponse> {
    return this.http.post<ContactResponse>(`${this.patientsUrl}/${patientId}/contacts`, request);
  }
}
