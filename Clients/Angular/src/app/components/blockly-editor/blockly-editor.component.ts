import { AfterViewInit, Component, Input, OnChanges, OnDestroy, OnInit, SimpleChanges } from '@angular/core';
import { Subscription } from 'rxjs';
import { ConfirmSettings } from 'src/app/data/confirm-settings';
import { ControllerAction } from 'src/app/data/controller-action';
import { EditorSettings } from 'src/app/data/editor-settings';
import { RunSettings } from 'src/app/data/run-settings';
import { StartupStatusTracker } from 'src/app/data/startup-status-tracker';
import { ConfirmService } from 'src/app/services/confirm.service';
import { ClientMessage, ClientMessageType, ConnectionService } from 'src/app/services/connection.service';
import { ExecutionStatusService } from 'src/app/services/execution-status.service';
import { ProgramControllerService } from 'src/app/services/program-controller.service';
import { ProgramService } from 'src/app/services/program.service';
import { IServiceMessageUpdater, ServerMessageProcessorService } from 'src/app/services/server-message-processor.service';
import { environment } from 'src/environments/environment';

declare var Blockly: any;

@Component({
  selector: 'app-blockly-editor',
  templateUrl: './blockly-editor.component.html',
  styleUrls: ['./blockly-editor.component.scss']
})
export class BlocklyEditorComponent implements OnInit, OnChanges, AfterViewInit, IServiceMessageUpdater, OnDestroy {

  @Input() controller?: ProgramControllerService;
  @Input() editorSettings: EditorSettings = new EditorSettings();

  controllerSubscription?: Subscription;
  error: string = '';
  hasChanged: boolean = false;
  invalidBlocks: any[] = [];
  isExecuting: boolean = false;
  isLoading: boolean = true;
  isValid: boolean = true;
  languageUrl: string = `${environment.apiURL}v1/ui/angular/language`;
  lastHighlightBlock?: string;
  messageProcessor?: ServerMessageProcessorService;
  requireEvents: boolean = false;
  startupStatus: StartupStatusTracker = new StartupStatusTracker();
  workspace: any;

  constructor(
    private connection: ConnectionService,
    private programService: ProgramService,
    private executionStatus: ExecutionStatusService,
    private confirm: ConfirmService) { }

  ngAfterViewInit(): void {
    this.controllerSubscription = this.controller!.onAction.subscribe(event => {
      switch (event.action) {
        case ControllerAction.play:
          this.play(event.data);
          break;

        case ControllerAction.stop:
          this.stop();
          break;

        case ControllerAction.clear:
          this.clear();
          break;
      }
    });
    if (this.controller?.lastWorkspace) Blockly.Xml.domToWorkspace(this.controller?.lastWorkspace, this.workspace);;
  }

  ngOnDestroy(): void {
    this.controllerSubscription?.unsubscribe();
  }

  ngOnChanges(_: SimpleChanges): void {
    if (this.workspace) {
      let xml = this.buildToolbox();
      this.workspace.updateToolbox(xml);
    }
    if (this.editorSettings) this.isLoading = !this.editorSettings.isLoaded;
  }

  ngOnInit(): void {
    this.initialiseWorkspace();
  }

  loadIntoWorkspace(xml: HTMLElement): void {
    console.log('[BlocklyEditor] Loading workspace');
    try {
      this.workspace.clear();
      Blockly.Xml.domToWorkspace(xml, this.workspace);
    } catch (err) {
      console.log('[BlocklyEditor] Load failed!');
      console.error(err);
    }
    console.groupEnd();

    var topBlocks = this.workspace.getTopBlocks();
    if (topBlocks) {
      var centreBlock = topBlocks[0];
      this.workspace.centerOnBlock(centreBlock.id);
    }
  }

  validateWorkspace(event?: any): void {
    if (this.workspace.isDragging()) return;
    var validate = !event ||
      (event.type == Blockly.Events.CREATE) ||
      (event.type == Blockly.Events.MOVE) ||
      (event.type == Blockly.Events.DELETE);
    if (!validate) return;

    let xml = Blockly.Xml.workspaceToDom(this.workspace);
    this.controller!.lastWorkspace = xml;

    console.log('[BlocklyEditor] Validating');
    this.hasChanged = true;
    this.error = '';

    this.invalidBlocks.forEach(function (block) {
      if (block.rendered) block.setWarningText(null);
    })
    this.invalidBlocks = [];
    this.isValid = true;
    var blocks = this.workspace.getTopBlocks() || [];

    if (this.requireEvents) {
      blocks.forEach((block: any) => {
        if (!block.type.startsWith('robot_on_')) {
          this.invalidBlocks.push(block);
          block.setWarningText('This cannot be a top level block.');
          this.isValid = false;
        }

        if (!this.isValid) this.error = 'Some top-level blocks are invalid';
      });
    } else {
      if (blocks.length > 1) {
        this.isValid = false;
        this.error = 'All blocks must be connected';
      }
    }
  }

