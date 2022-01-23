import { Injectable } from '@angular/core';
import { MatBottomSheet } from '@angular/material/bottom-sheet';
import { Observable } from 'rxjs';
import { ChangeViewComponent } from '../components/change-view/change-view.component';
import { EditorDefinition } from '../data/editor-definition';

@Injectable({
  providedIn: 'root'
})
export class ChangeViewService {

  constructor(private sheet: MatBottomSheet) { }

  show(currentView: string, views: EditorDefinition[]): Observable<string | undefined> {
    var ref = this.sheet.open(ChangeViewComponent, {
      data: {
        currentView: currentView,
        views: views
      }
    });
    return new Observable<string>(subscriber => {
      ref.afterDismissed().subscribe(view => {
        subscriber.next(view);
        subscriber.complete();
      });
    });
  }
}
