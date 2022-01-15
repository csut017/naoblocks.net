import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { RunSettings } from 'src/app/data/run-settings';

@Component({
  selector: 'app-settings-debug',
  templateUrl: './settings-debug.component.html',
  styleUrls: ['./settings-debug.component.scss']
})
export class SettingsDebugComponent implements OnInit, OnChanges {

  @Output() closed = new EventEmitter<RunSettings | undefined>();
  @Input() settings: RunSettings = new RunSettings();
  
  errors: string[] = [];
  form: FormGroup;
  isSaving: boolean = false;

  constructor() { 
    this.form = new FormGroup({
      delayTime: new FormControl('0', [Validators.required]),
    });
  }

  ngOnChanges(_: SimpleChanges): void {
    this.form.setValue({
      delayTime: this.settings?.delay || 0
    });
  }

  ngOnInit(): void {
  }

  doSave() {
    let settings = new RunSettings();
    settings.delay = parseInt(this.form.get('delayTime')?.value || '0');
    this.closed.emit(settings);
  }

  doClose() {
    this.closed.emit(undefined);
  }

}
