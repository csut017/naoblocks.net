import { SelectionModel } from '@angular/cdk/collections';
import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource } from '@angular/material/table';
import { forkJoin } from 'rxjs';
import { DeletionItems } from 'src/app/data/deletion-items';
import { ReportDialogSettings } from 'src/app/data/report-dialog-settings';
import { Student } from 'src/app/data/student';
import { DeletionConfirmationService } from 'src/app/services/deletion-confirmation.service';
import { FileDownloaderService } from 'src/app/services/file-downloader.service';
import { MultilineMessageService } from 'src/app/services/multiline-message.service';
import { ReportSettingsService } from 'src/app/services/report-settings.service';
import { StudentService } from 'src/app/services/student.service';

@Component({
  selector: 'app-students-list',
  templateUrl: './students-list.component.html',
  styleUrls: ['./students-list.component.scss']
})
export class StudentsListComponent implements OnInit {

  columns: string[] = ['select', 'name', 'gender', 'age', 'whenAdded'];
  currentItem?: Student;
  dataSource: MatTableDataSource<Student> = new MatTableDataSource();
  isLoading: boolean = true;
  isNew: boolean = true;
  selection = new SelectionModel<Student>(true, []);
  view: string = 'list';

  @Output() currentItemChanged = new EventEmitter<string>();

  constructor(private studentService: StudentService,
    private snackBar: MatSnackBar,
    private deleteConfirm: DeletionConfirmationService,
    private multilineMessage: MultilineMessageService,
    private exportSettings: ReportSettingsService,
    private downloaderService: FileDownloaderService) { }

  ngOnInit(): void {
    this.loadList();
  }

  add() {
    this.view = 'editor';
    this.isNew = true;
    this.currentItem = new Student(true);
    this.currentItemChanged.emit('<new>');
  }

  delete(): void {
    this.deleteConfirm.confirm(new DeletionItems('student', this.selection.selected.map(item => item.name || '')))
      .subscribe(confirmed => {
        if (!confirmed) return;
        this.doDelete();
      });
  }

  doDelete(): void {
    forkJoin(this.selection.selected.map(s => this.studentService.delete(s)))
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
  
  edit(): void {
    const item = this.selection.selected[0];
    if (!item?.isFullyLoaded) {
      this.studentService.get(item.id!)
        .subscribe(student => {
          if (student.successful) {
            item.isFullyLoaded = true;
            item.settings = student.output!.settings;
            this.doEdit(item);
          } else {
            this.snackBar.open(`Unable to retrieve student details!`);
          }
        });
    } else {
        this.doEdit(item);
    }
  }

  private doEdit(item: Student): void {
    this.view = 'editor';
    this.isNew = false;
    this.currentItem = item;
    this.currentItemChanged.emit(item.name);
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

  checkboxLabel(row?: Student): string {
    if (!row) {
      return `${this.isAllSelected() ? 'deselect' : 'select'} all`;
    }
    return `${this.selection.isSelected(row) ? 'deselect' : 'select'} ${row.name}`;
  }

  onClosed(saved: boolean) {
    this.view = 'list';
    this.currentItemChanged.emit('');
    if (saved) {
      if (this.isNew) {
        this.dataSource.data = [...this.dataSource.data, this.currentItem!];
        this.snackBar.open(`Added student '${this.currentItem!.name}'`);
      } else {
        this.snackBar.open(`Updated student '${this.currentItem!.name}'`);
      }
      this.currentItem!.id = this.currentItem!.name;
    }
  }

  exportList(): void {
    console.log('[StudentsListComponent] Showing export settings for student list');
    let settings = new ReportDialogSettings('Export Student List', false);
    settings.allowedFormats = [
      ReportDialogSettings.Excel,
      ReportDialogSettings.Csv,
      ReportDialogSettings.Text,
    ]
    this.exportSettings.show(settings)
      .subscribe(result => {
        if (!!result) {
          console.groupCollapsed('[StudentsListComponent] Generating student list');
          console.log(result);
          console.groupEnd();
          this.downloaderService.download(
            `v1/students/export.${result.selectedFormat}`, 
            `students.${result.selectedFormat}`);
        } else {
          console.log('[StudentsListComponent] Export cancelled');
        }
      });
  }

  exportDetails(): void {
    console.log('[StudentsListComponent] Showing export settings for details');
    let settings = new ReportDialogSettings('Export Details', true);
    settings.allowedFormats = [
      ReportDialogSettings.Excel,
      ReportDialogSettings.Csv,
      ReportDialogSettings.Text,
    ]
    this.exportSettings.show(settings)
      .subscribe(result => {
        if (!!result) {
          console.groupCollapsed('[StudentsListComponent] Generating details export');
          console.log(result);
          console.groupEnd();

          const fromDate = result.dateFrom?.toISODate() || '',
            toDate = result.dateTo?.toISODate() || '';

          this.selection.selected.forEach(t =>
            this.downloaderService.download(
              `v1/students/${t.name}/export.${result.selectedFormat}?from=${fromDate}&to=${toDate}`,
              `${t.name}_${fromDate}_${toDate}-details.${result.selectedFormat}`));
        } else {
          console.log('[StudentsListComponent] Export cancelled');
        }
      });
  }

  exportLogs(): void {
    console.log('[StudentsListComponent] Showing export settings for logs');
    let settings = new ReportDialogSettings('Export Logs', true);
    settings.allowedFormats = [
      ReportDialogSettings.Excel,
      ReportDialogSettings.Csv,
      ReportDialogSettings.Text,
    ]
    this.exportSettings.show(settings)
      .subscribe(result => {
        if (!!result) {
          console.groupCollapsed('[StudentsListComponent] Generating logs export');
          console.log(result);
          console.groupEnd();

          const fromDate = result.dateFrom?.toISODate() || '',
            toDate = result.dateTo?.toISODate() || '';

          this.selection.selected.forEach(t =>
            this.downloaderService.download(
              `v1/students/${t.name}/logs/export.${result.selectedFormat}?from=${fromDate}&to=${toDate}`, 
              `${t.name}_${fromDate}_${toDate}-logs.${result.selectedFormat}`));
        } else {
          console.log('[StudentsListComponent] Export cancelled');
        }
      });
  }

  exportSnapshots(): void {
    console.log('[StudentsListComponent] Showing export settings for snapshots');
    let settings = new ReportDialogSettings('Export Snapshots', true);
    settings.allowedFormats = [
      ReportDialogSettings.Excel,
      ReportDialogSettings.Csv,
      ReportDialogSettings.Text,
    ]
    this.exportSettings.show(settings)
      .subscribe(result => {
        if (!!result) {
          console.groupCollapsed('[StudentsListComponent] Generating snapshots export');
          console.log(result);
          console.groupEnd();

          const fromDate = result.dateFrom?.toISODate() || '',
            toDate = result.dateTo?.toISODate() || '';

          this.selection.selected.forEach(t =>
            this.downloaderService.download(
              `v1/students/${t.name}/snapshots/export.${result.selectedFormat}?from=${fromDate}&to=${toDate}`, 
              `${t.name}_${fromDate}_${toDate}-snapshots.${result.selectedFormat}`));
        } else {
          console.log('[StudentsListComponent] Export cancelled');
        }
      });
  }

  private loadList(): void {
    this.isLoading = true;
    this.studentService.list()
      .subscribe(data => {
        this.dataSource = new MatTableDataSource(data.items);
        this.isLoading = false;
      });
  }

  private generateCountText(count: number): string {
    const text = count == 1
      ? '1 student'
      : `${count} students`
    return text;
  }
}
