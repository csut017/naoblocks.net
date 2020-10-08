import { Directive, EventEmitter, HostBinding, HostListener, Output } from '@angular/core';
import { UserSettings } from './data/user-settings';

@Directive({
  selector: '[appDragAndDrop]'
})
export class DragAndDropDirective {

  @HostBinding('class.file-over') isFileOver: boolean = false;
  @Output() fileDropped = new EventEmitter<any>();

  constructor() { }

  @HostListener('dragover', ['$event']) onDragOver(evt) {
    evt.preventDefault();
    evt.stopPropagation();
    this.isFileOver = true;
    console.log('[DragAndDrop] Drag over');
  }

  @HostListener('dragleave', ['$event']) onDragLeave(evt) {
    evt.preventDefault();
    evt.stopPropagation();
    this.isFileOver = false;
    console.log('[DragAndDrop] Drag leave');
  }

  @HostListener('drop', ['$event']) onDrop(evt) {
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
