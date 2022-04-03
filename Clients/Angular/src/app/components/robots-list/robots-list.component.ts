import { SelectionModel } from '@angular/cdk/collections';
import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource } from '@angular/material/table';
import { forkJoin } from 'rxjs';
import { DeletionItems } from 'src/app/data/deletion-items';
import { Robot } from 'src/app/data/robot';
import { AuthenticationService } from 'src/app/services/authentication.service';
import { DeletionConfirmationService } from 'src/app/services/deletion-confirmation.service';
import { FileDownloaderService } from 'src/app/services/file-downloader.service';
import { MultilineMessageService } from 'src/app/services/multiline-message.service';
import { RobotService } from 'src/app/services/robot.service';

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

  constructor(private robotService: RobotService,
    private authenticationService: AuthenticationService,
    private snackBar: MatSnackBar,
    private deleteConfirm: DeletionConfirmationService,
    private multilineMessage: MultilineMessageService,
    private downloaderService: FileDownloaderService) { }

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

  edit(): void {
    this.view = 'editor';
    this.isNew = false;
    this.currentItem = this.selection.selected[0];
  }

  import(): void {

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
