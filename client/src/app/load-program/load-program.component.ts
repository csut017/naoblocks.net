import { Component, Output, EventEmitter } from '@angular/core';
import { ProgramDirectory } from '../data/program-directory';
import { ProgramService } from '../services/program.service';
import { Student } from '../data/student';
import { ProgramFile } from '../data/program-file';
import { ErrorHandlerService } from '../services/error-handler.service';

@Component({
  selector: 'app-load-program',
  templateUrl: './load-program.component.html',
  styleUrls: ['./load-program.component.scss']
})
export class LoadProgramComponent {

  opened: boolean;
  currentProgram: ProgramFile;
  directory: string;
  directories: ProgramDirectory[] = [];
  isLoading: boolean = false;
  isInitialised: boolean = false;
  canLoad: boolean = false;
  errorMessage: string;

  @Output() load = new EventEmitter<string>();

  constructor(private programService: ProgramService,
    private errorHandler: ErrorHandlerService) { }

  onBlur() {
    this.canLoad = !!this.currentProgram;
  }

  reset(): void {
    this.isInitialised = false;
    this.directories = [];
    this.currentProgram = undefined;
    this.canLoad = false;
  }

  show(...students: Student[]): void {
    this.opened = true;
    this.errorMessage = undefined;
    if (this.isInitialised) return;

    for (let student of students) {
      let directory = new ProgramDirectory();
      directory.name = student.name;
      this.directories.push(directory);
    }
    this.isInitialised = true;
  }

  doLoad(): void {
    this.programService.get(this.directory, this.currentProgram.name)
      .subscribe(program => {
        if (program.successful) {
          this.load.emit(program.output.code);
        } else {
          this.showError(this.errorHandler.formatError(program));
        }
      });
  }

  showError(msg: string): void {
    this.errorMessage = msg;
  }

  close(): void {
    this.opened = false;
  }

  selectFile(directory: string, file: ProgramFile) {
    if (this.currentProgram) this.currentProgram.active = false;
    this.directory = directory;
    this.currentProgram = file;
    this.canLoad = !!this.currentProgram;
    if (this.currentProgram) this.currentProgram.active = true;
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
