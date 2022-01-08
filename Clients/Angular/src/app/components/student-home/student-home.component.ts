import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { HomeBase } from 'src/app/home-base';
import { AuthenticationService } from 'src/app/services/authentication.service';

@Component({
  selector: 'app-student-home',
  templateUrl: './student-home.component.html',
  styleUrls: ['./student-home.component.scss']
})
export class StudentHomeComponent extends HomeBase implements OnInit {

  constructor(authenticationService: AuthenticationService,
    router: Router) {
      super(authenticationService, router);
     }

  ngOnInit(): void {
  }

}
