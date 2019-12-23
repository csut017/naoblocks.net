import { Component, OnInit } from '@angular/core';
import { HomeBase } from '../home-base';
import { AuthenticationService, UserRole } from '../services/authentication.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-teacher-home',
  templateUrl: './teacher-home.component.html',
  styleUrls: ['./teacher-home.component.scss']
})
export class TeacherHomeComponent extends HomeBase implements OnInit {

  constructor(authenticationService: AuthenticationService,
    router: Router) {
    super(authenticationService, router);
  }

  ngOnInit() {
    this.checkAccess(UserRole.Teacher);
  }

}
