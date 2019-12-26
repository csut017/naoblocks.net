import { Component, OnInit, OnDestroy } from '@angular/core';
import { AuthenticationService } from '../services/authentication.service';
import { Router } from '@angular/router';
import { HeartbeatService } from '../services/heartbeat.service';

const timeoutPeriod: number = 60;

@Component({
  selector: 'app-heartbeat',
  templateUrl: './heartbeat.component.html',
  styleUrls: ['./heartbeat.component.scss']
})
export class HeartbeatComponent implements OnInit, OnDestroy {

  interval: any;
  countDown: number = timeoutPeriod;
  opened: boolean = false;
  error: string;

  constructor(private heartbeatService: HeartbeatService,
    private authenticationService: AuthenticationService,
    private router: Router) { }

  ngOnInit() {
    this.start();
  }

  ngOnDestroy() {
    clearInterval(this.interval);
  }

  start(): void {
    this.interval = setInterval(() => this.onSecond(), 1000);
  }

  onSecond() {
    if (--this.countDown > 0) {
      return;
    }

    clearInterval(this.interval);
    this.heartbeatService.check()
      .subscribe(heartbeat => {
        if (heartbeat.error === 'Unauthorized') this.doLogout();

        this.start();
        if (heartbeat.timeRemaining > 5) return;
        if (heartbeat.timeRemaining < 0) this.doLogout();
        this.opened = true;
      })
    this.countDown = timeoutPeriod;
  }

  doRenew(): void {
    this.authenticationService.renew()
      .subscribe(result => {
        if (result.successful) {
          this.opened = false;
          this.error = undefined;
        } else {
          this.error = result.msg;
        }
      });
  }

  doLogout(): void {
    this.authenticationService.logout()
      .subscribe(_ => {
        this.router.navigateByUrl('/');
      });
  }

}
