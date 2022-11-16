import { Component, EventEmitter, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-system-configuration',
  templateUrl: './system-configuration.component.html',
  styleUrls: ['./system-configuration.component.scss']
})
export class SystemConfigurationComponent implements OnInit {

  @Output() currentItemChanged = new EventEmitter<string>();

  constructor() { }

  ngOnInit(): void {
  }

}
