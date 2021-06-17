import { AfterViewInit, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { RunSettings } from '../data/run-settings';
import { StartupStatusTracker } from '../data/startup-status-tracker';
import { User } from '../data/user';
import { HomeBase } from '../home-base';
import { RunSettingsComponent } from '../run-settings/run-settings.component';
import { AuthenticationService } from '../services/authentication.service';
import { ClientMessage, ClientMessageType, ConnectionService } from '../services/connection.service';
import { ProgramService } from '../services/program.service';
import { IServiceMessageUpdater, ServerMessageProcessorService } from '../services/server-message-processor.service';

@Component({
  selector: 'app-text-editor',
  templateUrl: './text-editor.component.html',
  styleUrls: ['./text-editor.component.scss']
})
export class TextEditorComponent extends HomeBase implements OnInit, IServiceMessageUpdater {

  canRun: boolean = false;
  code: string;
  currentUser: User;
  isExecuting: boolean = false;
  isProcessing: boolean = false;
  messageProcessor: ServerMessageProcessorService;
  runSettings: RunSettings = new RunSettings();
  sidebarCollapsed: boolean = false;
  startupStatus: StartupStatusTracker = new StartupStatusTracker();

  @ViewChild(RunSettingsComponent) runSettingsDisplay: RunSettingsComponent;

  constructor(authenticationService: AuthenticationService,
    router: Router,
    private programService: ProgramService,
    private connection: ConnectionService) {
    super(authenticationService, router);
  }

  ngOnInit(): void {
    this.authenticationService.getCurrentUser()
      .subscribe(u => this.currentUser = u);
  }

  onClearHighlight(): void {
  }

  onCloseConnection(): void {
    this.connection.close();
  }

  onStepCompleted(step: number): number {
    return this.startupStatus.completeStep(step);
  }

  onHighlightBlock(id: string, action: string): void {
  }

  onErrorOccurred(message: string): void {
    this.startupStatus.startMessage = message;
  }

  onShowDebug(): void {
    this.startupStatus.sendingToRobot = false;
  }

  onStateUpdate(): void {
    this.isExecuting = this.messageProcessor.isExecuting;
  }

  onStepFailed(step: number, reason: string): void {
    this.startupStatus.failStep(step, reason);
  }

  doCancelSend(): void {
    if (this.startupStatus.cancel()) {
      this.doStop();
    }
  }

  doChangeSpeed(): void {
    this.runSettingsDisplay.show(this.runSettings);
  }

  onChangeRunSettings(settings: RunSettings): void {
    this.runSettings = settings;
    this.runSettingsDisplay.close();
  }

  doPlay(): void {
    this.isProcessing = true;
    this.startupStatus.initialise();
    let code = this.code;
    if (!this.code || !this.code.length) {
      this.onStepFailed(0, 'There are no code in the current program!');
      this.isProcessing = false;
      return;
    }

    this.messageProcessor = new ServerMessageProcessorService(this.connection, this, this.runSettings);
    this.programService.compile(code)
      .subscribe(result => {
        if (!result.successful) {
          this.onStepFailed(0, 'Unable to compile code');
          this.isProcessing = false;
          return;
        }

        if (result.output.errors) {
          this.onStepFailed(0, 'There are errors in the code');
          this.isProcessing = false;
          return;
        }

        this.messageProcessor.storedProgram = result.output.programId.toString();
        this.messageProcessor.currentStartStep = 1;
        this.onStepCompleted(0);
        this.connection.start().subscribe(msg => this.messageProcessor.processServerMessage(msg));
        this.isProcessing = false;
      });
  }

  doStop(): void {
    let msg = new ClientMessage(ClientMessageType.StopProgram);
    msg.conversationId = this.messageProcessor.lastConversationId;
    msg.values['robot'] = this.messageProcessor.assignedRobot;
    this.connection.send(msg);
  }
}
