import { AuthenticationService, UserRole } from './services/authentication.service';

import { Router } from '@angular/router';
import { ViewChild, Directive } from '@angular/core';
import { User } from './data/user';
import { AboutComponent } from './components/about/about.component';
import { ChangeRoleComponent } from './components/change-role/change-role.component';
import { ChangeViewComponent } from './components/change-view/change-view.component';
import { ChangeRoleService } from './services/change-role.service';

@Directive()
export class HomeBase {

    @ViewChild(AboutComponent) about?: AboutComponent;
    @ViewChild(ChangeRoleComponent) roleSelector?: ChangeRoleComponent;
    @ViewChild(ChangeViewComponent) viewSelector?: ChangeViewComponent;

    hasAccess: boolean = false;
    currentUser?: User;

    constructor(protected authenticationService: AuthenticationService,
        protected router: Router,
        private changeRoleService: ChangeRoleService) {
    }

    canChangeRole(): boolean {
        return this.authenticationService.canAccess(UserRole.Teacher)
            || this.authenticationService.canAccess(UserRole.Administrator);
    }

    checkAccess(role: UserRole) {
        this.hasAccess = this.authenticationService.canAccess(role);
        this.authenticationService.getCurrentUser()
            .subscribe(u => {
                this.authenticationService.checkHttpResponse(u);
                this.currentUser = u.output;
            });
    }

    openChangeRole(currentRole: string): void {
        let role = (<any>UserRole)[currentRole];
        console.log('[HomeBase] Changing Role');
        this.changeRoleService.show(role);
    }

    canChangeView(): boolean {
        return true;
    }

    openChangeView(): void {
        console.log('[HomeBase] Changing View');
        this.viewSelector?.show();
    }

    openAbout(): void {
        console.log('[HomeBase] Showing about');
        this.about?.show();
    }

    logout(): void {
        this.authenticationService.logout()
            .subscribe(_ => {
                this.router.navigateByUrl('/');
            });
    }
}
