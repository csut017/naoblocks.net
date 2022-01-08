import { ClientMessage, ClientMessageType, ConnectionService } from './connection.service';
import { HubClient } from '../data/hub-client';
import { DebugMessage } from '../data/debug-message';
import { StatusMessage } from '../data/status-message';
import { EventEmitter } from '@angular/core';
import { NotificationAlert } from '../data/notification-alert';

export class MessageProcessor {

    constructor(private connection: ConnectionService) {
    }

    lastConversationId?: number;
    errorMessage?: string;

    users: HubClient[] = [];
    robots: HubClient[] = [];
    clients: { [key: number]: HubClient } = {};

    clientAdded: EventEmitter<HubClient> = new EventEmitter<HubClient>();
    programTransferred: EventEmitter<HubClient> = new EventEmitter<HubClient>();
    debugMessageReceived: EventEmitter<DebugMessage> = new EventEmitter<DebugMessage>();

    processServerMessage(msg: ClientMessage) {
        this.lastConversationId = msg.conversationId;
        const clientId = Number.parseInt(msg.values['SourceId']);
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
                newClient.id = Number.parseInt(msg.values['ClientId']);
                newClient.name = msg.values['Name'];
                newClient.type = msg.values['Type'];
                newClient.subType = msg.values['SubType'];
                newClient.status = msg.values['state'];
                this.clients[newClient.id] = newClient;
                switch (newClient.type) {
                    case "robot":
                        this.robots.push(newClient);
                        let getAlertsMsg = new ClientMessage(ClientMessageType.AlertsRequest);
                        getAlertsMsg.values = {
                            'robot': newClient.id.toString()
                        };
                        this.connection.send(getAlertsMsg);
                        break;

                    case "user":
                        newClient.student = msg.values['IsStudent'] == 'yes';
                        this.users.push(newClient);
                        break;
                }
                this.clientAdded.emit(newClient);
                break;

            case ClientMessageType.ClientRemoved:
                const oldId = Number.parseInt(msg.values['ClientId']);
                let robot = this.robots.find(r => r.id == oldId);
                let user = this.users.find(r => r.id == oldId);
                if (robot) this.robots = this.robots.filter(r => r.id != oldId);
                if (user) this.users = this.users.filter(r => r.id != oldId);
                break;

            case ClientMessageType.RobotAllocated:
                if (client && msg.values['robot']) {
                    const robotId = Number.parseInt(msg.values['robot']);
                    client.robot = this.clients[robotId];
                    if (client.robot) {
                        client.robot.messages.push(this.generateStatusMsg('assign-user', `Assigned to ${client.name}`));
                        client.robot.user = client;
                        client.robot.programId = undefined;
                    }
                }
                break;

            case ClientMessageType.ProgramTransferred:
                client.programId = Number.parseInt(msg.values['ProgramId']);
                client.messages.push(this.generateStatusMsg('install', `Program has been downloaded`));
                this.programTransferred.emit(client);
                break;

            case ClientMessageType.RobotStateUpdate:
                const state = msg.values['state'];
                if (client) {
                    client.status = state;
                }
                break;

            case ClientMessageType.RobotDebugMessage:
                if (client) {
                    let debugMsg = new DebugMessage();
                    debugMsg.function = msg.values['function'];
                    debugMsg.sourceID = msg.values['sourceID'];
                    debugMsg.status = msg.values['status'];
                    debugMsg.programId = client.programId;
                    client.messages.push(debugMsg);
                    this.debugMessageReceived.emit(debugMsg);
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

            case ClientMessageType.AlertBroadcast:
                let alert = new NotificationAlert();
                alert.message = msg.values['message'];
                alert.severity = msg.values['severity'];
                client.notifications.push(alert);
                client.hasNotifications = true;
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
        const msg = `[MessageProcessor] ${message}`;
        console.log(msg);
    }
}
