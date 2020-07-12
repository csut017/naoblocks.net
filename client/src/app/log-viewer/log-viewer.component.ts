import { Component, OnInit, Input } from '@angular/core';
import { HubClient } from '../data/hub-client';

@Component({
  selector: 'app-log-viewer',
  templateUrl: './log-viewer.component.html',
  styleUrls: ['./log-viewer.component.scss']
})
export class LogViewerComponent implements OnInit {

  @Input() client: HubClient;
  
  constructor() { }

  ngOnInit(): void {
  }

}
