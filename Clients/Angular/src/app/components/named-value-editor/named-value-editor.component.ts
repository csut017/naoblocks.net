import { Component, Inject } from '@angular/core';
import { UntypedFormGroup, UntypedFormControl, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { NamedValueEdit } from 'src/app/data/named-value-edit';

@Component({
  selector: 'app-named-value-editor',
  templateUrl: './named-value-editor.component.html',
  styleUrls: ['./named-value-editor.component.scss']
})
export class NamedValueEditorComponent {

  form: UntypedFormGroup;

  constructor(
    public dialogRef: MatDialogRef<NamedValueEditorComponent>,
    @Inject(MAT_DIALOG_DATA) public data: NamedValueEdit) {
      this.form = new UntypedFormGroup({
        name: new UntypedFormControl(data.value.name, [Validators.required]),
        value: new UntypedFormControl(data.value.value, []),
      });
     }

  doCancel(): void {
    this.dialogRef.close();
  }

  doSave() {
    if (!this.form.valid) return;
    this.data.value.name = this.form.get('name')?.value;
    this.data.value.value = this.form.get('value')?.value || '';
    this.dialogRef.close(this.data.value);
  }

}
