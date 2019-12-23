import { Component, OnInit, ViewChild } from '@angular/core';
import { Toolbox } from './toolbox';
import { AboutComponent } from '../about/about.component';
import { AuthenticationService, UserRole } from '../services/authentication.service';
import { Router } from '@angular/router';
import { ChangeRoleComponent } from '../change-role/change-role.component';
import { HomeBase } from '../home-base';

declare var Blockly: any;

@Component({
  selector: 'app-student-home',
  templateUrl: './student-home.component.html',
  styleUrls: ['./student-home.component.scss']
})
export class StudentHomeComponent extends HomeBase implements OnInit {

  aboutOpened: boolean;
  workspace: any;

  constructor(authenticationService: AuthenticationService,
    router: Router) {
    super(authenticationService, router);
  }

  ngOnInit() {
    this.checkAccess(UserRole.Student);

    let xml = new Toolbox()
      .includeConditionals()
      .includeLoops()
      .includeVariables()
      .build();

    this.workspace = Blockly.inject('blocklyDiv', {
      toolbox: xml,
      scrollbars: false
    });
  }
}
