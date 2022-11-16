import { AfterViewInit, Component, ElementRef, HostListener, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { Block } from '../data/block';
import { ExecutionStatusStep } from '../data/execution-status-step';
import { RunSettings } from '../data/run-settings';
import { StartupStatusTracker } from '../data/startup-status-tracker';
import { User } from '../data/user';
import { HomeBase } from '../home-base';
import { RunSettingsComponent } from '../run-settings/run-settings.component';
import { AuthenticationService } from '../services/authentication.service';
import { ClientMessage, ClientMessageType, ConnectionService } from '../services/connection.service';
import { ProgramService } from '../services/program.service';
import { IServiceMessageUpdater, ServerMessageProcessorService } from '../services/server-message-processor.service';
import { SnapshotService } from '../services/snapshot.service';

declare var TopCodes: any;

@Component({
  selector: 'app-tangible-editor',
  templateUrl: './tangible-editor.component.html',
  styleUrls: ['./tangible-editor.component.scss']
})
export class TangibleEditorComponent extends HomeBase implements OnInit, IServiceMessageUpdater, OnDestroy, AfterViewInit {

  blocks: Block[] = [];
  cameraStarted: boolean = false;
  canRun: boolean = false;
  currentUser: User;
  isExecuting: boolean = false;
  isInDebugMode: boolean = false;
  isInFlippedMode: boolean = false;
  isProcessing: boolean = false;
  isStoringState: boolean = false;
  lastState: string;
  messageProcessor: ServerMessageProcessorService;
  runSettings: RunSettings = new RunSettings();
  sidebarCollapsed: boolean = false;
  startupStatus: StartupStatusTracker = new StartupStatusTracker();

  private isInitialised: boolean = false;
  private context: CanvasRenderingContext2D;
  private blockMapping: { [index: number]: Block } = {
    // Both
    31: new Block("stand_block", "Stand", "position('Stand')"),
    93: new Block("rest_block", "Rest", "rest ()"),
    313: new Block("raise_right_arm_block", "Point Right", "point('right','out')"),

    // Normal
    47: new Block("wave_block", "Wave", "wave()"),
    55: new Block("dance_block", "Dance", "dance('gangnam', TRUE)"),
    157: new Block("stand_block", "Sit", "position('Sit')"),
    199: new Block("raise_left_arm_block", "Point Left", "point('left','out')"),
    283: new Block("stand_block", "Lie Down", "position('LyingBack')"),
    301: new Block("walk_block", "Walk", "walk ( 6 , 0 )"),
    331: new Block("speak_block", "Kia ora", "say('kia ora')"),

    // Mirrored
    59: new Block("dance_block", "Dance", "dance('gangnam', TRUE)"),
    61: new Block("wave_block", "Wave", "wave()"),
    185: new Block("stand_block", "Sit", "position('Sit')"),
    227: new Block("raise_left_arm_block", "Point Left", "point('left','out')"),
    361: new Block("walk_block", "Walk", "walk ( 6 , 0 )"),
    421: new Block("speak_block", "Kia ora", "say('kia ora')"),
    433: new Block("stand_block", "Lie Down", "position('LyingBack')"),
  };

  @ViewChild(RunSettingsComponent) runSettingsDisplay: RunSettingsComponent;
  @ViewChild('videoCanvas', { static: true }) videoCanvas: ElementRef<HTMLCanvasElement>;

  constructor(authenticationService: AuthenticationService,
    router: Router,
    private snapshotService: SnapshotService,
    private programService: ProgramService,
    private connection: ConnectionService) {
    super(authenticationService, router);
  }

  @HostListener('document:keydown', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent) {
    if (event.altKey && event.shiftKey) {
      switch (event.key) {
        case 'D':
          this.isInDebugMode = !this.isInDebugMode;
          console.log(`[TangibleEditor] ${this.isInDebugMode ? 'Showing' : 'Hiding'} debug display`);
          break;

        case 'F':
          this.isInFlippedMode = !this.isInFlippedMode;
          console.log(`[TangibleEditor] ${this.isInFlippedMode ? 'Flipped' : 'Normal'} block order`);
          break;
      }
    }
  }

  ngOnInit(): void {
    console.log('[TangibleEditor] retrieving context');
    this.context = this.videoCanvas.nativeElement.getContext('2d');

    this.authenticationService.getCurrentUser()
      .subscribe(u => this.currentUser = u);
    this.lastState = this.generateCode(false);
    setInterval(() => this.storeState(), 10000);
  }

  storeState(): void {
    let state = this.generateCode(false);
    if (state == this.lastState) return;

    console.log(`[TangibleEditor] Storing snapshot`);
    this.isStoringState = true;
    this.snapshotService.store(state, 'TangibleEditor', {
    })
      .subscribe(s => {
        this.isStoringState = false;
        if (s.successful) {
          this.lastState = state;
          console.log(`[TangibleEditor] Snapshot stored`);
        }
      });
  }

  ngAfterViewInit(): void {
    this.startCamera();
  }

  ngOnDestroy() {
    this.stopCamera();
  }

  highlightTags(topcodes: any): void {
    this.context.fillStyle = "rgba(255, 0, 0, 0.3)";
    for (let loop = 0; loop < topcodes.length; loop++) {
      this.context.beginPath();
      this.context.arc(
        topcodes[loop].x,
        topcodes[loop].y,
        topcodes[loop].radius,
        0,
        Math.PI * 2,
        true
      );
      this.context.fill();
    }
  }

  generateBlockList(tags: any[]): Block[] {
    let output: Block[] = [];
    let last: any;
    let number = 1000;
    for (let tag of tags) {
      // Filter the tags: only include those tags with a mapping and are close to the previous x position
      let defn = this.blockMapping[tag.code];
      if (!defn) {
        console.groupCollapsed('[TangibleEditor] Unknown tag');
        console.log(tag.code);
        console.groupEnd();
        continue;
      }

      if (last) {
        if ((tag.x > last.x + 10) && (tag.x < last.x - 10)) {
          continue;
        }
      }

      output.push(defn.initialise(number++));
      last = tag;
    }

    return output;
  }

  startCamera(): void {
    if (!this.isInitialised) {
      console.log('[TangibleEditor] Initialising callback');
      this.isInitialised = true;
      let me = this;
      TopCodes.setVideoFrameCallback("video-canvas", function (jsonString) {
        if (me.isExecuting || this.isProcessing) return;
        let json = JSON.parse(jsonString);
        me.highlightTags(json.topcodes);
        let blocks = me.generateBlockList(json.topcodes);
        if (me.isInFlippedMode) blocks.reverse();
        me.canRun = !!blocks.length;
        me.blocks = blocks;
      });
    }

    if (this.cameraStarted) return;

    console.log('[TangibleEditor] Starting camera');
    this.cameraStarted = true;
    TopCodes.startStopVideoScan('video-canvas');
  }

  stopCamera(): void {
    if (!this.cameraStarted) return;

    console.log('[TangibleEditor] Stopping camera');
    this.cameraStarted = false;
    TopCodes.startStopVideoScan('video-canvas');
    setTimeout(() => {
      this.context.fillStyle = "#ddd";
      this.context.fillRect(0, 0, 640, 480);
    }, 100);
  }

  private generateCode(forRobot: boolean): string {
    let blockCodes = this.blocks.map(b => b.generateCode(forRobot));
    let code = "reset()\nstart{\n" +
      blockCodes.join('\n') +
      "\n}\ngo()\n";

    console.groupCollapsed('[TangibleEditor] Generated code');
    console.log(code);
    console.groupEnd();

    return code;
  }

  doPlay(): void {
    this.isProcessing = true;
    this.startupStatus.initialise();
    if (!this.blocks.length) {
      this.onStepFailed(0, 'There are no blocks in the current program!');
      this.isProcessing = false;
      return;
    }

    let code = this.generateCode(true);
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

  onClearHighlight(): void {
    this.blocks.forEach(b => b.highlight = false);
  }

  onCloseConnection(): void {
    this.connection.close();
  }

  onStepCompleted(step: number): number {
    return this.startupStatus.completeStep(step);
  }

  onHighlightBlock(id: string, action: string): void {
    let isStart = (action === 'start');
    this.blocks.forEach(b => b.highlight = isStart && (b.id === id));
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

  doStop(): void {
    let msg = new ClientMessage(ClientMessageType.StopProgram);
    msg.conversationId = this.messageProcessor.lastConversationId;
    msg.values['robot'] = this.messageProcessor.assignedRobot;
    this.connection.send(msg);
  }
}