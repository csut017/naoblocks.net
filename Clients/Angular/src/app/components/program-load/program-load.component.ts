import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';

@Component({
  selector: 'app-program-load',
  templateUrl: './program-load.component.html',
  styleUrls: ['./program-load.component.scss']
})
export class ProgramLoadComponent implements OnInit {

  @Output() closed = new EventEmitter<boolean>();

  errors: string[] = [];
  form: FormGroup;
  isSaving: boolean = false;

  constructor() { 
    this.form = new FormGroup({
      name: new FormControl('', [Validators.required]),
      type: new FormControl('', []),
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
