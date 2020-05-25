import { Component, OnInit } from '@angular/core';
import { ResultSet } from '../data/result-set';
import { Student } from '../data/student';
import { StudentService } from '../services/student.service';
import { forkJoin } from 'rxjs';
import { FileDownloaderService } from '../services/file-downloader.service';

@Component({
  selector: 'app-students-list',
  templateUrl: './students-list.component.html',
  styleUrls: ['./students-list.component.scss']
})
export class StudentsListComponent implements OnInit {

  isLoading: boolean = false;
  isInList: boolean = true;
  isInEditor: boolean = false;
  isNew: boolean = false;
  selected: Student[] = [];
  students: ResultSet<Student> = new ResultSet<Student>();
  currentStudent: Student;
  message: string;
  errorMessage: string;

  constructor(private studentService: StudentService,
    private downloaderService: FileDownloaderService) { }

  ngOnInit() {
    this.loadList();
  }

  doAddNew() {
    this.isInEditor = true;
    this.isInList = false;
    this.isNew = true;
    this.currentStudent = new Student(true);
  }

  doDelete() {
    forkJoin(this.selected.map(s => this.studentService.delete(s)))
      .subscribe(results => {
        let successful = results.filter(r => r.successful).map(r => r.output);
        let failed = results.filter(r => !r.successful);
        this.message = `Deleted ${successful.length} students`;
        if (failed.length !== 0) {
          this.errorMessage = `Failed to delete ${failed.length} students`;
        } else {
          this.errorMessage = undefined;
        }

        this.students.items = this.students
            .items
            .filter(el => !successful.includes(el));
        this.students.count -= successful.length;
      });
  }

  doEdit() {
    this.isInEditor = true;
    this.isInList = false;
    this.isNew = false;
    this.currentStudent = this.selected[0];
  }
  doExportList(): void {
    this.downloaderService.download('v1/students/export/list', 'students-list.xlsx');
  }

  doExportDetails() {

  }

  doExportLogs() {

  }

  onClosed(saved: boolean) {
    this.isInEditor = false;
    this.isInList = true;
    if (saved) {
      if (this.isNew) {
        this.students.items.push(this.currentStudent);
        this.message = `Added student '${this.currentStudent.name}'`;
        this.currentStudent.id = this.currentStudent.name;
      } else {
        this.message = `Updated student '${this.currentStudent.name}'`;
      }
    }
  }

  private loadList(): void {
    this.isLoading = true;
    this.studentService.list()
      .subscribe(data => {
        this.students = data;
        this.isLoading = false;
      });
  }

}
