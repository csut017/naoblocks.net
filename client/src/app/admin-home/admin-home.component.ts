import { Component, OnInit } from '@angular/core';
import { HomeBase } from '../home-base';
import { AuthenticationService, UserRole } from '../services/authentication.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-admin-home',
  templateUrl: './admin-home.component.html',
  styleUrls: ['./admin-home.component.scss']
})
export class AdminHomeComponent extends HomeBase implements OnInit {

  currentView: string = 'status';

  constructor(authenticationService: AuthenticationService,
    router: Router) {
    super(authenticationService, router);
  }

  ngOnInit() {
    this.checkAccess(UserRole.Administrator);
  }

  changeView(newView: string): void {
    this.currentView = newView;
  }

}
