import { Component, OnInit } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';
import { HomeBase } from 'src/app/home-base';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthenticationService, UserRole } from 'src/app/services/authentication.service';
import { ChangeRoleService } from 'src/app/services/change-role.service';
import { ViewFormatterService } from 'src/app/services/view-formatter.service';

@Component({
  selector: 'app-teacher-home',
  templateUrl: './teacher-home.component.html',
  styleUrls: ['./teacher-home.component.scss']
})
export class TeacherHomeComponent extends HomeBase implements OnInit {

  currentView: string = 'Dashboard';
  isHandset$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.Handset)
    .pipe(
      map(result => result.matches),
      shareReplay()
    );

  constructor(authenticationService: AuthenticationService,
    router: Router,
    changeRoleService: ChangeRoleService,
    private viewFormatter: ViewFormatterService,
    private route: ActivatedRoute,
    private breakpointObserver: BreakpointObserver) {
    super(authenticationService, router, changeRoleService);
    this.route.paramMap.subscribe(params => {
      this.currentView = this.viewFormatter.fromUrl(params.get('view') || 'Dashboard');
    });
  }

  ngOnInit(): void {
    this.checkAccess(UserRole.Teacher);
  }

  changeView(event: MouseEvent, view: string): void {
    event.preventDefault();
    console.log('[TeacherHomeComponent] Changing to view ' + view);
    this.currentView = view;
    const viewUrl = this.viewFormatter.toUrl(view);
    this.router.navigate(['teacher', viewUrl], {});
  }

}
