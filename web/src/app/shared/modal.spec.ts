import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Modal } from './modal';

describe('Modal', () => {
  let fixture: ComponentFixture<Modal>;
  let dialog: HTMLDialogElement;

  const nextTask = () => new Promise((resolve) => setTimeout(resolve, 20));

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [Modal] }).compileComponents();
    fixture = TestBed.createComponent(Modal);
    fixture.componentRef.setInput('title', 'Nuevo paciente');
    fixture.detectChanges();
    dialog = fixture.nativeElement.querySelector('dialog');
  });

  it('se abre como modal con su título al crearse', () => {
    expect(dialog.open).toBeTrue();
    expect(dialog.matches(':modal')).toBeTrue();
    expect(dialog.querySelector('h2')?.textContent).toContain('Nuevo paciente');
  });

  it('emite closed al pulsar la equis', async () => {
    let closed = 0;
    fixture.componentInstance.closed.subscribe(() => closed++);

    dialog.querySelector<HTMLButtonElement>('.modal-close')!.click();
    await nextTask();

    expect(dialog.open).toBeFalse();
    expect(closed).toBe(1);
  });

  it('se cierra al hacer clic sobre el fondo, pero no al hacer clic dentro del contenido', async () => {
    let closed = 0;
    fixture.componentInstance.closed.subscribe(() => closed++);

    dialog.querySelector<HTMLElement>('.modal-box')!.click();
    await nextTask();
    expect(closed).toBe(0);

    dialog.click();
    await nextTask();
    expect(closed).toBe(1);
  });
});
