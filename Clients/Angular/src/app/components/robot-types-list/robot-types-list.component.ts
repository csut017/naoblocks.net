import { SelectionModel } from '@angular/cdk/collections';
import { Component, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { RobotType } from 'src/app/data/robot-type';
import { AuthenticationService } from 'src/app/services/authentication.service';
import { FileDownloaderService } from 'src/app/services/file-downloader.service';
import { RobotTypeService } from 'src/app/services/robot-type.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DeletionConfirmationService } from 'src/app/services/deletion-confirmation.service';
import { DeletionItems } from 'src/app/data/deletion-items';
import { forkJoin } from 'rxjs';
import { MultilineMessageService } from 'src/app/services/multiline-message.service';

@Component({
  selector: 'app-robot-types-list',
  templateUrl: './robot-types-list.component.html',
  styleUrls: ['./robot-types-list.component.scss']
})
export class RobotTypesListComponent implements OnInit {

  columns: string[] = ['select', 'name', 'isDefault'];
  currentItem?: RobotType;
  dataSource: MatTableDataSource<RobotType> = new MatTableDataSource();
  hasSystemDefault: boolean = true;
  isLoading: boolean = true;
  isNew: boolean = true;
  selection = new SelectionModel<RobotType>(true, []);
  view: string = 'list';

  constructor(private robotTypeService: RobotTypeService,
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

  checkboxLabel(row?: RobotType): string {
    if (!row) {
      return `${this.isAllSelected() ? 'deselect' : 'select'} all`;
    }
    return `${this.selection.isSelected(row) ? 'deselect' : 'select'} ${row.name}`;
  }

  add() {
    this.view = 'editor';
    this.isNew = true;
    this.currentItem = new RobotType(true);
  }

  delete(): void {
    this.deleteConfirm.confirm(new DeletionItems('robot type', this.selection.selected.map(item => item.name || '')))
      .subscribe(confirmed => {
        if (!confirmed) return;
        this.doDelete();
      });
  }

  doDelete(): void {
    forkJoin(this.selection.selected.map(s => this.robotTypeService.delete(s)))
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
        this.updateDefaultRobotMessage();
      });
  }
  updateDefaultRobotMessage() {
    this.hasSystemDefault = !!this.dataSource.data.find(rt => rt.isDefault);
  }

  edit(): void {
    this.view = 'editor';
    this.isNew = false;
    this.currentItem = this.selection.selected[0];
  }

  import(): void {

  }

  setSystemDefault(): void {
    const robotType = this.selection.selected[0];
    this.robotTypeService.setSystemDefault(robotType)
      .subscribe(result => {
        if (result.successful) {
          this.dataSource.data.forEach(type => type.isDefault = false);
          robotType.isDefault = true;
          this.snackBar.open(`Set ${robotType.name} as system default`);
        } else {
          this.snackBar.open(`ERROR: Unable to set ${robotType.name} as system default`);
        }
        this.updateDefaultRobotMessage();
      });
  }

  onClosed(saved: boolean) {
    this.view = 'list';
    if (saved) {
      if (this.isNew) {
        this.dataSource.data = [...this.dataSource.data, this.currentItem!];
        this.snackBar.open(`Added robot type '${this.currentItem!.name}'`);
      } else {
        this.snackBar.open(`Updated robot type '${this.currentItem!.name}'`);
      }
      this.currentItem!.id = this.currentItem!.name;
      this.updateDefaultRobotMessage();
    }
  }

  private loadList(): void {
    this.isLoading = true;
    this.robotTypeService.list()
      .subscribe(data => {
        if (!this.authenticationService.checkHttpResponse(data)) return;
        this.dataSource = new MatTableDataSource(data.items);
        this.isLoading = false;
        this.hasSystemDefault = !!data.items.find(rt => rt.isDefault);
      });
  }

  private generateCountText(count: number): string {
    const text = count == 1
      ? '1 robot type'
      : `${count} robot types`
    return text;
  }
}
