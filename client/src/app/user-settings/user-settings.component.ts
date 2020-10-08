import { Component, OnInit, EventEmitter, Output, Input } from '@angular/core';
import { UserSettings } from '../data/user-settings';

@Component({
  selector: 'app-user-settings',
  templateUrl: './user-settings.component.html',
  styleUrls: ['./user-settings.component.scss']
})
export class UserSettingsComponent {

  opened: boolean;
  settings: UserSettings = new UserSettings();
  errorMessage: string;

  @Input() showConfiguration: boolean = true;
  @Output() save = new EventEmitter<UserSettings>();

  constructor() { }

  show(settings: UserSettings): void {
    this.opened = true;
    this.errorMessage = undefined;
    this.settings = settings;
  }

  doSave(): void {
    this.save.emit(this.settings);
  }

  showError(msg: string): void {
    this.errorMessage = msg;
  }

  close(): void {
    this.opened = false;
  }

}
