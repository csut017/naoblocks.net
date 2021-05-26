import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { SystemService } from '../services/system.service';

@Component({
  selector: 'app-landing',
  templateUrl: './landing.component.html',
  styleUrls: ['./landing.component.scss']
})
export class LandingComponent implements OnInit {

  constructor(private systemService: SystemService,
    private router: Router) { }

  private log(message: string) {
    const msg = `[LandingComponent] ${message}`;
    console.log(msg);
  }

  ngOnInit(): void {
    this.log('Retrieving system information');
    this.systemService.getVersion()
      .subscribe(v => {
        if (v.status == 'pending') {
          this.log('System is uninitialised - redirecting to system initialisation');
          this.router.navigateByUrl('initialisation');
        } else {
          this.log('Redirecting to login');
          this.router.navigateByUrl('login');
        }
      });
  }

}
