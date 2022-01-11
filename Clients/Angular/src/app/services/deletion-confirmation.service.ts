import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { DeletionConfirmationComponent } from '../components/deletion-confirmation/deletion-confirmation.component';
import { DeletionItems } from '../data/deletion-items';

@Injectable({
  providedIn: 'root'
})
export class DeletionConfirmationService {

  constructor(private dialog: MatDialog) { }

  confirm(settings: DeletionItems): Observable<boolean> {
    const dialogRef = this.dialog.open(DeletionConfirmationComponent, {
      data: settings
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
