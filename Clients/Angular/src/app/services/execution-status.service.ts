import { Injectable } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { ExecutionStatusComponent } from '../components/execution-status/execution-status.component';
import { StartupStatusTracker } from '../data/startup-status-tracker';

@Injectable({
  providedIn: 'root'
})
export class ExecutionStatusService {
  dialogRef?: MatDialogRef<ExecutionStatusComponent>;

  constructor(
    private dialog: MatDialog) { 

  }

  show(status: StartupStatusTracker): Observable<boolean> {
    this.dialogRef = this.dialog.open(ExecutionStatusComponent, {
      data: status,
    });
    return new Observable<boolean>(subscriber => {
      this.dialogRef?.afterClosed()
        .subscribe(result => {
          subscriber.next(!!result);
          subscriber.complete();
        });
    });
  }

  close(): void {
    this.dialogRef?.close();
  }
}
