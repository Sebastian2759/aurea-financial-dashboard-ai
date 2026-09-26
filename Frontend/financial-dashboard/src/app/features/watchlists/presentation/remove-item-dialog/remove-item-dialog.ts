import { Component, ElementRef, afterNextRender, input, output, viewChild } from '@angular/core';
@Component({ selector: 'app-remove-item-dialog', templateUrl: './remove-item-dialog.html' })
export class RemoveItemDialog {
  readonly busy = input(false);
  readonly confirm = output<void>();
  readonly cancel = output<void>();
  private dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');
  constructor() {
    afterNextRender(() => this.dialog().nativeElement.showModal());
  }
}
