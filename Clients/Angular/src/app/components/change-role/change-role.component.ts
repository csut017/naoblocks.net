import { Component, Inject } from '@angular/core';
import { MatBottomSheet, MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA } from '@angular/material/bottom-sheet';
import { Router } from '@angular/router';
import { AuthenticationService, UserRole } from 'src/app/services/authentication.service';

@Component({
  selector: 'app-change-role',
  templateUrl: './change-role.component.html',
  styleUrls: ['./change-role.component.scss']
})
export class ChangeRoleComponent {

  roles: UserRole[] = [UserRole.Student, UserRole.Teacher, UserRole.Administrator];

  constructor(private authenticationService: AuthenticationService,
    private router: Router,
    private sheetRef: MatBottomSheetRef<ChangeRoleComponent>,
    @Inject(MAT_BOTTOM_SHEET_DATA) public data: { currentRole: UserRole }) { }

  canAccess(role: UserRole): boolean {
    return this.authenticationService.canAccess(role);
  }

  changeRole(event: MouseEvent, role: UserRole): void {
    this.sheetRef.dismiss();
    event.preventDefault();
    const roleName = UserRole[role];
    if (this.isCurrent(role)) {
      console.log('[ChangeRole] Closing change role');
      return;
    }

    console.log('[ChangeRole] Changing to ' + roleName);
    this.router.navigateByUrl(roleName.toLowerCase());
  }

  format(role: UserRole): string {
    const roleName = UserRole[role];
    return roleName;
  }

  isCurrent(role: UserRole): boolean {
    return role == this.data.currentRole;
  }

  generateUrl(role: UserRole): string {
    const roleName = `/${UserRole[role].toLowerCase()}`;
    return roleName;
  }

}
