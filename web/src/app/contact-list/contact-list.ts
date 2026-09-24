import { DatePipe } from '@angular/common';
import { Component, DestroyRef, OnInit, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { CHANNELS, ContactResponse, PatientListItem, RESULTS, labelOf } from '../models/api.models';
import { PatientApiService } from '../services/patient-api.service';
import { toApiError } from '../shared/api-error';

/** Lista completa de contactos vigentes de un paciente, del más reciente al más antiguo. */
@Component({
  selector: 'app-contact-list',
  imports: [DatePipe],
  templateUrl: './contact-list.html',
})
export class ContactList implements OnInit {
  private readonly api = inject(PatientApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly patient = input.required<PatientListItem>();

  protected readonly contacts = signal<ContactResponse[]>([]);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.api
      .listContacts(this.patient().id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (contacts) => {
          this.contacts.set(contacts);
          this.loading.set(false);
        },
        error: (error: unknown) => {
          this.errorMessage.set(toApiError(error).message);
          this.loading.set(false);
        },
      });
  }

  protected channelLabel(channel: string): string {
    return labelOf(CHANNELS, channel);
  }

  protected resultLabel(result: string): string {
    return labelOf(RESULTS, result);
  }
}
