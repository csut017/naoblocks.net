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

  isLoading: boolean;
  students: ResultSet<Student>;

  constructor(private studentService: StudentService) { }

  ngOnInit() {
    this.loadList();
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
