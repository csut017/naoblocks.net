import { Component, OnInit, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { Student } from '../data/student';
import { StudentService } from '../services/student.service';
import { UserSettings } from '../data/user-settings';

@Component({
  selector: 'app-student-editor',
  templateUrl: './student-editor.component.html',
  styleUrls: ['./student-editor.component.scss']
})
export class StudentEditorComponent implements OnChanges {
  @Input() student: Student;
  @Output() closed = new EventEmitter<boolean>();
  errors: string[];
  showPassword: boolean = false;

  constructor(private studentService: StudentService) { }

  ngOnChanges(_: SimpleChanges): void {
    if (this.student) this.student.settings = this.student.settings || new UserSettings();
  }

  doSave() {
    if (!this.student.isNew && !this.showPassword) this.student.password = undefined;
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
