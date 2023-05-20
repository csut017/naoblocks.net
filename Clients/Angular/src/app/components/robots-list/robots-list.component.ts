import { SelectionModel } from '@angular/cdk/collections';
import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource } from '@angular/material/table';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { DeletionItems } from 'src/app/data/deletion-items';
import { ReportDialogSettings } from 'src/app/data/report-dialog-settings';
import { ReportFlag } from 'src/app/data/report-flag';
import { Robot } from 'src/app/data/robot';
import { RobotImportSettings } from 'src/app/data/robot-import-settings';
import { AuthenticationService } from 'src/app/services/authentication.service';
import { DeletionConfirmationService } from 'src/app/services/deletion-confirmation.service';
import { FileDownloaderService } from 'src/app/services/file-downloader.service';
import { MultilineMessageService } from 'src/app/services/multiline-message.service';
import { ReportSettingsService } from 'src/app/services/report-settings.service';
import { RobotImportDialogService } from 'src/app/services/robot-import-dialog.service';
import { RobotService } from 'src/app/services/robot.service';
import { ViewFormatterService } from 'src/app/services/view-formatter.service';

@Component({
  selector: 'app-robots-list',
  templateUrl: './robots-list.component.html',
  styleUrls: ['./robots-list.component.scss']
})
export class RobotsListComponent implements OnInit {

  columns: string[] = ['select', 'machineName', 'friendlyName', 'type', 'whenAdded', 'isInitialised'];
  currentItem?: Robot;
  dataSource: MatTableDataSource<Robot> = new MatTableDataSource();
  isLoading: boolean = true;
  isNew: boolean = true;
  selection = new SelectionModel<Robot>(true, []);
  view: string = 'list';

  @Output() currentItemChanged = new EventEmitter<string>();

  constructor(private robotService: RobotService,
    private router: Router,
    private route: ActivatedRoute,
    private viewFormatter: ViewFormatterService,
    private authenticationService: AuthenticationService,
    private snackBar: MatSnackBar,
    private deleteConfirm: DeletionConfirmationService,
    private exportSettings: ReportSettingsService,
    private multilineMessage: MultilineMessageService,
    private importRobotService: RobotImportDialogService,
    private downloaderService: FileDownloaderService) {
    this.route.paramMap.subscribe(params => {
      let item = params.get('item');
      if (!!item) this.findAndEdit(item);
    });
  }

  ngOnInit(): void {
    this.loadList();
  }

  isAllSelected() {
    const numSelected = this.selection.selected.length;
    const numRows = this.dataSource.data.length;
    return numSelected === numRows;
  }

  masterToggle() {
    if (this.isAllSelected()) {
      this.selection.clear();
      return;
    }

    this.selection.select(...this.dataSource.data);
  }

  checkboxLabel(row?: Robot): string {
    if (!row) {
      return `${this.isAllSelected() ? 'deselect' : 'select'} all`;
    }
    return `${this.selection.isSelected(row) ? 'deselect' : 'select'} ${row.machineName}`;
  }

  add() {
    this.view = 'editor';
    this.isNew = true;
    this.currentItem = new Robot(true);
    this.currentItemChanged.emit('<new>');
  }

  delete(): void {
    this.deleteConfirm.confirm(new DeletionItems('robot type', this.selection.selected.map(item => item.machineName || '')))
      .subscribe(confirmed => {
        if (!confirmed) return;
        this.doDelete();
      });
  }

  doDelete(): void {
    forkJoin(this.selection.selected.map(s => this.robotService.delete(s)))
      .subscribe(results => {
        let successful = results.filter(r => r.successful).map(r => r.output);
        let failed = results.filter(r => !r.successful);
        let messages: string[] = [];
        if (successful.length !== 0) {
          messages.push(`Deleted ${this.generateCountText(successful.length)}`);
        }

        if (failed.length !== 0) {
          messages.push(`Failed to delete ${this.generateCountText(failed.length)}`);
        }

        this.multilineMessage.show(messages);
        this.selection.clear();
        this.dataSource.data = this.dataSource
          .data
          .filter(el => !successful.includes(el));
      });

  }

  findAndEdit(name: string): void {
    this.robotService.get(name)
      .subscribe(res => {
        if (res.successful) this.edit(res.output);
      });
  }

  edit(item: Robot | undefined = undefined): void {
    this.view = 'editor';
    this.isNew = false;
    this.currentItem = item || this.selection.selected[0];
    this.currentItemChanged.emit(this.currentItem.machineName);
    const viewUrl = this.viewFormatter.toUrl('robots');
    const itemUrl = this.viewFormatter.toUrl(this.currentItem.machineName || 'unknown');
    this.router.navigate(['administrator', viewUrl, itemUrl], {});
  }

