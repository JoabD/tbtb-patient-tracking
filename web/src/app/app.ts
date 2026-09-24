import { Component, signal, viewChild } from '@angular/core';

import { ContactForm } from './contact-form/contact-form';
import { PatientListItem } from './models/api.models';
import { PatientForm } from './patient-form/patient-form';
import { PatientTable } from './patient-table/patient-table';

@Component({
  selector: 'app-root',
  imports: [PatientForm, ContactForm, PatientTable],
  templateUrl: './app.html',
})
export class App {
  private readonly table = viewChild.required(PatientTable);

  /** Paciente elegido en la tabla para registrarle un contacto. */
  protected readonly selectedPatient = signal<PatientListItem | null>(null);

  // Un paciente nuevo es el más reciente, así que se vuelve a la primera página para verlo arriba.
  protected onPatientRegistered(): void {
    this.table().reload(1);
  }

  protected onContactRegistered(): void {
    this.table().reload();
  }
}
