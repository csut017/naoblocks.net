import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { RunSettings } from '../data/run-settings';

@Component({
  selector: 'app-run-settings',
  templateUrl: './run-settings.component.html',
  styleUrls: ['./run-settings.component.scss']
})
export class RunSettingsComponent {

  settings: RunSettings = new RunSettings();
  opened: boolean;
  errorMessage: string;

  @Output() updated = new EventEmitter<RunSettings>();

  constructor() { }

  show(settings: RunSettings): void {
    this.opened = true;
    this.errorMessage = undefined;
    this.settings = JSON.parse(JSON.stringify(settings));
  }

  doSave(): void {
    this.updated.emit(this.settings);
  }

  showError(msg: string): void {
    this.errorMessage = msg;
  }

  close(): void {
    this.opened = false;
  }

}