  private validateBlocks(): string | undefined {
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

    return undefined;
  }

  clear(): void {
    this.workspace.clear();
    this.validateWorkspace();
  }

  stop(): void {
    if (!this.messageProcessor?.isExecuting) {
      this.controller!.isPlaying = false;
      return;
    }

    console.log('[BlocklyEditor] Stopping program');
    let msg = new ClientMessage(ClientMessageType.StopProgram);
    msg.conversationId = this.messageProcessor?.lastConversationId;
    msg.values['robot'] = this.messageProcessor?.assignedRobot || '';
    this.connection.send(msg);
  }

  play(settings: RunSettings): void {
    console.groupCollapsed('[BlocklyEditor] Playing program');
    console.log(settings);
    let code = this.generateCode(true);
    console.groupEnd();
    this.messageProcessor = new ServerMessageProcessorService(this.connection, this, settings);
    this.startupStatus.initialise();
    this.executionStatus.show(this.startupStatus)
      .subscribe(isCancelling => {
        if (!!this.startupStatus.startMessage) {
          this.controller!.isPlaying = false;
        } else if (isCancelling) {
          this.stop();
        }
      });
    let validationResult = this.validateBlocks();
    if (!!validationResult) {
      this.onStepFailed(0, validationResult);
      return;
    }

    this.controller!.isPlaying = true;
    this.programService.compile(code)
      .subscribe(result => {
        if (!result.successful) {
          this.onStepFailed(0, 'Unable to compile code');
          return;
        }

        if (result.output?.errors) {
          this.onStepFailed(0, 'There are errors in the code');
          return;
        }

        this.messageProcessor!.storedProgram = result.output?.programId?.toString();
        this.messageProcessor!.currentStartStep = 1;
        this.onStepCompleted(0);
        this.connection.start()
          .subscribe(msg => this.messageProcessor!.processServerMessage(msg));
      });
  }

  private buildToolbox(): string {
    let xml = this.editorSettings.toolbox || '<xml><category name="..."></category></xml>';
    this.requireEvents = this.editorSettings.user.events || false;
    return xml;
  }

  private initialiseWorkspace(isReadonly: boolean = false) {
    console.log(`[BlocklyEditor] Initialising workspace`);
    const toolbox = this.buildToolbox();

    let currentBlocks: any;
    if (!!this.workspace) {
      currentBlocks = Blockly.Xml.workspaceToDom(this.workspace);
      this.workspace.dispose();
    }

    Blockly.BlockSvg.START_HAT = true;
    if (!!currentBlocks) {
      Blockly.Xml.domToWorkspace(currentBlocks, this.workspace);
    }

    this.workspace = Blockly.inject('blockly', {
      grid: {
        spacing: 20,
        snap: true
      },
      readOnly: isReadonly,
      scrollbars: true,
      toolbox: toolbox,
      trashcan: true,
      zoom: {
        controls: true,
        wheel: true
      },
    });

    this.configureEditor();
  }

  private configureEditor(): void {
    console.groupCollapsed('Initialising blockly editor');
    try {
      console.log('[BlocklyEditor] Defining colours');
      Blockly.FieldColour.COLOURS = [
        '#f00', '#0f0',
        '#00f', '#ff0',
        '#f0f', '#0ff',
        '#000', '#fff'
      ];
      Blockly.FieldColour.COLUMNS = 2;

      console.log('[BlocklyEditor] Configuring modals');
      let that = this;
      let settings = new ConfirmSettings('');
      Blockly.dialog.setAlert(function (message: string, callback: any) {
        settings.prompt = message;
        settings.showCancel = false;
        that.confirm.confirm(settings)
          .subscribe(_ => callback());
      });
      Blockly.dialog.setConfirm(function (message: string, callback: any) {
        settings.prompt = message;
        settings.showCancel = true;
        that.confirm.confirm(settings)
          .subscribe(result => callback(result));
      });
      // Blockly.dialog.setPrompt(function (message: string, defaultValue: any, callback: any) {
      //   that.userInput.title = message;
      //   that.userInput.action = callback;
      //   that.userInput.showOk = true;
      //   that.userInput.showValue = true;
      //   that.userInput.mode = userInputMode.prompt;
      //   that.userInput.value = defaultValue;
      //   that.userInput.open = true;
      // });

      console.log('[BlocklyEditor] Adding validator');
      this.workspace.addChangeListener((evt: any) => this.validateWorkspace(evt));
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

  onCloseConnection(): void {
    this.connection.close();
  }

  onErrorOccurred(message: string): void {
    this.error = message;
  }

  onStepCompleted(step: number): number {
    return this.startupStatus.completeStep(step);
  }

  onStateUpdate(): void {
    this.isExecuting = this.messageProcessor?.isExecuting || false;
    if (!this.isExecuting) this.controller!.isPlaying = false;
  }

  onStepFailed(step: number, reason: string): void {
    this.startupStatus.failStep(step, reason);
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
    this.executionStatus.close(false);
  }
}
