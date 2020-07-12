import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ConnectionService } from '../services/connection.service';
import { MessageProcessor } from '../services/message-processor';
import { HubClient } from '../data/hub-client';
import { ProgramDisplayComponent } from '../program-display/program-display.component';
import { DebugMessage } from '../data/debug-message';
import { AuthenticationService } from '../services/authentication.service';

@Component({
  selector: 'app-popout',
  templateUrl: './popout.component.html',
  styleUrls: ['./popout.component.scss']
})
export class PopoutComponent implements OnInit {

  @ViewChild(ProgramDisplayComponent)
  private programDisplay: ProgramDisplayComponent;

  isLoading: boolean = true;
  view: string;
  currentClient: HubClient;
  processor: MessageProcessor;

  constructor(private route: ActivatedRoute,
    private authenticationService: AuthenticationService,
    private connection: ConnectionService) { }

  ngOnInit(): void {
    this.route.params.subscribe(
      data => {
        console.groupCollapsed('[PopoutComponent] Retrieved route parameters');
        console.log(data);
        console.groupEnd();
        this.isLoading = false;
        this.view = data.view;

        const guid = atob(data.id);
        const json = localStorage.getItem(guid);
        const settings = JSON.parse(json);
        console.groupCollapsed('[PopoutComponent] Parsed settings');
        console.log(settings);
        console.groupEnd();
        this.authenticationService.resume(settings.session);

        this.processor = new MessageProcessor(this.connection);
        this.processor.clientAdded.subscribe((client: HubClient) => {
          if (client.id == settings.client) this.currentClient = client;
        });
        if (this.view == 'program') {
          this.processor.programTransferred.subscribe((client: HubClient) => this.programDisplay.display(client));
          this.processor.debugMessageReceived.subscribe((msg: DebugMessage) => this.programDisplay.updateStatus(msg));
        }
        this.connection.start().subscribe(msg => this.processor.processServerMessage(msg));
      });
  }

  close(): void {
    window.close();
  }

}
