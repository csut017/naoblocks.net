import { Component, OnInit, ViewChild } from '@angular/core';
import { Toolbox } from './toolbox';
import { AboutComponent } from '../about/about.component';
import { AuthenticationService } from '../services/authentication.service';
import { Router } from '@angular/router';

declare var Blockly: any;

@Component({
  selector: 'app-student-home',
  templateUrl: './student-home.component.html',
  styleUrls: ['./student-home.component.scss']
})
export class StudentHomeComponent implements OnInit {

  @ViewChild(AboutComponent, { static: false }) about: AboutComponent;

  aboutOpened: boolean;
  workspace: any;

  constructor(private authenticationService: AuthenticationService,
    private router: Router) {
  }

  ngOnInit() {
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

  logout(): void {
    this.authenticationService.logout()
      .subscribe(_ => {
        this.router.navigateByUrl('/');
      });
  }
}
