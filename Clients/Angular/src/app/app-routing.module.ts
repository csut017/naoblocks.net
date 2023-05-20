import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthenticationGuardService } from './services/authentication-guard.service';

import { LandingComponent } from './components/landing/landing.component';
import { LoginComponent } from './components/login/login.component';

import { AdministratorHomeComponent } from './components/administrator-home/administrator-home.component';
import { StudentHomeComponent } from './components/student-home/student-home.component';
import { TeacherHomeComponent } from './components/teacher-home/teacher-home.component';
import { PageNotFoundComponent } from './components/page-not-found/page-not-found.component';
import { SystemInitialisationComponent } from './components/system-initialisation/system-initialisation.component';
import { MobileHomeComponent } from './components/mobile-home/mobile-home.component';

const routes: Routes = [
  // Public (no authentication) routes
  { path: '', component: LandingComponent, pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'initialisation', component: SystemInitialisationComponent },

  // Private (authenticated) routes
  { path: 'student', component: StudentHomeComponent, canActivate: [AuthenticationGuardService] },
  { path: 'student/:view', component: StudentHomeComponent, canActivate: [AuthenticationGuardService] },
  { path: 'teacher', component: TeacherHomeComponent, canActivate: [AuthenticationGuardService] },
  { path: 'teacher/:view', component: TeacherHomeComponent, canActivate: [AuthenticationGuardService] },
  { path: 'administrator', component: AdministratorHomeComponent, canActivate: [AuthenticationGuardService] },
  { path: 'administrator/:view', component: AdministratorHomeComponent, canActivate: [AuthenticationGuardService] },
  { path: 'administrator/:view/:item', component: AdministratorHomeComponent, canActivate: [AuthenticationGuardService] },
  { path: 'mobile', component: MobileHomeComponent, canActivate: [AuthenticationGuardService] },
  { path: 'mobile/:view', component: MobileHomeComponent, canActivate: [AuthenticationGuardService] },
  { path: 'mobile/:view/:item', component: MobileHomeComponent, canActivate: [AuthenticationGuardService] },
  { path: '**', component: PageNotFoundComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
