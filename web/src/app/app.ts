import { Component, signal, viewChild } from '@angular/core';

import { ContactForm } from './contact-form/contact-form';
import { ContactList } from './contact-list/contact-list';
import { PatientListItem, PatientResponse } from './models/api.models';
import { PatientForm } from './patient-form/patient-form';
import { PatientTable } from './patient-table/patient-table';
import { Modal } from './shared/modal';

@Component({
  selector: 'app-root',
  imports: [PatientForm, ContactForm, ContactList, PatientTable, Modal],
  templateUrl: './app.html',
})
export class App {
  private readonly table = viewChild.required(PatientTable);

  protected readonly newPatientOpen = signal(false);
  /** Paciente al que se le está registrando un contacto (modal abierto si no es null). */
  protected readonly contactFormPatient = signal<PatientListItem | null>(null);
  /** Paciente cuya lista de contactos se está viendo (modal abierto si no es null). */
  protected readonly contactListPatient = signal<PatientListItem | null>(null);
  /** Aviso de éxito que queda visible al cerrarse el modal. */
  protected readonly notice = signal<string | null>(null);

  protected onPatientRegistered(patient: PatientResponse): void {
    this.newPatientOpen.set(false);
    this.notice.set(`Paciente ${patient.fullName} registrado correctamente.`);
    // Un paciente nuevo es el más reciente, así que se vuelve a la primera página para verlo arriba.
    this.table().reload(1);
  }

  protected onContactRegistered(patient: PatientListItem): void {
    this.contactFormPatient.set(null);
    this.notice.set(`Contacto registrado para ${patient.fullName}.`);
    this.table().reload();
  }
}
