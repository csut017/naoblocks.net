import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { NamedValueEditorComponent } from '../components/named-value-editor/named-value-editor.component';
import { NamedValue } from '../data/named-value';
import { NamedValueEdit } from '../data/named-value-edit';

@Injectable({
  providedIn: 'root'
})
export class NamedValueEditorService {

  constructor(private dialog: MatDialog) { }

  show(settings: NamedValueEdit): Observable<NamedValue> {
    const dialogRef = this.dialog.open(NamedValueEditorComponent, {
      data: settings
    });
    return new Observable<NamedValue>(subscriber => {
      dialogRef.afterClosed()
        .subscribe(result => {
          subscriber.next(result);
          subscriber.complete();
        });
    });
  }
}
