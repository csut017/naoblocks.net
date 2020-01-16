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
import { UserSettings } from '../data/user-settings';
import { SettingsService } from '../services/settings.service';
import { UserSettingsComponent } from '../user-settings/user-settings.component';
import { RunSettingsComponent } from '../run-settings/run-settings.component';
import { RunSettings } from '../data/run-settings';
import { FormGroup, FormBuilder } from '@angular/forms';
import { TutorialService } from '../services/tutorial.service';
import { Tutorial } from '../data/tutorial';
import { TutorialExercise } from '../data/tutorial-exercise';

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
  requireEvents: boolean = true;
  currentUser: User;
  currentStartStep: number;
  errorMessage: string;
  onResize: any;
  isSidebarOpen: boolean = true;
  assignedRobot: string;
  storedProgram: string;
  invalidBlocks: any[] = [];
  userInput: promptSettings = new promptSettings();
  userSettings: UserSettings = new UserSettings();
  runSettings: RunSettings = new RunSettings();
  lastConversationId: number;
  lastHighlightBlock: string;
  tutorialForm: FormGroup;
  isTutorialOpen: boolean = true;
  currentTutorial: Tutorial;
  tutorialLoading: boolean = false;
  tutorialSelectorOpen: boolean = false;
  loadingTutorials: boolean = false;
  tutorialList: Tutorial[];

  @ViewChild(LoadProgramComponent, { static: false }) loadProgram: LoadProgramComponent;
  @ViewChild(SaveProgramComponent, { static: false }) saveProgram: SaveProgramComponent;
  @ViewChild(UserSettingsComponent, { static: false }) userSettingsDisplay: UserSettingsComponent;
  @ViewChild(RunSettingsComponent, { static: false }) runSettingsDisplay: RunSettingsComponent;

  constructor(authenticationService: AuthenticationService,
    router: Router,
    private programService: ProgramService,
    private errorHandler: ErrorHandlerService,
    private astConverter: AstConverterService,
    private connection: ConnectionService,
    private settingsService: SettingsService,
    private formBuilder: FormBuilder,
    private tutorialService: TutorialService) {
    super(authenticationService, router);
    this.tutorialForm = this.formBuilder.group({});
  }

  ngOnInit() {
    this.checkAccess(UserRole.Student);
    this.authenticationService.getCurrentUser()
      .subscribe(u => this.currentUser = u);
    this.tutorialLoading = true;
    this.settingsService.get()
      .subscribe(s => {
        this.userSettings = s.output;
        let xml = this.buildToolboxXml();
        this.workspace.updateToolbox(xml);
        if (this.userSettings.currentTutorial) {
          this.loadTutorial();
        } else {
          this.currentTutorial = undefined;
          this.tutorialLoading = false;
        }
      });

    this.initialiseWorkspace();
  }

  markTutorialComplete(): void {
    alert('TODO');
  }

  showStartTutorial(): void {
    this.tutorialSelectorOpen = true;
    this.loadingTutorials = true;
    this.tutorialService.list()
      .subscribe(data => {
        this.loadingTutorials = false;
        this.tutorialList = data.items;
      });
  }

  selectTutorial(value?: Tutorial): void {
    if (!!value) {
      this.userSettings.currentTutorial = value.id;
      this.loadTutorial();
    } else {
      this.userSettings.currentTutorial = undefined;
      this.currentTutorial = undefined;
    }
    this.settingsService.update(this.userSettings)
      .subscribe(result => {
        console.log(result);
      });
    this.tutorialSelectorOpen = false;
  }

  markExerciseAsComplete(exercise: TutorialExercise): void {
    console.log(`Completed exercise ${exercise.name}`);
    this.userSettings.currentExercise = exercise.order + 1;
    this.settingsService.update(this.userSettings)
      .subscribe(result => {
        console.log(result);
      });
  }

  private loadTutorial(): void {
    this.tutorialLoading = true;
    this.tutorialService.get(this.userSettings.currentTutorial)
      .subscribe(data => {
        this.tutorialLoading = false;
        this.currentTutorial = data.output;

        let tutorialConfig = {};
        const lastPos = this.currentTutorial.exercises.length - 1;
        this.currentTutorial
          .exercises
          .forEach((ex, pos) => {
            tutorialConfig['ex' + ex.order] = this.formBuilder.group({});
            ex.isLast = pos == lastPos;
            ex.isCurrent = ex.order == this.userSettings.currentExercise;
          });
        this.tutorialForm = this.formBuilder.group(tutorialConfig);
      });
  }

  private initialiseWorkspace(isReadonly: boolean = false) {
    let xml = this.buildToolboxXml();
    let currentBlocks: any;
    if (!!this.workspace) {
      currentBlocks = Blockly.Xml.workspaceToDom(this.workspace);
      this.workspace.dispose();
    }

    Blockly.BlockSvg.START_HAT = true;
    let blocklyArea = document.getElementById('blocklyArea');
    let blocklyDiv = document.getElementById('blocklyDiv');
    this.workspace = Blockly.inject('blocklyDiv', {
      readOnly: isReadonly,
      toolbox: xml,
      grid: {
        spacing: 20,
        snap: true
      },
      zoom: {
        controls: true,
        wheel: true
      }
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

  private buildToolboxXml() {
    let toolbox = new Toolbox();
    if (this.userSettings.simple) {
      toolbox.useSimpleStyle();
    }
    else {
      toolbox.useDefaultStyle();
      if (this.userSettings.conditionals)
        toolbox.includeConditionals();
      if (this.userSettings.loops)
        toolbox.includeLoops();
      if (this.userSettings.variables)
        toolbox.includeVariables();
      if (this.userSettings.dances)
        toolbox.includeDances();
      if (this.userSettings.sensors)
        toolbox.includeSensors();
      if (this.userSettings.events)
        toolbox.includeEvents();
    }
    let xml = toolbox.build();
    this.requireEvents = this.userSettings.events && !this.userSettings.simple;
    return xml;
  }

  get sidebarCollapsed(): boolean {
    return !this.isSidebarOpen;
  }

  set sidebarCollapsed(value: boolean) {
    this.isSidebarOpen = !value;

    if (!this.onResize) return;
    setInterval(() => this.onResize(), 0);
  }

  get tutorialOpen(): boolean {
    return this.isTutorialOpen;
  }

  set tutorialOpen(value: boolean) {
    this.isTutorialOpen = value;

    if (!this.onResize) return;
    setInterval(() => this.onResize(), 0);
  }

  validateWorkspace(event?: any): void {
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
      blocks.forEach(block => {
        if (!block.type.startsWith('robot_on_')) {
          this.invalidBlocks.push(block);
          block.setWarningText('This cannot be a top level block.');
          this.isValid = false;
        }
      });
    }
  }

  showSettings(): void {
    let cloned = JSON.parse(JSON.stringify(this.userSettings));
    this.userSettingsDisplay.show(cloned);
  }

  doCancelSend(): void {
    if (this.sendingToRobot) {
      this.sendingToRobot = false;
      return;
    }
    this.doStop();
  }

  doChangeSpeed(): void {
    this.runSettingsDisplay.show(this.runSettings);
  }

  onChangeRunSettings(settings: RunSettings): void {
    this.runSettings = settings;
    this.runSettingsDisplay.close();
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

  private generateReply(msg: ClientMessage, type: ClientMessageType): ClientMessage {
    let newMsg = new ClientMessage(type);
    newMsg.conversationId = msg.conversationId;
    return newMsg;
  }

  processServerMessage(msg: ClientMessage) {
    this.lastConversationId = msg.conversationId;
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
        this.connection.send(this.generateReply(msg, ClientMessageType.RequestRobot));
        break;

      case ClientMessageType.RobotAllocated:
        this.assignedRobot = msg.values.robot;
        this.currentStartStep = this.completeStep(this.currentStartStep);
        let transferCmd = this.generateReply(msg, ClientMessageType.TransferProgram);
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
        let startCmd = this.generateReply(msg, ClientMessageType.StartProgram);
        startCmd.values['robot'] = this.assignedRobot;
        startCmd.values['program'] = this.storedProgram;
        startCmd.values['opts'] = JSON.stringify(this.runSettings);
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
        if (this.lastHighlightBlock) {
          this.workspace.highlightBlock(this.lastHighlightBlock, false);
          this.lastHighlightBlock = undefined;
        }
        break;

      case ClientMessageType.RobotDebugMessage:
        let sId = msg.values['sourceID'];
        let action = msg.values['status'];
        this.lastHighlightBlock = sId;
        this.workspace.highlightBlock(sId, action === 'start');
        break;
    }
  }

  doStop(): void {
    let msg = new ClientMessage(ClientMessageType.StopProgram);
    msg.conversationId = this.lastConversationId;
    msg.values['robot'] = this.assignedRobot;
    this.connection.send(msg);
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

  onSaveSettings(settings: UserSettings): void {
    this.settingsService.update(settings)
      .subscribe(result => {
        if (result.successful) {
          this.userSettings = result.output;
          let xml = this.buildToolboxXml();
          this.workspace.updateToolbox(xml);
          this.userSettingsDisplay.close();
        } else {
          let error = this.errorHandler.formatError(result);
          this.userSettingsDisplay.showError(error);
        }
      });
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
