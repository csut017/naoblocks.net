import { Injectable } from '@angular/core';
import { ClientService } from './client.service';
import { ErrorHandlerService } from './error-handler.service';
import { environment } from 'src/environments/environment';
import { Observable, Subject } from 'rxjs';
import { AuthenticationService } from './authentication.service';

export class ClientMessage {
  conversationId: number;
  values: { [id: string]: string } = {};
  constructor(public type: ClientMessageType) { }
}

export enum ClientMessageType {
  // These values are copied from ClientMessageType.cs in NaoBlocks.Web
  Authenticate = 1,               // Send the client credentials to the server
  Authenticated = 2,              // Credentials are valid
  RequestRobot = 11,              // Request a robot to run a program on
  RobotAllocated = 12,            // Allocate a robot to the client
  NoRobotsAvailable = 13,         // There are no clients available
  TransferProgram = 20,           // Request the server to inform the robot to download a program
  ProgramTransferred = 21,        // Reply from the server when the robot has finished downloading
  DownloadProgram = 22,           // Request the robot to download a program
  ProgramDownloaded = 23,         // The robot has finished downloading the program
  UnableToDownloadProgram = 24,   // The program cannot be downloaded to the robot
  StartProgram = 101,             // Start execution of a program
  ProgramStarted = 102,           // Program execution has started
  ProgramFinished = 103,          // Program execution has finished
  StopProgram = 201,              // Request cancellation of a program
  ProgramStopped = 202,           // Program has been cancelled
  RobotStateUpdate = 501,         // An update from the robot about its state
  RobotDebugMessage = 502,        // A debug message from the robot (normally a step has started)
  RobotError = 503,               // An error that occurred during execution of a program
  Error = 1000,                   // A general error (e.g. message type not recognised)
  NotAuthenticated = 1001,        // The client has not been authenticated
  Forbidden = 1002,               // The client is not allowed to call the functionality
  StartMonitoring = 1100,         // Start monitoring all client changes
  StopMonitoring = 1101,          // Stop monitoring all client changes
  ClientAdded = 1102,             // A new client has connected to the system
  ClientRemoved = 1103,           // An existing client has disconnected

  // These values are client specific codes, they do not exist on the server
  Closed = 5001
}

@Injectable({
  providedIn: 'root'
})
export class ConnectionService extends ClientService {

  isConnected: boolean = false;
  private socket: WebSocket;
  private output: Subject<ClientMessage> = new Subject<ClientMessage>();

  constructor(errorHandler: ErrorHandlerService,
    private authenticationService: AuthenticationService) {
    super(errorHandler);
    this.serviceName = 'ConnectionService';
}

  start(): Observable<ClientMessage> {
    if (!this.isConnected) {
      const url = `${environment.wsURL}v1/connections/user`;
      this.log('Starting connection');
      this.socket = new WebSocket(url);
      this.socket.onclose = _ => {
        this.log('Connection closed');
        this.isConnected = false;
        this.output.next(new ClientMessage(ClientMessageType.Closed));
      };
      this.socket.onopen = _ => {
        let msg = new ClientMessage(ClientMessageType.Authenticate);
        msg.values["token"] = this.authenticationService.token;
        this.send(msg);
      };
      this.socket.onerror = _ => this.error('An error has been received');
      this.socket.onmessage = msg => this.processMessage(msg);
      this.isConnected = true;
    }

    return this.output.asObservable();
  }

  close(): void {
    this.log('Closing connection');
    this.socket.close();
    this.output = new Subject<ClientMessage>();
  }

  send(msg: ClientMessage): void {
    const data = JSON.stringify(msg);
    this.socket.send(data);
  }

  private processMessage(msg: any): void {
    const data = JSON.parse(msg.data);
    this.logData('Message received', data);
    this.output.next(data);
  }
}
