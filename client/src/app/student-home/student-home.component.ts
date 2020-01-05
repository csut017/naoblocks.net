import { Component, OnInit, ViewChild } from '@angular/core';
import { Toolbox } from './toolbox';
import { AuthenticationService, UserRole } from '../services/authentication.service';
import { Router } from '@angular/router';
import { HomeBase } from '../home-base';
import { ProgramService } from '../services/program.service';
import { SaveProgramComponent, SaveDetails } from '../save-program/save-program.component';
import { User } from '../data/user';
import { Student } from '../data/student';
import { ErrorHandlerService } from '../services/error-handler.service';
import { saveAs } from 'file-saver';
import { LoadProgramComponent } from '../load-program/load-program.component';
import { AstConverterService } from '../services/ast-converter.service';
import { ConnectionService, ClientMessage, ClientMessageType } from '../services/connection.service';

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

class promptSettings {
  open: boolean = false;
  title: string;
  value: string;
  action: any;
  showOk: boolean = false;
  showValue: boolean = false;
  mode: userInputMode;
}

enum userInputMode {
  prompt,
  confirm,
  alert
}

@Component({
  selector: 'app-student-home',
  templateUrl: './student-home.component.html',
  styleUrls: ['./student-home.component.scss']
})
export class StudentHomeComponent extends HomeBase implements OnInit {

  workspace: any;
  sendingToRobot: boolean = false;
  steps: executionStatusStep[];
  startMessage: string;
  isExecuting: boolean = false;
  isValid: boolean = true;
  canStop: boolean = false;
  requireEvents: boolean = false;
  currentUser: User;
  currentStartStep: number;
  errorMessage: string;
  onResize: any;
  isSidebarOpen: boolean = true;
  assignedRobot: string;
  storedProgram: string;
  invalidBlocks: any[] = [];
  userInput: promptSettings = new promptSettings();

  @ViewChild(LoadProgramComponent, { static: false }) loadProgram: LoadProgramComponent;
  @ViewChild(SaveProgramComponent, { static: false }) saveProgram: SaveProgramComponent;

  constructor(authenticationService: AuthenticationService,
    router: Router,
    private programService: ProgramService,
    private errorHandler: ErrorHandlerService,
    private astConverter: AstConverterService,
    private connection: ConnectionService) {
    super(authenticationService, router);
  }

  ngOnInit() {
    this.checkAccess(UserRole.Student);
    this.authenticationService.getCurrentUser()
      .subscribe(u => this.currentUser = u);

    this.initialiseWorkspace();
  }

  private initialiseWorkspace(isReadonly: boolean = false) {
    let xml = new Toolbox()
      .includeConditionals()
      .includeLoops()
      .includeVariables()
      .includeDances()
      .includeSensors()
      .build();

    let currentBlocks: any;
    if (!!this.workspace) {
      currentBlocks = Blockly.Xml.workspaceToDom(this.workspace);
      this.workspace.dispose();
    }

    let blocklyArea = document.getElementById('blocklyArea');
    let blocklyDiv = document.getElementById('blocklyDiv');
    this.workspace = Blockly.inject('blocklyDiv', {
      readOnly: isReadonly,
      toolbox: xml
    });
    if (!!currentBlocks) {
      Blockly.Xml.domToWorkspace(currentBlocks, this.workspace);
    }

    this.configureEditor();

    // Initialise the resizing
    const workspace = this.workspace;
    this.onResize = function () {
      let element: any = blocklyArea;
      var x = 0;
      var y = 0;
      do {
        x += element.offsetLeft;
        y += element.offsetTop;
        element = element.offsetParent;
      } while (element);
      blocklyDiv.style.left = x + 'px';
      blocklyDiv.style.top = y + 'px';
      blocklyDiv.style.width = blocklyArea.offsetWidth + 'px';
      blocklyDiv.style.height = blocklyArea.offsetHeight + 'px';
      Blockly.svgResize(workspace);
    };
    window.addEventListener('resize', _ => this.onResize(), false);
    this.onResize();
  }

  get sidebarCollapsed(): boolean {
    return !this.isSidebarOpen;
  }

  set sidebarCollapsed(value: boolean) {
    this.isSidebarOpen = !value;

    if (!this.onResize) return;
    setInterval(() => this.onResize(), 0);
  }

