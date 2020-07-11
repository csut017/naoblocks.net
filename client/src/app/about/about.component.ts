import { Component, OnInit } from '@angular/core';
import { SystemVersion } from '../data/system-version';
import { SystemService } from '../services/system.service';

@Component({
  selector: 'app-about',
  templateUrl: './about.component.html',
  styleUrls: ['./about.component.scss']
})
export class AboutComponent implements OnInit {

  opened: boolean;
  serverVersion: SystemVersion;
  clientVersion: string = '1.0.0.0 [alpha 3]';

  constructor(private systemService: SystemService) { }

  ngOnInit() {
    this.systemService.getVersion()
      .subscribe(v => this.serverVersion = v);
  }

  show(): void {
    this.opened = true;
  }

}
