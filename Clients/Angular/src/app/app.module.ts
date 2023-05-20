import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { AuthenticationInterceptor } from './authentication-interceptor';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MaterialModule } from './material.module';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { LandingComponent } from './components/landing/landing.component';
import { LoginComponent } from './components/login/login.component';
import { ChangeRoleComponent } from './components/change-role/change-role.component';
import { ChangeViewComponent } from './components/change-view/change-view.component';
import { AboutComponent } from './components/about/about.component';
import { StudentHomeComponent } from './components/student-home/student-home.component';
import { TeacherHomeComponent } from './components/teacher-home/teacher-home.component';
import { LayoutModule } from '@angular/cdk/layout';
import { AdministratorHomeComponent } from './components/administrator-home/administrator-home.component';
import { RobotTypesListComponent } from './components/robot-types-list/robot-types-list.component';
import { LogsListComponent } from './components/logs-list/logs-list.component';
import { UsersListComponent } from './components/users-list/users-list.component';
import { SystemConfigurationComponent } from './components/system-configuration/system-configuration.component';
import { SystemDashboardComponent } from './components/system-dashboard/system-dashboard.component';
import { RobotsListComponent } from './components/robots-list/robots-list.component';
import { StudentsListComponent } from './components/students-list/students-list.component';
import { RobotEditorComponent } from './components/robot-editor/robot-editor.component';
import { RobotTypeEditorComponent } from './components/robot-type-editor/robot-type-editor.component';
import { UserEditorComponent } from './components/user-editor/user-editor.component';
import { PageNotFoundComponent } from './components/page-not-found/page-not-found.component';
import { StudentEditorComponent } from './components/student-editor/student-editor.component';
import { DeletionConfirmationComponent } from './components/deletion-confirmation/deletion-confirmation.component';
import { MultilineMessageComponent } from './components/multiline-message/multiline-message.component';
import { BlocklyEditorComponent } from './components/blockly-editor/blockly-editor.component';
import { SettingsDebugComponent } from './components/settings-debug/settings-debug.component';
import { SettingsStudentComponent } from './components/settings-student/settings-student.component';
import { ProgramSaveComponent } from './components/program-save/program-save.component';
import { ProgramLoadComponent } from './components/program-load/program-load.component';
import { ConfirmDialogComponent } from './components/confirm-dialog/confirm-dialog.component';
import { ExecutionStatusComponent } from './components/execution-status/execution-status.component';
import { TangibleEditorComponent } from './components/tangible-editor/tangible-editor.component';
import { SystemInitialisationComponent } from './components/system-initialisation/system-initialisation.component';
import { ImportDialogComponent } from './components/import-dialog/import-dialog.component';
import { UserInterfaceConfigurationComponent } from './components/user-interface-configuration/user-interface-configuration.component';
import { UserSettingsComponent } from './components/user-settings/user-settings.component';
import { ToolboxEditorComponent } from './components/toolbox-editor/toolbox-editor.component';
import { ToolboxListComponent } from './components/toolbox-list/toolbox-list.component';
import { UserInterfaceEditorComponent } from './components/user-interface-editor/user-interface-editor.component';
import { ReportSettingsComponent } from './components/report-settings/report-settings.component';
import { MAT_DATE_LOCALE } from '@angular/material/core';
import { MatLuxonDateModule } from '@angular/material-luxon-adapter';
import { ExportDataComponent } from './components/export-data/export-data.component';
import { RobotImportDialogComponent } from './components/robot-import-dialog/robot-import-dialog.component';
import { DragAndDropDirective } from './drag-and-drop.directive';
import { UserImportDialogComponent } from './components/user-import-dialog/user-import-dialog.component';
import { RobotTypeAllowedValuesListComponent } from './components/robot-type-allowed-values-list/robot-type-allowed-values-list.component';
import { NamedValueEditorComponent } from './components/named-value-editor/named-value-editor.component';
import { RobotValuesListComponent } from './components/robot-values-list/robot-values-list.component';
import { RobotTypeDefinitionImportComponent } from './components/robot-type-definition-import/robot-type-definition-import.component';
import { ZXingScannerModule } from '@zxing/ngx-scanner';
import { QRCodeReaderComponent } from './components/qrcode-reader/qrcode-reader.component';
import { RobotQuickLinksComponent } from './components/robot-quick-links/robot-quick-links.component';
import { MobileHomeComponent } from './components/mobile-home/mobile-home.component';

@NgModule({
  declarations: [
    DragAndDropDirective,
    AppComponent,
    LandingComponent,
    LoginComponent,
    StudentHomeComponent,
    ChangeRoleComponent,
    ChangeViewComponent,
    AboutComponent,
    TeacherHomeComponent,
    AdministratorHomeComponent,
    RobotTypesListComponent,
    LogsListComponent,
    UsersListComponent,
    SystemConfigurationComponent,
    SystemDashboardComponent,
    RobotsListComponent,
    StudentsListComponent,
    RobotEditorComponent,
    RobotTypeEditorComponent,
    UserEditorComponent,
    PageNotFoundComponent,
    StudentEditorComponent,
    DeletionConfirmationComponent,
    MultilineMessageComponent,
    BlocklyEditorComponent,
    SettingsDebugComponent,
    SettingsStudentComponent,
    ProgramSaveComponent,
    ProgramLoadComponent,
    ConfirmDialogComponent,
    ExecutionStatusComponent,
    TangibleEditorComponent,
    SystemInitialisationComponent,
    ImportDialogComponent,
    UserInterfaceConfigurationComponent,
    UserSettingsComponent,
    ToolboxEditorComponent,
    ToolboxListComponent,
    UserInterfaceEditorComponent,
    ReportSettingsComponent,
    ExportDataComponent,
    RobotImportDialogComponent,
    UserImportDialogComponent,
    RobotTypeAllowedValuesListComponent,
    NamedValueEditorComponent,
    RobotValuesListComponent,
    RobotTypeDefinitionImportComponent,
    QRCodeReaderComponent,
    RobotQuickLinksComponent,
    MobileHomeComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    MaterialModule,
    LayoutModule,
    MatLuxonDateModule,
    ZXingScannerModule,
  ],
  providers: [{
    provide: HTTP_INTERCEPTORS,
    useClass: AuthenticationInterceptor,
    multi: true,
  }, {
    provide: MAT_DATE_LOCALE, 
    useValue: 'en-NZ',
  }],
  bootstrap: [AppComponent]
})
export class AppModule { }
