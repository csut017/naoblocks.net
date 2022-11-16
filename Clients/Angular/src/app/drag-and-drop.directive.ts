import { Directive, EventEmitter, HostBinding, HostListener, Output } from '@angular/core';

@Directive({
  selector: '[appDragAndDrop]'
})
export class DragAndDropDirective {

  @HostBinding('class.file-over') isFileOver: boolean = false;
  @Output() fileDropped = new EventEmitter<any>();

  constructor() { }

  @HostListener('dragover', ['$event']) onDragOver(evt: any) {
    evt.preventDefault();
    evt.stopPropagation();
    this.isFileOver = true;
    console.log('[DragAndDrop] Drag over');
  }

  @HostListener('dragleave', ['$event']) onDragLeave(evt: any) {
    evt.preventDefault();
    evt.stopPropagation();
    this.isFileOver = false;
    console.log('[DragAndDrop] Drag leave');
  }

  @HostListener('drop', ['$event']) onDrop(evt: any) {
    evt.preventDefault();
    evt.stopPropagation();
    this.isFileOver = false;
    const files = evt.dataTransfer.files;
    if (files.length) {
      console.log(`[DragAndDrop] Dropped ${files.length} files`);
      this.fileDropped.emit(files);
    }
  }

}
