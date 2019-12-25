import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Student } from '../data/student';
import { StudentService } from '../services/student.service';

@Component({
  selector: 'app-student-editor',
  templateUrl: './student-editor.component.html',
  styleUrls: ['./student-editor.component.scss']
})
export class StudentEditorComponent implements OnInit {

  @Input() student: Student;
  @Output() closed = new EventEmitter<boolean>();
  errors: string[];

  constructor(private studentService: StudentService) { }

  ngOnInit() {
  }

  doSave() {
    this.studentService.save(this.student)
      .subscribe(result => {
        if (result.successful) {
          if (result.output) {
            this.student.whenAdded = result.output.whenAdded;
          }
          this.student.isNew = false;
          this.student.password = undefined;
          this.closed.emit(true);
        } else {
          this.errors = result.allErrors();
        }
      });
  }

  doCancel() {
    this.closed.emit(false);
  }

}
