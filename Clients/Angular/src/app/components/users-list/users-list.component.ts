import { SelectionModel } from '@angular/cdk/collections';
import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { MatLegacyTableDataSource as MatTableDataSource } from '@angular/material/legacy-table';
import { User } from 'src/app/data/user';
import { FileDownloaderService } from 'src/app/services/file-downloader.service';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-users-list',
  templateUrl: './users-list.component.html',
  styleUrls: ['./users-list.component.scss']
})
export class UsersListComponent implements OnInit {

  columns: string[] = ['select', 'name', 'role', 'whenAdded'];
  dataSource: MatTableDataSource<User> = new MatTableDataSource();
  isLoading: boolean = true;
  selection = new SelectionModel<User>(true, []);

  @Output() currentItemChanged = new EventEmitter<string>();

  constructor(private userService: UserService,
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

  checkboxLabel(row?: User): string {
    if (!row) {
      return `${this.isAllSelected() ? 'deselect' : 'select'} all`;
    }
    return `${this.selection.isSelected(row) ? 'deselect' : 'select'} ${row.name}`;
  }

  private loadList(): void {
    this.isLoading = true;
    this.userService.list()
      .subscribe(data => {
        this.dataSource = new MatTableDataSource(data.items);
        this.isLoading = false;
      });
  }

}
