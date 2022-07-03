import { Breakpoints, BreakpointObserver } from '@angular/cdk/layout';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { map, Observable, shareReplay } from 'rxjs';
import { HomeBase } from 'src/app/home-base';
import { AuthenticationService, UserRole } from 'src/app/services/authentication.service';
import { ChangeRoleService } from 'src/app/services/change-role.service';
import { ViewFormatterService } from 'src/app/services/view-formatter.service';

@Component({
  selector: 'app-administrator-home',
  templateUrl: './administrator-home.component.html',
  styleUrls: ['./administrator-home.component.scss']
})
export class AdministratorHomeComponent extends HomeBase implements OnInit {

  currentItem: string ='';
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
    this.checkAccess(UserRole.Administrator);
  }

  changeView(event: MouseEvent, view: string): void {
    event.preventDefault();
    console.log('[AdministratorHomeComponent] Changing to view ' + view);
    this.currentView = view;
    const viewUrl = this.viewFormatter.toUrl(view);
    this.router.navigate(['administrator', viewUrl], {});
    this.currentItem = '';
  }

  onItemChanged(itemName: string): void {
    this.currentItem = itemName;
  }
}
