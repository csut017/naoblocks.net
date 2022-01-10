import { SelectionModel } from '@angular/cdk/collections';
import { Component, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { Student } from 'src/app/data/student';
import { FileDownloaderService } from 'src/app/services/file-downloader.service';
import { StudentService } from 'src/app/services/student.service';

@Component({
  selector: 'app-students-list',
  templateUrl: './students-list.component.html',
  styleUrls: ['./students-list.component.scss']
})
export class StudentsListComponent implements OnInit {

  columns: string[] = ['select', 'name', 'role', 'whenAdded'];
  dataSource: MatTableDataSource<Student> = new MatTableDataSource();
  isLoading: boolean = true;
  selection = new SelectionModel<Student>(true, []);

  constructor(private studentService: StudentService,
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

  checkboxLabel(row?: Student): string {
    if (!row) {
      return `${this.isAllSelected() ? 'deselect' : 'select'} all`;
    }
    return `${this.selection.isSelected(row) ? 'deselect' : 'select'} ${row.name}`;
  }

  private loadList(): void {
    this.isLoading = true;
    this.studentService.list()
      .subscribe(data => {
        this.dataSource = new MatTableDataSource(data.items);
        this.isLoading = false;
      });
  }

}
