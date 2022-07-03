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

  allocationMode: number = 0;
  blockSets: BlockSet[] = [];
  configurationMode: number = 1;
  form: FormGroup;
  interfaceStyle: number = 0;
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
      configMode: new FormControl('1', [Validators.required]),
      interfaceStyle: new FormControl('', [Validators.required]),
      displayDances: new FormControl(false, []),
      displayConditionals: new FormControl(false, []),
      displayLoops: new FormControl(false, []),
      displaySensors: new FormControl(false, []),
      displayVariables: new FormControl(false, []),
      customBlockSet: new FormControl('', [Validators.required]),
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
    this.configurationMode = !!this.settings.customBlockSet ? 2 : 1;
    this.allocationMode = this.settings.allocationMode || 0;
    this.form.setValue({
      type: this.settings.robotType,
      allocationMode: this.allocationMode,
      robotId: this.settings.robotId,
      configMode: this.configurationMode,
      interfaceStyle: (this.settings.simple ? 2 : (this.settings.events ? 3 : 1)) || 1,
      displayDances: this.settings.dances,
      displayConditionals: this.settings.conditionals,
      displayLoops: this.settings.loops,
      displaySensors: this.settings.sensors,
      displayVariables: this.settings.variables,
      customBlockSet: this.settings.customBlockSet,
    });
    this.onRobotTypeChange();
  }

  onAllocationModeChange() {
    this.allocationMode = parseInt(this.form.get('allocationMode')?.value);
  }

  onRobotTypeChange() {
    const robotType = this.form.get('type')?.value;
    if (robotType != this.lastRobotType) {
      this.lastRobotType = robotType;
      this.robots = [];
      if (this.lastRobotType) {
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

  onConfigurationModeChange() {
    this.configurationMode = parseInt(this.form.get('configMode')?.value);
  }

  onInterfaceStyleChange() {
    this.interfaceStyle = parseInt(this.form.get('interfaceStyle')?.value);
  }

  save(): void {
    this.internalSettings.robotType = this.form.get('type')?.value;
  }
}
