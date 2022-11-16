import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { StartupStatusTracker } from 'src/app/data/startup-status-tracker';

@Component({
  selector: 'app-execution-status',
  templateUrl: './execution-status.component.html',
  styleUrls: ['./execution-status.component.scss']
})
export class ExecutionStatusComponent implements OnInit {

  constructor(
    private dialogRef: MatDialogRef<ExecutionStatusComponent>,
    @Inject(MAT_DIALOG_DATA) public data: StartupStatusTracker) {
    dialogRef.disableClose = true;
  }

  ngOnInit(): void {
  }

  doClose(): void {
    this.dialogRef.close(true);
  }
}
