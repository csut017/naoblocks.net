import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ChangeViewComponent } from '../change-view/change-view.component';
import { User } from '../data/user';
import { HomeBase } from '../home-base';
import { AuthenticationService } from '../services/authentication.service';

@Component({
  selector: 'app-tangible-editor',
  templateUrl: './tangible-editor.component.html',
  styleUrls: ['./tangible-editor.component.scss']
})
export class TangibleEditorComponent extends HomeBase implements OnInit {

  currentUser: User;

  constructor(authenticationService: AuthenticationService,
    router: Router) {
    super(authenticationService, router);
  }

  ngOnInit(): void {
  }
}
