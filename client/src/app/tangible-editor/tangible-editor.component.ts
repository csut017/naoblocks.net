import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { Block } from '../data/block';
import { ExecutionStatusStep } from '../data/execution-status-step';
import { RunSettings } from '../data/run-settings';
import { User } from '../data/user';
import { HomeBase } from '../home-base';
import { AuthenticationService } from '../services/authentication.service';
import { ConnectionService } from '../services/connection.service';
import { ProgramService } from '../services/program.service';
import { IServiceMessageUpdater, ServerMessageProcessorService } from '../services/server-message-processor.service';

declare var TopCodes: any;

@Component({
  selector: 'app-tangible-editor',
  templateUrl: './tangible-editor.component.html',
  styleUrls: ['./tangible-editor.component.scss']
})
export class TangibleEditorComponent extends HomeBase implements OnInit, IServiceMessageUpdater {

  blocks: Block[] = [];
  cameraStarted: boolean = false;
  canRun: boolean = false;
  currentUser: User;
  messageProcessor: ServerMessageProcessorService;
  runSettings: RunSettings = new RunSettings();
  sendingToRobot: boolean = false;
  startMessage: string;
  steps: ExecutionStatusStep[];

  private isInitialised: boolean = false;
  private context: CanvasRenderingContext2D;
  private blockMapping: { [index: number]: Block } = {
    31: new Block("stand_block", "position('Stand')"),
    47: new Block("speak_block", "say('hello')"),
    59: new Block("dance_block", "dance('gangnam', TRUE)"),
    61: new Block("wave_block", "wave()"),
    79: new Block("raise_right_arm_block", "point('right','out')"),
    93: new Block("raise_left_arm_block", "point('left','out')"),
    109: new Block("look_right_block", "look('right')"),
    117: new Block("look_left_block", "look('left')"),
    121: new Block("walk_block", "walk ( 6 , 0 )"),
    233: new Block("rest_block", "rest ()"),
  };


  @ViewChild('videoCanvas', { static: true }) videoCanvas: ElementRef<HTMLCanvasElement>;

  constructor(authenticationService: AuthenticationService,
    router: Router,
    private programService: ProgramService,
    private connection: ConnectionService) {
    super(authenticationService, router);
  }

  ngOnInit(): void {
    console.log('[TangibleEditorComponent] retrieving context');
    this.context = this.videoCanvas.nativeElement.getContext('2d');
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
        console.groupCollapsed('[TangibleEditorComponent] Unknown tag');
        console.log(tag.code);
        console.groupEnd();
        continue;
      }

      if (last) {
        if ((tag.x > last.x + 10) && (tag.x < last.x - 10)) {
          continue;
        }
      }

      output.push(defn.generate(number++));
      last = tag;
    }

    return output;
  }

  startCamera(): void {
    if (!this.isInitialised) {
      console.log('[TangibleEditorComponent] Initialising callback');
      this.isInitialised = true;
      let me = this;
      TopCodes.setVideoFrameCallback("video-canvas", function (jsonString) {
        let json = JSON.parse(jsonString);
        me.highlightTags(json.topcodes);
        let blocks = me.generateBlockList(json.topcodes);
        me.canRun = !!blocks.length;
        me.blocks = blocks;
      });
    }

    if (this.cameraStarted) return;

    console.log('[TangibleEditorComponent] Starting camera');
    this.cameraStarted = true;
    TopCodes.startStopVideoScan('video-canvas');
  }

  stopCamera(): void {
    if (!this.cameraStarted) return;

    console.log('[TangibleEditorComponent] Stopping camera');
    this.cameraStarted = false;
    TopCodes.startStopVideoScan('video-canvas');
  }

  executeCode(): void {
    let blockCodes = this.blocks.map(b => b.action);
    let code = "reset()\nstart{\n" +
      blockCodes.join('\n') +
      "\n}\ngo()\n";
    console.groupCollapsed('[TangibleEditorComponent] Generated code');
    console.log(code);
    console.groupEnd();

    this.messageProcessor = new ServerMessageProcessorService(this.connection, this, this.runSettings);
    this.initialiseStartingUI();
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

  onClearHighlight(): void {
  }

  onCloseConnection(): void {
    this.connection.close();
  }

  onStepCompleted(step: number): number {
    if (step >= this.steps.length) return step;
    this.steps[step].isCurrent = false;
    this.steps[step].image = 'success-standard';

    if (++step >= this.steps.length) return step;
    this.steps[step].isCurrent = true;
    return step;
  }

  onHighlightBlock(id: string, action: string): void {
  }

  onErrorOccurred(message: string): void {
  }

  onShowDebug(): void {
  }

  onStateUpdate(): void {
  }

  onStepFailed(step: number, reason: string): void {
    this.startMessage = reason;
    if (!this.steps[step]) return;
    this.steps[step].isCurrent = false;
    this.steps[step].image = 'error-standard';
  }
}
