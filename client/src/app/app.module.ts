import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ClarityModule } from '@clr/angular';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { LoginComponent } from './login/login.component';
import { StudentHomeComponent } from './student-home/student-home.component';
import { TeacherHomeComponent } from './teacher-home/teacher-home.component';
import { AdminHomeComponent } from './admin-home/admin-home.component';
import { AuthenticationInterceptor } from './authentication-interceptor';
import { AboutComponent } from './about/about.component';
import { ChangeRoleComponent } from './change-role/change-role.component';
import { StudentsListComponent } from './students-list/students-list.component';
import { RobotsListComponent } from './robots-list/robots-list.component';
import { SettingsEditorComponent } from './settings-editor/settings-editor.component';
import { UsersListComponent } from './users-list/users-list.component';
import { LogsListComponent } from './logs-list/logs-list.component';
import { StudentEditorComponent } from './student-editor/student-editor.component';
import { RobotEditorComponent } from './robot-editor/robot-editor.component';
import { HeartbeatComponent } from './heartbeat/heartbeat.component';
import { SaveProgramComponent } from './save-program/save-program.component';
import { LoadProgramComponent } from './load-program/load-program.component';
import { SystemStatusComponent } from './system-status/system-status.component';
import { UserSettingsComponent } from './user-settings/user-settings.component';
import { UserSettingsEditorComponent } from './user-settings-editor/user-settings-editor.component';
import { RunSettingsComponent } from './run-settings/run-settings.component';
import { RobotTypesListComponent } from './robot-types-list/robot-types-list.component';
import { RobotTypeEditorComponent } from './robot-type-editor/robot-type-editor.component';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    StudentHomeComponent,
    TeacherHomeComponent,
    AdminHomeComponent,
    AboutComponent,
    ChangeRoleComponent,
    StudentsListComponent,
    RobotsListComponent,
    SettingsEditorComponent,
    UsersListComponent,
    LogsListComponent,
    StudentEditorComponent,
    RobotEditorComponent,
    HeartbeatComponent,
    SaveProgramComponent,
    LoadProgramComponent,
    SystemStatusComponent,
    UserSettingsComponent,
    UserSettingsEditorComponent,
    RunSettingsComponent,
    RobotTypesListComponent,
    RobotTypeEditorComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    ClarityModule,
    BrowserAnimationsModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule
  ],
  providers: [{
    provide: HTTP_INTERCEPTORS,
    useClass: AuthenticationInterceptor,
    multi: true,
  }],
  bootstrap: [AppComponent]
})
export class AppModule { }
