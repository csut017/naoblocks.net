import { Component, OnInit, ViewChild } from '@angular/core';
import { Toolbox } from './toolbox';
import { AboutComponent } from '../about/about.component';
import { AuthenticationService, UserRole } from '../services/authentication.service';
import { Router } from '@angular/router';
import { ChangeRoleComponent } from '../change-role/change-role.component';
import { HomeBase } from '../home-base';
import { ProgramService } from '../services/program.service';

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
  isValid: boolean = true;
  canStop: boolean = false;
  requireEvents: boolean = false;

  constructor(authenticationService: AuthenticationService,
    router: Router,
    private programService: ProgramService) {
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
    let validationResult = this.validateBlocks();
    if (!!validationResult) {
      this.failStep(0, validationResult);
      return;
    }

    let code = this.generateCode();
    this.programService.compile(code)
      .subscribe(result => {
        if (!result.successful) {
          this.failStep(0, 'Unable to compile code');
          return;
        } 

        if (result.output.errors) {
          this.failStep(0, 'There are errors in the code');
          return;
        }
        
        this.completeStep(0);
      });
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

  private validateBlocks(): string {
    var blocks = this.workspace.getTopBlocks();
    if (!blocks.length) {
        return 'There are no blocks in the current program!';
    }

    if (!this.requireEvents) {
        if (blocks.length > 1) {
            return 'All blocks must be joined!';
        }
    } else {
        if (!this.isValid) {
            return 'Program is not valid!';
        }
    }
  }

  private generateCode(): string {
    console.groupCollapsed('Generating code');
    try {
        Blockly.NaoLang.addStart = !this.requireEvents;
        let generated = Blockly.NaoLang.workspaceToCode(this.workspace);
        console.log(generated);
        return generated;
    } finally {
        console.groupEnd();
    }
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
