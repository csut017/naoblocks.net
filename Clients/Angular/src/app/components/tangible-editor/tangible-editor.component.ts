import { AfterViewInit, Component, ElementRef, HostListener, Input, OnChanges, OnDestroy, OnInit, Renderer2, SimpleChanges, ViewChild } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { forkJoin, Subscription } from 'rxjs';
import { Block } from 'src/app/data/block';
import { ControllerAction } from 'src/app/data/controller-action';
import { EditorSettings } from 'src/app/data/editor-settings';
import { RunSettings } from 'src/app/data/run-settings';
import { StartupStatusTracker } from 'src/app/data/startup-status-tracker';
import { TangibleDefinition } from 'src/app/data/tangible-definition';
import { ConfirmService } from 'src/app/services/confirm.service';
import { ClientMessage, ClientMessageType, ConnectionService } from 'src/app/services/connection.service';
import { ExecutionStatusService } from 'src/app/services/execution-status.service';
import { ProgramControllerService } from 'src/app/services/program-controller.service';
import { ProgramService } from 'src/app/services/program.service';
import { ScriptLoaderService } from 'src/app/services/script-loader.service';
import { IServiceMessageUpdater, ServerMessageProcessorService } from 'src/app/services/server-message-processor.service';
import { environment } from 'src/environments/environment';

declare var Tangibles: any;
declare var TopCodes: any;

@Component({
  selector: 'app-tangible-editor',
  templateUrl: './tangible-editor.component.html',
  styleUrls: ['./tangible-editor.component.scss']
})
export class TangibleEditorComponent implements OnInit, OnChanges, AfterViewInit, IServiceMessageUpdater, OnDestroy {

  @Input() controller?: ProgramControllerService;
  @Input() editorSettings: EditorSettings = new EditorSettings();

  blocks: Block[] = [];
  cameraStarted: boolean = false;
  controllerSubscription?: Subscription;
  error: string = '';
  isExecuting: boolean = false;
  isInDebugMode: boolean = false;
  isInFlippedMode: boolean = false;
  isLoading: boolean = true;
  isProcessing: boolean = false;
  lastState: string = '';
  messageProcessor?: ServerMessageProcessorService;
  startupStatus: StartupStatusTracker = new StartupStatusTracker();

  private tangiblesMapping: { [index: number]: TangibleDefinition } = {};
  private context?: CanvasRenderingContext2D | null;
  private isInitialised: boolean = false;

  @ViewChild('videoCanvas', { static: true }) videoCanvas?: ElementRef<HTMLCanvasElement>;

  constructor(
    private connection: ConnectionService,
    private programService: ProgramService,
    private executionStatus: ExecutionStatusService,
    private renderer: Renderer2,
    private scriptLoader: ScriptLoaderService,
    private sanitizer: DomSanitizer,
    private confirm: ConfirmService) {
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
    console.log('[TangibleEditor] Retrieving block definitions');
    this.scriptLoader.loadScript(this.renderer, `${environment.apiURL}v1/ui/tangibles/all`)
      .subscribe(_ => {
        this.tangiblesMapping = {};
        Tangibles.blocks.forEach((tangible: TangibleDefinition) => {
          this.tangiblesMapping[tangible.number] = tangible;
        });
      });

    console.log('[TangibleEditor] Retrieving canvas');
    this.context = this.videoCanvas?.nativeElement.getContext('2d');
    if (this.context) {
      this.context.textAlign = 'center';
      this.context.textBaseline = 'middle'
      this.context.font = 'bold 36px serif';
      this.context.fillText('Initialising, please wait...', 320, 240);
    }
    this.lastState = this.generateCode(false);
  }

  ngAfterViewInit(): void {
    this.controllerSubscription = this.controller!.onAction.subscribe(event => {
      switch (event.action) {
        case ControllerAction.play:
          this.play(event.data);
          break;

        case ControllerAction.stop:
          this.stop();
          break;
      }
    });
    this.startCamera();
  }

  ngOnDestroy(): void {
    this.controllerSubscription?.unsubscribe();
    this.stopCamera();
  }


