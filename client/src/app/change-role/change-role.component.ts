import { Component, OnInit, Input } from '@angular/core';
import { AuthenticationService, UserRole } from '../services/authentication.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-change-role',
  templateUrl: './change-role.component.html',
  styleUrls: ['./change-role.component.scss']
})
export class ChangeRoleComponent implements OnInit {

  @Input() role: string;
  roles: UserRole[] = [UserRole.Student, UserRole.Teacher, UserRole.Administrator];
  opened: boolean;

  constructor(private authenticationService: AuthenticationService,
    private router: Router) { }

  ngOnInit() {
  }

  canAccess(role: UserRole): boolean {
    return this.authenticationService.canAccess(role);
  }

  changeRole(role: UserRole): void {
    const roleName = UserRole[role];
    console.log('[ChangeRole] Changing to ' + roleName);
    this.router.navigateByUrl(roleName.toLowerCase());
  }

  format(role: UserRole): string {
    const roleName = UserRole[role];
    return roleName;
  }

  isCurrent(role: UserRole): boolean {
    return role == UserRole[this.role];
  }

  show(): void {
    this.opened = true;
  }

}
