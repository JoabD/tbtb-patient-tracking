import { AfterViewInit, Component, ElementRef, input, output, viewChild } from '@angular/core';

/**
 * Ventana modal sobre el elemento nativo <dialog>: el navegador se encarga del foco, de la tecla Esc
 * y del fondo. Quien la usa la crea con @if y la destruye al recibir `closed`, así el contenido
 * (por ejemplo un formulario) nace limpio cada vez que se abre.
 */
@Component({
  selector: 'app-modal',
  templateUrl: './modal.html',
})
export class Modal implements AfterViewInit {
  readonly title = input.required<string>();
  /** Modal más ancho, para tablas. */
  readonly wide = input(false);
  readonly closed = output<void>();

  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');

  ngAfterViewInit(): void {
    const dialog = this.dialog().nativeElement;
    dialog.showModal();

    // Un clic sobre el fondo (el propio <dialog>, no su contenido) cierra la ventana. Se registra aquí
    // y no en la plantilla porque es una comodidad para el ratón: con teclado se cierra con Esc.
    dialog.addEventListener('click', (event) => {
      if (event.target === dialog) {
        this.close();
      }
    });

    // Foco inicial en el primer campo del formulario (en lugar del atributo autofocus).
    dialog.querySelector<HTMLElement>('input, select, textarea')?.focus();
  }

  protected close(): void {
    this.dialog().nativeElement.close();
  }
}
