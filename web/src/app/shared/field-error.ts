import { Component, input } from '@angular/core';
import { AbstractControl } from '@angular/forms';

/** Muestra el error de un control: el que puso el API (`server`) o el de campo obligatorio. */
@Component({
  selector: 'app-field-error',
  template: `@if (message(); as text) {
    <p class="field-error" role="alert">{{ text }}</p>
  }`,
})
export class FieldError {
  readonly control = input.required<AbstractControl>();
  readonly requiredMessage = input('Este campo es obligatorio.');

  protected message(): string | null {
    const control = this.control();
    if (!control.errors || !(control.touched || control.dirty)) {
      return null;
    }
    if (typeof control.errors['server'] === 'string') {
      return control.errors['server'];
    }
    if (control.errors['required']) {
      return this.requiredMessage();
    }
    return null;
  }
}
