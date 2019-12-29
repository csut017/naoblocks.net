import { Component, OnInit, OnChanges, SimpleChanges, Output, EventEmitter } from '@angular/core';
import { ProgramDirectory } from '../data/program-directory';
import { Student } from '../data/student';
import { ProgramService } from '../services/program.service';

export class SaveDetails {
  user: string;
  name: string;

  constructor(user: string, name: string) {
    this.user = user;
    this.name = name;
  }
}

@Component({
  selector: 'app-save-program',
  templateUrl: './save-program.component.html',
  styleUrls: ['./save-program.component.scss']
})
export class SaveProgramComponent implements OnChanges {

  opened: boolean;
  programName: string;
  directory: string;
  saveToServer: boolean = true;
  directories: ProgramDirectory[] = [];
  isLoading: boolean = false;
  isInitialised: boolean = false;
  canSave: boolean = false;
  errorMessage: string;

  @Output() save = new EventEmitter<SaveDetails>();

  constructor(private programService: ProgramService) { }

  ngOnChanges(_: SimpleChanges): void {
    this.canSave = !!this.programName;
  }

  reset(): void {
    this.isInitialised = false;
    this.directories = [];
    this.programName = undefined;
    this.saveToServer = true;
    this.canSave = false;
  }

  show(...students: Student[]): void {
    this.opened = true;
    if (this.isInitialised) return;

    for (let student of students) {
      let directory = new ProgramDirectory();
      directory.name = student.name;
      this.directories.push(directory);
    }
    this.isInitialised = true;
  }

  doSave(): void {
    this.save.emit(new SaveDetails(this.directory, this.programName));
  }

  showError(msg: string): void {
    this.errorMessage = msg;
  }

  close(): void {
    this.opened = false;
  }

  selectFile(directory: string, file: string) {
    this.directory = directory;
    this.programName = file;
    this.canSave = !!this.programName;
  }

  loadFiles(directory: ProgramDirectory) {
    this.isLoading = true;
    this.programService.list(directory.name)
      .subscribe(data => {
        if (data.errorMsg) {

        } else {
          directory.files = data.items;
        }
        this.isLoading = false;
      });
  }

}
