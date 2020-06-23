import { Component, OnInit } from '@angular/core';
import { SystemService } from '../services/system.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-robot-configuration',
  templateUrl: './robot-configuration.component.html',
  styleUrls: ['./robot-configuration.component.scss']
})
export class RobotConfigurationComponent implements OnInit {

  public addresses: string[] = [];
  public connectTextUrl: string = '';
  public downloading: boolean = true;

  constructor(private systemService: SystemService) { }

  ngOnInit(): void {
    this.connectTextUrl = `${environment.apiURL}v1/system/addresses/connect.txt`;
    this.systemService.listRobotAddresses()
      .subscribe(result => {
        this.addresses = result.items;
        this.downloading = false;
      });
  }

}
