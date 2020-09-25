import { RunSettings } from '../data/run-settings';
import { ClientMessage, ClientMessageType, ConnectionService } from './connection.service';

export class ServerMessageProcessorService {

  assignedRobot: string;
  currentStartStep: number;
  isExecuting: boolean = false;
  lastConversationId: number;
  storedProgram: string;

  constructor(private connection: ConnectionService,
    private updater: IServiceMessageUpdater,
    private runSettings: RunSettings) { 
  }

  processServerMessage(msg: ClientMessage) {
    this.lastConversationId = msg.conversationId;
    switch (msg.type) {
      case ClientMessageType.Closed:
        console.log('[ServerMessageProcessorService] Connection to server lost');
        if (this.currentStartStep) {
          this.updater.onStepFailed(this.currentStartStep, 'Connection to server lost');
        } else if (this.isExecuting) {
          this.updater.onErrorOccurred('Connection to the server has been lost');
          this.changeExecuting(false);
        }
        break;

      case ClientMessageType.Error:
        console.groupCollapsed('[ServerMessageProcessorService] An error has occurred');
        console.log(msg.values);
        console.groupEnd();
        if (this.currentStartStep) {
          let errMsg = msg.values['error'] || 'Unknown';
          this.updater.onStepFailed(this.currentStartStep, `Server error: ${errMsg}`);
          this.currentStartStep = undefined;
          this.updater.onCloseConnection();
        }
        break;

      case ClientMessageType.Authenticated:
        console.log('[ServerMessageProcessorService] Authenticated');
        this.connection.send(this.generateReply(msg, ClientMessageType.RequestRobot));
        break;

      case ClientMessageType.RobotAllocated:
        console.log('[ServerMessageProcessorService] Robot allocated');
        this.assignedRobot = msg.values.robot;
        this.currentStartStep = this.updater.onStepCompleted(this.currentStartStep);
        let transferCmd = this.generateReply(msg, ClientMessageType.TransferProgram);
        transferCmd.values['robot'] = this.assignedRobot;
        transferCmd.values['program'] = this.storedProgram;
        this.connection.send(transferCmd);
        break;

      case ClientMessageType.NoRobotsAvailable:
        console.log('[ServerMessageProcessorService] Unable to allocate robot');
        this.updater.onStepFailed(this.currentStartStep, 'No robots available');
        this.currentStartStep = undefined;
        this.updater.onCloseConnection();
        break;

      case ClientMessageType.ProgramTransferred:
        console.log('[ServerMessageProcessorService] Programed transferred');
        this.currentStartStep = this.updater.onStepCompleted(this.currentStartStep);
        let startCmd = this.generateReply(msg, ClientMessageType.StartProgram);
        startCmd.values['robot'] = this.assignedRobot;
        startCmd.values['program'] = this.storedProgram;
        startCmd.values['opts'] = JSON.stringify(this.runSettings);
        this.connection.send(startCmd);
        break;

      case ClientMessageType.UnableToDownloadProgram:
        console.log('[ServerMessageProcessorService] Unable to transfer program');
        this.updater.onStepFailed(this.currentStartStep, 'Program download failed');
        this.currentStartStep = undefined;
        this.updater.onCloseConnection();
        break;

      case ClientMessageType.ProgramStarted:
        console.log('[ServerMessageProcessorService] Program started');
        this.updater.onShowDebug();
        this.changeExecuting(true);
        this.currentStartStep = undefined;
        break;

      case ClientMessageType.ProgramFinished:
      case ClientMessageType.ProgramStopped:
        console.log('[ServerMessageProcessorService] Program stopped');
        this.changeExecuting(false);
        this.updater.onCloseConnection();
        this.updater.onClearHighlight();
        break;

      case ClientMessageType.RobotDebugMessage:
        console.groupCollapsed('[ServerMessageProcessorService] Debug message received');
        console.log(msg.values);
        console.groupEnd();
        let sId = msg.values['sourceID'];
        let action = msg.values['status'];
        this.updater.onHighlightBlock(sId, action);
        break;
    }
  }

  private generateReply(msg: ClientMessage, type: ClientMessageType): ClientMessage {
    let newMsg = new ClientMessage(type);
    newMsg.conversationId = msg.conversationId;
    return newMsg;
  }

  private changeExecuting(newValue: boolean): void {
    if (newValue == this.isExecuting) return;

    this.isExecuting = newValue;
    this.updater.onStateUpdate();
  }
}

export interface IServiceMessageUpdater {
  onClearHighlight(): void;
  onCloseConnection(): void;
  onStepCompleted(step: number): number;
  onHighlightBlock(id: string, action: string): void;
  onErrorOccurred(message: string): void;
  onShowDebug(): void;
  onStateUpdate(): void;
  onStepFailed(step: number, reason: string): void;
}