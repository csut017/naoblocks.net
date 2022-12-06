import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { RobotTypeDefinitionImportComponent } from '../components/robot-type-definition-import/robot-type-definition-import.component';
import { RobotImportSettings } from '../data/robot-import-settings';

@Injectable({
  providedIn: 'root'
})
export class RobotTypeDefinitionService {

  constructor(private dialog: MatDialog) { }

  start(settings?: RobotImportSettings): Observable<boolean> {
    const dialogRef = this.dialog.open(RobotTypeDefinitionImportComponent, {
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
