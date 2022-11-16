import { Component, Inject, OnInit } from '@angular/core';
import { MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA } from '@angular/material/bottom-sheet';
import { EditorDefinition } from 'src/app/data/editor-definition';

@Component({
  selector: 'app-change-view',
  templateUrl: './change-view.component.html',
  styleUrls: ['./change-view.component.scss']
})
export class ChangeViewComponent implements OnInit {

  constructor(private sheetRef: MatBottomSheetRef<ChangeViewComponent>,
    @Inject(MAT_BOTTOM_SHEET_DATA) public data: {
      currentView: string,
      views: EditorDefinition[]
    }) { }

  ngOnInit(): void {
  }

  changeView(event: MouseEvent, view: string) {
    this.sheetRef.dismiss(view);
    event.preventDefault();
  }

  isCurrent(view: EditorDefinition): boolean {
    const current = this.data.currentView == view.name;
    return current;
  }
}
