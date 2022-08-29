import { Component, OnInit } from '@angular/core';
import { ReportDialogSettings } from 'src/app/data/report-dialog-settings';
import { FileDownloaderService } from 'src/app/services/file-downloader.service';
import { ReportSettingsService } from 'src/app/services/report-settings.service';

@Component({
  selector: 'app-export-data',
  templateUrl: './export-data.component.html',
  styleUrls: ['./export-data.component.scss']
})
export class ExportDataComponent implements OnInit {

  constructor(private exportSettings: ReportSettingsService,
    private downloaderService: FileDownloaderService) { }

  ngOnInit(): void {
  }

  downloadPrograms(): void {
    console.log('[ExportData] Showing export settings for programs');
    let settings = new ReportDialogSettings('Export Programs', true);
    settings.allowedFormats = [
      ReportDialogSettings.Excel,
      ReportDialogSettings.Csv,
      ReportDialogSettings.Text,
      ReportDialogSettings.Xml,
    ]
    this.exportSettings.show(settings)
      .subscribe(result => {
        if (!!result) {
          console.groupCollapsed('[ExportData] Generating programs export');
          console.log(result);
          console.groupEnd();

          const fromDate = result.dateFrom?.toISODate() || '',
            toDate = result.dateTo?.toISODate() || '';

          this.downloaderService.download(
            `v1/system/export/programs.${result.selectedFormat}?from=${fromDate}&to=${toDate}`,
            `All_${fromDate}_${toDate}-programs.${result.selectedFormat}`);
        } else {
          console.log('[ExportData] Export cancelled');
        }
      });
  }


  downloadRobotLogs(): void {
    console.log('[ExportData] Showing export settings for logs');
    let settings = new ReportDialogSettings('Export Logs', true);
    settings.allowedFormats = [
      ReportDialogSettings.Excel,
      ReportDialogSettings.Csv,
      ReportDialogSettings.Text,
      ReportDialogSettings.Xml,
    ]
    this.exportSettings.show(settings)
      .subscribe(result => {
        if (!!result) {
          console.groupCollapsed('[ExportData] Generating logs export');
          console.log(result);
          console.groupEnd();

          const fromDate = result.dateFrom?.toISODate() || '',
            toDate = result.dateTo?.toISODate() || '';

          this.downloaderService.download(
            `v1/system/export/logs.${result.selectedFormat}?from=${fromDate}&to=${toDate}`,
            `All_${fromDate}_${toDate}-logs.${result.selectedFormat}`);
        } else {
          console.log('[ExportData] Export cancelled');
        }
      });
  }
}
