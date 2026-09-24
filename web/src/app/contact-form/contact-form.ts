import { Component, DestroyRef, effect, inject, input, output, signal, untracked } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import {
  CHANNELS,
  ContactResponse,
  PatientListItem,
  RESULTS,
  RegisterContactRequest,
} from '../models/api.models';
import { PatientApiService } from '../services/patient-api.service';
import { applyApiError, toApiError } from '../shared/api-error';
import { FieldError } from '../shared/field-error';

/** Fecha y hora local con el formato que espera <input type="datetime-local"> (yyyy-MM-ddTHH:mm). */
function nowForInput(): string {
  const now = new Date();
  const local = new Date(now.getTime() - now.getTimezoneOffset() * 60_000);
  return local.toISOString().slice(0, 16);
}

/** CA-2: formulario de registro de contacto para el paciente elegido en la tabla. */
@Component({
  selector: 'app-contact-form',
  imports: [ReactiveFormsModule, FieldError],
  templateUrl: './contact-form.html',
})
export class ContactForm {
  private readonly api = inject(PatientApiService);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly patient = input.required<PatientListItem>();
  readonly registered = output<ContactResponse>();
  readonly cancelled = output<void>();

  protected readonly channels = CHANNELS;
  protected readonly results = RESULTS;
  protected readonly maxDate = nowForInput();

  protected readonly form = this.fb.group({
    gestorUsername: ['', Validators.required],
    contactDate: [nowForInput(), Validators.required],
    channel: ['', Validators.required],
    resultCode: ['', Validators.required],
    observations: [''],
  });

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly successMessage = signal<string | null>(null);

  constructor() {
    // Al elegir otro paciente se limpian los mensajes del anterior.
    effect(() => {
      this.patient();
      untracked(() => {
        this.errorMessage.set(null);
        this.successMessage.set(null);
      });
    });
  }

  protected submit(): void {
    this.successMessage.set(null);
    this.errorMessage.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request: RegisterContactRequest = {
      gestorUsername: value.gestorUsername,
      // El navegador entrega la hora local sin zona; se envía como instante ISO 8601.
      contactDate: new Date(value.contactDate).toISOString(),
      channel: value.channel,
      resultCode: value.resultCode,
      observations: value.observations.trim() || null,
    };

    this.submitting.set(true);
    // takeUntilDestroyed cancela la petición si el componente se destruye antes de que responda.
    this.api
      .registerContact(this.patient().id, request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (contact) => {
          this.submitting.set(false);
          this.successMessage.set('Contacto registrado correctamente.');
          this.form.reset({ gestorUsername: value.gestorUsername, contactDate: nowForInput() });
          this.registered.emit(contact);
        },
        error: (error: unknown) => {
          this.submitting.set(false);
          this.errorMessage.set(applyApiError(this.form, toApiError(error)));
        },
      });
  }
}
