import { SelectionModel } from '@angular/cdk/collections';
import { Component, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { RobotType } from 'src/app/data/robot-type';
import { AuthenticationService } from 'src/app/services/authentication.service';
import { FileDownloaderService } from 'src/app/services/file-downloader.service';
import { RobotTypeService } from 'src/app/services/robot-type.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-robot-types-list',
  templateUrl: './robot-types-list.component.html',
  styleUrls: ['./robot-types-list.component.scss']
})
export class RobotTypesListComponent implements OnInit {

  columns: string[] = ['select', 'name', 'isDefault'];
  currentItem?: RobotType;
  dataSource: MatTableDataSource<RobotType> = new MatTableDataSource();
  isLoading: boolean = true;
  isNew: boolean = true;
  selection = new SelectionModel<RobotType>(true, []);
  view: string = 'list';

  constructor(private robotTypeService: RobotTypeService,
    private authenticationService: AuthenticationService,
    private snackBar: MatSnackBar,
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
        this.snackBar.open(`Added robot type '${this.currentItem!.name}'`, 'OK');
      } else {
        this.snackBar.open(`Updated robot type '${this.currentItem!.name}'`, 'OK');
      }
      this.currentItem!.id = this.currentItem!.name;
    }
  }

  private loadList(): void {
    this.isLoading = true;
    this.robotTypeService.list()
      .subscribe(data => {
        if (!this.authenticationService.checkHttpResponse(data)) return;
        this.dataSource = new MatTableDataSource(data.items);
        this.isLoading = false;
      });
  }

}
