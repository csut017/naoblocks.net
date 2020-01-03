import { Component, OnInit } from '@angular/core';
import { SystemStatus } from '../data/system-status';
import { SystemService } from '../services/system.service';

@Component({
  selector: 'app-system-status',
  templateUrl: './system-status.component.html',
  styleUrls: ['./system-status.component.scss']
})
export class SystemStatusComponent implements OnInit {

  status: SystemStatus = new SystemStatus();
  isLoading: boolean = false;

  constructor(private systemService: SystemService) { }

  ngOnInit() {
    this.refreshStatus();
  }

  refreshStatus(): void {
    this.isLoading = true;
    this.systemService.refresh().subscribe(data => {
      this.status = data;
      this.isLoading = false;
    })
  }

}
