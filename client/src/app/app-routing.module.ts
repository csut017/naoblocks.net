import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { AdminHomeComponent } from './admin-home/admin-home.component';
import { LoginComponent } from './login/login.component';
import { StudentHomeComponent } from './student-home/student-home.component';
import { TeacherHomeComponent } from './teacher-home/teacher-home.component';
import { AuthenticationGuardService } from './services/authentication-guard.service';
import { PopoutComponent } from './popout/popout.component';
import { TangibleEditorComponent } from './tangible-editor/tangible-editor.component';
import { LandingComponent } from './landing/landing.component';
import { SystemInitialisationComponent } from './system-initialisation/system-initialisation.component';
import { TextEditorComponent } from './text-editor/text-editor.component';

const routes: Routes = [
  { path: '', component: LandingComponent, pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'initialisation', component: SystemInitialisationComponent },
  { path: 'popout/:view/:id', component: PopoutComponent },
  { path: 'student', component: StudentHomeComponent, canActivate: [AuthenticationGuardService] },
  { path: 'tangible', component: TangibleEditorComponent, canActivate: [AuthenticationGuardService] },
  { path: 'texteditor', component: TextEditorComponent, canActivate: [AuthenticationGuardService] },
  { path: 'teacher', component: TeacherHomeComponent, canActivate: [AuthenticationGuardService] },
  { path: 'administrator', component: AdminHomeComponent, canActivate: [AuthenticationGuardService] },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
