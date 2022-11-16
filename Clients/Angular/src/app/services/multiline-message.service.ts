import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MultilineMessageComponent } from '../components/multiline-message/multiline-message.component';

@Injectable({
  providedIn: 'root'
})
export class MultilineMessageService {

  constructor(private snackBar: MatSnackBar) { }

  show(lines: string[]): void {
    this.snackBar.openFromComponent(MultilineMessageComponent, {
      data: lines
    });
  }
}
