import { Component, OnInit, Input } from '@angular/core';
import { UserSettings } from '../data/user-settings';
import { RobotTypeService } from '../services/robot-type.service';
import { RobotType } from '../data/robot-type';

class InterfaceHelper {
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
  selector: 'app-user-settings-editor',
  templateUrl: './user-settings-editor.component.html',
  styleUrls: ['./user-settings-editor.component.scss']
})
export class UserSettingsEditorComponent implements OnInit {

  interfaces: InterfaceHelper;
  types: RobotType[] = [];

  private _settings: UserSettings;

  get settings(): UserSettings {
    return this._settings;
  }

  @Input() set settings(value: UserSettings) {
    this._settings = value;
    this.interfaces = new InterfaceHelper(this.settings);
  }

  constructor(private robotTypeService: RobotTypeService) { }

  ngOnInit() {
    this.robotTypeService.list()
      .subscribe(results => {
        this.types = results.items;
      });
  }

}
