import { Component, Input, OnInit } from '@angular/core';
import { SystemService } from '../services/system.service';
import { environment } from 'src/environments/environment';
import { RobotTypeService } from '../services/robot-type.service';
import { RobotType } from '../data/robot-type';
import { SiteAddress } from '../data/site-address';

@Component({
  selector: 'app-robot-configuration',
  templateUrl: './robot-configuration.component.html',
  styleUrls: ['./robot-configuration.component.scss']
})
export class RobotConfigurationComponent implements OnInit {

  addresses: SiteAddress[] = [];
  robotTypes: RobotType[] = [];
  connectTextUrl: string = '';
  downloadingAddresses: boolean = true;
  downloadingRobotTypes: boolean = true;
  currentRobotType: RobotType;
  addressQRCode: string = '';
  isQRCodeWindowOpen: boolean = false;
  selectedAddress: SiteAddress = new SiteAddress();

  @Input() canConfigureAddresses: boolean = false;

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

  setDefault(address: SiteAddress) : void {
    if (!this.canConfigureAddresses) return;
    this.addresses.forEach(a => a.isDefault = false);
    address.isDefault = true;
  }

  showQRCode(address: SiteAddress): void {
    this.isQRCodeWindowOpen = true;
    this.selectedAddress = address;
    this.addressQRCode = `${environment.apiURL}v1/system/qrcode/${encodeURIComponent(address.url)}`;
  }
}
