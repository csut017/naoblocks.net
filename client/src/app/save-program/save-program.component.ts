import { Component, OnInit } from '@angular/core';
import { ProgramDirectory } from '../data/program-directory';
import { Student } from '../data/student';
import { ProgramService } from '../services/program.service';

@Component({
  selector: 'app-save-program',
  templateUrl: './save-program.component.html',
  styleUrls: ['./save-program.component.scss']
})
export class SaveProgramComponent implements OnInit {

  opened: boolean;
  programName: string;
  directory: string;
  saveToServer: boolean = true;
  directories: ProgramDirectory[] = [];
  isLoading: boolean = false;

  constructor(private programService: ProgramService) { }

  ngOnInit() {
  }

  show(...students: Student[]): void {
    this.opened = true;
    for (let student of students) {
      let directory = new ProgramDirectory();
      directory.name = student.name;
      this.directories.push(directory);
    }
  }

  doSave(): void {
    this.opened = false;
  }

  selectFile(directory: string, file: string) {
    this.directory = directory;
    this.programName = file;
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
