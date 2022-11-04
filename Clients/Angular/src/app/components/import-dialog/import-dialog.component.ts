import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ImportSettings } from 'src/app/data/import-settings';
import { ImportStatus } from 'src/app/data/import-status';

@Component({
  selector: 'app-import-dialog',
  templateUrl: './import-dialog.component.html',
  styleUrls: ['./import-dialog.component.scss']
})
export class ImportDialogComponent implements OnInit {

  status: ImportStatus = new ImportStatus();

  constructor(
    public dialogRef: MatDialogRef<ImportDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ImportSettings<any>) {
  }

  ngOnInit(): void {
  }

  fileBrowseHandler($event: any): void {
    let files = $event.target ? $event.target.files : $event;
    for (const file of files) {
      if (this.status.files.length == 0 || this.data.allowMultiple) {
        this.status.files.push(file);
      }
    }
  }

  cancelUpload(): void {
    if (this.status.isUploading) {
      this.status.isUploadCancelling = true;
      return;
    }

    this.dialogRef.close();
  }

  startUpload(): void {
    this.status.isUploading = true;
    this.status.uploadState = 0;
    this.status.errors = [];
    this.status.uploadProgress = 0;
    this.data.uploadAction(this.status, this.data);
  }

  deleteFile(index: number) {
    this.status.files.splice(index, 1);
  }

  formatBytes(bytes: number, decimals: number = 2): string {
    if (bytes === 0) {
      return "0 Bytes";
    }
    const k = 1024;
    const dm = decimals <= 0 ? 0 : decimals;
    const sizes = ["Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + " " + sizes[i];
  }
}
