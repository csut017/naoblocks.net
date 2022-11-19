import { Component, OnInit } from '@angular/core';
import { ReportDialogSettings } from 'src/app/data/report-dialog-settings';
import { AuthenticationService, UserRole } from 'src/app/services/authentication.service';
import { FileDownloaderService } from 'src/app/services/file-downloader.service';
import { ReportSettingsService } from 'src/app/services/report-settings.service';

@Component({
  selector: 'app-export-data',
  templateUrl: './export-data.component.html',
  styleUrls: ['./export-data.component.scss']
})
export class ExportDataComponent implements OnInit {

  constructor(private exportSettings: ReportSettingsService,
    private authenticationService: AuthenticationService,
    private downloaderService: FileDownloaderService) { }

  ngOnInit(): void {
  }

  isAdmin(): boolean {
    return this.authenticationService.canAccess(UserRole.Administrator);
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

  downloadRobots(): void {
    console.log('[ExportData] Showing export settings for robots');
    let settings = new ReportDialogSettings('Export robots', false);
    settings.allowedFormats = [
      ReportDialogSettings.Excel,
      ReportDialogSettings.Csv,
      ReportDialogSettings.Text,
      ReportDialogSettings.Pdf,
    ];
    this.exportSettings.show(settings)
      .subscribe(result => {
        if (!!result) {
          console.groupCollapsed('[ExportData] Generating robots');
          console.log(result);
          console.groupEnd();
          this.downloaderService.download(
            `v1/robots/export.${result.selectedFormat}?flags=${result.encodeFlags()}`, 
            `robots.${result.selectedFormat}`);
        } else {
          console.log('[ExportData] Export cancelled');
        }
      });
  }

  downloadRobotTypes(): void {
    console.log('[ExportData] Showing export settings for robot types');
    let settings = new ReportDialogSettings('Export robot types', false);
    settings.allowedFormats = [
      ReportDialogSettings.Excel,
      ReportDialogSettings.Csv,
      ReportDialogSettings.Text,
      ReportDialogSettings.Pdf,
    ];
    this.exportSettings.show(settings)
      .subscribe(result => {
        if (!!result) {
          console.groupCollapsed('[ExportData] Generating robot types');
          console.log(result);
          console.groupEnd();
          this.downloaderService.download(
            `v1/robots/types/export.${result.selectedFormat}`, 
            `robots-types.${result.selectedFormat}`);
        } else {
          console.log('[ExportData] Export cancelled');
        }
      });
  }

  downloadStudents(): void {
    console.log('[ExportData] Showing export settings for students');
    let settings = new ReportDialogSettings('Export students', false);
    settings.allowedFormats = [
      ReportDialogSettings.Excel,
      ReportDialogSettings.Csv,
      ReportDialogSettings.Text,
      ReportDialogSettings.Pdf,
    ]
    this.exportSettings.show(settings)
      .subscribe(result => {
        if (!!result) {
          console.groupCollapsed('[ExportData] Generating students');
          console.log(result);
          console.groupEnd();
          this.downloaderService.download(
            `v1/students/export.${result.selectedFormat}`, 
            `students.${result.selectedFormat}`);
        } else {
          console.log('[ExportData] Export cancelled');
        }
      });
  }

  downloadAllConfig(): void {
    console.log('[ExportData] Showing export settings for all configuration');
    let settings = new ReportDialogSettings('Export all configuration', false);
    settings.allowedFormats = [
      ReportDialogSettings.Excel,
      ReportDialogSettings.Csv,
      ReportDialogSettings.Text,
      ReportDialogSettings.Pdf,
    ]
    this.exportSettings.show(settings)
      .subscribe(result => {
        if (!!result) {
          console.groupCollapsed('[ExportData] Generating all configuration');
          console.log(result);
          console.groupEnd();
          this.downloaderService.download(
            `v1/system/export/allConfig.${result.selectedFormat}`,
            `all-config.${result.selectedFormat}`);
        } else {
          console.log('[ExportData] Export cancelled');
        }
      });
  }
}
