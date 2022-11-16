import { Injectable } from '@angular/core';
import { MatBottomSheet } from '@angular/material/bottom-sheet';
import { ChangeRoleComponent } from '../components/change-role/change-role.component';
import { UserRole } from './authentication.service';

@Injectable({
  providedIn: 'root'
})
export class ChangeRoleService {

  constructor(private sheet: MatBottomSheet) { }

  show(currentRole: UserRole) {
    this.sheet.open(ChangeRoleComponent, {
      data: { currentRole: currentRole }
    });
  }
}
