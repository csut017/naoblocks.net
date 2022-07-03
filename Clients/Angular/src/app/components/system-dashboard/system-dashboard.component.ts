import { Component, EventEmitter, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-system-dashboard',
  templateUrl: './system-dashboard.component.html',
  styleUrls: ['./system-dashboard.component.scss']
})
export class SystemDashboardComponent implements OnInit {

  @Output() currentItemChanged = new EventEmitter<string>();

  constructor() { }

  ngOnInit(): void {
  }

}
