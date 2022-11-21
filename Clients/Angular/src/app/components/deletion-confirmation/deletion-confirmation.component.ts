import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { DeletionItems } from 'src/app/data/deletion-items';

@Component({
  selector: 'app-deletion-confirmation',
  templateUrl: './deletion-confirmation.component.html',
  styleUrls: ['./deletion-confirmation.component.scss']
})
export class DeletionConfirmationComponent implements OnInit {

  constructor(
    public dialogRef: MatDialogRef<DeletionConfirmationComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DeletionItems) { }

  ngOnInit(): void {
  }

  getTypeAndCount(): string {
    const type = this.data.items.length == 1
      ? this.data.type
      : (this.data.pluralType || (this.data.type + 's'));
    return `${this.data.items.length} ${type}`;
  }

  doCancel(): void {
    this.dialogRef.close();
  }

}
