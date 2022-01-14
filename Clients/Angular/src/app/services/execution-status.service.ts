import { Injectable } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
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

  show(status: StartupStatusTracker): void {
    this.dialogRef = this.dialog.open(ExecutionStatusComponent, {
      data: status,
    });
  }

  close(): void {
    this.dialogRef?.close();
  }
}
