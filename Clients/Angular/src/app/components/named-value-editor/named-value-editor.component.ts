import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { NamedValueEdit } from 'src/app/data/named-value-edit';

@Component({
  selector: 'app-named-value-editor',
  templateUrl: './named-value-editor.component.html',
  styleUrls: ['./named-value-editor.component.scss']
})
export class NamedValueEditorComponent {

  constructor(
    public dialogRef: MatDialogRef<NamedValueEditorComponent>,
    @Inject(MAT_DIALOG_DATA) public data: NamedValueEdit) { }

  doCancel(): void {
    this.dialogRef.close();
  }

}
