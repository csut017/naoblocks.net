import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { User } from '../data/user';
import { HomeBase } from '../home-base';
import { AuthenticationService } from '../services/authentication.service';

declare var TopCodes: any;

@Component({
  selector: 'app-tangible-editor',
  templateUrl: './tangible-editor.component.html',
  styleUrls: ['./tangible-editor.component.scss']
})
export class TangibleEditorComponent extends HomeBase implements OnInit {

  currentUser: User;
  cameraStarted: boolean = false;

  constructor(authenticationService: AuthenticationService,
    router: Router) {
    super(authenticationService, router);
  }

  ngOnInit(): void {
  }

  startCamera(): void {
    this.cameraStarted = true;
    TopCodes.startStopVideoScan('video-canvas');
  }

  stopCamera(): void {
    this.cameraStarted = false;
    TopCodes.startStopVideoScan('video-canvas');
  }
}