  importRobots(): void {
    let settings = new RobotImportSettings();
    this.importRobotService.start(settings)
      .subscribe(result => {
        if (result) this.loadList();
      });
  }

  onClosed(saved: boolean) {
    this.view = 'list';
    if (saved) {
      if (this.isNew) {
        this.dataSource.data = [...this.dataSource.data, this.currentItem!];
        this.snackBar.open(`Added robot '${this.currentItem!.machineName}'`);
      } else {
        this.snackBar.open(`Updated robot '${this.currentItem!.machineName}'`);
      }
      this.currentItem!.id = this.currentItem!.machineName;
    }
    this.currentItemChanged.emit('');
    const viewUrl = this.viewFormatter.toUrl('robots');
    this.router.navigate(['administrator', viewUrl], {});
  }

  exportList(): void {
    console.log('[RobotsListComponent] Showing export settings for robot list');
    let settings = new ReportDialogSettings('Export robot List', false);
    settings.allowedFormats = [
      ReportDialogSettings.Excel,
      ReportDialogSettings.Csv,
      ReportDialogSettings.Text,
      ReportDialogSettings.Pdf,
    ];
    settings.flags = [
      new ReportFlag('import', 'Use import format')
    ];
    this.exportSettings.show(settings)
      .subscribe(result => {
        if (!!result) {
          console.groupCollapsed('[RobotsListComponent] Generating robot list');
          console.log(result);
          console.groupEnd();
          this.downloaderService.download(
            `v1/robots/export.${result.selectedFormat}?flags=${result.encodeFlags()}`,
            `robots.${result.selectedFormat}`);
        } else {
          console.log('[RobotsListComponent] Export cancelled');
        }
      });
  }

  viewQuickLinks(): void {
    this.view = 'quickLinks';
    this.currentItem = this.selection.selected[0];
    this.currentItemChanged.emit(`${this.currentItem?.machineName} [Quick Links]`);
  }

  viewValues(): void {
    this.view = 'values';
    this.currentItem = this.selection.selected[0];
    this.currentItemChanged.emit(`${this.currentItem?.machineName} [Values]`);
  }

  exportDetails(): void {
    console.log('[RobotsListComponent] Showing export settings for details');
    let settings = new ReportDialogSettings('Export Details', true);
    settings.allowedFormats = [
      ReportDialogSettings.Excel,
      ReportDialogSettings.Csv,
      ReportDialogSettings.Text,
      ReportDialogSettings.Pdf,
    ]
    this.exportSettings.show(settings)
      .subscribe(result => {
        if (!!result) {
          console.groupCollapsed('[RobotsListComponent] Generating details export');
          console.log(result);
          console.groupEnd();

          const fromDate = result.dateFrom?.toISODate() || '',
            toDate = result.dateTo?.toISODate() || '';

          this.selection.selected.forEach(t =>
            this.downloaderService.download(
              `v1/robots/${t.machineName}/export.${result.selectedFormat}?from=${fromDate}&to=${toDate}`,
              `${t.friendlyName}_${fromDate}_${toDate}-details.${result.selectedFormat}`));
        } else {
          console.log('[RobotsListComponent] Export cancelled');
        }
      });
  }

  exportLogs(): void {
    console.log('[RobotsListComponent] Showing export settings for logs');
    let settings = new ReportDialogSettings('Export Logs', true);
    settings.allowedFormats = [
      ReportDialogSettings.Excel,
      ReportDialogSettings.Csv,
      ReportDialogSettings.Text,
    ]
    this.exportSettings.show(settings)
      .subscribe(result => {
        if (!!result) {
          console.groupCollapsed('[RobotsListComponent] Generating logs export');
          console.log(result);
          console.groupEnd();

          const fromDate = result.dateFrom?.toISODate() || '',
            toDate = result.dateTo?.toISODate() || '';

          this.selection.selected.forEach(t =>
            this.downloaderService.download(
              `v1/robots/${t.machineName}/logs/export.${result.selectedFormat}?from=${fromDate}&to=${toDate}`,
              `${t.friendlyName}_${fromDate}_${toDate}-logs.${result.selectedFormat}`));
        } else {
          console.log('[RobotsListComponent] Export cancelled');
        }
      });
  }

  private loadList(): void {
    this.isLoading = true;
    this.robotService.list()
      .subscribe(data => {
        if (!this.authenticationService.checkHttpResponse(data)) return;
        this.dataSource = new MatTableDataSource(data.items);
        this.isLoading = false;
      });
  }

  private generateCountText(count: number): string {
    const text = count == 1
      ? '1 robot'
      : `${count} robots`
    return text;
  }
}
