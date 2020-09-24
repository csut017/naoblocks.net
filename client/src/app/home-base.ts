import { AuthenticationService, UserRole } from './services/authentication.service';

import { Router } from '@angular/router';
import { ChangeRoleComponent } from './change-role/change-role.component';
import { ViewChild, Directive } from '@angular/core';
import { AboutComponent } from './about/about.component';
import { ChangeViewComponent } from './change-view/change-view.component';

@Directive()
export class HomeBase {

    @ViewChild(AboutComponent) about: AboutComponent;
    @ViewChild(ChangeRoleComponent) roleSelector: ChangeRoleComponent;
    @ViewChild(ChangeViewComponent) viewSelector: ChangeViewComponent;

    hasAccess: boolean;

    constructor(protected authenticationService: AuthenticationService,
        protected router: Router) {
    }

    canChangeRole(): boolean {
        return this.authenticationService.canAccess(UserRole.Teacher)
            || this.authenticationService.canAccess(UserRole.Administrator);
    }

    checkAccess(role: UserRole) {
        this.hasAccess = this.authenticationService.canAccess(role);
    }

    openChangeRole(): void {
        console.log('[HomeBase] Changing Role');
        this.roleSelector.show();
    }

    canChangeView(): boolean {
        return true;
    }

    openChangeView(): void {
        console.log('[HomeBase] Changing View');
        this.viewSelector.show();
    }

    logout(): void {
        this.authenticationService.logout()
            .subscribe(_ => {
                this.router.navigateByUrl('/');
            });
    }
}
