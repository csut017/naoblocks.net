import { Component, OnInit, ViewChild } from '@angular/core';
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
import { AstConversionMode, AstConverterService } from '../services/ast-converter.service';
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
import { EditorSettings } from '../data/editor-settings';
import { ExecutionStatusStep } from '../data/execution-status-step';
import { IServiceMessageUpdater, ServerMessageProcessorService } from '../services/server-message-processor.service';
import { SnapshotService } from '../services/snapshot.service';

declare var Blockly: any;

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
export class StudentHomeComponent extends HomeBase implements OnInit, IServiceMessageUpdater {

  canStop: boolean = false;
  currentTutorial: Tutorial;
  currentUser: User;
  editorSettings: EditorSettings = new EditorSettings();
  errorMessage: string;
  hasChanged: boolean = false;
  invalidBlocks: any[] = [];
  isExecuting: boolean = false;
  isSidebarOpen: boolean = true;
  isStoringState: boolean = false;
  isTutorialHidden: boolean = true;
  isTutorialOpen: boolean = true;
  isValid: boolean = true;
  lastHighlightBlock: string;
  loadingTutorials: boolean = false;
  messageProcessor: ServerMessageProcessorService;
  onResize: any;
  requireEvents: boolean = true;
  runSettings: RunSettings = new RunSettings();
  sendingToRobot: boolean = false;
  startMessage: string;
  steps: ExecutionStatusStep[];
  tutorialForm: FormGroup;
  tutorialList: Tutorial[];
  tutorialLoading: boolean = false;
  tutorialSelectorOpen: boolean = false;
  userInput: promptSettings = new promptSettings();
  workspace: any;

  @ViewChild(LoadProgramComponent) loadProgram: LoadProgramComponent;
  @ViewChild(SaveProgramComponent) saveProgram: SaveProgramComponent;
  @ViewChild(UserSettingsComponent) userSettingsDisplay: UserSettingsComponent;
  @ViewChild(RunSettingsComponent) runSettingsDisplay: RunSettingsComponent;

