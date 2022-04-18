import { Component, EventEmitter, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { BlockSet } from 'src/app/data/block-set';
import { Robot } from 'src/app/data/robot';
import { RobotType } from 'src/app/data/robot-type';
import { UserSettings } from 'src/app/data/user-settings';
import { RobotTypeService } from 'src/app/services/robot-type.service';
import { RobotService } from 'src/app/services/robot.service';

@Component({
  selector: 'app-user-settings',
  templateUrl: './user-settings.component.html',
  styleUrls: ['./user-settings.component.scss']
})
export class UserSettingsComponent implements OnInit {

  @Input() showAllocation: boolean = false;
  @Input() showConfiguration: boolean = true;

  blockSets: BlockSet[] = [];
  configurationMode: number = 1;
  interface?: InterfaceHelper;
  interfaceStyle: number = 1;
  form: FormGroup;
  robots: Robot[] = [];
  types: RobotType[] = [];

  private internalSettings: UserSettings = new UserSettings();
  private lastAllocation?: number = 0;
  private lastRobotType?: string = '';

  constructor(private robotTypeService: RobotTypeService,
    private robotService: RobotService) {
    this.form = new FormGroup({
      type: new FormControl('', [Validators.required]),
      allocationMode: new FormControl('', [Validators.required]),
      robotId: new FormControl('', []),
      configMode: new FormControl('', [Validators.required]),
      interfaceStyle: new FormControl('', [Validators.required]),
      displayDances: new FormControl(false, []),
      displayConditionals: new FormControl(false, []),
      displayLoops: new FormControl(false, []),
      displaySensors: new FormControl(false, []),
      displayVariables: new FormControl(false, []),
    });
  }

  ngOnInit(): void {
    this.robotTypeService.list()
      .subscribe(data => this.types = data.items);
  }

  get settings(): UserSettings {
    return this.internalSettings;
  }

  @Input() set settings(value: UserSettings) {
    this.internalSettings = value;
    this.interface = new InterfaceHelper(this.settings);
    this.interface.allocationChanged.subscribe(_ => this.onRobotTypeChange());
    this.configurationMode = !!value.customBlockSet ? 2 : 1;
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

      this.blockSets = [];
      if (this.lastRobotType) {
        this.robotTypeService.listBlockSets(this.lastRobotType)
          .subscribe(r => {
            this.blockSets = r.items;
          });
      }
    }
  }

  save(): void {
    this.internalSettings.robotType = this.form.get('type')?.value;
  }
}

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