  validateWorkspace(event): void {
    if (this.workspace.isDragging()) return;
    console.log('Validating');
    var validate = !event ||
      (event.type == Blockly.Events.CREATE) ||
      (event.type == Blockly.Events.MOVE) ||
      (event.type == Blockly.Events.DELETE);
    if (!validate) return;

    this.invalidBlocks.forEach(function (block) {
      if (block.rendered) block.setWarningText(null);
    })
    this.invalidBlocks = [];
    this.isValid = true;
    var blocks = this.workspace.getTopBlocks();

    if (this.requireEvents) {
      blocks.forEach(function (block) {
        if (!block.type.startsWith('robot_on_')) {
          this.invalidBlocks.push(block);
          block.setWarningText('This cannot be a top level block.');
          this.isValid = false;
        }
      });
    }
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

  doClear(): void {
    this.workspace.clear();
    this.validateWorkspace();
  }

  doPlay(): void {
    this.initialiseStartingUI();
    let validationResult = this.validateBlocks();
    if (!!validationResult) {
      this.failStep(0, validationResult);
      return;
    }

    let code = this.generateCode(true);
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

        this.storedProgram = result.output.programId.toString();
        this.currentStartStep = 1;
        this.completeStep(0);
        this.connection.start().subscribe(msg => this.processServerMessage(msg));
      });
  }

  changeExecuting(newValue: boolean): void {
    if (newValue == this.isExecuting) return;

    this.isExecuting = newValue;
    // this.initialiseWorkspace(this.isExecuting);
  }

  processServerMessage(msg: ClientMessage) {
    switch (msg.type) {
      case ClientMessageType.Closed:
        if (this.currentStartStep) {
          this.failStep(this.currentStartStep, 'Connection to server lost');
        } else if (this.isExecuting) {
          this.errorMessage = 'Connection to the server has been lost';
          this.changeExecuting(false);
        }
        break;

      case ClientMessageType.Error:
        if (this.currentStartStep) {
          let errMsg = msg.values['error'] || 'Unknown';
          this.failStep(this.currentStartStep, `Server error: ${errMsg}`);
          this.currentStartStep = undefined;
          this.connection.close();
        }
        break;

      case ClientMessageType.Authenticated:
        this.connection.send(new ClientMessage(ClientMessageType.RequestRobot));
        break;

      case ClientMessageType.RobotAllocated:
        this.assignedRobot = msg.values.robot;
        this.currentStartStep = this.completeStep(this.currentStartStep);
        let transferCmd = new ClientMessage(ClientMessageType.TransferProgram);
        transferCmd.values['robot'] = this.assignedRobot;
        transferCmd.values['program'] = this.storedProgram;
        this.connection.send(transferCmd);
        break;

      case ClientMessageType.NoRobotsAvailable:
        this.failStep(this.currentStartStep, 'No robots available');
        this.currentStartStep = undefined;
        this.connection.close();
        break;

      case ClientMessageType.ProgramTransferred:
        this.currentStartStep = this.completeStep(this.currentStartStep);
        let startCmd = new ClientMessage(ClientMessageType.StartProgram);
        startCmd.values['robot'] = this.assignedRobot;
        startCmd.values['program'] = this.storedProgram;
        this.connection.send(startCmd);
        break;

      case ClientMessageType.UnableToDownloadProgram:
        this.failStep(this.currentStartStep, 'Program download failed');
        this.currentStartStep = undefined;
        this.connection.close();
        break;

      case ClientMessageType.ProgramStarted:
        this.sendingToRobot = false;
        this.changeExecuting(true);
        this.currentStartStep = undefined;
        break;

      case ClientMessageType.ProgramFinished:
      case ClientMessageType.ProgramStopped:
        this.changeExecuting(false);
        this.connection.close();
        break;

      case ClientMessageType.RobotDebugMessage:
        let sId = msg.values['sourceID'];
        let action = msg.values['status'];
        this.workspace.highlightBlock(sId, action === 'start');
        break;
    }
  }

  doStop(): void {
    this.connection.send(new ClientMessage(ClientMessageType.StopProgram));
  }

  doLoad(): void {
    let student = new Student();
    student.name = this.currentUser.name;
    this.loadProgram.show(student);
  }

  onLoad(code: string) {
    this.programService.compile(code, false)
      .subscribe(result => {
        if (result.successful) {
          console.groupCollapsed('Converting to XML');
          try {
            let xml = this.astConverter.convert(result.output.nodes, this.requireEvents);
            this.loadIntoWorkspace(xml);
            console.log(xml);
            this.loadProgram.close();
            this.validateWorkspace();
          } catch (error) {
            console.error(error);
            this.loadProgram.showError('Something went wrong loading the blocks');
          } finally {
            console.groupEnd();
          }
        } else {
          let error = this.errorHandler.formatError(result);
          this.loadProgram.showError(error);
        }
      });
  }

  loadIntoWorkspace(xml: HTMLElement): void {
    console.log('Loading workspace');
    try {
      this.workspace.clear();
      Blockly.Xml.domToWorkspace(xml, this.workspace);
    } catch (err) {
      console.log('Load failed!');
      console.error(err);
    }
    console.groupEnd();

    var topBlocks = this.workspace.getTopBlocks();
    if (topBlocks) {
      var centreBlock = topBlocks[0];
      this.workspace.centerOnBlock(centreBlock.id);
    }
  }

  doSave(): void {
    let student = new Student();
    student.name = this.currentUser.name;
    this.saveProgram.show(student);
  }

  onSave(details: SaveDetails): void {
    let code = this.generateCode(false);
    if (details.toServer) {
      this.programService.save(details.name, code)
        .subscribe(result => {
          if (result.successful) {
            this.saveProgram.close();
          } else {
            let error = this.errorHandler.formatError(result);
            this.saveProgram.showError(error);
          }
        });
    } else {
      var blob = new Blob([code], { type: "text/plain;charset=utf-8" });
      saveAs(blob, details.name + '.txt');
      this.saveProgram.close();
    }
  }

  closeUserInput(ok: boolean): void {
    this.userInput.open = false;
    if (this.userInput.action) {
      switch (this.userInput.mode) {
        case userInputMode.alert:
          this.userInput.action();
          break;

        case userInputMode.confirm:
          this.userInput.action(ok);
          break;

        case userInputMode.prompt:
          this.userInput.action(this.userInput.value);
          break;
      }
    }
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

  private configureEditor(): void {
    console.groupCollapsed('Initialising blockly editor');
    try {
      console.log('Defining colours');
      Blockly.FieldColour.COLOURS = [
        '#f00', '#0f0',
        '#00f', '#ff0',
        '#f0f', '#0ff',
        '#000', '#fff'
      ];
      Blockly.FieldColour.COLUMNS = 2;

      console.log('Configuring modals');
      let that = this;
      Blockly.alert = function (message: string, callback: any) {
        that.userInput.title = message;
        that.userInput.action = callback;
        that.userInput.showOk = false;
        that.userInput.showValue = false;
        that.userInput.mode = userInputMode.alert;
        that.userInput.open = true;
      };
      Blockly.confirm = function (message: string, callback: any) {
        that.userInput.title = message;
        that.userInput.action = callback;
        that.userInput.showOk = true;
        that.userInput.showValue = false;
        that.userInput.mode = userInputMode.alert;
        that.userInput.open = true;
      };
      Blockly.prompt = function (message: string, defaultValue: any, callback: any) {
        that.userInput.title = message;
        that.userInput.action = callback;
        that.userInput.showOk = true;
        that.userInput.showValue = true;
        that.userInput.mode = userInputMode.prompt;
        that.userInput.value = defaultValue;
        that.userInput.open = true;
      };

      console.log('Adding validator');
      this.workspace.addChangeListener(evt => this.validateWorkspace(evt));
    } finally {
      console.groupEnd();
    }
  }

  private generateCode(forRobot: boolean): string {
    console.groupCollapsed('Generating code');
    try {
      var xml = Blockly.Xml.workspaceToDom(this.workspace);
      console.log(Blockly.Xml.domToText(xml));
      Blockly.NaoLang.addStart = !this.requireEvents;
      Blockly.NaoLang.includeId = forRobot;
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

  private completeStep(step: number): number {
    if (step >= this.steps.length) return step;
    this.steps[step].isCurrent = false;
    this.steps[step].image = 'success-standard';

    if (++step >= this.steps.length) return step;
    this.steps[step].isCurrent = true;
    return step;
  }

  private failStep(step: number, reason: string) {
    this.startMessage = reason;
    if (!this.steps[step]) return;
    this.steps[step].isCurrent = false;
    this.steps[step].image = 'error-standard';
  }
}
