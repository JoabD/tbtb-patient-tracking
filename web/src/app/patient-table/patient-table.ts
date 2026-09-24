import { DatePipe } from '@angular/common';
import { Component, DestroyRef, OnInit, computed, inject, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subscription } from 'rxjs';

import {
  CHANNELS,
  COUNTRIES,
  DOCUMENT_TYPES,
  PagedResponse,
  PatientListItem,
  RESULTS,
  TRACKING_STATUSES,
  labelOf,
} from '../models/api.models';
import { PatientApiService } from '../services/patient-api.service';
import { toApiError } from '../shared/api-error';

/** Tabla de solo lectura con el resumen de contactos de cada paciente (verificación visual de CA-1 y CA-2). */
@Component({
  selector: 'app-patient-table',
  imports: [DatePipe],
  templateUrl: './patient-table.html',
})
export class PatientTable implements OnInit {
  private readonly api = inject(PatientApiService);
  private readonly destroyRef = inject(DestroyRef);

  /** Petición de lista en curso. Se cancela al pedir otra o al destruir el componente. */
  private request?: Subscription;

  /** El gestor quiere registrar un contacto para este paciente. */
  readonly contactRequested = output<PatientListItem>();
  /** El gestor quiere ver todos los contactos de este paciente. */
  readonly contactsRequested = output<PatientListItem>();

  protected readonly pageSize = 25;
  protected readonly page = signal(1);
  protected readonly data = signal<PagedResponse<PatientListItem> | null>(null);
  protected readonly loading = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly totalPages = computed(() =>
    Math.max(1, Math.ceil((this.data()?.totalCount ?? 0) / this.pageSize)),
  );

  ngOnInit(): void {
    this.reload(1);
  }

  /** Vuelve a pedir la lista. Sin argumento conserva la página actual. */
  reload(page: number = this.page()): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    // Si todavía hay una petición en curso se cancela: así una respuesta lenta no pisa a una más reciente.
    this.request?.unsubscribe();
    this.request = this.api
      .listPatients(page, this.pageSize)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.data.set(result);
          this.page.set(result.page);
          this.loading.set(false);
        },
        error: (error: unknown) => {
          this.errorMessage.set(toApiError(error).message);
          this.loading.set(false);
        },
      });
  }

  protected goTo(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.reload(page);
    }
  }

  protected documentLabel(patient: PatientListItem): string {
    return `${labelOf(DOCUMENT_TYPES, patient.documentType)} ${patient.documentNumber}`;
  }

  protected countryLabel(code: string): string {
    return labelOf(COUNTRIES, code);
  }

  protected trackingLabel(status: string): string {
    return labelOf(TRACKING_STATUSES, status);
  }

  protected channelLabel(channel: string): string {
    return labelOf(CHANNELS, channel);
  }

  protected resultLabel(result: string): string {
    return labelOf(RESULTS, result);
  }
}
