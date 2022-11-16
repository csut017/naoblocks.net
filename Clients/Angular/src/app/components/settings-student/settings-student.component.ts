import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { UntypedFormGroup, UntypedFormControl, Validators } from '@angular/forms';
import { EditorSettings } from 'src/app/data/editor-settings';
import { SettingsService } from 'src/app/services/settings.service';
import { UserSettingsComponent } from '../user-settings/user-settings.component';

@Component({
  selector: 'app-settings-student',
  templateUrl: './settings-student.component.html',
  styleUrls: ['./settings-student.component.scss']
})
export class SettingsStudentComponent implements OnInit {

  @Input() settings: EditorSettings = new EditorSettings();
  @Output() closed = new EventEmitter<boolean>();

  errors: string[] = [];
  form: UntypedFormGroup;
  isSaving: boolean = false;

  @ViewChild(UserSettingsComponent) private settingsComponent!: UserSettingsComponent;

  constructor(private settingsService: SettingsService) {
    this.form = new UntypedFormGroup({
      name: new UntypedFormControl('', [Validators.required]),
      type: new UntypedFormControl('', []),
    });
  }

  ngOnInit(): void {
  }

  doSave() {
    this.settings.user = this.settingsComponent.save();
    this.settingsService.update(this.settings)
      .subscribe(resp => {
        if (resp.successful) {
          this.closed.emit(true);
        }
        else {
          // TODO: Display the error message
        }
      })
  }

  doClose() {
    this.closed.emit(false);
  }
}
