import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Student } from '../data/student';

@Component({
  selector: 'app-student-editor',
  templateUrl: './student-editor.component.html',
  styleUrls: ['./student-editor.component.scss']
})
export class StudentEditorComponent implements OnInit {

  @Input() student: Student;
  @Output() closed = new EventEmitter<boolean>();

  constructor() { }

  ngOnInit() {
  }

  doSave() {
    this.closed.emit(true);
  }

  doCancel() {
    this.closed.emit(false);
  }

}
