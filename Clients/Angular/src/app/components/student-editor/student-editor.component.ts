import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges, ViewChild } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Student } from 'src/app/data/student';
import { UserSettings } from 'src/app/data/user-settings';
import { StudentService } from 'src/app/services/student.service';
import { UserSettingsComponent } from '../user-settings/user-settings.component';

@Component({
  selector: 'app-student-editor',
  templateUrl: './student-editor.component.html',
  styleUrls: ['./student-editor.component.scss']
})
export class StudentEditorComponent implements OnInit, OnChanges {

  @Input() item?: Student;
  @Output() closed = new EventEmitter<boolean>();
  @ViewChild(UserSettingsComponent) userSettingsEditor!: UserSettingsComponent;

  errors: string[] = [];
  form: FormGroup;
  isSaving: boolean = false;

  private readonly passwordControl = new FormControl('', []);

  constructor(private studentService: StudentService) {
    this.form = new FormGroup({
      name: new FormControl('', [Validators.required]),
      age: new FormControl(0, []),
      gender: new FormControl('', []),
      password: this.passwordControl,
    });
  }
  
  ngOnChanges(_: SimpleChanges): void {
    this.form.setValue({
      name: this.item?.name || '',
      age: this.item?.age || 0,
      gender: this.item?.gender || 'Unknown',
      password: ''
    });

    this.passwordControl.clearValidators();
    if (this.item?.isNew) {
      this.passwordControl.addValidators([Validators.required]);
    }

    if (!!this.item) this.item.settings = this.item.settings || new UserSettings();
  }

  ngOnInit(): void {
  }

  doSave() {
    if (!this.item) return;
    this.item.name = this.form.get('name')?.value;
    this.item.age = this.form.get('age')?.value;
    this.item.gender = this.form.get('gender')?.value || 'Unknown';
    this.userSettingsEditor.save();
    let password = this.form.get('password')?.value || '';
    if (password !== '') this.item.password = password;
    this.studentService.save(this.item)
      .subscribe(result => {
        if (result.successful) {
          this.item!.isNew = false;
          this.item!.whenAdded = result.output?.whenAdded;
          this.closed.emit(true);
        } else {
          this.errors = result.allErrors();
        }
      });
  }

  doClose() {
    this.closed.emit(false);
  }
}
