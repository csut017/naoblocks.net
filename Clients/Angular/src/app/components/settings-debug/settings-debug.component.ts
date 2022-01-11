import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';

@Component({
  selector: 'app-settings-debug',
  templateUrl: './settings-debug.component.html',
  styleUrls: ['./settings-debug.component.scss']
})
export class SettingsDebugComponent implements OnInit {

  @Output() closed = new EventEmitter<boolean>();
  
  errors: string[] = [];
  form: FormGroup;
  isSaving: boolean = false;

  constructor() { 
    this.form = new FormGroup({
      delayTime: new FormControl('0', [Validators.required]),
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
