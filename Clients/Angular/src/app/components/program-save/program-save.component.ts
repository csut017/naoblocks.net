import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { UntypedFormGroup, UntypedFormControl, Validators } from '@angular/forms';

@Component({
  selector: 'app-program-save',
  templateUrl: './program-save.component.html',
  styleUrls: ['./program-save.component.scss']
})
export class ProgramSaveComponent implements OnInit {

  @Output() closed = new EventEmitter<boolean>();

  errors: string[] = [];
  form: UntypedFormGroup;
  isSaving: boolean = false;

  constructor() { 
    this.form = new UntypedFormGroup({
      name: new UntypedFormControl('', [Validators.required]),
      type: new UntypedFormControl('', []),
    });
  }

  ngOnInit(): void {
  }

  doSave() {
  }

  doClose() {
    this.closed.emit(false);
  }

}
