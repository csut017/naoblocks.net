import { Component, OnInit } from '@angular/core';
import { ResultSet } from '../data/result-set';
import { Student } from '../data/student';
import { StudentService } from '../services/student.service';

@Component({
  selector: 'app-students-list',
  templateUrl: './students-list.component.html',
  styleUrls: ['./students-list.component.scss']
})
export class StudentsListComponent implements OnInit {

  isLoading: boolean = false;
  isInEditor: boolean = false;
  selected: Student[] = [];
  students: ResultSet<Student> = new ResultSet<Student>();
  currentStudent: Student;

  constructor(private studentService: StudentService) { }

  ngOnInit() {
    this.loadList();
  }

  doAddNew() {
    this.isInEditor = true;
    this.currentStudent = new Student();
  }

  doDelete() {

  }

  doEdit() {
    this.isInEditor = true;
    this.currentStudent = this.selected[0];
  }

  doExportDetails() {

  }

  doExportLogs() {

  }

  onClosed(saved: boolean) {
    this.isInEditor = false;
    if (saved) this.loadList();
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
