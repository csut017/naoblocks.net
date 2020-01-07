import { Component, OnInit, EventEmitter, Output } from '@angular/core';
import { UserSettings } from '../data/user-settings';

class interfaceHelper {
  constructor(private settings: UserSettings) {}

  get simple(): boolean {
    return this.settings.simple;
  }

  set simple(value: boolean) {
    this.settings.simple = value;
    this.settings.events = false;
  }

  get default(): boolean {
    return !this.settings.simple && !this.settings.events;
  }

  set default(value: boolean) {
    this.settings.simple = !value;
    this.settings.events = false;
  }

  get events(): boolean {
    return this.settings.events;
  }

  set events(value: boolean) {
    this.settings.simple = false;
    this.settings.events = value;
  }
}

@Component({
  selector: 'app-user-settings',
  templateUrl: './user-settings.component.html',
  styleUrls: ['./user-settings.component.scss']
})
export class UserSettingsComponent {

  opened: boolean;
  settings: UserSettings = new UserSettings();
  errorMessage: string;
  interfaces: interfaceHelper = new interfaceHelper(this.settings);

  @Output() save = new EventEmitter<UserSettings>();

  constructor() { }

  show(settings: UserSettings): void {
    this.opened = true;
    this.errorMessage = undefined;
    this.settings = settings;
    this.interfaces = new interfaceHelper(this.settings);
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
