import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { DebugMessage } from 'src/app/data/debug-message';
import { HubClient } from 'src/app/data/hub-client';
import { AuthenticationService } from 'src/app/services/authentication.service';
import { ConnectionService } from 'src/app/services/connection.service';
import { MessageProcessor } from 'src/app/services/message-processor';

@Component({
  selector: 'app-system-dashboard',
  templateUrl: './system-dashboard.component.html',
  styleUrls: ['./system-dashboard.component.scss']
})
export class SystemDashboardComponent implements OnInit {

  @Output() currentItemChanged = new EventEmitter<string>();

  currentClient?: HubClient;
  processor: MessageProcessor;
  programOpen: boolean = false;

  constructor(private connection: ConnectionService,
    private authenticationService: AuthenticationService) {
    this.processor = new MessageProcessor(this.connection);
  }

  ngOnInit(): void {
    this.processor.programTransferred.subscribe((client: HubClient) => {
      if (this.programOpen) this.displayProgram(client);
    });
    // this.processor.debugMessageReceived.subscribe((msg: DebugMessage) => this.programDisplay.updateStatus(msg));
    this.connection.start().subscribe(msg => this.processor.processServerMessage(msg));
  }

  displayProgram(client: HubClient) {
    this.currentClient = client;
    this.programOpen = true;
    // this.programDisplay.display(client);
  }

  displayMessages(client: HubClient) {

  }

  displayAlerts(client: HubClient) {
    
  }
}
