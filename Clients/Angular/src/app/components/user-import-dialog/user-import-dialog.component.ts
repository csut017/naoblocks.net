import { SelectionModel } from '@angular/cdk/collections';
import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatStepper } from '@angular/material/stepper';
import { MatTableDataSource } from '@angular/material/table';
import { User } from 'src/app/data/user';
import { UserImportSettings } from 'src/app/data/user-import-settings';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-user-import-dialog',
  templateUrl: './user-import-dialog.component.html',
  styleUrls: ['./user-import-dialog.component.scss']
})
export class UserImportDialogComponent implements OnInit {

  controlFile: FormControl = new FormControl(null, { validators: Validators.required });
  controlVerification: FormControl = new FormControl(true, { validators: Validators.required });
  columns: string[] = ['select', 'name', 'age', 'gender', 'password', 'message'];
  currentIndex: number = 1;
  errorMessage: string = '';
  isFinished: boolean = false;
  isImporting: boolean = false;
  dataSource: MatTableDataSource<User> = new MatTableDataSource();
  results: string[] = [];
  selection = new SelectionModel<User>(true, []);
  successful: number = 0;
  @ViewChild('stepper') stepper?: MatStepper;

  constructor(private userService: UserService,
    @Inject(MAT_DIALOG_DATA) public settings: UserImportSettings) {
  }

  ngOnInit(): void {
  }

  fileBrowseHandler($event: any): void {
    const files = $event.target ? $event.target.files : $event;
    this.userService.parseStudentImportFile(files[0])
      .subscribe(result => {
        if (!result.successful) {
          this.errorMessage = result.allErrors().join();
          return;
        }

        let users = result.output?.items || [];
        this.selection.clear();
        this.dataSource = new MatTableDataSource(users.map(u => {
          if (!u.message) this.selection.select(u);
          return u;
        }));

        this.errorMessage = '';
        this.controlFile.setValue(files[0]);
        this.stepper?.next();
      });
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

  startImport(): void {
    this.isImporting = true;
    this.currentIndex = 0;
    this.results = [];
    this.successful = 0;
    this.importUser();
  }

  importUser(): void {
    let user = this.selection.selected[this.currentIndex++];
    user.isNew = true;
    this.userService.save(user)
      .subscribe(res => {
        if (res.successful) {
          this.results.push(`Successfully imported ${user.name}`);
          this.successful++;
        } else {
          this.results.push(`Failed to import ${user.name}: ${res.allErrors().join(', ')}`);
        }
        if (this.currentIndex < this.selection.selected.length) {
          this.importUser();
        } else {
          this.currentIndex = this.selection.selected.length;
          this.isFinished = true;
        }
      });
  }
}
