import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, map, shareReplay } from 'rxjs';
import { ConfirmSettings } from 'src/app/data/confirm-settings';
import { EditorDefinition } from 'src/app/data/editor-definition';
import { EditorSettings } from 'src/app/data/editor-settings';
import { RunSettings } from 'src/app/data/run-settings';
import { HomeBase } from 'src/app/home-base';
import { AuthenticationService, UserRole } from 'src/app/services/authentication.service';
import { ChangeRoleService } from 'src/app/services/change-role.service';
import { ChangeViewService } from 'src/app/services/change-view.service';
import { ConfirmService } from 'src/app/services/confirm.service';
import { ProgramControllerService } from 'src/app/services/program-controller.service';
import { SettingsService } from 'src/app/services/settings.service';

@Component({
  selector: 'app-student-home',
  templateUrl: './student-home.component.html',
  styleUrls: ['./student-home.component.scss']
})
export class StudentHomeComponent extends HomeBase implements OnInit {

  controller: ProgramControllerService = new ProgramControllerService();
  editorSettings: EditorSettings = new EditorSettings();
  editorView: string = 'blockly';
  isHandset$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.Handset)
    .pipe(
      map(result => result.matches),
      shareReplay()
    );
  isAdmin: boolean = false;
  runSettings: RunSettings = new RunSettings();
  showCommands: boolean = true;
  showFileCommands: boolean = true;
  title: string = '';
  view: string = '';

  private editorDefinitions: EditorDefinition[] = [
    new EditorDefinition('blockly', 'Block Editor', true),
    new EditorDefinition('tangibles', 'Tangible Editor', false),
    new EditorDefinition('yolo', 'Yolo Editor', true),
  ];
  private editors: { [index: string]: EditorDefinition } = {};

  constructor(authenticationService: AuthenticationService,
    router: Router,
    changeRoleService: ChangeRoleService,
    private changeViewService: ChangeViewService,
    private route: ActivatedRoute,
    private confirm: ConfirmService,
    private settingsService: SettingsService,
    private breakpointObserver: BreakpointObserver) {
    super(authenticationService, router, changeRoleService);
    this.editorDefinitions.forEach(e => this.editors[e.name] = e);
    this.changeView('editor', this.editors[this.editorView].title, true);
    this.route.paramMap.subscribe(params => {
      this.changeEditorView(params.get('view') || this.editorDefinitions[0].name);
    });
  }

  ngOnInit(): void {
    this.checkAccess(UserRole.Student);
    this.isAdmin = this.authenticationService.canAccess(UserRole.Administrator);
    this.settingsService.get()
      .subscribe(s => {
        this.editorSettings = s.output || new EditorSettings();
      });
  }

  showSettings(): void {
    this.changeView('user-settings', 'User Settings', false);
  }

  showDebugSettings() {
    this.changeView('debug-settings', 'Debug Settings', false);
  }

  onClosed(saved: boolean, reloadEditor: boolean) {
    this.changeView('editor', this.editors[this.editorView].title, true);
    if (saved && reloadEditor) {
      this.settingsService.get()
        .subscribe(s => {
          this.editorSettings = s.output || new EditorSettings();
        });
    }
  }

  onSettingsClosed(settings?: RunSettings) {
    if (settings) this.runSettings = settings;
    this.changeView('editor', this.editors[this.editorView].title, true);
  }

  canChangeView(): boolean {
    return true;
  }

  openChangeView(): void {
    console.log('[StudentHome] Changing View');
    this.changeViewService
      ?.show(this.editorView, this.editorDefinitions)
      .subscribe(view => {
        if (!!view) {
          console.log('[StudentHome] New view is ' + view);
          this.changeEditorView(view);
        }
      });
  }

  playProgram() {
    console.log('[StudentHome] Playing current program');
    this.controller.play(this.runSettings);
  }

  stopProgram() {
    console.log('[StudentHome] Stopping current program');
    this.controller.stop();
  }

  deleteProgram() {
    this.confirm.confirm(new ConfirmSettings('This action will delete your current program. Are you sure?', 'Clear Program', 'Clear'))
      .subscribe(result => {
        if (!result) return;
        console.log('[StudentHome] Clearing current program');
        this.controller.clear();
      });
  }

  loadProgram() {
    this.changeView('load-program', 'Load Program', false);
  }

  saveProgram() {
    this.changeView('save-program', 'Save Program', false);
  }

  private changeView(view: string, title: string, showCommands: boolean): void {
    this.view = view;
    this.title = title;
    this.showCommands = showCommands;
  }

  private changeEditorView(view: string) {
    this.editorView = view;
    const editor = this.editors[this.editorView];
    this.changeView('editor', editor.title, true);
    this.showFileCommands = editor.fileCommands;
    this.router.navigate(['student', view], {});
  }
}
