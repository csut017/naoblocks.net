import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { AdminHomeComponent } from './admin-home/admin-home.component';
import { LoginComponent } from './login/login.component';
import { StudentHomeComponent } from './student-home/student-home.component';
import { TeacherHomeComponent } from './teacher-home/teacher-home.component';
import { AuthenticationGuardService } from './services/authentication-guard.service';

const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'student', component: StudentHomeComponent, canActivate: [AuthenticationGuardService] },
  { path: 'teacher', component: TeacherHomeComponent, canActivate: [AuthenticationGuardService] },
  { path: 'admin', component: AdminHomeComponent, canActivate: [AuthenticationGuardService] },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
