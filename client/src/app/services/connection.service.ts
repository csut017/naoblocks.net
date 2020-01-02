import { Injectable } from '@angular/core';
import { ClientService } from './client.service';
import { ErrorHandlerService } from './error-handler.service';
import { environment } from 'src/environments/environment';
import { Observable, Subject } from 'rxjs';
import { AuthenticationService } from './authentication.service';

export class ClientMessage {
  values: { [id: string]: string } = {};
  constructor(public type: ClientMessageType) { }
}

export enum ClientMessageType {
  Authenticate = 1,
  Authenticated = 2,
  RequestRobot = 11,
  RobotAllocated = 12,
  NoRobotsAvailable = 13,
  TransferProgram = 20,
  ProgramTransferred = 21,
  StartProgram = 101,
  ProgramStarted = 102,
  StopProgram = 201,
  ProgramStopped = 202,
  Error = 1000,
  Closed = 5001,
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
    super('ConnectionService', errorHandler);
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
