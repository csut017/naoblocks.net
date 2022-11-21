import { Component, Inject, OnInit } from '@angular/core';
import { UntypedFormControl, UntypedFormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ReportDialogSettings } from 'src/app/data/report-dialog-settings';
import { ReportSettings } from 'src/app/data/report-settings';
import { DateTime } from 'luxon';

@Component({
  selector: 'app-report-settings',
  templateUrl: './report-settings.component.html',
  styleUrls: ['./report-settings.component.scss']
})
export class ReportSettingsComponent implements OnInit {

  form: UntypedFormGroup;

  constructor(
    public dialogRef: MatDialogRef<ReportSettingsComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ReportDialogSettings) {
    let controls = {
      format: new UntypedFormControl(data.allowedFormats[0].value, [Validators.required]),
    };
    if (data.showDateRange) {
      const now = DateTime.now(),
        fromDate = now.minus({days: 7});
      controls['dateFrom'] = new UntypedFormControl(fromDate, [Validators.required]);
      controls['dateTo'] = new UntypedFormControl(now, [Validators.required]);
    }
    for (let flag of data.flags) {
      controls[flag.name] = new UntypedFormControl(flag.value);
    }
    this.form = new UntypedFormGroup(controls);
  }

  ngOnInit(): void {
  }

  doCancel(): void {
    this.dialogRef.close();
  }

  doExport(): void {
    let settings = new ReportSettings();
    settings.selectedFormat = this.form.get('format')?.value;
    if (this.data.showDateRange) {
      settings.dateFrom = this.form.get('dateFrom')?.value;
      settings.dateTo = this.form.get('dateTo')?.value;
    }

    for (let flag of this.data.flags) {
      let flagValue = this.form.get(flag.name)?.value;
      if (flagValue) settings.flags.push(flag.name);
    }

    this.dialogRef.close(settings);
  }

}