  ngOnChanges(_: SimpleChanges): void {
    if (this.editorSettings) this.isLoading = !this.editorSettings.isLoaded;
  }

  stop(): void {
    if (!this.messageProcessor?.isExecuting) {
      this.controller!.isPlaying = false;
      return;
    }

    let msg = new ClientMessage(ClientMessageType.StopProgram);
    msg.conversationId = this.messageProcessor?.lastConversationId;
    msg.values['robot'] = this.messageProcessor?.assignedRobot || '';
    this.connection.send(msg);
  }

  play(settings: RunSettings): void {
    console.groupCollapsed('[TangibleEditor] Playing program');
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

  private validateBlocks(): string | undefined {
    if (!this.blocks.length) {
      return 'There are no blocks in the current program!';
    }

    return undefined;
  }

  startCamera(): void {
    if (!this.isInitialised) {
      console.log('[TangibleEditor] Initialising callback');
      this.isInitialised = true;
      let me = this;
      TopCodes.setVideoFrameCallback("video-canvas", function (jsonString: string) {
        if (me.isExecuting || me.isProcessing) return;
        let json = JSON.parse(jsonString);
        me.highlightTags(json.topcodes);
        let blocks = me.generateBlockList(json.topcodes);
        if (me.isInFlippedMode) blocks.reverse();
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
      this.context!.fillStyle = "#fff";
      this.context!.fillRect(0, 0, 640, 480);
    }, 100);
  }

  generateBlockList(tags: any[]): Block[] {
    let output: Block[] = [];
    let last: any;
    let number = 1000;
    if (!tags.length) return output;

    // Sort the tags so we process from top to bottom, then left to right
    tags.sort((b1, b2) => {
      let yOffset = (b1.y - b2.y);
      if (Math.abs(yOffset) < 10) {
        return b2.x - b1.x;
      } else {
        return yOffset;
      }
    });

    for (let tag of tags) {
      // Filter the tags: only include those tags with a mapping and are close to the previous x position
      let definition = this.tangiblesMapping[tag.code];
      if (!definition) {
        console.log(`[TangibleEditor] Unknown tag ${tag.code}`);
        last = tag;
        continue;
      }

      if (last) {
        // We'll allow for a little bit of an offset
        const xOffset = Math.abs(tag.x - last.x);

        if (xOffset > 10) {
          console.log(`[TangibleEditor] Skipping tag ${tag.code} due to x offset ${xOffset}`);
          continue;
        }
      }

      output.push(Block.initialise(number++, definition, this.sanitizer));
      last = tag;
    }

    return output;
  }

  highlightTags(topcodes: any): void {
    if (!this.context) {
      console.error('Unable to retrieve Canvas context');
      return;
    }

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

    this.context.textAlign = 'right';
    this.context.textBaseline = 'top'
    this.context.font = '12px serif';
    this.context.fillStyle = "rgb(0, 0, 0)";
    this.context.fillText(this.isInFlippedMode ? 'Flipped' : 'Normal', 635, 5);
    this.context.fillStyle = "rgb(255, 255, 255)";
    this.context.fillText(this.isInFlippedMode ? 'Flipped' : 'Normal', 634, 4);
  }

  private generateCode(forRobot: boolean): string {
    try {
      // Try to intialise in a try-catch as Tangibles may not be loaded yet
      Tangibles.NaoLang.init();
    } catch {
      return '';
    }

    let blockCodes = this.blocks.map(b => b.generateCode(forRobot));
    let code = Tangibles.NaoLang.finish(
      blockCodes.join(''));

    console.groupCollapsed('[TangibleEditor] Generated code');
    console.log(code);
    console.groupEnd();

    return code;
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
    this.blocks.forEach(b => b.highlight = false);
  }

  onHighlightBlock(id: string, action: string): void {
    let isStart = (action === 'start');
    this.blocks.forEach(b => b.highlight = isStart && (b.id === id));
  }

  onShowDebug(): void {
    this.executionStatus.close(false);
  }
}
