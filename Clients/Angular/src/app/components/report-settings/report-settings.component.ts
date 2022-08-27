import { Component, Inject, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ReportDialogSettings } from 'src/app/data/report-dialog-settings';
import { ReportSettings } from 'src/app/data/report-settings';

@Component({
  selector: 'app-report-settings',
  templateUrl: './report-settings.component.html',
  styleUrls: ['./report-settings.component.scss']
})
export class ReportSettingsComponent implements OnInit {

  form: FormGroup;

  constructor(
    public dialogRef: MatDialogRef<ReportSettingsComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ReportDialogSettings) {
    let controls = {
      format: new FormControl(data.allowedFormats[0].value, [Validators.required]),
    };
    if (data.showDateRange) {
      const now = new Date(),
            fromDate = now.setDate(now.getDate() - 7);
      controls['dateFrom'] = new FormControl(fromDate, [Validators.required]);
      controls['dateTo'] = new FormControl(now, [Validators.required]);
    }
    this.form = new FormGroup(controls);
  }

  ngOnInit(): void {
  }

  doCancel(): void {
    this.dialogRef.close();
  }

  doExport(): void {
    let settings = new ReportSettings();
    this.dialogRef.close(settings);
  }

}
