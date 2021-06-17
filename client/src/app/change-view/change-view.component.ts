import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-change-view',
  templateUrl: './change-view.component.html',
  styleUrls: ['./change-view.component.scss']
})
export class ChangeViewComponent implements OnInit {

  @Input() view: string;
  views: string[] = ['Blocks', 'Tangibles', 'Text Editor'];
  viewRoutes: {[index: string]: string} = {'Blocks': 'student', 'Tangibles': 'tangible', 'Text Editor': 'texteditor'};
  opened: boolean;

  constructor(private router: Router) { }

  ngOnInit(): void {
  }

  changeView(view: string): void {
    const viewName = this.viewRoutes[view];
    console.log('[ChangeView] Changing to ' + viewName);
    this.router.navigateByUrl(viewName.toLowerCase());
  }

  isCurrent(view: string): boolean {
    return view == this.view;
  }

  show(): void {
    this.opened = true;
  }
}
