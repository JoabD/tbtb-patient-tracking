import { Component, DestroyRef, inject, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { COUNTRIES, DOCUMENT_TYPES, PatientResponse, RegisterPatientRequest } from '../models/api.models';
import { GestorContext } from '../services/gestor-context';
import { PatientApiService } from '../services/patient-api.service';
import { applyApiError, toApiError } from '../shared/api-error';
import { FieldError } from '../shared/field-error';

/** CA-1: formulario de registro de paciente. La validación de negocio vive en el API; aquí solo se marca lo obligatorio. */
@Component({
  selector: 'app-patient-form',
  imports: [ReactiveFormsModule, FieldError],
  templateUrl: './patient-form.html',
})
export class PatientForm {
  private readonly api = inject(PatientApiService);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly gestor = inject(GestorContext);

  readonly registered = output<PatientResponse>();
  readonly cancelled = output<void>();

  protected readonly documentTypes = DOCUMENT_TYPES;
  protected readonly countries = COUNTRIES;

  protected readonly form = this.fb.group({
    fullName: ['', Validators.required],
    documentType: ['', Validators.required],
    documentNumber: ['', Validators.required],
    country: ['', Validators.required],
    city: ['', Validators.required],
    phone: ['', Validators.required],
    email: [''],
    treatmentStartDate: ['', Validators.required],
    privacyAccepted: [false, Validators.requiredTrue],
    gestorUsername: [this.gestor.username(), Validators.required],
  });

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected submit(): void {
    this.errorMessage.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request: RegisterPatientRequest = { ...value, email: value.email.trim() || null };

    this.submitting.set(true);
    // takeUntilDestroyed cancela la petición si el componente se destruye antes de que responda.
    this.api.registerPatient(request).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (patient) => {
        this.submitting.set(false);
        this.gestor.username.set(value.gestorUsername.trim());
        this.registered.emit(patient);
      },
      error: (error: unknown) => {
        this.submitting.set(false);
        this.errorMessage.set(applyApiError(this.form, toApiError(error)));
      },
    });
  }
}
