import { Component, OnInit } from '@angular/core';
import { SystemService } from '../services/system.service';
import { environment } from 'src/environments/environment';
import { RobotTypeService } from '../services/robot-type.service';
import { RobotType } from '../data/robot-type';

@Component({
  selector: 'app-robot-configuration',
  templateUrl: './robot-configuration.component.html',
  styleUrls: ['./robot-configuration.component.scss']
})
export class RobotConfigurationComponent implements OnInit {

  addresses: string[] = [];
  robotTypes: RobotType[] = [];
  connectTextUrl: string = '';
  downloadingAddresses: boolean = true;
  downloadingRobotTypes: boolean = true;
  currentRobotType: RobotType;

  constructor(private systemService: SystemService,
    private robotTypeService: RobotTypeService) { }

  ngOnInit(): void {
    this.connectTextUrl = `${environment.apiURL}v1/system/addresses/connect.txt`;
    this.systemService.listRobotAddresses()
      .subscribe(result => {
        this.addresses = result.items;
        this.downloadingAddresses = false;
      });
      this.robotTypeService.list()
      .subscribe(data => {
        this.robotTypes = data.items;
        this.downloadingRobotTypes = false;
      });
  }

  generateDownloadPackageUrl(robotType: RobotType): string {
    const url = `${environment.apiURL}v1/robots/types/${robotType.name}/package`;
    return url;
  }

}
