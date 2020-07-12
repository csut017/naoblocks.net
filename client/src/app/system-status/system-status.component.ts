import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { SystemStatus } from '../data/system-status';
import { ConnectionService, ClientMessage, ClientMessageType } from '../services/connection.service';
import { HubClient } from '../data/hub-client';
import { DebugMessage } from '../data/debug-message';
import { StatusMessage } from '../data/status-message';
import { ProgramService } from '../services/program.service';
import { Compilation } from '../data/compilation';
import { ProgramDisplayComponent } from '../program-display/program-display.component';
import { v4 as uuidv4 } from 'uuid';
import { AuthenticationService } from '../services/authentication.service';
 
@Component({
  selector: 'app-system-status',
  templateUrl: './system-status.component.html',
  styleUrls: ['./system-status.component.scss']
})
export class SystemStatusComponent implements OnInit, OnDestroy {

  @ViewChild(ProgramDisplayComponent)
  private programDisplay: ProgramDisplayComponent;

  status: SystemStatus = new SystemStatus();
  isLoading: boolean = false;
  lastConversationId: number;
  errorMessage: string;
  messagesOpen: boolean;
  programOpen: boolean;
  isProgramLoading: boolean;
  currentClient: HubClient;
  currentProgram: Compilation;

  users: HubClient[] = [];
  robots: HubClient[] = [];
  clients: { [key: number]: HubClient } = {};

  constructor(private connection: ConnectionService,
    private programService: ProgramService,
    private authenticationService: AuthenticationService) { }

  ngOnInit() {
    this.connection.start().subscribe(msg => this.processServerMessage(msg));
  }

  ngOnDestroy() {
    this.connection.close();
  }

  displayMessages(client: HubClient) {
    this.messagesOpen = true;
    this.currentClient = client;
  }

  displayProgram(client: HubClient) {
    this.programOpen = true;
    this.currentClient = client;
    this.isProgramLoading = true;
    this.programService.getAST(client.user.name, client.programId.toString())
      .subscribe(result => {
        this.currentProgram = result.output;
        this.programDisplay.loadProgram(this.currentProgram);
        client.messages.forEach(msg => {
          let debug = msg as DebugMessage;
          if (debug && debug.sourceID && (debug.programId == client.programId)) this.programDisplay.updateStatus(debug);
        });
        this.isProgramLoading = false;
      });
  }

  openPopout(view: string) {
    const guid = uuidv4();
    const encodedGuid = btoa(guid);
    const baseUrl = location.protocol + '//' + location.hostname + (location.port ? ':' + location.port : '');
    const url = `${baseUrl}/popout/${view}/${encodedGuid}`;
    const data = {
      session: this.authenticationService.token,
      client: this.currentClient.id
    };
    localStorage.setItem(guid, JSON.stringify(data));
    console.log('Opening popout ' + url);
    window.open(url, '_blank', 'menubar=no,resizable=yes,scrollbars=no,status=no');
  }

  processServerMessage(msg: ClientMessage) {
    this.lastConversationId = msg.conversationId;
    const clientId = Number.parseInt(msg.values.SourceId);
    let client = this.clients[clientId];
    switch (msg.type) {
      case ClientMessageType.Closed:
        this.errorMessage = 'Connection to the server has been lost';
        break;

      case ClientMessageType.Authenticated:
        this.connection.send(this.generateReply(msg, ClientMessageType.StartMonitoring));
        break;

      case ClientMessageType.ClientAdded:
        let newClient = new HubClient();
        newClient.id = Number.parseInt(msg.values.ClientId);
        newClient.name = msg.values.Name;
        newClient.type = msg.values.Type;
        newClient.subType = msg.values.SubType;
        newClient.status = msg.values.state;
        this.clients[newClient.id] = newClient;
        switch (newClient.type) {
          case "robot":
            this.robots.push(newClient);
            break;

          case "user":
            newClient.student = msg.values.IsStudent == 'yes';
            this.users.push(newClient);
            break;
        }
        break;

      case ClientMessageType.ClientRemoved:
        const oldId = Number.parseInt(msg.values.ClientId);
        let robot = this.robots.find(r => r.id == oldId);
        let user = this.users.find(r => r.id == oldId);
        if (robot) this.robots = this.robots.filter(r => r.id != oldId);
        if (user) this.users = this.users.filter(r => r.id != oldId);
        break;

      case ClientMessageType.RobotAllocated:
        if (client && msg.values.robot) {
          const robotId = Number.parseInt(msg.values.robot);
          client.robot = this.clients[robotId];
          if (client.robot) {
            client.robot.messages.push(this.generateStatusMsg('assign-user', `Assigned to ${client.name}`));
            client.robot.user = client;
            client.robot.programId = undefined;
          }
        }
        break;

      case ClientMessageType.ProgramTransferred:
        client.programId = Number.parseInt(msg.values.ProgramId);
        client.messages.push(this.generateStatusMsg('install', `Program has been downloaded`));
        if (this.programOpen) this.displayProgram(client);
        break;

      case ClientMessageType.RobotStateUpdate:
        const state = msg.values.state;
        if (client) {
          client.status = state;
        }
        break;

      case ClientMessageType.RobotDebugMessage:
        if (client) {
          let debugMsg = new DebugMessage();
          debugMsg.function = msg.values.function;
          debugMsg.sourceID = msg.values.sourceID;
          debugMsg.status = msg.values.status;
          debugMsg.programId = client.programId;
          client.messages.push(debugMsg);

          this.programDisplay.updateStatus(debugMsg);
        }
        break;

      case ClientMessageType.ProgramStarted:
        client.messages.push(this.generateStatusMsg('play', `Program has started`));
        break;

      case ClientMessageType.ProgramFinished:
        client.messages.push(this.generateStatusMsg('check', `Program has finished`));
        break;

      case ClientMessageType.ProgramStopped:
        client.messages.push(this.generateStatusMsg('stop', `Program was stopped`));
        break;

      default:
        this.log(`Unhandled message type ${msg.type}`);
        break;
    }
  }

  private generateStatusMsg(type: string, message: string): StatusMessage {
    let msg = new StatusMessage();
    msg.type = type;
    msg.message = message;
    return msg;
  }

  private generateReply(msg: ClientMessage, type: ClientMessageType): ClientMessage {
    let newMsg = new ClientMessage(type);
    newMsg.conversationId = msg.conversationId;
    return newMsg;
  }

  private log(message: string) {
    const msg = `[SystemStatus] ${message}`;
    console.log(msg);
  }
}