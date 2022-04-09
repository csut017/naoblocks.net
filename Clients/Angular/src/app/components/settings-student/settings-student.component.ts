import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';

@Component({
  selector: 'app-settings-student',
  templateUrl: './settings-student.component.html',
  styleUrls: ['./settings-student.component.scss']
})
export class SettingsStudentComponent implements OnInit {

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
