import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { SystemStatus } from '../data/system-status';
import { ConnectionService } from '../services/connection.service';
import { HubClient } from '../data/hub-client';
import { DebugMessage } from '../data/debug-message';
import { ProgramDisplayComponent } from '../program-display/program-display.component';
import { v4 as uuidv4 } from 'uuid';
import { AuthenticationService } from '../services/authentication.service';
import { MessageProcessor } from '../services/message-processor';

@Component({
  selector: 'app-system-status',
  templateUrl: './system-status.component.html',
  styleUrls: ['./system-status.component.scss']
})
export class SystemStatusComponent implements OnInit, OnDestroy {

  @ViewChild(ProgramDisplayComponent)
  programDisplay: ProgramDisplayComponent;

  status: SystemStatus = new SystemStatus();
  isLoading: boolean = false;
  messagesOpen: boolean;
  programOpen: boolean;
  currentClient: HubClient;

  processor: MessageProcessor;

  constructor(private connection: ConnectionService,
    private authenticationService: AuthenticationService) {
  }

  ngOnInit() {
    this.processor = new MessageProcessor(this.connection);
    this.processor.programTransferred.subscribe((client: HubClient) => {
      if (this.programOpen) this.displayProgram(client);
    });
    this.processor.debugMessageReceived.subscribe((msg: DebugMessage) => this.programDisplay.updateStatus(msg));
    this.connection.start().subscribe(msg => this.processor.processServerMessage(msg));
  }
  displayProgram(client: HubClient) {
    this.currentClient = client;
    this.programOpen = true;
    this.programDisplay.display(client);
  }

  ngOnDestroy() {
    this.connection.close();
  }

  displayMessages(client: HubClient) {
    this.messagesOpen = true;
    this.currentClient = client;
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
}