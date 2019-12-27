import { Component, OnInit, ViewChild } from '@angular/core';
import { Toolbox } from './toolbox';
import { AboutComponent } from '../about/about.component';
import { AuthenticationService, UserRole } from '../services/authentication.service';
import { Router } from '@angular/router';
import { ChangeRoleComponent } from '../change-role/change-role.component';
import { HomeBase } from '../home-base';

declare var Blockly: any;

class executionStatusStep {
  image: string = 'circle';
  title: string;
  description: string;
  isCurrent: boolean = false;

  constructor(title: string, description: string) {
    this.title = title;
    this.description = description;
  }
}

@Component({
  selector: 'app-student-home',
  templateUrl: './student-home.component.html',
  styleUrls: ['./student-home.component.scss']
})
export class StudentHomeComponent extends HomeBase implements OnInit {

  aboutOpened: boolean;
  workspace: any;
  sendingToRobot: boolean = false;
  steps: executionStatusStep[];
  startMessage: string;
  isExecuting: boolean = false;
  canStop: boolean = false;

  constructor(authenticationService: AuthenticationService,
    router: Router) {
    super(authenticationService, router);
  }

  ngOnInit() {
    this.checkAccess(UserRole.Student);

    let xml = new Toolbox()
      .includeConditionals()
      .includeLoops()
      .includeVariables()
      .build();

    this.workspace = Blockly.inject('blocklyDiv', {
      toolbox: xml,
      scrollbars: false
    });
  }

  doCancelSend(): void {
    if (this.sendingToRobot) {
      this.sendingToRobot = false;
      return;
    }
    alert('TODO');
  }

  doChangeSpeed(): void {
    alert('TODO');
  }

  doPlay(): void {
    this.initialiseStartingUI();
    this.completeStep(0);
    this.failStep(1, 'Testing');
  }

  doStop(): void {
    alert('TODO');
  }

  doLoad(): void {
    alert('TODO');
  }

  doSave(): void {
    alert('TODO');
  }

  private initialiseStartingUI() {
    this.steps = [
      new executionStatusStep('Check Program', 'Checks that the program is valid and can run on the robot.'),
      new executionStatusStep('Select Robot', 'Finds an available robot to run the program on.'),
      new executionStatusStep('Send to Robot', 'Sends the program to the robot.'),
      new executionStatusStep('Start Execution', 'Starts the program running on the robot.')
    ];
    this.steps[0].isCurrent = true;
    this.startMessage = undefined;
    this.sendingToRobot = true;
  }

  private completeStep(step: number) {
    if (step >= this.steps.length) return;
    this.steps[step].isCurrent = false;
    this.steps[step].image = 'success-standard';

    if (++step >= this.steps.length) return;
    this.steps[step].isCurrent = true;
  }

  private failStep(step: number, reason: string) {
    this.startMessage = reason;
    if (step >= this.steps.length) return;
    this.steps[step].isCurrent = false;
    this.steps[step].image = 'error-standard';
  }
}
