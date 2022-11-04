import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { RobotImportDialogComponent } from '../components/robot-import-dialog/robot-import-dialog.component';
import { RobotImportSettings } from '../data/robot-import-settings';

@Injectable({
  providedIn: 'root'
})
export class RobotImportDialogService {

  constructor(private dialog: MatDialog) { }

  start<Type>(settings?: RobotImportSettings): Observable<boolean> {
    const dialogRef = this.dialog.open(RobotImportDialogComponent, {
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
