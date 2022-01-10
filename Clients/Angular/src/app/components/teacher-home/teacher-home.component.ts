import { Component, OnInit } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';
import { HomeBase } from 'src/app/home-base';
import { Router } from '@angular/router';
import { AuthenticationService, UserRole } from 'src/app/services/authentication.service';
import { ChangeRoleService } from 'src/app/services/change-role.service';

@Component({
  selector: 'app-teacher-home',
  templateUrl: './teacher-home.component.html',
  styleUrls: ['./teacher-home.component.scss']
})
export class TeacherHomeComponent extends HomeBase implements OnInit {

  currentView: string = 'dashboard';
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
    this.checkAccess(UserRole.Teacher);
  }

  changeView(event: MouseEvent, view: string): void {
    event.preventDefault();
    console.log('[TeacherHomeComponent] Changing to view ' + view);
    this.currentView = view;
  }

}
