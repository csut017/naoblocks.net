import { Component, OnInit, Input, EventEmitter } from '@angular/core';
import { UserSettings } from '../data/user-settings';
import { RobotTypeService } from '../services/robot-type.service';
import { RobotType } from '../data/robot-type';
import { RobotService } from '../services/robot.service';
import { Robot } from '../data/robot';
import { BlockSet } from '../data/block-set';

class InterfaceHelper {
  allocationChanged: EventEmitter<number> = new EventEmitter<number>();
  constructor(private settings: UserSettings) { }

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

  get requireRobot(): boolean {
    return this.settings.allocationMode == 1;
  }

  set requireRobot(value: boolean) {
    this.settings.allocationMode = value ? 1 : 0;
    this.allocationChanged.emit(this.settings.allocationMode);
  }

  get preferRobot(): boolean {
    return this.settings.allocationMode == 2;
  }

  set preferRobot(value: boolean) {
    this.settings.allocationMode = value ? 2 : 0;
    this.allocationChanged.emit(this.settings.allocationMode);
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
  robots: Robot[] = [];
  configurationMode: number = 1;
  blockSets: BlockSet[] = [];
  @Input() showAllocation: boolean = false;
  @Input() showConfiguration: boolean = true;

  private lastAllocation: number = 0;
  private lastRobotType: string = '';
  private _settings: UserSettings;

  get settings(): UserSettings {
    return this._settings;
  }

  @Input() set settings(value: UserSettings) {
    this._settings = value;
    this.interfaces = new InterfaceHelper(this.settings);
    this.interfaces.allocationChanged.subscribe(_ => this.onRobotTypeChange());
    this.configurationMode = !!value.customBlockSet ? 2 : 1;
  }

  constructor(private robotTypeService: RobotTypeService,
    private robotService: RobotService) { }

  ngOnInit() {
    this.robotTypeService.list()
      .subscribe(results => {
        this.types = results.items;
        this.onRobotTypeChange();
      });
  }

  onRobotTypeChange() {
    if ((this.settings.robotType != this.lastRobotType) || (this.settings.allocationMode != this.lastAllocation)) {
      this.lastRobotType = this.settings.robotType;
      this.lastAllocation = this.settings.allocationMode;
      if (this.lastAllocation && this.lastRobotType) {
        this.robotService.listType(this.lastRobotType)
          .subscribe(data => {
            this.robots = data.items;
          })
      }

      this.robotTypeService.listBlockSets(this.lastRobotType)
        .subscribe(r => {
          this.blockSets = r.items;
        });
    }
  }

}
