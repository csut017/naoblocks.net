import { Component, OnInit } from '@angular/core';
import { SystemService } from '../services/system.service';

@Component({
  selector: 'app-robot-configuration',
  templateUrl: './robot-configuration.component.html',
  styleUrls: ['./robot-configuration.component.scss']
})
export class RobotConfigurationComponent implements OnInit {

  public addresses: string[] = [];

  constructor(private systemService: SystemService) { }

  ngOnInit(): void {
    this.systemService.listRobotAddresses()
      .subscribe(result => {
        this.addresses = result.items;
      });
  }

}
