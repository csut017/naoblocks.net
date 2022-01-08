import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, map, shareReplay } from 'rxjs';
import { EditorSettings } from 'src/app/data/editor-settings';
import { HomeBase } from 'src/app/home-base';
import { AuthenticationService } from 'src/app/services/authentication.service';

@Component({
  selector: 'app-student-home',
  templateUrl: './student-home.component.html',
  styleUrls: ['./student-home.component.scss']
})
export class StudentHomeComponent extends HomeBase implements OnInit {

  editorSettings: EditorSettings = new EditorSettings();
  isHandset$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.Handset)
    .pipe(
      map(result => result.matches),
      shareReplay()
    );

  constructor(authenticationService: AuthenticationService,
    router: Router,
    private breakpointObserver: BreakpointObserver) {
      super(authenticationService, router);
     }

  ngOnInit(): void {
  }

}
