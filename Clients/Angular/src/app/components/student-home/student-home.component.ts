import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, map, shareReplay } from 'rxjs';
import { ConfirmSettings } from 'src/app/data/confirm-settings';
import { EditorSettings } from 'src/app/data/editor-settings';
import { HomeBase } from 'src/app/home-base';
import { AuthenticationService, UserRole } from 'src/app/services/authentication.service';
import { ChangeRoleService } from 'src/app/services/change-role.service';
import { ConfirmService } from 'src/app/services/confirm.service';
import { SettingsService } from 'src/app/services/settings.service';

@Component({
  selector: 'app-student-home',
  templateUrl: './student-home.component.html',
  styleUrls: ['./student-home.component.scss']
})
export class StudentHomeComponent extends HomeBase implements OnInit {

  editorSettings: EditorSettings = new EditorSettings();
  hasChanged: boolean = false;
  invalidBlocks: any[] = [];
  isHandset$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.Handset)
    .pipe(
      map(result => result.matches),
      shareReplay()
    );
  isPlaying: boolean = false;
  isValid: boolean = true;
  onResize: any;
  requireEvents: boolean = true;
  view: string = 'blockly';

  constructor(authenticationService: AuthenticationService,
    router: Router,
    changeRoleService: ChangeRoleService,
    private settingsService: SettingsService,
    private confirm: ConfirmService,
    private breakpointObserver: BreakpointObserver) {
    super(authenticationService, router, changeRoleService);
  }

  ngOnInit(): void {
    this.checkAccess(UserRole.Student);
    this.settingsService.get()
      .subscribe(s => {
        this.editorSettings = s.output || new EditorSettings();
      });
  }

  showSettings(): void {
    this.view = 'user-settings';
  }

  showDebugSettings() {
    this.view = 'debug-settings';
  }

  onClosed(saved: boolean) {
    this.view = 'blockly';
  }

  playProgram() {
    this.isPlaying = true;
  }

  stopProgram() {
    this.isPlaying = false;
  }

  deleteProgram() {
    this.confirm.confirm(new ConfirmSettings('This action will delete your current program. Are you sure?', 'Clear Program', 'Clear'))
      .subscribe(result => {
        if (!result) return;
        console.log('[StudentHome] Clearing current program');
      });
  }

  loadProgram() {
    this.view = 'load-program';
  }

  saveProgram() {
    this.view = 'save-program';
  }
}
