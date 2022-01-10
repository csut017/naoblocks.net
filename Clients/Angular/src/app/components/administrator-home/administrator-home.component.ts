import { Breakpoints, BreakpointObserver } from '@angular/cdk/layout';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { map, Observable, shareReplay } from 'rxjs';
import { HomeBase } from 'src/app/home-base';
import { AuthenticationService, UserRole } from 'src/app/services/authentication.service';
import { ChangeRoleService } from 'src/app/services/change-role.service';

@Component({
  selector: 'app-administrator-home',
  templateUrl: './administrator-home.component.html',
  styleUrls: ['./administrator-home.component.scss']
})
export class AdministratorHomeComponent extends HomeBase implements OnInit {

  currentView: string = 'Dashboard';
  isHandset$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.Handset)
    .pipe(
      map(result => result.matches),
      shareReplay()
    );

  constructor(authenticationService: AuthenticationService,
    router: Router,
    changeRoleService: ChangeRoleService,
    private breakpointObserver: BreakpointObserver) {
    super(authenticationService, router, changeRoleService);
  }

  ngOnInit(): void {
    this.checkAccess(UserRole.Administrator);
  }

  changeView(event: MouseEvent, view: string): void {
    event.preventDefault();
    console.log('[AdministratorHomeComponent] Changing to view ' + view);
    this.currentView = view;
  }

}
