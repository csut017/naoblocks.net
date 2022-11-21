import { Injectable } from '@angular/core';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
import { Observable } from 'rxjs';
import { ReportSettingsComponent } from '../components/report-settings/report-settings.component';
import { ReportDialogSettings } from '../data/report-dialog-settings';
import { ReportSettings } from '../data/report-settings';

@Injectable({
  providedIn: 'root'
})
export class ReportSettingsService {

  constructor(private dialog: MatDialog) { }

  show(settings: ReportDialogSettings): Observable<ReportSettings> {
    const dialogRef = this.dialog.open(ReportSettingsComponent, {
      data: settings
    });
    return new Observable<ReportSettings>(subscriber => {
      dialogRef.afterClosed()
        .subscribe(result => {
          subscriber.next(result);
          subscriber.complete();
        });
    });
  }
}
