import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthenticationGuardService } from './services/authentication-guard.service';

import { LandingComponent } from './components/landing/landing.component';
import { LoginComponent } from './components/login/login.component';

import { AdministratorHomeComponent } from './components/administrator-home/administrator-home.component';
import { StudentHomeComponent } from './components/student-home/student-home.component';
import { TeacherHomeComponent } from './components/teacher-home/teacher-home.component';
import { PageNotFoundComponent } from './components/page-not-found/page-not-found.component';

const routes: Routes = [
  // Public (no authentication) routes
  { path: '', component: LandingComponent, pathMatch: 'full' },
  { path: 'login', component: LoginComponent },

  // Private (authenticated) routes
  { path: 'student', component: StudentHomeComponent, canActivate: [AuthenticationGuardService] },
  { path: 'teacher', component: TeacherHomeComponent, canActivate: [AuthenticationGuardService] },
  { path: 'teacher/:view', component: TeacherHomeComponent, canActivate: [AuthenticationGuardService] },
  { path: 'administrator', component: AdministratorHomeComponent, canActivate: [AuthenticationGuardService] },
  { path: 'administrator/:view', component: AdministratorHomeComponent, canActivate: [AuthenticationGuardService] },
  { path: '**', component: PageNotFoundComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
