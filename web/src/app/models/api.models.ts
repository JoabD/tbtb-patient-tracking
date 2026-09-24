// Modelos tipados del API. Los valores de los catálogos son los mismos que guarda el backend (en inglés);
// las etiquetas en español solo existen aquí, en la interfaz.

export type DocumentType = 'Cedula' | 'Dni' | 'Passport';
export type CountryCode = 'CO' | 'PE' | 'EC';
export type ContactChannel = 'Call' | 'WhatsApp' | 'Email';
export type ContactResult = 'Contacted' | 'NoAnswer' | 'WrongNumber';
export type TrackingStatus = 'Reachable' | 'Unreachable';

export interface CatalogOption<T extends string = string> {
  value: T;
  label: string;
}

export const DOCUMENT_TYPES: CatalogOption<DocumentType>[] = [
  { value: 'Cedula', label: 'Cédula' },
  { value: 'Dni', label: 'DNI' },
  { value: 'Passport', label: 'Pasaporte' },
];

export const COUNTRIES: CatalogOption<CountryCode>[] = [
  { value: 'CO', label: 'Colombia' },
  { value: 'PE', label: 'Perú' },
  { value: 'EC', label: 'Ecuador' },
];

export const CHANNELS: CatalogOption<ContactChannel>[] = [
  { value: 'Call', label: 'Llamada' },
  { value: 'WhatsApp', label: 'WhatsApp' },
  { value: 'Email', label: 'Correo' },
];

export const RESULTS: CatalogOption<ContactResult>[] = [
  { value: 'Contacted', label: 'Contactado' },
  { value: 'NoAnswer', label: 'No contesta' },
  { value: 'WrongNumber', label: 'Número equivocado' },
];

export const TRACKING_STATUSES: CatalogOption<TrackingStatus>[] = [
  { value: 'Reachable', label: 'Localizable' },
  { value: 'Unreachable', label: 'Ilocalizable' },
];

/** Devuelve la etiqueta en español de un valor de catálogo; si no la conoce, devuelve el valor tal cual. */
export function labelOf(options: readonly CatalogOption[], value: string): string {
  return options.find((option) => option.value === value)?.label ?? value;
}

export interface RegisterPatientRequest {
  fullName: string;
  documentType: string;
  documentNumber: string;
  country: string;
  city: string;
  phone: string;
  email: string | null;
  /** Fecha sin hora, formato yyyy-MM-dd. */
  treatmentStartDate: string;
  privacyAccepted: boolean;
  gestorUsername: string;
}

export interface PatientResponse {
  id: string;
  fullName: string;
  documentType: string;
  documentNumber: string;
  country: string;
  city: string;
  phone: string;
  email: string | null;
  treatmentStartDate: string;
  trackingStatus: string;
  isActive: boolean;
  createdAt: string;
}

export interface LastContact {
  contactDate: string;
  channel: string;
  resultCode: string;
  gestorUsername: string;
}

export interface PatientListItem extends PatientResponse {
  contactCount: number;
  lastContact: LastContact | null;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface RegisterContactRequest {
  gestorUsername: string;
  /** Fecha y hora en formato ISO 8601 con zona horaria. */
  contactDate: string;
  channel: string;
  resultCode: string;
  observations: string | null;
}

export interface ContactResponse {
  id: string;
  patientId: string;
  gestorUsername: string;
  contactDate: string;
  channel: string;
  resultCode: string;
  observations: string | null;
  createdAt: string;
}
