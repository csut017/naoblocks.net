import { Injectable } from '@angular/core';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { Observable } from 'rxjs';
import { ImportDialogComponent } from '../components/import-dialog/import-dialog.component';
import { ImportSettings } from '../data/import-settings';

@Injectable({
  providedIn: 'root'
})
export class ImportService {

  constructor(private dialog: MatDialog) { }

  start<Type>(settings: ImportSettings<Type>): Observable<boolean> {
    const dialogRef = this.dialog.open(ImportDialogComponent, {
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
