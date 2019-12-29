import { AuthenticationService, UserRole } from './services/authentication.service';

import { Router } from '@angular/router';
import { ChangeRoleComponent } from './change-role/change-role.component';
import { ViewChild } from '@angular/core';
import { AboutComponent } from './about/about.component';

export class HomeBase {

    @ViewChild(AboutComponent, { static: false }) about: AboutComponent;
    @ViewChild(ChangeRoleComponent, { static: false }) roleSelector: ChangeRoleComponent;

    hasAccess: boolean;

    constructor(protected authenticationService: AuthenticationService,
        protected router: Router) {
    }

    checkAccess(role: UserRole) {
        this.hasAccess = this.authenticationService.canAccess(role);
    }

    changeRole(): void {
        console.log('[HomeBase] Changing Role');
        this.roleSelector.show();
    }

    logout(): void {
        this.authenticationService.logout()
            .subscribe(_ => {
                this.router.navigateByUrl('/');
            });
    }
}
