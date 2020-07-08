import { Component, OnInit, OnDestroy } from '@angular/core';
import { SystemStatus } from '../data/system-status';
import { ConnectionService, ClientMessage, ClientMessageType } from '../services/connection.service';
import { HubClient } from '../data/hub-client';

@Component({
  selector: 'app-system-status',
  templateUrl: './system-status.component.html',
  styleUrls: ['./system-status.component.scss']
})
export class SystemStatusComponent implements OnInit, OnDestroy {

  status: SystemStatus = new SystemStatus();
  isLoading: boolean = false;
  lastConversationId: number;
  errorMessage: string;

  users: HubClient[] = [];
  robots: HubClient[] = [];
  clients: {[key:number]: HubClient} = {};

  constructor(private connection: ConnectionService) { }

  ngOnInit() {
    this.connection.start().subscribe(msg => this.processServerMessage(msg));
  }

  ngOnDestroy() {
    this.connection.close();
  }

  processServerMessage(msg: ClientMessage) {
    this.lastConversationId = msg.conversationId;
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

      case ClientMessageType.RobotStateUpdate:
        const clientId = Number.parseInt(msg.values.SourceId);
        const state = msg.values.state;
        let client = this.clients[clientId];
        if (client) client.status = state;
        break;
    }
  }

  private generateReply(msg: ClientMessage, type: ClientMessageType): ClientMessage {
    let newMsg = new ClientMessage(type);
    newMsg.conversationId = msg.conversationId;
    return newMsg;
  }
}