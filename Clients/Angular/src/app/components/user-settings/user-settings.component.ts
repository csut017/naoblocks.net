import { Component, EventEmitter, Input, OnInit } from '@angular/core';
import { UntypedFormControl, UntypedFormGroup, Validators } from '@angular/forms';
import { Robot } from 'src/app/data/robot';
import { RobotType } from 'src/app/data/robot-type';
import { Toolbox } from 'src/app/data/toolbox';
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
  toolboxes: Toolbox[] = [];
  configurationMode: number = 1;
  form: UntypedFormGroup;
  interfaceStyle: number = 0;
  hasRobot: boolean = false;
  robots: Robot[] = [];
  types: RobotType[] = [];

  private internalSettings: UserSettings = new UserSettings();
  private lastRobotType?: string = '';

  constructor(private robotTypeService: RobotTypeService,
    private robotService: RobotService) {
    this.form = new UntypedFormGroup({
      type: new UntypedFormControl('', [Validators.required]),
      allocationMode: new UntypedFormControl('', [Validators.required]),
      robotId: new UntypedFormControl('', []),
      toolbox: new UntypedFormControl('', [Validators.required]),
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
    this.form.setValue({
      type: this.settings.robotType,
      allocationMode: this.allocationMode,
      robotId: this.settings.robotId || '',
      toolbox: this.settings.toolbox || '',
    });
    this.onRobotTypeChange();
  }

  onAllocationModeChange() {
    this.allocationMode = parseInt(this.form.get('allocationMode')?.value);
  }

  onRobotTypeChange() {
    const robotType = this.form.get('type')?.value;
    this.hasRobot = !!robotType;
    if (robotType != this.lastRobotType) {
      this.lastRobotType = robotType;
      this.robots = [];
      this.toolboxes = [];
      if (this.lastRobotType) {
        this.robotService.listType(this.lastRobotType)
          .subscribe(data => {
            this.robots = data.items;
          })
        this.robotTypeService.get(this.lastRobotType)
          .subscribe(r => {
            this.toolboxes = r.output?.toolboxes || [];
          });
      }
    }
  }

  onToolboxChanged(): void {
    
  }

  save(): UserSettings {
    let settings = new UserSettings();
    settings.robotType = this.form.get('type')?.value;
    settings.allocationMode = this.form.get('allocationMode')?.value;
    settings.robotId = this.form.get('robotId')?.value;
    this.configurationMode = this.form.get('configMode')?.value;
    settings.toolbox = this.form.get('toolbox')?.value;
    return settings;
  }
}