  constructor(authenticationService: AuthenticationService,
    router: Router,
    private programService: ProgramService,
    private errorHandler: ErrorHandlerService,
    private astConverter: AstConverterService,
    private connection: ConnectionService,
    private settingsService: SettingsService,
    private snapshotService: SnapshotService,
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
        this.editorSettings = s.output;
        let xml = this.buildToolboxXml();
        this.isTutorialHidden = !this.editorSettings.user.tutorials;
        this.workspace.updateToolbox(xml);
        if (this.editorSettings.user.currentTutorial) {
          this.loadTutorial();
        } else {
          this.currentTutorial = undefined;
          this.tutorialLoading = false;
        }
      });

    this.initialiseWorkspace();
    setInterval(() => this.storeState(), 10000);
  }

  storeState(): void {
    if (!this.hasChanged || this.isStoringState) return;

    console.log(`[StudentHome] Storing snapshot`);
    let state = this.generateCode(false);
    this.isStoringState = true;
    this.snapshotService.store(state, 'StudentHome', {
      'isValid': this.isValid ? 'yes' : 'no'
    })
      .subscribe(s => {
        this.isStoringState = false;
        if (s.successful) {
          this.hasChanged = false;
          console.log(`[StudentHome] Snapshot stored`);
        }
      });
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
      this.editorSettings.user.currentTutorial = value.id;
      this.loadTutorial();
    } else {
      this.editorSettings.user.currentTutorial = undefined;
      this.currentTutorial = undefined;
    }
    this.settingsService.update(this.editorSettings)
      .subscribe(result => {
        console.log(result);
      });
    this.tutorialSelectorOpen = false;
  }

  markExerciseAsComplete(exercise: TutorialExercise): void {
    console.log(`[StudentHome] Completed exercise ${exercise.name}`);
    this.editorSettings.user.currentExercise = exercise.order + 1;
    this.settingsService.update(this.editorSettings)
      .subscribe(result => {
        console.log(result);
      });
  }

  private loadTutorial(): void {
    this.tutorialLoading = true;
    this.tutorialService.get(this.editorSettings.user.currentTutorial)
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
            ex.isCurrent = ex.order == this.editorSettings.user.currentExercise;
          });
        this.tutorialForm = this.formBuilder.group(tutorialConfig);
      });
  }

  private initialiseWorkspace(isReadonly: boolean = false) {
    console.log(`[StudentHome] Initialising workspace`);
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
    setInterval(() => this.onResize(), 0);
  }

  private buildToolboxXml() {
    let xml = this.editorSettings.toolbox || '<xml><category name="Loading..."></category></xml>';
    this.requireEvents = this.editorSettings.user.events && !this.editorSettings.user.simple;
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
    var validate = !event ||
      (event.type == Blockly.Events.CREATE) ||
      (event.type == Blockly.Events.MOVE) ||
      (event.type == Blockly.Events.DELETE);
    if (!validate) return;

    console.log('[StudentHome] Validating');
    this.hasChanged = true;

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
    let cloned = JSON.parse(JSON.stringify(this.editorSettings.user));
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
    this.messageProcessor = new ServerMessageProcessorService(this.connection, this, this.runSettings);
    this.initialiseStartingUI();
    let validationResult = this.validateBlocks();
    if (!!validationResult) {
      this.onStepFailed(0, validationResult);
      return;
    }

    let code = this.generateCode(true);
    this.programService.compile(code)
      .subscribe(result => {
        if (!result.successful) {
          this.onStepFailed(0, 'Unable to compile code');
          return;
        }

        if (result.output.errors) {
          this.onStepFailed(0, 'There are errors in the code');
          return;
        }

        this.messageProcessor.storedProgram = result.output.programId.toString();
        this.messageProcessor.currentStartStep = 1;
        this.onStepCompleted(0);
        this.connection.start().subscribe(msg => this.messageProcessor.processServerMessage(msg));
      });
  }

  doStop(): void {
    let msg = new ClientMessage(ClientMessageType.StopProgram);
    msg.conversationId = this.messageProcessor.lastConversationId;
    msg.values['robot'] = this.messageProcessor.assignedRobot;
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
            let conversionMode = AstConversionMode.Default;
            if (this.editorSettings.user.simple) {
              conversionMode = AstConversionMode.Simplified;
            }

            let xml = this.astConverter.convert(result.output.nodes, this.requireEvents, conversionMode);
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
    console.log('[StudentHome] Loading workspace');
    try {
      this.workspace.clear();
      Blockly.Xml.domToWorkspace(xml, this.workspace);
    } catch (err) {
      console.log('[StudentHome] Load failed!');
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
    let newSettings = new EditorSettings();
    newSettings.user = settings;
    this.settingsService.update(newSettings)
      .subscribe(result => {
        if (result.successful) {
          this.editorSettings = result.output;
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
      console.log('[StudentHome] Defining colours');
      Blockly.FieldColour.COLOURS = [
        '#f00', '#0f0',
        '#00f', '#ff0',
        '#f0f', '#0ff',
        '#000', '#fff'
      ];
      Blockly.FieldColour.COLUMNS = 2;

      console.log('[StudentHome] Configuring modals');
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

      console.log('[StudentHome] Adding validator');
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
      new ExecutionStatusStep('Check Program', 'Checks that the program is valid and can run on the robot.'),
      new ExecutionStatusStep('Select Robot', 'Finds an available robot to run the program on.'),
      new ExecutionStatusStep('Send to Robot', 'Sends the program to the robot.'),
      new ExecutionStatusStep('Start Execution', 'Starts the program running on the robot.')
    ];
    this.steps[0].isCurrent = true;
    this.startMessage = undefined;
    this.sendingToRobot = true;
  }

  onCloseConnection(): void {
    this.connection.close();
  }

  onErrorOccurred(message: string): void {
    this.errorMessage = message;
  }

  onStepCompleted(step: number): number {
    if (step >= this.steps.length) return step;
    this.steps[step].isCurrent = false;
    this.steps[step].image = 'success-standard';

    if (++step >= this.steps.length) return step;
    this.steps[step].isCurrent = true;
    return step;
  }

  onStateUpdate(): void {
    this.isExecuting = this.messageProcessor.isExecuting;
  }

  onStepFailed(step: number, reason: string): void {
    this.startMessage = reason;
    if (!this.steps[step]) return;
    this.steps[step].isCurrent = false;
    this.steps[step].image = 'error-standard';
  }

  onClearHighlight(): void {
    if (this.lastHighlightBlock) {
      this.workspace.highlightBlock(this.lastHighlightBlock, false);
      this.lastHighlightBlock = undefined;
    }
  }

  onHighlightBlock(id: string, action: string): void {
    this.lastHighlightBlock = id;
    this.workspace.highlightBlock(id, action === 'start');
  }

  onShowDebug(): void {
    this.sendingToRobot = false;
  }
}
