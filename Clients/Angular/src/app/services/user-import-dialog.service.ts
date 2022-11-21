import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { UserImportDialogComponent } from '../components/user-import-dialog/user-import-dialog.component';
import { UserImportSettings } from '../data/user-import-settings';

@Injectable({
  providedIn: 'root'
})
export class UserImportDialogService {

  constructor(private dialog: MatDialog) { }

  start(settings?: UserImportSettings): Observable<boolean> {
    const dialogRef = this.dialog.open(UserImportDialogComponent, {
      data: settings,
      width: '75vw'
    });
    return new Observable<boolean>(subscriber => {
      dialogRef.afterClosed()
        .subscribe(result => {
          subscriber.next(!!result);
          subscriber.complete();
        });
    });
  }
}
