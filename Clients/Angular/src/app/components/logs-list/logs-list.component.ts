import { Component, EventEmitter, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-logs-list',
  templateUrl: './logs-list.component.html',
  styleUrls: ['./logs-list.component.scss']
})
export class LogsListComponent implements OnInit {

  @Output() currentItemChanged = new EventEmitter<string>();

  constructor() { }

  ngOnInit(): void {
  }

}
